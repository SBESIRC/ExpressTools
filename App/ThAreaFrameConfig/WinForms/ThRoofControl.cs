using System;
using System.Collections.Generic;
using ThAreaFrameConfig.View;
using ThAreaFrameConfig.Model;
using ThAreaFrameConfig.Presenter;
using DevExpress.XtraGrid.Views.Grid;
using Autodesk.AutoCAD.Runtime;
using DevExpress.Utils;
using DevExpress.Utils.Menu;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;

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
            DbRepository.AppendDefaultRoof();
            gridControl_roof.DataSource = DbRepository.Roofs;
            gridControl_roof.RefreshDataSource();
        }

        public void InitializeGridControl()
        {
            Presenter = new ThRoofPresenter(this);
            DbRepository = new ThRoofDbRepository();
            DbRepository.AppendDefaultRoof();
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
            if (e.HitInfo.Column == null)
            {
                return;
            }
            if (e.HitInfo.Column.FieldName != "gridColumn_pick")
            {
                return;
            }

            GridView gridView = sender as GridView;
            ThRoof roof = gridView.GetRow(e.RowHandle) as ThRoof;
            if (!roof.IsDefined)
            {
                // 面积框线图层名
                string name = ThResidentialRoomUtil.LayerName(roof);

                // 选取面积框线
                Presenter.OnPickAreaFrames(name);

                // 更新界面
                this.Reload();
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
                if (!roof.IsDefined)
                {
                    return;
                }

                // 更新图纸
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
                        if (!double.TryParse((string)e.Value, out double value))
                        {
                            e.Valid = false;
                            e.ErrorText = "请输入浮点数";
                        }
                    }
                    break;
            };
        }

        private void gridView_roof_DoubleClick(object sender, EventArgs e)
        {
            DXMouseEventArgs ea = e as DXMouseEventArgs;
            GridView view = sender as GridView;
            GridHitInfo info = view.CalcHitInfo(ea.Location);
            if (info.InRow || info.InRowCell)
            {
                ThRoof roof = view.GetRow(info.RowHandle) as ThRoof;
                if (roof.IsDefined)
                {
                    Presenter.OnHighlightAreaFrame(roof.Frame);
                }
            }
        }

        private void gridView_roof_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
        {
            GridView view = sender as GridView;
            if (e.MenuType == GridMenuType.Row)
            {
                if (e.HitInfo.InRow || e.HitInfo.InRowCell)
                {
                    ThRoof roof = view.GetRow(e.HitInfo.RowHandle) as ThRoof;
                    if (!roof.IsDefined)
                        return;

                    e.Menu.Items.Clear();
                    e.Menu.Items.Add(CreateDeleteMenuItem(view, e.HitInfo.RowHandle));
                    e.Menu.Items.Add(CreateDeleteAllMenuItem(view, e.HitInfo.RowHandle));
                }
            }
        }

        DXMenuItem CreateDeleteMenuItem(GridView view, int rowHandle)
        {
            return new DXMenuItem("删除", new EventHandler(OnDeleteRoofItemClick))
            {
                Tag = new RowInfo(view, rowHandle)
            };
        }

        DXMenuItem CreateDeleteAllMenuItem(GridView view, int rowHandle)
        {
            return new DXMenuItem("全部删除", new EventHandler(OnDeleteAllRoofItemsClick))
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

        void OnDeleteRoofItemClick(object sender, EventArgs e)
        {
            DXMenuItem menuItem = sender as DXMenuItem;
            if (menuItem.Tag is RowInfo ri)
            { 
                // 更新图纸
                ThRoof roof = ri.View.GetRow(ri.RowHandle) as ThRoof;
                string layer = ThResidentialRoomDbUtil.LayerName(roof.Frame);
                Presenter.OnDeleteAreaFrame(roof.Frame);
                Presenter.OnDeleteAreaFrameLayer(layer);

                // 更新界面
                this.Reload();
            }
        }

        void OnDeleteAllRoofItemsClick(object sender, EventArgs e)
        {
            DXMenuItem menuItem = sender as DXMenuItem;
            if (menuItem.Tag is RowInfo ri)
            {
                // 更新图纸
                foreach(var roof in DbRepository.Roofs)
                {
                    if (!roof.IsDefined)
                        continue;

                    string layer = ThResidentialRoomDbUtil.LayerName(roof.Frame);
                    Presenter.OnDeleteAreaFrame(roof.Frame);
                    Presenter.OnDeleteAreaFrameLayer(layer);
                }

                // 更新界面
                this.Reload();
            }
        }
    }
}
