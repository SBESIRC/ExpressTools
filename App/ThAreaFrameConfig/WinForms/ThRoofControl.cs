﻿using System;
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
    public partial class ThRoofControl : DevExpress.XtraEditors.XtraUserControl, IRoofView
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

        public void Attach(IRoofPresenterCallback presenter)
        {
            //
        }

        public void Reload()
        {
            DbRepository = new ThRoofDbRepository();
            gridControl_roof.DataSource = DbRepository.Roofs;
            gridControl_roof.RefreshDataSource();
        }

        public void InitializeGridControl()
        {
            Presenter = new ThRoofPresenter(this);
            DbRepository = new ThRoofDbRepository();
            if (DbRepository.Roofs.Count == 0)
            {
                DbRepository.AppendRoof();
            }
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
                e.Value = "选择";
            }
        }

        private void gridView1_RowClick(object sender, DevExpress.XtraGrid.Views.Grid.RowClickEventArgs e)
        {
            if (!(sender is GridView view))
            {
                return;
            }
            if (e.HitInfo.Column.FieldName != "gridColumn_pick")
            {
                return;
            }

            GridView gridView = (GridView)sender;
            ThRoof roof = (ThRoof)gridView.GetRow(e.RowHandle);
            string name = ThResidentialRoomUtil.LayerName(roof);
            Presenter.OnPickAreaFrames(name);

            // 更新界面
            this.Reload();
        }

        private void gridView1_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName == "Area" && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                double area = Convert.ToDouble(e.Value);
                e.DisplayText = Converter.DistanceToString(area, DistanceUnitFormat.Decimal, 2);
            }
        }
    }
}
