using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ThAreaFrameConfig.View;
using ThAreaFrameConfig.Model;
using ThAreaFrameConfig.Command;
using ThAreaFrameConfig.Presenter;
using DevExpress.Utils;
using DevExpress.Utils.Menu;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using Autodesk.AutoCAD.ApplicationServices;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
using AcHelper;

namespace ThAreaFrameConfig.WinForms
{
    public partial class ThUnderGroundParkingControl : DevExpress.XtraEditors.XtraUserControl, 
        IUnderGroundParkingView,
        IAreaFrameDocumentReactor
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
                ThCreateAreaFrameCmdHandler.LayerName = name;
                ThCreateAreaFrameCmdHandler.Handler = new ThCreateAreaFrameCommand()
                {
                    LayerCreator = ThResidentialRoomDbUtil.ConfigLayer
                };
                ThCreateAreaFrameCmdHandler.ExecuteFromCommandLine("*THCREATAREAFRAME");
            }
        }

        private void gridView_parking_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (!(sender is GridView view))
            {
                return;
            }

            var columns = new List<string>
            {
                "Floors",
                "Storey"
            };
            if (!columns.Contains(e.Column.FieldName))
            {
                return;
            }

            ThUnderGroundParking parking = view.GetRow(e.RowHandle) as ThUnderGroundParking;
            if (parking.IsDefined)
            {
                // 面积框线图层名
                string name = ThResidentialRoomUtil.LayerName(parking);

                // 更新面积框线图层名
                Presenter.OnRenameAreaFrameLayer(name, parking.Frames.ToArray());

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
                        if (!ValidStorey(e.Value.ToString()))
                        {
                            e.Valid = false;
                            e.ErrorText = "请输入正确的楼层格式";
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
                    foreach(var item in DbRepository.Parkings)
                    {
                        if (!item.IsDefined)
                        {
                            continue;
                        }

                        if (item.ID == parking.ID)
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

        private void gridView_parking_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
        {
            GridView view = sender as GridView;
            if (e.MenuType == GridMenuType.Row)
            {
                if (e.HitInfo.InRow || e.HitInfo.InRowCell)
                {
                    foreach (var handle in view.GetSelectedRows())
                    {
                        var parking = view.GetRow(handle) as ThUnderGroundParking;
                        if (!parking.IsDefined)
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
            return new DXMenuItem("删除", new EventHandler(OnDeleteUnderGroundParkingItemClick))
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
                // 更新图纸
                // 支持多选
                foreach (var handle in ri.View.GetSelectedRows())
                {
                    var parking = ri.View.GetRow(handle) as ThUnderGroundParking;
                    if (!parking.IsDefined)
                    {
                        continue;
                    }

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

        private bool ValidStorey(string storey)
        {
            var floors = new List<int>();

            // 匹配X^Y
            string pattern = @"^-?[0-9]+[\^]-?[0-9]+$";
            Match m = Regex.Match(storey, pattern);
            while (m.Success)
            {
                int[] numbers = Array.ConvertAll(m.Value.Split('^'), int.Parse);
                floors.AddRange(Enumerable.Range(numbers[0], (numbers[1] - numbers[0] + 1)));

                m = m.NextMatch();
            }

            // 匹配X'Y
            pattern = @"^-?[0-9]+'-?[0-9]+$";
            m = Regex.Match(storey, pattern);
            while (m.Success)
            {
                int[] numbers = Array.ConvertAll(m.Value.Split('\''), int.Parse);
                floors.AddRange(numbers);

                m = m.NextMatch();
            }

            // 匹配数字
            pattern = @"^-?[0-9]+$";
            m = Regex.Match(storey, pattern);
            while (m.Success)
            {
                floors.Add(Int16.Parse(m.Value));

                m = m.NextMatch();
            }

            return (floors.Count > 0);
        }

        #region IAreaFrameDocumentReactor

        public void RegisterCommandWillStartEvent()
        {
            Active.Document.CommandWillStart += OnAreaFrameCommandWillStart;
        }

        public void UnRegisterCommandWillStartEvent()
        {
            Active.Document.CommandWillStart -= OnAreaFrameCommandWillStart;
        }

        public void RegisterCommandEndedEvent()
        {
            Active.Document.CommandEnded += OnAreaFrameCommandEnded;
        }

        public void UnRegisterCommandEndedEvent()
        {
            Active.Document.CommandEnded -= OnAreaFrameCommandEnded;
        }

        public void RegisterCommandFailedEvent()
        {
            Active.Document.CommandFailed += OnAreaFrameCommandFailed;
        }

        public void UnRegisterCommandFailedEvent()
        {
            Active.Document.CommandFailed -= OnAreaFrameCommandFailed;
        }

        public void RegisterCommandCancelledEvent()
        {
            Active.Document.CommandCancelled += OnAreaFrameCommandCancelled;
        }

        public void UnRegisterCommandCancelledEvent()
        {
            Active.Document.CommandCancelled -= OnAreaFrameCommandCancelled;
        }

        private void OnAreaFrameCommandWillStart(object sender, CommandEventArgs e)
        {
        }

        private void OnAreaFrameCommandEnded(object sender, CommandEventArgs e)
        {
            if (e.GlobalCommandName == "*THCREATAREAFRAME")
            {
                if (ThCreateAreaFrameCmdHandler.Handler.Success)
                {
                    AcadApp.Idle += Application_OnIdle;
                }
            }
        }

        private void OnAreaFrameCommandFailed(object sender, CommandEventArgs e)
        {
        }

        private void OnAreaFrameCommandCancelled(object sender, CommandEventArgs e)
        {
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
