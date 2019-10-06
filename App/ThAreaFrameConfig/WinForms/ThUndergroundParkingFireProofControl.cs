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
            throw new NotImplementedException();
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
            if (!compartment.IsDefined && compartment.Storey > 0)
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
    }
}
