using System;
using System.Collections.Generic;
using ThAreaFrameConfig.View;
using ThAreaFrameConfig.Model;
using ThAreaFrameConfig.Presenter;
using DevExpress.XtraGrid.Views.Grid;
using Autodesk.AutoCAD.Runtime;

namespace ThAreaFrameConfig.WinForms
{
    public partial class ThRoofControl : DevExpress.XtraEditors.XtraUserControl, IRoofView
    {
        private ThRoofPresenter Presenter;
        private ThRoofDbRepository DbRepository;


        public ThRoofControl()
        {
            InitializeComponent();
            InitializeGridControl();
        }

        public List<ThRoof> Roofs
        {
            get
            {
                return (List<ThRoof>)gridControl_roof.DataSource;
            }

            set
            {
                gridControl_roof.DataSource = value;
            }
        }

        public void Attach(IRoofPresenterCallback presenter)
        {
            //
        }

        public void Reload()
        {
            DbRepository = new ThRoofDbRepository();
            gridControl_roof.DataSource = DbRepository.Roofs;
            gridControl_roof.RefreshDataSource();
        }

        public void InitializeGridControl()
        {
            Presenter = new ThRoofPresenter(this);
            DbRepository = new ThRoofDbRepository();
            if (DbRepository.Roofs.Count == 0)
            {
                DbRepository.AppendRoof();
            }
            gridControl_roof.DataSource = DbRepository.Roofs;
            gridControl_roof.RefreshDataSource();
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
            if (e.HitInfo.Column.FieldName != "gridColumn_pick")
            {
                return;
            }

            try
            {
                GridView gridView = (GridView)sender;
                ThRoof roof = (ThRoof)gridView.GetRow(e.RowHandle);
                string name = ThResidentialRoomUtil.LayerName(roof);
                Presenter.OnPickAreaFrames(name);

                // 更新界面
                this.Reload();
            }
            catch(System.Exception exception)
            {
#if DEBUG
                Presenter.OnHandleAcadException(exception);
#endif
            }
        }

        private void gridView_roof_RowUpdated(object sender, DevExpress.XtraGrid.Views.Base.RowObjectEventArgs e)
        {
            if (!(sender is GridView view))
            {
                return;
            }

            try
            {
                ThRoof roof = (ThRoof)e.Row;
                string name = ThResidentialRoomUtil.LayerName(roof);
                Presenter.OnRenameAreaFrameLayer(name, roof.Frame);

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

        private void gridView1_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName == "Area" && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                double area = Convert.ToDouble(e.Value);
                e.DisplayText = Converter.DistanceToString(area, DistanceUnitFormat.Decimal, 2);
            }
        }

        private void gridView_roof_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            GridView view = sender as GridView;
            switch(view.FocusedColumn.FieldName)
            {
                case "Coefficient":
                case "FARCoefficient":
                    {
                        double value;
                        if (!double.TryParse((string)e.Value, out value))
                        {
                            e.Valid = false;
                            e.ErrorText = "请输入浮点数";
                        }
                    }
                    break;
            };
        }
    }
}
