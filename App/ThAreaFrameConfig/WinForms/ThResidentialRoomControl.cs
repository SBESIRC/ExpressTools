using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using ThAreaFrameConfig.View;
using ThAreaFrameConfig.Model;
using ThAreaFrameConfig.Presenter;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.Utils.Menu;
using DevExpress.XtraTab;
using DevExpress.XtraTab.Buttons;
using DevExpress.XtraEditors.Controls;
using Autodesk.AutoCAD.Runtime;

namespace ThAreaFrameConfig.WinForms
{
    public partial class ThResidentialRoomControl : DevExpress.XtraEditors.XtraUserControl, IResidentialRoomView
    {
        private ThResidentialRoomPresenter Presenter;
        private ThResidentialRoomDbRepository DbRepository;

        public ThResidentialRoomControl()
        {
            InitializeComponent();
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
            gridControl_room.DataSource = DbRepository.Rooms(this.CurrentStorey);
            gridControl_room.RefreshDataSource();
            RefreshGridControl();
        }

        public void InitializeTabControl()
        {
            this.xtraTabControl1.CustomHeaderButtons.Add(new CustomHeaderButton(ButtonPredefines.Plus));
            this.xtraTabControl1.CustomHeaderButtons.Add(new CustomHeaderButton(ButtonPredefines.Minus));

            Presenter = new ThResidentialRoomPresenter(this);
            DbRepository = new ThResidentialRoomDbRepository();
            if (!DbRepository.Storeys().Any())
            {
                DbRepository.AppendStorey("c1");
            }
            foreach (var storey in DbRepository.Storeys())
            {
                XtraTabPage page = this.xtraTabControl1.TabPages.Add(storey.Identifier);
            }

            this.xtraTabControl1.TabPages.RemoveAt(0);
        }

        private void RefreshGridControl()
        {
            for (int i = 0; i < Rooms.Count; i++)
            {
                gdv_room.SetMasterRowExpanded(i, true);
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

        public void Attach(IThAreaFramePresenterCallback presenter)
        {
            //
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
            gridControl_room.DataSource = DbRepository.Rooms(e.Page.Text);
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
                e.Value = "选择";
            }
        }

        private void gdv_room_area_frame_RowClick(object sender, RowClickEventArgs e)
        {
            if (!(sender is GridView view))
            {
                return;
            }
            if (e.HitInfo.Column.FieldName != "room_area_frame_pick")
            {
                return;
            }

            GridView gridView = (GridView)sender;
            ThResidentialAreaFrame areaFrame = (ThResidentialAreaFrame)gridView.GetRow(e.RowHandle);
            ThResidentialRoom room = DbRepository.Rooms(this.CurrentStorey).Where(o => o.ID == areaFrame.RoomID).First();
            ThResidentialStorey storey = DbRepository.Storeys().Where(o => o.ID == room.StoreyID).First();
            ThResidentialRoomComponent component = room.Components.Find(o => o.ID == areaFrame.ComponentID);
            string name = ThResidentialRoomUtil.LayerName(storey, room, component, areaFrame);
            Presenter.OnPickAreaFrames(name);

            // 更新界面
            this.Reload();
        }

        private void gdv_room_area_frame_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName == "Area" && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                double area = Convert.ToDouble(e.Value);
                e.DisplayText = Converter.DistanceToString(area, DistanceUnitFormat.Decimal, 2);
            }
        }

        private void gdv_room_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
        {
            if (e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                switch(e.Column.FieldName)
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
            /*
            GridView view = sender as GridView;
            switch (view.FocusedColumn.FieldName)
            {
                case "Coefficient":
                case "FARCoefficient":
                    {
                        double? coefficient = e.Value as double?;
                        if (coefficient == 0.0)
                        {
                            e.Valid = false;
                            e.ErrorText = "系数不能为0";
                        }

                    };
                    break;
                default:
                    break;

            };
            */
        }

        private void gdv_room_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
        {
            if (e.MenuType == GridMenuType.Row)
            {
                e.Menu.Items.Clear();
                e.Menu.Items.Add(new DXMenuItem("添加户型", new EventHandler(OnAppendRoomItemClick)));
                e.Menu.Items.Add(new DXMenuItem("删除户型", new EventHandler(OnRemoveRoomItemClick)));
            }
        }

        void OnAppendRoomItemClick(object sender, EventArgs e)
        {
            this.CurrentStorey.Rooms.Add(ThResidentialRoomUtil.DefaultResidentialRoom(this.CurrentStorey.ID));
            gridControl_room.RefreshDataSource();
            RefreshGridControl();
        }

        void OnRemoveRoomItemClick(object sender, EventArgs e)
        {
            //
        }

        private void xtraTabControl1_CustomHeaderButtonClick(object sender, DevExpress.XtraTab.ViewInfo.CustomHeaderButtonEventArgs e)
        {
            if (e.Button.Kind == DevExpress.XtraEditors.Controls.ButtonPredefines.Plus)
            {
                using (var dlg = new ThResidentialStoreyDialog())
                {
                    if (DialogResult.OK != dlg.ShowDialog())
                        return;

                    DbRepository.AppendStorey(dlg.Storey);
                    XtraTabPage page = this.xtraTabControl1.TabPages.Add(dlg.Storey);
                    this.xtraTabControl1.SelectedTabPage = page;
                }
            }
            else if (e.Button.Kind == DevExpress.XtraEditors.Controls.ButtonPredefines.Minus)
            {
                if (this.xtraTabControl1.TabPages.Count == 1)
                    return;

                DbRepository.RemoveStorey(this.CurrentStorey.Identifier);
                this.xtraTabControl1.TabPages.Remove(this.xtraTabControl1.SelectedTabPage);
                Presenter.OnRemoveStorey(ThResidentialRoomUtil.LayerNames(this.CurrentStorey).ToArray());
            }
        }
    }
}
