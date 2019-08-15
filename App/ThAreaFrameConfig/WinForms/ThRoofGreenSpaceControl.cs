using System;
using System.Collections.Generic;
using ThAreaFrameConfig.View;
using ThAreaFrameConfig.Model;
using ThAreaFrameConfig.Presenter;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Base;
using Autodesk.AutoCAD.Runtime;

namespace ThAreaFrameConfig.WinForms
{
    public partial class ThRoofGreenSpaceControl : DevExpress.XtraEditors.XtraUserControl, IRoofGreenSpaceView
    {
        private ThRoofGreenSpacePresenter Presenter;
        private ThRoofGreenSpaceDbRepository DbRepository;


        public ThRoofGreenSpaceControl()
        {
            InitializeComponent();
            InitializeGridControl();
        }

        public void Attach(IRoofGreenSpacePresenterCallback presenter)
        {
            //
        }

        public List<ThRoofGreenSpace> Spaces
        {
            get
            {
                return (List<ThRoofGreenSpace>)gridControl_space.DataSource;
            }

            set
            {
                gridControl_space.DataSource = value;
            }
        }

        public void Reload()
        {
            DbRepository = new ThRoofGreenSpaceDbRepository();
            gridControl_space.DataSource = DbRepository.Spaces;
            gridControl_space.RefreshDataSource();
        }

        public void InitializeGridControl()
        {
            Presenter = new ThRoofGreenSpacePresenter(this);
            DbRepository = new ThRoofGreenSpaceDbRepository();
            if (DbRepository.Spaces.Count == 0)
            {
                DbRepository.AppendRoofGreenSpace();
            }
            gridControl_space.DataSource = DbRepository.Spaces;
            gridControl_space.RefreshDataSource();
        }

        private void gridView1_CustomUnboundColumnData(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDataEventArgs e)
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
                e.Value = "选择";
            }
        }

        private void gridView1_RowClick(object sender, RowClickEventArgs e)
        {
            if (!(sender is GridView view))
            {
                return;
            }
            if (e.HitInfo.Column.FieldName != "gridColumn_pick")
            {
                return;
            }

            GridView gridView = (GridView)sender;
            ThRoofGreenSpace roofGreenSpace = (ThRoofGreenSpace)gridView.GetRow(e.RowHandle);
            string name = ThResidentialRoomUtil.LayerName(roofGreenSpace);
            Presenter.OnPickAreaFrames(name);

            // 更新界面
            this.Reload();
        }

        private void gridView1_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName == "Area" && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                double area = Convert.ToDouble(e.Value);
                e.DisplayText = Converter.DistanceToString(area, DistanceUnitFormat.Decimal, 2);
            }
        }
    }
}
