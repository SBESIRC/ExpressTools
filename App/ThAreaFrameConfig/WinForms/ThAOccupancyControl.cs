﻿using System;
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
    public partial class ThAOccupancyControl : DevExpress.XtraEditors.XtraUserControl, 
        IAOccupancyView, 
        IAreaFrameDatabaseReactor,
        IAreaFrameDocumentCollectionReactor
    {
        private ThAOccupanyPresenter Presenter;
        private ThAOccupancyDbRepository DbRepository;

        public ThAOccupancyControl()
        {
            InitializeComponent();
            InitializeTabControl();
        }

        public List<ThAOccupancy> AOccupancies
        {
            get
            {
                return (List<ThAOccupancy>)gridControl_aoccupancy.DataSource;
            }

            set
            {
                gridControl_aoccupancy.DataSource = value;
            }
        }

        public void Reload()
        {
            DbRepository = new ThAOccupancyDbRepository();
            if (DbRepository.Storeys.Count == 0)
            {
                DbRepository.AppendStorey(xtraTabControl1.SelectedTabPage.Text);
            }
            gridControl_aoccupancy.DataSource = DbRepository.AOccupancies(this.CurrentStorey);
            gridControl_aoccupancy.RefreshDataSource();
        }

        public void InitializeTabControl()
        {
            Presenter = new ThAOccupanyPresenter(this);
            DbRepository = new ThAOccupancyDbRepository();
            if (DbRepository.Storeys.Count == 0)
            {
                DbRepository.AppendStorey(xtraTabControl1.SelectedTabPage.Text);
            }
            foreach (var storey in DbRepository.Storeys)
            {
                XtraTabPage page = this.xtraTabControl1.TabPages.Add(storey.Identifier);
            }

            this.xtraTabControl1.TabPages.RemoveAt(0);
        }

        private ThAOccupancyStorey CurrentStorey
        {
            get
            {
                return DbRepository.Storeys.Where(o => o.Identifier == xtraTabControl1.SelectedTabPage.Text).FirstOrDefault();
            }
        }

        // XtraTabControl.SelectedPageChanged Event handler
        private void xtraTabControl1_SelectedPageChanged(object sender, TabPageChangedEventArgs e)
        {
            e.PrevPage.Controls.RemoveAt(0);
            e.Page.Controls.Add(gridControl_aoccupancy);
            string storey = e.Page.Text;
            if (!DbRepository.Storeys.Where(o => o.Identifier == storey).Any())
            {
                DbRepository.AppendStorey(storey);
            }
            gridControl_aoccupancy.DataSource = DbRepository.AOccupancies(storey);
            gridControl_aoccupancy.RefreshDataSource();
        }

        private void gridView_aoccupancy_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
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
                    case "FARCoefficient":
                        {
                            e.DisplayText = String.Format("{0:0.0}", Convert.ToDouble(e.Value));
                        }
                        break;
                }
            }
        }

        // ColumnView.CustomUnboundColumnData Event handler
        private void gridView_aoccupancy_CustomUnboundColumnData(object sender, CustomColumnDataEventArgs e)
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
                ThAOccupancy aoccupancy = e.Row as ThAOccupancy;
                e.Value = aoccupancy.IsDefined ? "" : "选择";
            }
        }

        private void gridView_aoccupancy_RowClick(object sender, RowClickEventArgs e)
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
            ThAOccupancy aoccupancy = (ThAOccupancy)gridView.GetRow(e.RowHandle);
            if (!aoccupancy.IsDefined)
            {
                // 面积框线图层名
                ThAOccupancyStorey storey = DbRepository.Storeys.Where(o => o.ID == aoccupancy.StoreyID).First();
                string name = ThResidentialRoomUtil.LayerName(storey, aoccupancy);

                // 选取面积框线
                ThCreateAreaFrameCmdHandler.LayerName = name;
                ThCreateAreaFrameCmdHandler.Handler = new ThCreateAreaFrameCommand()
                {
                    LayerCreator = ThResidentialRoomDbUtil.ConfigLayer
                };
                ThCreateAreaFrameCmdHandler.ExecuteFromCommandLine("*THCREATEAREAFRAME");
            }
        }

        private void gridView_aoccupancy_CellValueChanging(object sender, CellValueChangedEventArgs e)
        {
            if (!(sender is GridView view))
            {
                return;
            }

            if (e.Column.FieldName != "Component")
            {
                return;
            }

            double coefficient = 1.0;
            double farCoefficient = 1.0;
            switch (e.Value.ToString())
            {
                case "主体":
                case "架空":
                    {
                        coefficient = 1.0;
                        farCoefficient = 1.0;
                    }
                    break;
                case "阳台":
                case "雨棚":
                case "附属其他构件":
                    {
                        coefficient = 0.5;
                        farCoefficient = 0.5;
                    }
                    break;
                case "飘窗":
                    {
                        coefficient = 0.0;
                        farCoefficient = 0.0;
                    }
                    break;
            }
            view.SetRowCellValue(e.RowHandle, view.Columns["Coefficient"], coefficient);
            view.SetRowCellValue(e.RowHandle, view.Columns["FARCoefficient"], farCoefficient);
        }

        private void gridView_aoccupancy_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            if (!(sender is GridView view))
            {
                return;
            }

            var columns = new List<string>
            {
                "Component",
                "Category",
                "Coefficient",
                "FARCoefficient",
                "Floors"
            };
            if (!columns.Contains(e.Column.FieldName))
            {
                return;
            }

            ThAOccupancy aoccupancy = view.GetRow(e.RowHandle) as ThAOccupancy;
            // 面积框线图层名
            ThAOccupancyStorey storey = DbRepository.Storeys.Where(o => o.ID == aoccupancy.StoreyID).First();
            string newName = ThResidentialRoomUtil.LayerName(storey, aoccupancy);
            if (aoccupancy.IsDefined)
            {
                // 更新面积框线图层名
                Presenter.OnMoveAreaFrameToLayer(newName, aoccupancy.Frame);

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

        private void gridView_aoccupancy_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
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

        private void gridView_aoccupancy_ShowingEditor(object sender, System.ComponentModel.CancelEventArgs e)
        {
            GridView View = sender as GridView;
            string cellValue = View.GetRowCellValue(View.FocusedRowHandle, "Category").ToString();
            if (cellValue != "室内停车库" && View.FocusedColumn.FieldName == "Floors")
                e.Cancel = true;
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

        private void barButtonItem_add_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            using (var dlg = new ThResidentialStoreyDialog(""))
            {
                dlg.Text = "增加层";
                if (AcadApp.ShowModalDialog(dlg) != DialogResult.OK)
                    return;

                // 楼层是否已经存在
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
            foreach (var aoccupancy in CurrentStorey.AOccupancies)
            {
                if (!aoccupancy.IsDefined)
                {
                    continue;
                }

                string name = ThResidentialRoomUtil.LayerName(CurrentStorey, aoccupancy);
                Presenter.OnDeleteAreaFrame(aoccupancy.Frame);
                Presenter.OnDeleteAreaFrameLayer(name);
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
                foreach (var aoccupancy in CurrentStorey.AOccupancies)
                {
                    // 将面积框线置于新的图层上
                    string newName = ThResidentialRoomUtil.LayerName(dlg.Storey, aoccupancy);
                    Presenter.OnMoveAreaFrameToLayer(newName, aoccupancy.Frame);

                    // 删除旧的图层
                    string name = ThResidentialRoomUtil.LayerName(storey, aoccupancy);
                    Presenter.OnDeleteAreaFrameLayer(name);
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

        private void gridView_aoccupancy_CustomRowCellEdit(object sender, CustomRowCellEditEventArgs e)
        {
            if (e.Column.FieldName != "gridColumn_pick")
            {
                return;
            }

            GridView view = sender as GridView;
            ThAOccupancy aoccupancy = view.GetRow(e.RowHandle) as ThAOccupancy;
            if (!aoccupancy.IsDefined)
            {
                e.RepositoryItem = repositoryItemHyperLinkEdit1;
            }
            else
            {
                e.RepositoryItem = null;
            }
        }

        private void gridView_aoccupancy_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
        {
            GridView view = sender as GridView;
            if (e.MenuType == GridMenuType.Row)
            {
                if (e.HitInfo.InRow || e.HitInfo.InRowCell)
                {
                    foreach (var handle in view.GetSelectedRows())
                    {
                        var aoccupancy = view.GetRow(handle) as ThAOccupancy;
                        if (!aoccupancy.IsDefined)
                        {
                            return;
                        }
                    }

                    e.Menu.Items.Clear();
                    e.Menu.Items.Add(CreateDeleteAreaFrameMenuItem(view, e.HitInfo.RowHandle));
                }
            }
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
                foreach (var handle in ri.View.GetSelectedRows())
                {
                    var aoccupancy = ri.View.GetRow(handle) as ThAOccupancy;
                    if (!aoccupancy.IsDefined)
                    {
                        continue;
                    }

                    string name = ThResidentialRoomDbUtil.LayerName(aoccupancy.Frame);
                    Presenter.OnDeleteAreaFrame(aoccupancy.Frame);
                    Presenter.OnDeleteAreaFrameLayer(name);
                }

                // 更新界面
                this.Reload();
            }
        }

        private void gridView_aoccupancy_DoubleClick(object sender, EventArgs e)
        {
            DXMouseEventArgs ea = e as DXMouseEventArgs;
            GridView view = sender as GridView;
            GridHitInfo info = view.CalcHitInfo(ea.Location);
            if (info.InRow || info.InRowCell)
            {
                var aoccupancy = view.GetRow(info.RowHandle) as ThAOccupancy;
                if (aoccupancy.IsDefined)
                {
                    foreach (var item in CurrentStorey.AOccupancies)
                    {
                        if (!item.IsDefined)
                        {
                            continue;
                        }

                        if (item.ID == aoccupancy.ID)
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
