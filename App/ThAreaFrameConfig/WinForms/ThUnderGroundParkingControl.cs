using System;
using System.Collections.Generic;
using ThAreaFrameConfig.View;
using ThAreaFrameConfig.Model;
using ThAreaFrameConfig.Presenter;
using DevExpress.XtraGrid.Views.Grid;

namespace ThAreaFrameConfig.WinForms
{
    public partial class ThUnderGroundParkingControl : DevExpress.XtraEditors.XtraUserControl, IUnderGroundParkingView
    {
        private ThUnderGroundParkingPresenter Presenter;
        private ThUnderGroundParkingDbRepository DbRepository;

        public ThUnderGroundParkingControl()
        {
            InitializeComponent();
            InitializeGridControl();
        }

        public void Attach(IUnderGroundParkingPresenterCallback presenter)
        {
            //
        }

        public List<ThUnderGroundParking> Parkings
        {
            get
            {
                return (List<ThUnderGroundParking>)gridControl_parking.DataSource;
            }

            set
            {
                gridControl_parking.DataSource = value;
            }
        }

        public void Reload()
        {
            DbRepository = new ThUnderGroundParkingDbRepository();
            DbRepository.AppendDefaultUnderGroundParking();
            gridControl_parking.DataSource = DbRepository.Parkings;
            gridControl_parking.RefreshDataSource();
        }

        public void InitializeGridControl()
        {
            Presenter = new ThUnderGroundParkingPresenter(this);
            DbRepository = new ThUnderGroundParkingDbRepository();
            DbRepository.AppendDefaultUnderGroundParking();
            gridControl_parking.DataSource = DbRepository.Parkings;
            gridControl_parking.RefreshDataSource();
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

        private void gridView1_RowClick(object sender, DevExpress.XtraGrid.Views.Grid.RowClickEventArgs e)
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

            try
            {
                GridView gridView = (GridView)sender;
                ThUnderGroundParking parking = (ThUnderGroundParking)gridView.GetRow(e.RowHandle);
                string name = ThResidentialRoomUtil.LayerName(parking);
                Presenter.OnPickAreaFrames(name);

                // 更新界面
                this.Reload();
            }
            catch (Exception exception)
            {
#if DEBUG
                Presenter.OnHandleAcadException(exception);
#endif
            }
        }

        private void gridView_parking_RowUpdated(object sender, DevExpress.XtraGrid.Views.Base.RowObjectEventArgs e)
        {
            if (!(sender is GridView view))
            {
                return;
            }

            try
            {
                ThUnderGroundParking parking = (ThUnderGroundParking)e.Row;
                if (!parking.IsDefined)
                {
                    return;
                }

                // 更新图纸
                string name = ThResidentialRoomUtil.LayerName(parking);
                Presenter.OnRenameAreaFrameLayer(name, parking.Frames[0]);

                // 更新界面
                this.Reload();
            }
            catch (System.Exception exception)
            {
#if DEBUG
                Presenter.OnHandleAcadException(exception);
#endif
            }
        }

        private void gridView_parking_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            GridView view = sender as GridView;
            switch (view.FocusedColumn.FieldName)
            {
                case "Floors":
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
    }
}
