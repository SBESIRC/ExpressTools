using System;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using ThAreaFrameConfig.Model;
using ThAreaFrameConfig.View;
using ThAreaFrameConfig.Presenter;
using DevExpress.Utils.Menu;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;
using Autodesk.AutoCAD.Runtime;

namespace ThAreaFrameConfig.WinForms
{
    public partial class ThCommerceFireProofControl : XtraUserControl, IFCCommerceView
    {
        private BindingSource bindingSource;
        private ThFCCommercePresenter Presenter;
        private ThFCCommerceDbRepository DbRepository;

        public ThCommerceFireProofControl()
        {
            InitializeComponent();
            InitializeGridControl();
            InitializeDataBindings();
        }

        public ThFCCommerceSettings Settings
        {
            get
            {
                return DbRepository.Settings;
            }
        }

        public List<ThFireCompartment> Compartments
        {
            get
            {
                return (List<ThFireCompartment>)gridControl_fire_compartment.DataSource;
            }

            set
            {
                gridControl_fire_compartment.DataSource = value;
            }
        }

        public void Reload()
        {
            DbRepository.ReloadFireCompartments();
            DbRepository.AppendDefaultFireCompartment();
            gridControl_fire_compartment.DataSource = Settings.Compartments;
            gridControl_fire_compartment.RefreshDataSource();
            label_merge_compartment.Text = MergedCompartmentCountString();
        }

        public void InitializeGridControl()
        {
            Presenter = new ThFCCommercePresenter(this);
            DbRepository = new ThFCCommerceDbRepository();
            DbRepository.AppendDefaultFireCompartment();
            gridControl_fire_compartment.DataSource = Settings.Compartments;
            gridControl_fire_compartment.RefreshDataSource();
            label_merge_compartment.Text = MergedCompartmentCountString();
        }

        private string MergedCompartmentCountString()
        {
            int count = Settings.Compartments.Where(o => o.IsMerged).Count();
            return string.Format("点击合并防火分区：已建立合并关系{0}个", count);
        }

        public void InitializeDataBindings()
        {
            // 创建BindingSource
            bindingSource = new BindingSource()
            {
                DataSource = Settings
            };

            // 子项编号
            textBox_sub_key.DataBindings.Add(
                "Text",
                bindingSource,
                "SubKey",
                true,
                DataSourceUpdateMode.OnPropertyChanged);

            // 地上层数
            textBox_storey.DataBindings.Add(
                "Text",
                bindingSource,
                "Storey",
                true,
                DataSourceUpdateMode.OnPropertyChanged);

            // 耐火等级
            comboBox_fire_resistance.SelectedIndex = (int)Settings.Resistance;

            // 人员密度
            comboBox_density.SelectedIndex = (int)Settings.Density;

            // 指定外框线图层
            comboBox_outer_frame.DataSource = DbRepository.Layers;
            comboBox_outer_frame.SelectedItem = Settings.Layers["OUTERFRAME"];

            // 指定内框线图层
            comboBox_inner_frame.DataSource = DbRepository.Layers;
            comboBox_inner_frame.SelectedItem = Settings.Layers["INNERFRAME"];
        }

        private void gridView_fire_compartment_CustomUnboundColumnData(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDataEventArgs e)
        {
            if (!(sender is GridView view))
            {
                return;
            }
            if (e.Column.FieldName != "gridColumn_pick")
            {
                return;
            }
            if (e.IsGetData)
            {
                ThFireCompartment compartment = e.Row as ThFireCompartment;
                e.Value = (compartment.IsDefined || compartment.Storey == 0) ? "" : "选择";
            }
        }

        private void gridView_fire_compartment_RowClick(object sender, RowClickEventArgs e)
        {
            if (!(sender is GridView view))
            {
                return;
            }
            if (e.HitInfo.Column == null)
            {
                return;
            }
            if (e.HitInfo.Column.FieldName != "gridColumn_pick")
            {
                return;
            }

            GridView gridView = sender as GridView;
            ThFireCompartment compartment = gridView.GetRow(e.RowHandle) as ThFireCompartment;
            if (!compartment.IsDefined)
            {
                // 面积框线图层名
                string layer = Settings.Layers["OUTERFRAME"];
                string islandLayer = Settings.Layers["INNERFRAME"];

                // 选取面积框线
                if (Presenter.OnPickAreaFrames(compartment, layer, islandLayer))
                {
                    // 更新界面
                    this.Reload();
                }
            }
        }

