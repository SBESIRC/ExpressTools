using System;
using System.Collections.Generic;
using ThAreaFrameConfig.Model;
using ThAreaFrameConfig.View;
using ThAreaFrameConfig.Command;
using ThAreaFrameConfig.Presenter;
using DevExpress.Utils;
using DevExpress.Utils.Menu;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
using AcHelper;

namespace ThAreaFrameConfig.WinForms
{
    public partial class ThOutdoorParkingSpaceControl : DevExpress.XtraEditors.XtraUserControl, 
        IOutdoorParkingSpaceView, 
        IAreaFrameDatabaseReactor,
        IAreaFrameDocumentCollectionReactor
    {
        private ThOutdoorParkingSpacePresenter Presenter;
        private ThOutdoorParkingSpaceDbRepository DbRepository;

        public ThOutdoorParkingSpaceControl()
        {
            InitializeComponent();
            InitializeGridControl();
        }

        public List<ThOutdoorParkingSpace> Spaces
        {
            get
            {
                return (List<ThOutdoorParkingSpace>)gridControl_outdoor_parking_space.DataSource;
            }

            set
            {
                gridControl_outdoor_parking_space.DataSource = value;
            }
        }

        public void Reload()
        {
            DbRepository = new ThOutdoorParkingSpaceDbRepository();
            DbRepository.AppendDefaultOutdoorParkingSpace();
            gridControl_outdoor_parking_space.DataSource = DbRepository.Spaces;
            gridControl_outdoor_parking_space.RefreshDataSource();
            gridView_outdoor_parking_space.BestFitColumns();
        }

        public void InitializeGridControl()
        {
            Presenter = new ThOutdoorParkingSpacePresenter(this);
            DbRepository = new ThOutdoorParkingSpaceDbRepository();
            DbRepository.AppendDefaultOutdoorParkingSpace();
            gridControl_outdoor_parking_space.DataSource = DbRepository.Spaces;
            gridControl_outdoor_parking_space.RefreshDataSource();
            gridView_outdoor_parking_space.BestFitColumns();
        }

        private void gridView_outdoor_parking_space_CustomUnboundColumnData(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDataEventArgs e)
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
                ThOutdoorParkingSpace space = e.Row as ThOutdoorParkingSpace;
                e.Value = space.IsDefined ? "" : "选择";
            }
        }

        private void gridView_outdoor_parking_space_RowClick(object sender, DevExpress.XtraGrid.Views.Grid.RowClickEventArgs e)
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

            GridView gridView = (GridView)sender;
            ThOutdoorParkingSpace space = gridView.GetRow(e.RowHandle) as ThOutdoorParkingSpace;
            if (!space.IsDefined)
            {
                // 面积框线图层名
                string name = ThResidentialRoomUtil.LayerName(space);

                // 选取面积框线
                ThCreateAreaFrameCmdHandler.LayerName = name;
                ThCreateAreaFrameCmdHandler.Handler = new ThCreateAreaFrameCommand()
                {
                    LayerCreator = ThResidentialRoomDbUtil.ConfigOutdoorParkingSpaceLayer
                };
                ThCreateAreaFrameCmdHandler.ExecuteFromCommandLine("*THCREATEAREAFRAME");
            }
        }

        private void gridView_outdoor_parking_space_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (!(sender is GridView view))
            {
                return;
            }

            var columns = new List<string>
            {
                "Storey",
            };
            if (!columns.Contains(e.Column.FieldName))
            {
                return;
            }

            ThOutdoorParkingSpace space = view.GetRow(e.RowHandle) as ThOutdoorParkingSpace;
            // 面积框线图层名
            string newName = ThResidentialRoomUtil.LayerName(space);
            if (space.IsDefined)
            {
                // 选取面积框线
                foreach(var frame in space.Frames)
                {
                    Presenter.OnMoveAreaFrameToLayer(newName, frame);
                }

                // 更新界面
                this.Reload();
            }
            else
            {
                if (ThCreateAreaFrameCmdHandler.Handler != null)
                {
                    ThCreateAreaFrameCmdHandler.LayerName = newName;
                }
            }
        }

        private void gridView_outdoor_parking_space_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName == "Area" && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                double area = Convert.ToDouble(e.Value);
                e.DisplayText = Converter.DistanceToString(area, DistanceUnitFormat.Decimal, 2);
            }
        }

        private void gridView_outdoor_parking_space_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            GridView view = sender as GridView;
            switch (view.FocusedColumn.FieldName)
            {
                case "Storey":
                    {
                        if (!UInt16.TryParse((string)e.Value, out UInt16 value))
                        {
                            e.Valid = false;
                            e.ErrorText = "请输入整数";
                        }
                    }
                    break;
            };
        }

        private void gridView_outdoor_parking_space_DoubleClick(object sender, EventArgs e)
        {
            DXMouseEventArgs ea = e as DXMouseEventArgs;
            GridView view = sender as GridView;
            GridHitInfo info = view.CalcHitInfo(ea.Location);
            if (info.InRow || info.InRowCell)
            {
                ThOutdoorParkingSpace space = view.GetRow(info.RowHandle) as ThOutdoorParkingSpace;
                if (space.IsDefined)
                {
                    foreach (var item in DbRepository.Spaces)
                    {
                        if (!item.IsDefined)
                        {
                            continue;
                        }

                        if (item.ID == space.ID)
                        {
                            Presenter.OnHighlightAreaFrames(item.Frames.ToArray());
                        }
                        else
                        {
                            Presenter.OnUnhighlightAreaFrames(item.Frames.ToArray());
                        }
                    }
                }
            }
        }

        private void gridView_outdoor_parking_space_PopupMenuShowing(object sender, DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventArgs e)
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
            return new DXMenuItem("删除", new EventHandler(OnDeleteOutdoorParkingSpaceItemClick))
            {
                Tag = new RowInfo(view, rowHandle)
            };
        }

        DXMenuItem CreateDeleteAllMenuItem(GridView view, int rowHandle)
        {
            return new DXMenuItem("全部删除", new EventHandler(OnDeleteAllOutdoorParkingSpaceItemsClick))
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

        void OnDeleteOutdoorParkingSpaceItemClick(object sender, EventArgs e)
        {
            DXMenuItem menuItem = sender as DXMenuItem;
            if (menuItem.Tag is RowInfo ri)
            {
                ThOutdoorParkingSpace space = ri.View.GetRow(ri.RowHandle) as ThOutdoorParkingSpace;
                if (space.IsDefined)
                {
                    // 更新图纸
                    string name = ThResidentialRoomUtil.LayerName(space);
                    Presenter.OnDeleteAreaFrames(space.Frames.ToArray());
                    Presenter.OnDeleteAreaFrameLayer(name);

                    // 更新界面
                    this.Reload();
                }
            }
        }

        void OnDeleteAllOutdoorParkingSpaceItemsClick(object sender, EventArgs e)
        {
            DXMenuItem menuItem = sender as DXMenuItem;
            if (menuItem.Tag is RowInfo ri)
            {
                // 更新图纸
                foreach (var space in DbRepository.Spaces)
                {
                    if (!space.IsDefined)
                        continue;

                    string name = ThResidentialRoomUtil.LayerName(space);
                    Presenter.OnDeleteAreaFrames(space.Frames.ToArray());
                    Presenter.OnDeleteAreaFrameLayer(name);
                }

                // 更新界面
                this.Reload();
            }
        }

        private void gridView_outdoor_parking_space_CustomRowCellEdit(object sender, CustomRowCellEditEventArgs e)
        {
            if (e.Column.FieldName != "gridColumn_pick")
            {
                return;
            }

            GridView view = sender as GridView;
            ThOutdoorParkingSpace space = view.GetRow(e.RowHandle) as ThOutdoorParkingSpace;
            if (!space.IsDefined)
            {
                e.RepositoryItem = repositoryItemHyperLinkEdit1;
            }
            else
            {
                e.RepositoryItem = null;
            }
        }

        #region IAreaFrameDocumentCollectionReactor

        public void RegisterDocumentLockModeChangedEvent()
        {
            AcadApp.DocumentManager.DocumentLockModeChanged += DocumentLockModeChangedHandler;
        }

        public void UnRegisterDocumentLockModeChangedEvent()
        {
            AcadApp.DocumentManager.DocumentLockModeChanged -= DocumentLockModeChangedHandler;
        }

        private void DocumentLockModeChangedHandler(object sender, DocumentLockModeChangedEventArgs e)
        {
            if (e.GlobalCommandName == "#*THCREATEAREAFRAME")
            {
                if (ThCreateAreaFrameCmdHandler.Handler == null)
                {
                    return;
                }
                if (ThCreateAreaFrameCmdHandler.Handler.Success)
                {
                    AcadApp.Idle += Application_OnIdle;
                    ThCreateAreaFrameCmdHandler.Handler = null;
                }
            }
        }

        #endregion

        #region IAreaFrameDatabaseReactor

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

        #endregion
    }
}
