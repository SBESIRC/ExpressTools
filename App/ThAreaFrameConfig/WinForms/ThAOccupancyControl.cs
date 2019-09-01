using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using ThAreaFrameConfig.View;
using ThAreaFrameConfig.Model;
using ThAreaFrameConfig.Presenter;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraTab;
using DevExpress.XtraTab.ViewInfo;
using Autodesk.AutoCAD.Runtime;

namespace ThAreaFrameConfig.WinForms
{
    public partial class ThAOccupancyControl : DevExpress.XtraEditors.XtraUserControl, IAOccupancyView
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

        public void Attach(IAOccupanyPresenterCallback presenter)
        {
            //
        }

        public void Reload()
        {
            DbRepository = new ThAOccupancyDbRepository();
            gridControl_aoccupancy.DataSource = DbRepository.AOccupancies(this.CurrentStorey);
            gridControl_aoccupancy.RefreshDataSource();
        }

        public void InitializeTabControl()
        {
            Presenter = new ThAOccupanyPresenter(this);
            DbRepository = new ThAOccupancyDbRepository();
            if (DbRepository.Storeys.Count == 0)
            {
                DbRepository.AppendStorey("c1");
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
            gridControl_aoccupancy.DataSource = DbRepository.AOccupancies(e.Page.Text);
            gridControl_aoccupancy.RefreshDataSource();
        }

        private void gridView_aoccupancy_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName == "Area" && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                double area = Convert.ToDouble(e.Value);
                e.DisplayText = Converter.DistanceToString(area, DistanceUnitFormat.Decimal, 2);
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
                Presenter.OnPickAreaFrames(name);

                // 更新界面
                this.Reload();
            }
        }

        private void gridView_aoccupancy_RowUpdated(object sender, RowObjectEventArgs e)
        {

            if (!(sender is GridView view))
            {
                return;
            }

            try
            {
                ThAOccupancy aoccupancy = (ThAOccupancy)e.Row;
                ThAOccupancyStorey storey = DbRepository.Storeys.Where(o => o.ID == aoccupancy.StoreyID).First();
                string name = ThResidentialRoomUtil.LayerName(storey, aoccupancy);
                Presenter.OnRenameAreaFrameLayer(name, aoccupancy.Frame);

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
            using (var dlg = new ThResidentialStoreyDialog())
            {
                dlg.Text = "增加层";
                if (DialogResult.OK != dlg.ShowDialog())
                    return;

                // 增加一个新的楼层不会导致图纸变化
                DbRepository.AppendStorey(dlg.Storey);
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

            // 更新界面
            this.Reload();
        }

        private void barButtonItem_modify_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            using (var dlg = new ThResidentialStoreyDialog())
            {
                dlg.Text = "修改层";
                if (DialogResult.OK != dlg.ShowDialog())
                    return;

                // 更新图纸
                XtraTabPage page = this.xtraTabControl1.SelectedTabPage;
                ThAOccupancyStorey storey = DbRepository.Storeys.Where(o => o.Identifier == page.Text).FirstOrDefault();
                if (storey != null)
                {
                    foreach(var aoccupancy in storey.AOccupancies)
                    {
                        string newName = ThResidentialRoomUtil.LayerName(dlg.Storey, aoccupancy);
                        Presenter.OnRenameAreaFrameLayer(newName, aoccupancy.Frame);
                    }
                }

                // 更新界面
                this.Reload();
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
    }
}