        private void gridView_fire_compartment_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName == "Area" && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                double area = Convert.ToDouble(e.Value);
                e.DisplayText = Converter.DistanceToString(area, DistanceUnitFormat.Decimal, 2);
            }
        }

        private void gridView_fire_compartment_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (!(sender is GridView view))
            {
                return;
            }

            var columns = new List<string>
            {
                "Storey",
                "SelfExtinguishingSystem"
            };
            if (!columns.Contains(e.Column.FieldName))
            {
                return;
            }

            ThFireCompartment compartment = view.GetRow(e.RowHandle) as ThFireCompartment;
            if (compartment.IsDefined)
            {
                if (Presenter.OnModifyFireCompartment(compartment))
                {
                    // 更新界面
                    this.Reload();
                }
            }
            else
            {
                // 计算编号索引
                var compartments = Settings.Compartments.Where(
                    o => o.IsDefined &&
                    o.Subkey == compartment.Subkey &&
                    o.Storey == compartment.Storey);

                // 由于编号索引是连续的，下一个索引即为个数+1
                compartment.Index = (UInt16)(compartments.Count() + 1);

                // 刷新Row
                view.RefreshRow(e.RowHandle);
            }
        }

        private void gridView_fire_compartment_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            GridView view = sender as GridView;
            switch (view.FocusedColumn.FieldName)
            {
                case "Storey":
                    {
                        if (!int.TryParse(e.Value.ToString(), out int value))
                        {
                            e.Valid = false;
                            e.ErrorText = "请输入整数";
                        }

                    }
                    break;
            };
        }

        private void gridView_fire_compartment_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
        {
            GridView view = sender as GridView;
            if (e.MenuType == GridMenuType.Row)
            {
                if (e.HitInfo.InRow || e.HitInfo.InRowCell)
                {
                    foreach (var handle in view.GetSelectedRows())
                    {
                        var compartment = view.GetRow(handle) as ThFireCompartment;
                        if (!compartment.IsDefined)
                        {
                            return;
                        }
                    }

                    e.Menu.Items.Clear();
                    e.Menu.Items.Add(CreateMergeMenuItem(view, e.HitInfo.RowHandle));
                    e.Menu.Items.Add(CreateDeleteMenuItem(view, e.HitInfo.RowHandle));
                }
            }
        }

        DXMenuItem CreateMergeMenuItem(GridView view, int rowHandle)
        {
            return new DXMenuItem("合并", new EventHandler(OnMergeFireCompartmentsItemClick))
            {
                Tag = new RowInfo(view, rowHandle)
            };
        }

        DXMenuItem CreateDeleteMenuItem(GridView view, int rowHandle)
        {
            return new DXMenuItem("删除", new EventHandler(OnDeleteFireCompartmentsClick))
            {
                Tag = new RowInfo(view, rowHandle)
            };
        }

        class RowInfo
        {
            public RowInfo(GridView view, int rowHandle)
            {
                this.RowHandle = rowHandle;
                this.View = view;
            }
            public GridView View;
            public int RowHandle;
        }

        void OnMergeFireCompartmentsItemClick(object sender, EventArgs e)
        {
            DXMenuItem menuItem = sender as DXMenuItem;
            if (menuItem.Tag is RowInfo ri)
            {
                // 更新图纸
                // 支持多选
                var compartments = new List<ThFireCompartment>();
                foreach (var handle in ri.View.GetSelectedRows())
                {
                    var compartment = ri.View.GetRow(handle) as ThFireCompartment;
                    if (compartment.IsDefined)
                    {
                        compartments.Add(compartment);
                    }
                }

                if (Presenter.OnMergeFireCompartments(compartments))
                {
                    // 更新界面
                    this.Reload();
                }
            }
        }

        void OnDeleteFireCompartmentsClick(object sender, EventArgs e)
        {
            DXMenuItem menuItem = sender as DXMenuItem;
            if (menuItem.Tag is RowInfo ri)
            {
                // 更新图纸
                // 支持多选
                var compartments = new List<ThFireCompartment>();
                foreach (var handle in ri.View.GetSelectedRows())
                {
                    var compartment = ri.View.GetRow(handle) as ThFireCompartment;
                    if (compartment.IsDefined)
                    {
                        compartments.Add(compartment);
                    }
                }

                if (Presenter.OnDeleteFireCompartments(compartments))
                {
                    // 更新界面
                    this.Reload();
                }
            }
        }

        private void button_create_table_Click(object sender, EventArgs e)
        {
            Presenter.OnCreateFCCommerceTable(Settings);
        }

        private void button_create_fill_Click(object sender, EventArgs e)
        {
            var compartments = Compartments.Where(o => o.IsDefined).ToList();
            Presenter.OnCreateFCCommerceFills(compartments);
        }

        private void button_pick_outer_frame_Click(object sender, EventArgs e)
        {
            if (Presenter.OnSetFCCommerceLayer(Settings, "OUTERFRAME"))
            {
                comboBox_outer_frame.SelectedItem = Settings.Layers["OUTERFRAME"];
            }
        }

        private void button_pick_inner_frame_Click(object sender, EventArgs e)
        {
            if (Presenter.OnSetFCCommerceLayer(Settings, "INNERFRAME"))
            {
                comboBox_inner_frame.SelectedItem = Settings.Layers["INNERFRAME"];
            }
        }

        private void comboBox_outer_frame_SelectionChangeCommitted(object sender, EventArgs e)
        {
            System.Windows.Forms.ComboBox control = sender as System.Windows.Forms.ComboBox;
            Settings.Layers["OUTERFRAME"] = control.SelectedItem as string;
        }

        private void comboBox_inner_frame_SelectionChangeCommitted(object sender, EventArgs e)
        {
            System.Windows.Forms.ComboBox control = sender as System.Windows.Forms.ComboBox;
            Settings.Layers["INNERFRAME"] = control.SelectedItem as string;
        }

        private void comboBox_fire_resistance_SelectedIndexChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.ComboBox control = sender as System.Windows.Forms.ComboBox;
            Settings.Resistance = (ThFCCommerceSettings.FireResistance)control.SelectedIndex;
        }

        private void comboBox_density_SelectedIndexChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.ComboBox control = sender as System.Windows.Forms.ComboBox;
            Settings.Density = (ThFCCommerceSettings.OccupantDensity)control.SelectedIndex;
        }

        private void button_merge_Click(object sender, EventArgs e)
        {
            if (Presenter.OnMergePickedFireCompartments(Settings))
            {
                // 更新界面
                this.Reload();
            }
        }
    }
}
