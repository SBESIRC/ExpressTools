using System;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using ThAreaFrameConfig.View;
using ThAreaFrameConfig.Model;
using ThAreaFrameConfig.Presenter;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.Utils.Menu;
using DevExpress.XtraTab;
using DevExpress.XtraTab.ViewInfo;
using DevExpress.Utils;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace ThAreaFrameConfig.WinForms
{
    public partial class ThResidentialRoomControl : DevExpress.XtraEditors.XtraUserControl, IResidentialRoomView, IAreaFrameDatabaseReactor
    {
        private ThResidentialRoomPresenter Presenter;
        private ThResidentialRoomDbRepository DbRepository;

        public ThResidentialRoomControl()
        {
            InitializeComponent();
            InitializeGridControl();
            InitializeTabControl();
        }

        public List<ThResidentialRoom> Rooms
        {
            get
            {
                return (List<ThResidentialRoom>)gridControl_room.DataSource;
            }

            set
            {
                gridControl_room.DataSource = value;
            }
        }

        public void Reload()
        {
            DbRepository = new ThResidentialRoomDbRepository();
            if (!DbRepository.Storeys().Any())
            {
                DbRepository.AppendStorey(xtraTabControl1.SelectedTabPage.Text);
            }
            gridControl_room.DataSource = DbRepository.Rooms(this.CurrentStorey);
            gridControl_room.RefreshDataSource();
            RefreshGridControl();
        }

        public void InitializeTabControl()
        {
            Presenter = new ThResidentialRoomPresenter(this);
            DbRepository = new ThResidentialRoomDbRepository();
            if (!DbRepository.Storeys().Any())
            {
                DbRepository.AppendStorey(xtraTabControl1.SelectedTabPage.Text);
            }
            foreach (var storey in DbRepository.Storeys())
            {
                XtraTabPage page = this.xtraTabControl1.TabPages.Add(storey.Identifier);
            }

            this.xtraTabControl1.TabPages.RemoveAt(0);
        }

        private void InitializeGridControl()
        {
            gdv_room.OptionsDetail.DetailMode = DetailMode.Embedded;
            gdv_room.OptionsCustomization.AllowColumnMoving = false;
        }

        private void RefreshGridControl()
        {
            // Prevent excessive visual updates. 
            gdv_room.BeginUpdate();

            // Recursively Expand all Master Rows
            for (int i = 0; i < Rooms.Count; i++)
            {
                RecursiveExpand(gdv_room, i);
            }

            // Enable visual updates. 
            gdv_room.EndUpdate();
        }

        // custom method for expanding nested details
        private void RecursiveExpand(GridView masterView, int masterRowHandle)
        {
            var relationCount = masterView.GetRelationCount(masterRowHandle);
            for (var index = relationCount - 1; index >= 0; index--)
            {
                masterView.ExpandMasterRow(masterRowHandle, index);
                var childView = masterView.GetDetailView(masterRowHandle, index) as GridView;
                if (childView != null)
                {
                    var childRowCount = childView.DataRowCount;
                    for (var handle = 0; handle < childRowCount; handle++)
                        RecursiveExpand(childView, handle);
                }
            }
        }

        private void gdv_room_MasterRowGetLevelDefaultView(object sender, DevExpress.XtraGrid.Views.Grid.MasterRowGetLevelDefaultViewEventArgs e)
        {
            e.DefaultView = gdv_room_area_unit;
        }

        private void gdv_room_area_unit_MasterRowGetLevelDefaultView(object sender, DevExpress.XtraGrid.Views.Grid.MasterRowGetLevelDefaultViewEventArgs e)
        {
            e.DefaultView = gdv_room_area_frame;
        }

        private ThResidentialStorey CurrentStorey
        {
            get
            {
                return DbRepository.Storeys().Where(o => o.Identifier == xtraTabControl1.SelectedTabPage.Text).FirstOrDefault();
            }
        }

        // XtraTabControl.SelectedPageChanged Event handler
        private void xtraTabControl1_SelectedPageChanged(object sender, TabPageChangedEventArgs e)
        {
            e.PrevPage.Controls.RemoveAt(0);
            e.Page.Controls.Add(gridControl_room);
            string storey = e.Page.Text;
            if (!DbRepository.Storeys().Where(o => o.Identifier == storey).Any())
            {
                DbRepository.AppendStorey(storey);
            }
            gridControl_room.DataSource = DbRepository.Rooms(storey);
            gridControl_room.RefreshDataSource();
            RefreshGridControl();
        }

        // ColumnView.CustomUnboundColumnData Event handler
        private void gdv_room_area_frame_CustomUnboundColumnData(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDataEventArgs e)
        {
            if (!(sender is GridView view))
            {
                return;
            }
            if (e.Column.FieldName != "room_area_frame_pick")
            {
                return;
            }
            if (e.IsGetData)
            {
                ThResidentialAreaFrame areaFrame = e.Row as ThResidentialAreaFrame;
                e.Value = areaFrame.IsDefined ? "" : "选择";
            }
        }

        private void gdv_room_area_frame_RowClick(object sender, RowClickEventArgs e)
        {
            if (!(sender is GridView view))
            {
                return;
            }
            if (e.HitInfo.Column == null)
            {
                return;
            }
            if (e.HitInfo.Column.FieldName != "room_area_frame_pick")
            {
                return;
            }

            GridView gridView = (GridView)sender;
            ThResidentialAreaFrame areaFrame = (ThResidentialAreaFrame)gridView.GetRow(e.RowHandle);
            if (!areaFrame.IsDefined)
            {
                // 面积框线图层名
                ThResidentialRoom room = DbRepository.Rooms(this.CurrentStorey).Where(o => o.ID == areaFrame.RoomID).First();
                ThResidentialStorey storey = DbRepository.Storeys().Where(o => o.ID == room.StoreyID).First();
                ThResidentialRoomComponent component = room.Components.Find(o => o.ID == areaFrame.ComponentID);
                string name = ThResidentialRoomUtil.LayerName(storey, room, component, areaFrame);
                // “阳台”，“飘窗”和“其他构件”都必须依附于“套内”，在创建他们之前必须保证“套内”已经存在
                switch (component.Name)
                {
                    case "阳台":
                    case "飘窗":
                    case "其他构件":
                        {
                            if (room.DwellingArea == 0)
                            {
                                AcadApp.ShowAlertDialog("请先创建套内！");
                                return;
                            }
                        };
                        break;
                };

                // 选取面积框线
                if (Presenter.OnPickAreaFrames(name))
                {
                    // 更新界面
                    this.Reload();
                }
            }
        }

        private void gdv_room_area_frame_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            if (!(sender is GridView view))
            {
                return;
            }

            var columns = new List<string>
            {
                "Coefficient",
                "FARCoefficient"
            };
            if (!columns.Contains(e.Column.FieldName))
            {
                return;
            }

            ThResidentialAreaFrame areaFrame = (ThResidentialAreaFrame)view.GetRow(e.RowHandle);
            if (areaFrame.IsDefined)
            {
                // 面积框线图层名
                ThResidentialRoom room = DbRepository.Rooms(this.CurrentStorey).Where(o => o.ID == areaFrame.RoomID).First();
                ThResidentialStorey storey = DbRepository.Storeys().Where(o => o.ID == room.StoreyID).First();
                ThResidentialRoomComponent component = room.Components.Find(o => o.ID == areaFrame.ComponentID);
                string name = ThResidentialRoomUtil.LayerName(storey, room, component, areaFrame);

                // 更新面积框线图层名
                Presenter.OnMoveAreaFrameToLayer(name, areaFrame.Frame);

                // 更新界面
                this.Reload();
            }
        }

        private void gdv_room_area_frame_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                switch (e.Column.FieldName)
                {
                    case "Area":
                        {
                            double area = Convert.ToDouble(e.Value);
                            e.DisplayText = Converter.DistanceToString(area, DistanceUnitFormat.Decimal, 2);
                        }
                        break;
                    case "Coefficient":
                    case "FARCoefficient":
                        {
                            e.DisplayText = String.Format("{0:0.0}", Convert.ToDouble(e.Value));
                        }
                        break;
                }
            }
        }

        private void gdv_room_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
        {
            if (e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                switch (e.Column.FieldName)
                {
                    case "DwellingArea":
                    case "BalconyArea":
                    case "BaywindowArea":
                    case "MiscellaneousArea":
                    case "AggregationArea":
                        {
                            double area = Convert.ToDouble(e.Value);
                            e.DisplayText = Converter.DistanceToString(area, DistanceUnitFormat.Decimal, 2);
                        };
                        break;
                    default:
                        break;
                };
            }
        }

        private void gdv_room_area_frame_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            GridView view = sender as GridView;
            switch (view.FocusedColumn.FieldName)
            {
                case "Coefficient":
                case "FARCoefficient":
                    {
                        if (!double.TryParse(e.Value.ToString(), out double value))
                        {
                            e.Valid = false;
                            e.ErrorText = "请输入浮点数";
                        }
                    }
                    break;
            };
        }

        private void gdv_room_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
        {
            GridView view = sender as GridView;
            if (e.MenuType == GridMenuType.Row)
            {
                if (e.HitInfo.InRow || e.HitInfo.InRowCell)
                {
                    ThResidentialRoom room = view.GetRow(e.HitInfo.RowHandle) as ThResidentialRoom;

                    e.Menu.Items.Clear();
                    e.Menu.Items.Add(CreateAppendRoomItemMenuItem(view, e.HitInfo.RowHandle));
                    e.Menu.Items.Add(CreateRemoveRoomItemMenuItem(view, e.HitInfo.RowHandle));
                }
            }
        }

        DXMenuItem CreateAppendRoomItemMenuItem(GridView view, int rowHandle)
        {
            return new DXMenuItem("添加户型", new EventHandler(OnAppendRoomItemClick))
            {
                Tag = new RowInfo(view, rowHandle)
            };
        }

        DXMenuItem CreateRemoveRoomItemMenuItem(GridView view, int rowHandle)
        {
            return new DXMenuItem("删除户型", new EventHandler(OnRemoveRoomItemClick))
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

        void OnAppendRoomItemClick(object sender, EventArgs e)
        {
            DXMenuItem menuItem = sender as DXMenuItem;
            if (menuItem.Tag is RowInfo ri)
            {
                this.CurrentStorey.Rooms.Add(ThResidentialRoomUtil.DefaultResidentialRoom(this.CurrentStorey.ID));
                gridControl_room.RefreshDataSource();
                RefreshGridControl();
            }
        }

        void OnRemoveRoomItemClick(object sender, EventArgs e)
        {
            DXMenuItem menuItem = sender as DXMenuItem;
            if (menuItem.Tag is RowInfo ri)
            {
                // 更新图纸
                ThResidentialRoom room = ri.View.GetRow(ri.RowHandle) as ThResidentialRoom;
                foreach (var component in room.Components)
                {
                    foreach (var frame in component.AreaFrames)
                    {
                        if (!frame.IsDefined)
                        {
                            continue;
                        }

                        string name = ThResidentialRoomUtil.LayerName(CurrentStorey, room, component, frame);
                        Presenter.OnDeleteAreaFrame(frame.Frame);
                        Presenter.OnDeleteAreaFrameLayer(name);
                    }
                }

                // 更新界面
                this.Reload();
            }
        }

        private void barButtonItem_add_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            using (var dlg = new ThResidentialStoreyDialog(""))
            {
                dlg.Text = "增加层";
                if (AcadApp.ShowModalDialog(dlg) != DialogResult.OK)
                {
                    return;
                }

                if (DbRepository.Storey(dlg.Storey) != null)
                {
                    MessageBox.Show("楼层已存在！请指定新的楼层");
                    return;
                }

                // 更新数据源
                DbRepository.AppendStorey(dlg.Storey);

                // 更新界面
                XtraTabPage page = this.xtraTabControl1.TabPages.Add(dlg.Storey);
                this.xtraTabControl1.SelectedTabPage = page;
            }
        }

        private void barButtonItem_delete_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            // 更新图纸
            foreach (var room in CurrentStorey.Rooms)
            {
                foreach (var component in room.Components)
                {
                    foreach (var frame in component.AreaFrames)
                    {
                        if (!frame.IsDefined)
                        {
                            continue;
                        }

                        string name = ThResidentialRoomUtil.LayerName(CurrentStorey, room, component, frame);
                        Presenter.OnDeleteAreaFrame(frame.Frame);
                        Presenter.OnDeleteAreaFrameLayer(name);
                    }
                }
            }

            // 更新数据源
            DbRepository.RemoveStorey(CurrentStorey.Identifier);

            // 更新界面
            this.xtraTabControl1.TabPages.Remove(this.xtraTabControl1.SelectedTabPage, true);
        }

        private void barButtonItem_modify_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            string storey = CurrentStorey.Identifier;
            using (var dlg = new ThResidentialStoreyDialog(storey))
            {
                dlg.Text = "修改层";
                if (AcadApp.ShowModalDialog(dlg) != DialogResult.OK)
                {
                    return;
                }

                // 更新图纸
                foreach (var room in CurrentStorey.Rooms)
                {
                    foreach (var component in room.Components)
                    {
                        foreach (var frame in component.AreaFrames)
                        {
                            if (!frame.IsDefined)
                            {
                                continue;
                            }

                            // 将面积框线置于新的图层上
                            string newName = ThResidentialRoomUtil.LayerName(dlg.Storey, room, component, frame);
                            Presenter.OnMoveAreaFrameToLayer(newName, frame.Frame);

                            // 删除旧的图层
                            string name = ThResidentialRoomUtil.LayerName(storey, room, component, frame);
                            Presenter.OnDeleteAreaFrameLayer(name);
                        }
                    }
                }

                // 更新数据源
                DbRepository.AppendStorey(dlg.Storey);
                DbRepository.RemoveStorey(storey);

                // 更新界面
                XtraTabPage page = this.xtraTabControl1.TabPages.Add(dlg.Storey);
                this.xtraTabControl1.TabPages.Remove(this.xtraTabControl1.SelectedTabPage, true);
                this.xtraTabControl1.SelectedTabPage = page;
            }
        }

        private void xtraTabControl1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                XtraTabControl tabCtrl = sender as XtraTabControl;
                Point pt = MousePosition;
                XtraTabHitInfo info = tabCtrl.CalcHitInfo(tabCtrl.PointToClient(pt));
                if (info.HitTest == XtraTabHitTest.PageHeader)
                {
                    popupMenu_storey.ShowPopup(pt);
                }
            }
        }

        private void gdv_room_area_frame_DoubleClick(object sender, EventArgs e)
        {
            DXMouseEventArgs ea = e as DXMouseEventArgs;
            GridView view = sender as GridView;
            GridHitInfo info = view.CalcHitInfo(ea.Location);
            if (info.InRow || info.InRowCell)
            {
                ThResidentialAreaFrame areaFrame = view.GetRow(info.RowHandle) as ThResidentialAreaFrame;
                if (areaFrame.IsDefined)
                {
                    Presenter.OnHighlightAreaFrame(areaFrame.Frame);
                }
            }
        }

        private void gdv_room_area_frame_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
        {
            GridView view = sender as GridView;
            if (e.MenuType == GridMenuType.Row)
            {
                if (e.HitInfo.InRow || e.HitInfo.InRowCell)
                {
                    foreach (var handle in view.GetSelectedRows())
                    {
                        var frame = view.GetRow(handle) as ThResidentialAreaFrame;
                        if (!frame.IsDefined)
                        {
                            return;
                        }
                    }

                    e.Menu.Items.Clear();
                    e.Menu.Items.Add(CreateDeleteAreaFrameMenuItem(view, e.HitInfo.RowHandle));
                }
            }
        }

        DXMenuItem CreateDeleteAreaFrameMenuItem(GridView view, int rowHandle)
        {
            return new DXMenuItem("删除", new EventHandler(OnDeleteAreaFrameItemClick))
            {
                Tag = new RowInfo(view, rowHandle)
            };
        }

        void OnDeleteAreaFrameItemClick(object sender, EventArgs e)
        {
            DXMenuItem menuItem = sender as DXMenuItem;
            if (menuItem.Tag is RowInfo ri)
            {
                // 更新图纸
                // 支持多选
                foreach(var handle in ri.View.GetSelectedRows())
                {
                    var frame = ri.View.GetRow(handle) as ThResidentialAreaFrame;
                    if (!frame.IsDefined)
                    {
                        continue;
                    }

                    string name = ThResidentialRoomDbUtil.LayerName(frame.Frame);
                    Presenter.OnDeleteAreaFrame(frame.Frame);
                    Presenter.OnDeleteAreaFrameLayer(name);
                }

                // 更新界面
                this.Reload();
            }
        }

        private void gdv_room_area_frame_CustomRowCellEdit(object sender, CustomRowCellEditEventArgs e)
        {
            if (e.Column.FieldName != "room_area_frame_pick")
            {
                return;
            }

            GridView view = sender as GridView;
            ThResidentialAreaFrame areaFrame = view.GetRow(e.RowHandle) as ThResidentialAreaFrame;
            if (!areaFrame.IsDefined)
            {
                e.RepositoryItem = repositoryItemHyperLinkEdit1;
            }
            else
            {
                e.RepositoryItem = null;
            }
        }

        private void gdv_room_CustomUnboundColumnData(object sender, CustomColumnDataEventArgs e)
        {
            if (!(sender is GridView view))
            {
                return;
            }
            if (e.Column.FieldName != "room_area_adjustment")
            {
                return;
            }
            if (e.IsGetData)
            {
                e.Value = "规整";
            }
        }

        private void gdv_room_RowClick(object sender, RowClickEventArgs e)
        {
            if (!(sender is GridView view))
            {
                return;
            }
            if (e.HitInfo.Column == null)
            {
                return;
            }
            if (e.HitInfo.Column.FieldName != "room_area_adjustment")
            {
                return;
            }


            GridView gridView = sender as GridView;
            ThResidentialRoom room = gridView.GetRow(e.RowHandle) as ThResidentialRoom;
            ThResidentialStorey storey = DbRepository.Storeys().Where(o => o.ID == room.StoreyID).First();

            // 规整参数
            Dictionary<string, string> parameters = new Dictionary<string, string>
            {
                { "storey_identifier",  storey.Identifier   },
                { "room_name",          room.Name           },
                { "room_identifier",    room.Identifier     }
            };

            // 选取面积框线
            if (Presenter.OnAdjustAreaFrames(parameters))
            {
                // 更新界面
                this.Reload();
            }
        }

        public void RegisterAreaFrameModifiedEvent()
        {
            DbRepository.RegisterAreaFrameModifiedEvent(OnAreaFrameModified);
        }

        public void UnRegisterAreaFrameModifiedEvent()
        {
            DbRepository.UnRegisterAreaFrameModifiedEvent(OnAreaFrameModified);
        }

        public void RegisterAreaFrameErasedEvent()
        {
            DbRepository.RegisterAreaFrameErasedEvent(OnAreaFrameErased);
        }

        public void UnRegisterAreaFrameErasedEvent()
        {
            DbRepository.UnRegisterAreaFrameErasedEvent(OnAreaFrameErased);
        }

        private void OnAreaFrameModified(object sender, ObjectEventArgs e)
        {
            if (DbRepository.AreaFrame(e.DBObject) != null)
            {
                AcadApp.Idle += Application_OnIdle;
            }
        }

        private void OnAreaFrameErased(object sender, ObjectErasedEventArgs e)
        {
            if (DbRepository.AreaFrame(e.DBObject) != null)
            {
                AcadApp.Idle += Application_OnIdle;
            }
        }

        private void Application_OnIdle(object sender, EventArgs e)
        {
            AcadApp.Idle -= Application_OnIdle;

            // 更新界面
            this.Reload();
        }
    }
}
