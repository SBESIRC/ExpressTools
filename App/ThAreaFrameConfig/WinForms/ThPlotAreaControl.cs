using System;
using System.Collections.Generic;
using ThAreaFrameConfig.View;
using ThAreaFrameConfig.Presenter;
using ThAreaFrameConfig.Model;
using DevExpress.Utils;
using DevExpress.Utils.Menu;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using Autodesk.AutoCAD.Runtime;

namespace ThAreaFrameConfig.WinForms
{
    public partial class ThPlotAreaControl : DevExpress.XtraEditors.XtraUserControl, IPlotSpaceView
    {
        private ThPlotSpacePresenter Presenter;
        private ThPlotSpaceDbRepository DbRepository;


        public ThPlotAreaControl()
        {
            InitializeComponent();
            InitializeGridControl();
        }

        public List<ThPlotSpace> Spaces
        {
            get
            {
                return (List<ThPlotSpace>)gridControl_plot_area.DataSource;
            }

            set
            {
                gridControl_plot_area.DataSource = value;
            }
        }

        public void Reload()
        {
            DbRepository = new ThPlotSpaceDbRepository();
            DbRepository.AppendDefaultPlotSpace();
            gridControl_plot_area.DataSource = DbRepository.Spaces;
            gridControl_plot_area.RefreshDataSource();
        }

        public void InitializeGridControl()
        {
            Presenter = new ThPlotSpacePresenter(this);
            DbRepository = new ThPlotSpaceDbRepository();
            DbRepository.AppendDefaultPlotSpace();
            gridControl_plot_area.DataSource = DbRepository.Spaces;
            gridControl_plot_area.RefreshDataSource();
        }

        private void gridView_plot_area_CustomUnboundColumnData(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDataEventArgs e)
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
                ThPlotSpace space = e.Row as ThPlotSpace;
                e.Value = space.IsDefined ? "" : "选择";
            }
        }

        private void gridView_plot_area_RowClick(object sender, RowClickEventArgs e)
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
            ThPlotSpace space = gridView.GetRow(e.RowHandle) as ThPlotSpace;
            if (!space.IsDefined)
            {
                // 面积框线图层名
                string name = ThResidentialRoomUtil.LayerName(space);

                // 选取面积框线
                if (Presenter.OnPickAreaFrames(name))
                {
                    // 更新界面
                    this.Reload();
                }
            }
        }

        private void gridView_plot_area_RowUpdated(object sender, DevExpress.XtraGrid.Views.Base.RowObjectEventArgs e)
        {
            if (!(sender is GridView view))
            {
                return;
            }

            ThPlotSpace roofGreenSpace = (ThPlotSpace)e.Row;
            if (roofGreenSpace.IsDefined)
            {
                // 面积框线图层名
                string name = ThResidentialRoomUtil.LayerName(roofGreenSpace);

                // 更新面积框线图层名
                Presenter.OnRenameAreaFrameLayer(name, roofGreenSpace.Frame);

                // 更新界面
                this.Reload();
            }
        }

        private void gridView_plot_area_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName == "Area" && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                double area = Convert.ToDouble(e.Value);
                e.DisplayText = Converter.DistanceToString(area, DistanceUnitFormat.Decimal, 2);
            }
        }

        private void gridView_plot_area_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            GridView view = sender as GridView;
            switch (view.FocusedColumn.FieldName)
            {
                case "HouseHold":
                    {
                        if (!UInt16.TryParse(e.Value.ToString(), out UInt16 value))
                        {
                            e.Valid = false;
                            e.ErrorText = "请输入整数";
                        }

                    }
                    break;
            };
        }

        private void gridView_plot_area_DoubleClick(object sender, EventArgs e)
        {
            DXMouseEventArgs ea = e as DXMouseEventArgs;
            GridView view = sender as GridView;
            GridHitInfo info = view.CalcHitInfo(ea.Location);
            if (info.InRow || info.InRowCell)
            {
                ThPlotSpace space = view.GetRow(info.RowHandle) as ThPlotSpace;
                if (space.IsDefined)
                {
                    Presenter.OnHighlightAreaFrame(space.Frame);
                }
            }
        }

        private void gridView_plot_area_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
        {
            GridView view = sender as GridView;
            if (e.MenuType == GridMenuType.Row)
            {
                if (e.HitInfo.InRow || e.HitInfo.InRowCell)
                {
                    e.Menu.Items.Clear();
                    e.Menu.Items.Add(CreateDeleteMenuItem(view, e.HitInfo.RowHandle));
                    e.Menu.Items.Add(CreateDeleteAllMenuItem(view, e.HitInfo.RowHandle));
                }
            }
        }

        DXMenuItem CreateDeleteMenuItem(GridView view, int rowHandle)
        {
            return new DXMenuItem("删除", new EventHandler(OnDeletePlotSpaceItemClick))
            {
                Tag = new RowInfo(view, rowHandle)
            };
        }

        DXMenuItem CreateDeleteAllMenuItem(GridView view, int rowHandle)
        {
            return new DXMenuItem("全部删除", new EventHandler(OnDeleteAllPlotSpaceItemsClick))
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

        void OnDeletePlotSpaceItemClick(object sender, EventArgs e)
        {
            DXMenuItem menuItem = sender as DXMenuItem;
            if (menuItem.Tag is RowInfo ri)
            {
                ThPlotSpace space = ri.View.GetRow(ri.RowHandle) as ThPlotSpace;
                if (space.IsDefined)
                {
                    // 更新图纸
                    string layer = ThResidentialRoomDbUtil.LayerName(space.Frame);
                    Presenter.OnDeleteAreaFrame(space.Frame);
                    Presenter.OnDeleteAreaFrameLayer(layer);

                    // 更新界面
                    this.Reload();
                }
            }
        }

        void OnDeleteAllPlotSpaceItemsClick(object sender, EventArgs e)
        {
            DXMenuItem menuItem = sender as DXMenuItem;
            if (menuItem.Tag is RowInfo ri)
            {
                // 更新图纸
                foreach (var space in DbRepository.Spaces)
                {
                    if (!space.IsDefined)
                        continue;

                    string layer = ThResidentialRoomDbUtil.LayerName(space.Frame);
                    Presenter.OnDeleteAreaFrame(space.Frame);
                    Presenter.OnDeleteAreaFrameLayer(layer);
                }

                // 更新界面
                this.Reload();
            }
        }

        private void gridView_plot_area_CustomRowCellEdit(object sender, CustomRowCellEditEventArgs e)
        {
            if (e.Column.FieldName != "gridColumn_pick")
            {
                return;
            }

            GridView view = sender as GridView;
            ThPlotSpace space = view.GetRow(e.RowHandle) as ThPlotSpace;
            if (!space.IsDefined)
            {
                e.RepositoryItem = repositoryItemHyperLinkEdit1;
            }
            else
            {
                e.RepositoryItem = null;
            }
        }
    }
}
