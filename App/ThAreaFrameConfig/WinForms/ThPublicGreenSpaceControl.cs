using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ThAreaFrameConfig.View;
using ThAreaFrameConfig.Presenter;
using ThAreaFrameConfig.Model;
using DevExpress.XtraGrid.Views.Grid;
using Autodesk.AutoCAD.Runtime;
using DevExpress.Utils;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.Utils.Menu;

namespace ThAreaFrameConfig.WinForms
{
    public partial class ThPublicGreenSpaceControl : DevExpress.XtraEditors.XtraUserControl, IPublicGreenSpaceView
    {
        private ThPublicGreenSpacePresenter Presenter;
        private ThPublicGreenSpaceNullRepository DbRepository;

        public ThPublicGreenSpaceControl()
        {
            InitializeComponent();
            InitializeGridControl();
        }

        public List<ThPublicGreenSpace> Spaces
        {
            get
            {
                return (List<ThPublicGreenSpace>)gridControl_public_green_space.DataSource;
            }

            set
            {
                gridControl_public_green_space.DataSource = value;
            }
        }

        public void Reload()
        {
            DbRepository = new ThPublicGreenSpaceNullRepository();
            DbRepository.AppendDefaultPublicGreenSpace();
            gridControl_public_green_space.DataSource = DbRepository.Spaces;
            gridControl_public_green_space.RefreshDataSource();
        }

        public void Attach(IPublicGreenSpacePresenterCallback presenter)
        {
            //
        }

        public void InitializeGridControl()
        {
            Presenter = new ThPublicGreenSpacePresenter(this);
            DbRepository = new ThPublicGreenSpaceNullRepository();
            DbRepository.AppendDefaultPublicGreenSpace();
            gridControl_public_green_space.DataSource = DbRepository.Spaces;
            gridControl_public_green_space.RefreshDataSource();
        }

        private void gridView_public_green_space_CustomUnboundColumnData(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDataEventArgs e)
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

        private void gridView_public_green_space_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName == "Area" && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                double area = Convert.ToDouble(e.Value);
                e.DisplayText = Converter.DistanceToString(area, DistanceUnitFormat.Decimal, 2);
            }
        }

        private void gridView_public_green_space_RowClick(object sender, RowClickEventArgs e)
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
                GridView gridView = sender as GridView;
                ThPublicGreenSpace space = gridView.GetRow(e.RowHandle) as ThPublicGreenSpace;
                Presenter.OnPickAreaFrames(ThResidentialRoomUtil.LayerName(space));

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

        private void gridView_public_green_space_DoubleClick(object sender, EventArgs e)
        {
            DXMouseEventArgs ea = e as DXMouseEventArgs;
            GridView view = sender as GridView;
            GridHitInfo info = view.CalcHitInfo(ea.Location);
            if (info.InRow || info.InRowCell)
            {
                ThPublicGreenSpace space = view.GetRow(info.RowHandle) as ThPublicGreenSpace;
                Presenter.OnHighlightAreaFrame(space.Frame);
            }
        }

        private void gridView_public_green_space_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
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
            return new DXMenuItem("删除", new EventHandler(OnDeleteRoofItemClick))
            {
                Tag = new RowInfo(view, rowHandle)
            };
        }

        DXMenuItem CreateDeleteAllMenuItem(GridView view, int rowHandle)
        {
            return new DXMenuItem("全部删除", new EventHandler(OnDeleteRoofItemClick))
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
                ThPublicGreenSpace space = ri.View.GetRow(ri.RowHandle) as ThPublicGreenSpace;
                string layer = ThResidentialRoomDbUtil.LayerName(space.Frame);
                Presenter.OnDeleteAreaFrame(space.Frame);
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
    }
}
