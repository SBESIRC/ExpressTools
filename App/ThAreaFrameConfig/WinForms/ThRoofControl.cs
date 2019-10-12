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
using Autodesk.AutoCAD.DatabaseServices;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace ThAreaFrameConfig.WinForms
{
    public partial class ThRoofControl : DevExpress.XtraEditors.XtraUserControl, IRoofView, IAreaFrameDatabaseReactor
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
                ThRoof roof = e.Row as ThRoof;
                e.Value = roof.IsDefined ? "" : "选择";
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
                if (Presenter.OnPickAreaFrames(name))
                {
                    // 更新界面
                    this.Reload();
                }
            }
        }

        private void gridView_roof_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (!(sender is GridView view))
            {
                return;
            }

            var columns = new List<string>
            {
                "Category",
                "Coefficient",
                "FARCoefficient"
            };
            if (!columns.Contains(e.Column.FieldName))
            {
                return;
            }

            ThRoof roof = view.GetRow(e.RowHandle) as ThRoof;
            if (roof.IsDefined)
            {
                // 面积框线图层名
                string name = ThResidentialRoomUtil.LayerName(roof);

                // 更新面积框线图层名
                Presenter.OnMoveAreaFrameToLayer(name, roof.Frame);

                // 更新界面
                this.Reload();
            }
        }

        private void gridView1_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
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
                    foreach(var item in DbRepository.Roofs)
                    {
                        if (!item.IsDefined)
                        {
                            continue;
                        }

                        if (item.ID == roof.ID)
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

        private void gridView_roof_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
        {
            GridView view = sender as GridView;
            if (e.MenuType == GridMenuType.Row)
            {
                if (e.HitInfo.InRow || e.HitInfo.InRowCell)
                {
                    foreach (var handle in view.GetSelectedRows())
                    {
                        var roof = view.GetRow(handle) as ThRoof;
                        if (!roof.IsDefined)
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
            return new DXMenuItem("删除", new EventHandler(OnDeleteRoofItemClick))
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
                // 支持多选
                foreach (var handle in ri.View.GetSelectedRows())
                {
                    var roof = ri.View.GetRow(handle) as ThRoof;
                    if (!roof.IsDefined)
                    {
                        continue;
                    }

                    string layer = ThResidentialRoomDbUtil.LayerName(roof.Frame);
                    Presenter.OnDeleteAreaFrame(roof.Frame);
                    Presenter.OnDeleteAreaFrameLayer(layer);
                }

                // 更新界面
                this.Reload();
            }
        }

        private void gridView_roof_CustomRowCellEdit(object sender, CustomRowCellEditEventArgs e)
        {
            if (e.Column.FieldName != "gridColumn_pick")
            {
                return;
            }

            GridView view = sender as GridView;
            ThRoof roof = view.GetRow(e.RowHandle) as ThRoof;
            if (!roof.IsDefined)
            {
                e.RepositoryItem = repositoryItemHyperLinkEdit1;
            }
            else
            {
                e.RepositoryItem = null;
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
