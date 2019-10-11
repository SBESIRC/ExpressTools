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
using AcHelper;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace ThAreaFrameConfig.WinForms
{
    public partial class ThUndergroundParkingFireProofControl : XtraUserControl, IFireCompartmentView
    {
        private ThFCUndergroundParkingPresenter Presenter;
        private ThFCUnderGroundParkingDbRepository DbRepository;


        public ThUndergroundParkingFireProofControl()
        {
            InitializeComponent();
            InitializeGridControl();
            InitializeDataBindings();
        }

        public ThFCUnderGroundParkingSettings Settings
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
            Presenter = new ThFCUndergroundParkingPresenter(this);
            DbRepository = new ThFCUnderGroundParkingDbRepository();
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
            // 指定外框线图层
            comboBox_outer_frame.DataSource = DbRepository.Layers;
            comboBox_outer_frame.SelectedItem = Settings.Layers["OUTERFRAME"];

            // 指定内框线图层
            comboBox_inner_frame.DataSource = DbRepository.Layers;
            comboBox_inner_frame.SelectedItem = Settings.Layers["INNERFRAME"];
        }

        private void gridView_fire_compartment_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName == "Area" && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                double area = Convert.ToDouble(e.Value);
                e.DisplayText = Converter.DistanceToString(area, DistanceUnitFormat.Decimal, 2);
            }
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
                e.Value = compartment.IsDefined ? "" : "选择";
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

        private void gridView_fire_compartment_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (!(sender is GridView view))
            {
                return;
            }

            var columns = new List<string>
            {
                "Storey"
            };
            if (!columns.Contains(e.Column.FieldName))
            {
                return;
            }

            ThFireCompartment compartment = view.GetRow(e.RowHandle) as ThFireCompartment;
            // 计算编号索引，即所有防火分区（地下车库）的下一个
            var compartments = Settings.Compartments.Where(
                o => o.IsDefined &&
                o.Subkey == compartment.Subkey);
            // 由于编号索引是连续的，下一个索引即为个数+1
            UInt16 index = (UInt16)(compartments.Count() + 1);
            if (compartment.IsDefined)
            {
                // 修改图纸
                if (Presenter.OnModifyFireCompartment(compartment))
                {
                    // 更新界面
                    this.Reload();
                }
            }
            else
            {
                // 刷新Row
                view.RefreshRow(e.RowHandle);
            }
        }

        private void gridView_fire_compartment_CellValueChanging(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {

        }

        private void gridView_fire_compartment_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            GridView view = sender as GridView;
            switch (view.FocusedColumn.FieldName)
            {
                case "Storey":
                    {
                        if (!int.TryParse(e.Value.ToString(), out int value) || (value >= 0))
                        {
                            e.Valid = false;
                            e.ErrorText = "请输入负整数";
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
                    e.Menu.Items.Add(CreateModifyMenuItem(view, e.HitInfo.RowHandle));
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

        DXMenuItem CreateModifyMenuItem(GridView view, int rowHandle)
        {
            return new DXMenuItem("修改", new EventHandler(OnModifyFireCompartmentsClick))
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

        void OnModifyFireCompartmentsClick(object sender, EventArgs e)
        {
            DXMenuItem menuItem = sender as DXMenuItem;
            if (menuItem.Tag is RowInfo ri)
            {
                using (var dlg = new ThFireCompartmentModifyDialog())
                {
                    if (AcadApp.ShowModalDialog(dlg) != DialogResult.OK)
                        return;

                    // 更新图纸
                    // 支持多选
                    var modifiedCompartments = new List<ThFireCompartment>();
                    foreach (var handle in ri.View.GetSelectedRows())
                    {
                        var compartment = ri.View.GetRow(handle) as ThFireCompartment;
                        if (compartment.IsDefined)
                        {
                            bool bModified = false;
                            if (dlg.Storey != null && dlg.Storey.Value != 0)
                            {
                                bModified = true;
                                compartment.Storey = dlg.Storey.Value;
                            }
                            if (dlg.SelfExtinguishingSystem != null)
                            {
                                bModified = true;
                                compartment.SelfExtinguishingSystem = dlg.SelfExtinguishingSystem.Value;
                            }
                            if (bModified)
                            {
                                modifiedCompartments.Add(compartment);
                            }
                        }
                    }

                    // 计算编号索引，即所有防火分区（地下车库）的下一个
                    // 由于编号索引是连续的，下一个索引即为个数+1
                    foreach (var compartment in modifiedCompartments)
                    {
                        var compartments = Settings.Compartments.Where(
                            o => o.IsDefined &&
                            o.Subkey == compartment.Subkey);
                        compartment.Index = (UInt16)(compartments.Count() + 1);
                    }

                    // 修改图纸并刷新界面
                    if (Presenter.OnModifyFireCompartments(modifiedCompartments))
                    {
                        // 更新界面
                        this.Reload();
                    }
                }
            }
        }

        private void comboBox_inner_frame_SelectionChangeCommitted(object sender, EventArgs e)
        {
            System.Windows.Forms.ComboBox control = sender as System.Windows.Forms.ComboBox;
            Settings.Layers["INNERFRAME"] = control.SelectedItem as string;
        }

        private void comboBox_outer_frame_SelectionChangeCommitted(object sender, EventArgs e)
        {
            System.Windows.Forms.ComboBox control = sender as System.Windows.Forms.ComboBox;
            Settings.Layers["OUTERFRAME"] = control.SelectedItem as string;
        }

        private void button_merge_Click(object sender, EventArgs e)
        {
            if (Presenter.OnMergePickedFireCompartments(Settings))
            {
                // 更新界面
                this.Reload();
            }
        }

        private void button_pick_outer_frame_Click(object sender, EventArgs e)
        {
            // 隐藏非模态对话框
            var dlg = FindForm();
            if (dlg != null)
            {
                dlg.Hide();
            }

            if (Presenter.OnSetFireCompartmentLayer(Settings, "OUTERFRAME"))
            {
                comboBox_outer_frame.SelectedItem = Settings.Layers["OUTERFRAME"];
            }

            // 恢复非模态对话框
            if (dlg != null)
            {
                dlg.Show();
            }
        }

        private void button_pick_inner_frame_Click(object sender, EventArgs e)
        {
            // 隐藏非模态对话框
            var dlg = FindForm();
            if (dlg != null)
            {
                dlg.Hide();
            }

            if (Presenter.OnSetFireCompartmentLayer(Settings, "INNERFRAME"))
            {
                comboBox_inner_frame.SelectedItem = Settings.Layers["INNERFRAME"];
            }

            // 恢复非模态对话框
            if (dlg != null)
            {
                dlg.Show();
            }
        }

        private void button_create_fill_Click(object sender, EventArgs e)
        {
            var compartments = Compartments.Where(o => o.IsDefined).ToList();
            Presenter.OnCreateFireCompartmentFills(compartments);
        }

        private void button_create_table_Click(object sender, EventArgs e)
        {
            Presenter.OnCreateFireCompartmentTable(Settings);
        }
    }
}
