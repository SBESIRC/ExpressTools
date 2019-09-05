﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ThAreaFrameConfig.View;
using ThAreaFrameConfig.Model;
using ThAreaFrameConfig.Presenter;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.Utils;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.Utils.Menu;

namespace ThAreaFrameConfig.WinForms
{
    public partial class ThUnderGroundParkingControl : DevExpress.XtraEditors.XtraUserControl, IUnderGroundParkingView
    {
        private ThUnderGroundParkingPresenter Presenter;
        private ThUnderGroundParkingDbRepository DbRepository;

        public ThUnderGroundParkingControl()
        {
            InitializeComponent();
            InitializeGridControl();
        }

        public List<ThUnderGroundParking> Parkings
        {
            get
            {
                return (List<ThUnderGroundParking>)gridControl_parking.DataSource;
            }

            set
            {
                gridControl_parking.DataSource = value;
            }
        }

        public void Reload()
        {
            DbRepository = new ThUnderGroundParkingDbRepository();
            DbRepository.AppendDefaultUnderGroundParking();
            gridControl_parking.DataSource = DbRepository.Parkings;
            gridControl_parking.RefreshDataSource();
        }

        public void InitializeGridControl()
        {
            Presenter = new ThUnderGroundParkingPresenter(this);
            DbRepository = new ThUnderGroundParkingDbRepository();
            DbRepository.AppendDefaultUnderGroundParking();
            gridControl_parking.DataSource = DbRepository.Parkings;
            gridControl_parking.RefreshDataSource();
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
                ThUnderGroundParking parking = e.Row as ThUnderGroundParking;
                e.Value = parking.IsDefined ? "" : "选择";
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
            ThUnderGroundParking parking = gridView.GetRow(e.RowHandle) as ThUnderGroundParking;
            if (!parking.IsDefined)
            {
                // 面积框线图层名
                string name = ThResidentialRoomUtil.LayerName(parking);

                // 选取面积框线
                if (Presenter.OnPickAreaFrames(name))
                {
                    // 更新界面
                    this.Reload();
                }
            }
        }

        private void gridView_parking_RowUpdated(object sender, DevExpress.XtraGrid.Views.Base.RowObjectEventArgs e)
        {
            if (!(sender is GridView view))
            {
                return;
            }

            ThUnderGroundParking parking = e.Row as ThUnderGroundParking;
            if (parking.IsDefined)
            {
                // 面积框线图层名
                string name = ThResidentialRoomUtil.LayerName(parking);

                // 更新面积框线图层名
                Presenter.OnMoveAreaFrameToLayer(name, parking.Frames[0]);

                // 更新界面
                this.Reload();
            }
        }

        private void gridView_parking_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            GridView view = sender as GridView;
            switch (view.FocusedColumn.FieldName)
            {
                // "车场层数"
                case "Floors":
                    {
                        if (!int.TryParse(e.Value.ToString(), out int value))
                        {
                            e.Valid = false;
                            e.ErrorText = "请输入整数";
                        }

                    }
                    break;
                // "所属层"
                case "Storey":
                    {
                        string pattern = @"^[cC]*\d+$";
                        if (!Regex.IsMatch(e.Value.ToString(), pattern))
                        {
                            e.Valid = false;
                            e.ErrorText = "";
                        }
                    }
                    break;
            };
        }

        private void gridView_parking_DoubleClick(object sender, EventArgs e)
        {
            DXMouseEventArgs ea = e as DXMouseEventArgs;
            GridView view = sender as GridView;
            GridHitInfo info = view.CalcHitInfo(ea.Location);
            if (info.InRow || info.InRowCell)
            {
                ThUnderGroundParking parking = view.GetRow(info.RowHandle) as ThUnderGroundParking;
                if (parking.IsDefined)
                {
                    Presenter.OnHighlightAreaFrames(parking.Frames.ToArray());
                }
            }
        }

        private void gridView_parking_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
        {
            GridView view = sender as GridView;
            if (e.MenuType == GridMenuType.Row)
            {
                if (e.HitInfo.InRow || e.HitInfo.InRowCell)
                {
                    ThUnderGroundParking parking = view.GetRow(e.HitInfo.RowHandle) as ThUnderGroundParking;
                    if (!parking.IsDefined)
                        return;

                    e.Menu.Items.Clear();
                    e.Menu.Items.Add(CreateDeleteMenuItem(view, e.HitInfo.RowHandle));
                    e.Menu.Items.Add(CreateDeleteAllMenuItem(view, e.HitInfo.RowHandle));
                }
            }
        }

        DXMenuItem CreateDeleteMenuItem(GridView view, int rowHandle)
        {
            return new DXMenuItem("删除", new EventHandler(OnDeleteUnderGroundParkingItemClick))
            {
                Tag = new RowInfo(view, rowHandle)
            };
        }

        DXMenuItem CreateDeleteAllMenuItem(GridView view, int rowHandle)
        {
            return new DXMenuItem("全部删除", new EventHandler(OnDeleteAllUnderGroundParkingItemsClick))
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

        void OnDeleteUnderGroundParkingItemClick(object sender, EventArgs e)
        {
            DXMenuItem menuItem = sender as DXMenuItem;
            if (menuItem.Tag is RowInfo ri)
            {
                ThUnderGroundParking parking = ri.View.GetRow(ri.RowHandle) as ThUnderGroundParking;
                if (parking.IsDefined)
                {
                    // 更新图纸
                    string layer = ThResidentialRoomUtil.LayerName(parking);
                    Presenter.OnDeleteAreaFrames(parking.Frames.ToArray());
                    Presenter.OnDeleteAreaFrameLayer(layer);

                    // 更新界面
                    this.Reload();
                }
            }
        }

        void OnDeleteAllUnderGroundParkingItemsClick(object sender, EventArgs e)
        {
            DXMenuItem menuItem = sender as DXMenuItem;
            if (menuItem.Tag is RowInfo ri)
            {
                // 更新图纸
                foreach (var parking in DbRepository.Parkings)
                {
                    if (!parking.IsDefined)
                        continue;

                    string layer = ThResidentialRoomUtil.LayerName(parking);
                    Presenter.OnDeleteAreaFrames(parking.Frames.ToArray());
                    Presenter.OnDeleteAreaFrameLayer(layer);
                }

                // 更新界面
                this.Reload();
            }
        }

        private void gridView_parking_CustomRowCellEdit(object sender, CustomRowCellEditEventArgs e)
        {
            if (e.Column.FieldName != "gridColumn_pick")
            {
                return;
            }

            GridView view = sender as GridView;
            ThUnderGroundParking parking = view.GetRow(e.RowHandle) as ThUnderGroundParking;
            if (!parking.IsDefined)
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
