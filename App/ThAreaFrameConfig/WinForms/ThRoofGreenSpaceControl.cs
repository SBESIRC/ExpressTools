using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using ThAreaFrameConfig.View;
using ThAreaFrameConfig.Model;
using ThAreaFrameConfig.Command;
using ThAreaFrameConfig.Presenter;
using DevExpress.Utils;
using DevExpress.Utils.Menu;
using DevExpress.XtraTab;
using DevExpress.XtraTab.ViewInfo;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
using AcHelper;

namespace ThAreaFrameConfig.WinForms
{
    public partial class ThRoofGreenSpaceControl : DevExpress.XtraEditors.XtraUserControl, 
        IRoofGreenSpaceView, 
        IAreaFrameDatabaseReactor,
        IAreaFrameDocumentCollectionReactor
    {
        private ThRoofGreenSpacePresenter Presenter;
        private ThRoofGreenSpaceDbRepository DbRepository;


        public ThRoofGreenSpaceControl()
        {
            InitializeComponent();
            InitializeGridControl();
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
            DbRepository.AppendRoofGreenSpace();
            gridControl_space.DataSource = DbRepository.Spaces;
            gridControl_space.RefreshDataSource();
        }

        public void InitializeGridControl()
        {
            Presenter = new ThRoofGreenSpacePresenter(this);
            DbRepository = new ThRoofGreenSpaceDbRepository();
            DbRepository.AppendRoofGreenSpace();
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
                ThRoofGreenSpace space = e.Row as ThRoofGreenSpace;
                e.Value = space.IsDefined ? "" : "选择";
            }
        }

        private void gridView1_RowClick(object sender, RowClickEventArgs e)
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
            ThRoofGreenSpace space = (ThRoofGreenSpace)gridView.GetRow(e.RowHandle);
            if (!space.IsDefined)
            {
                // 面积框线图层名
                string name = ThResidentialRoomUtil.LayerName(space);

                // 选取面积框线
                ThCreateAreaFrameCmdHandler.LayerName = name;
                ThCreateAreaFrameCmdHandler.Handler = new ThCreateAreaFrameCommand()
                {
                    LayerCreator = ThResidentialRoomDbUtil.ConfigLayer
                };
                ThCreateAreaFrameCmdHandler.ExecuteFromCommandLine("*THCREATEAREAFRAME");
            }
        }

        private void gridView_space_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            if (!(sender is GridView view))
            {
                return;
            }

            var columns = new List<string>
            {
                "Coefficient"
            };
            if (!columns.Contains(e.Column.FieldName))
            {
                return;
            }

            ThRoofGreenSpace roofGreenSpace = view.GetRow(e.RowHandle) as ThRoofGreenSpace;
            // 面积框线图层名
            string newName = ThResidentialRoomUtil.LayerName(roofGreenSpace);
            if (roofGreenSpace.IsDefined)
            { 
                // 更新面积框线图层名
                Presenter.OnMoveAreaFrameToLayer(newName, roofGreenSpace.Frame);

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

        private void gridView1_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
        {
            if (e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                switch(e.Column.FieldName)
                {
                    case "Area":
                        {
                            double area = Convert.ToDouble(e.Value);
                            e.DisplayText = Converter.DistanceToString(area, DistanceUnitFormat.Decimal, 2);
                        }
                        break;
                    case "Coefficient":
                        {
                            e.DisplayText = String.Format("{0:0.0}", Convert.ToDouble(e.Value));
                        }
                        break;
                }
            }
        }

        private void gridView_space_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            GridView view = sender as GridView;
            switch (view.FocusedColumn.FieldName)
            {
                case "Coefficient":
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

        private void gridView_space_DoubleClick(object sender, EventArgs e)
        {
            DXMouseEventArgs ea = e as DXMouseEventArgs;
            GridView view = sender as GridView;
            GridHitInfo info = view.CalcHitInfo(ea.Location);
            if (info.InRow || info.InRowCell)
            {
                ThRoofGreenSpace space = view.GetRow(info.RowHandle) as ThRoofGreenSpace;
                if (space.IsDefined)
                {
                    foreach(var item in DbRepository.Spaces)
                    {
                        if (!item.IsDefined)
                        {
                            continue;
                        }

                        if (item.ID == space.ID)
                        {
                            Presenter.OnHighlightAreaFrame(item.Frame);
                        }
                        else
                        {
                            Presenter.OnUnhighlightAreaFrame(item.Frame);
                        }
                    }
                }
            }
        }

        private void gridView_space_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
        {
            GridView view = sender as GridView;
            if (e.MenuType == GridMenuType.Row)
            {
                if (e.HitInfo.InRow || e.HitInfo.InRowCell)
                {
                    foreach (var handle in view.GetSelectedRows())
                    {
                        var space = view.GetRow(handle) as ThRoofGreenSpace;
                        if (!space.IsDefined)
                        {
                            return;
                        }
                    }

                    e.Menu.Items.Clear();
                    e.Menu.Items.Add(CreateDeleteMenuItem(view, e.HitInfo.RowHandle));
                }
            }
        }

        DXMenuItem CreateDeleteMenuItem(GridView view, int rowHandle)
        {
            return new DXMenuItem("删除", new EventHandler(OnDeleteRoofGreenSpaceItemClick))
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

        void OnDeleteRoofGreenSpaceItemClick(object sender, EventArgs e)
        {
            DXMenuItem menuItem = sender as DXMenuItem;
            if (menuItem.Tag is RowInfo ri)
            {
                // 更新图纸
                // 支持多选
                foreach (var handle in ri.View.GetSelectedRows())
                {
                    var space = ri.View.GetRow(handle) as ThRoofGreenSpace;
                    if (!space.IsDefined)
                    {
                        continue;
                    }

                    string layer = ThResidentialRoomDbUtil.LayerName(space.Frame);
                    Presenter.OnDeleteAreaFrame(space.Frame);
                    Presenter.OnDeleteAreaFrameLayer(layer);
                }

                // 更新界面
                this.Reload();
            }
        }

        private void gridView_space_CustomRowCellEdit(object sender, CustomRowCellEditEventArgs e)
        {
            if (e.Column.FieldName != "gridColumn_pick")
            {
                return;
            }

            GridView view = sender as GridView;
            ThRoofGreenSpace space = view.GetRow(e.RowHandle) as ThRoofGreenSpace;
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
