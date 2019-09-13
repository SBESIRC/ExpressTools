namespace ThAreaFrameConfig.WinForms
{
    partial class ThResidentialRoomControl
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            DevExpress.XtraGrid.GridLevelNode gridLevelNode1 = new DevExpress.XtraGrid.GridLevelNode();
            DevExpress.XtraGrid.GridLevelNode gridLevelNode2 = new DevExpress.XtraGrid.GridLevelNode();
            this.gdv_room_area_unit = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.gridControl_room = new DevExpress.XtraGrid.GridControl();
            this.gdv_room_area_frame = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.room_are_frame_id = new DevExpress.XtraGrid.Columns.GridColumn();
            this.room_area_frame_coefficient = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repositoryItemComboBox_coefficient = new DevExpress.XtraEditors.Repository.RepositoryItemComboBox();
            this.room_area_frame_far_coefficient = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repositoryItemComboBox_farcoefficient = new DevExpress.XtraEditors.Repository.RepositoryItemComboBox();
            this.room_area_frame_area = new DevExpress.XtraGrid.Columns.GridColumn();
            this.room_area_frame_pick = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gdv_room = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.room_name = new DevExpress.XtraGrid.Columns.GridColumn();
            this.room_identifier = new DevExpress.XtraGrid.Columns.GridColumn();
            this.room_area_dwelling = new DevExpress.XtraGrid.Columns.GridColumn();
            this.room_area_balcony = new DevExpress.XtraGrid.Columns.GridColumn();
            this.room_area_baywindow = new DevExpress.XtraGrid.Columns.GridColumn();
            this.room_area_miscellaneous = new DevExpress.XtraGrid.Columns.GridColumn();
            this.room_area_aggregation = new DevExpress.XtraGrid.Columns.GridColumn();
            this.room_area_adjustment = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repositoryItemHyperLinkEdit2 = new DevExpress.XtraEditors.Repository.RepositoryItemHyperLinkEdit();
            this.repositoryItemHyperLinkEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemHyperLinkEdit();
            this.xtraTabControl1 = new DevExpress.XtraTab.XtraTabControl();
            this.xtraTabPage1 = new DevExpress.XtraTab.XtraTabPage();
            this.barManager1 = new DevExpress.XtraBars.BarManager(this.components);
            this.barDockControlTop = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlBottom = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlLeft = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlRight = new DevExpress.XtraBars.BarDockControl();
            this.barButtonItem_add = new DevExpress.XtraBars.BarButtonItem();
            this.barButtonItem_modify = new DevExpress.XtraBars.BarButtonItem();
            this.barButtonItem_delete = new DevExpress.XtraBars.BarButtonItem();
            this.popupMenu_storey = new DevExpress.XtraBars.PopupMenu(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.gdv_room_area_unit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl_room)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gdv_room_area_frame)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBox_coefficient)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBox_farcoefficient)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gdv_room)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemHyperLinkEdit2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemHyperLinkEdit1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.xtraTabControl1)).BeginInit();
            this.xtraTabControl1.SuspendLayout();
            this.xtraTabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.popupMenu_storey)).BeginInit();
            this.SuspendLayout();
            // 
            // gdv_room_area_unit
            // 
            this.gdv_room_area_unit.GridControl = this.gridControl_room;
            this.gdv_room_area_unit.HorzScrollVisibility = DevExpress.XtraGrid.Views.Base.ScrollVisibility.Never;
            this.gdv_room_area_unit.Name = "gdv_room_area_unit";
            this.gdv_room_area_unit.OptionsBehavior.SmartVertScrollBar = false;
            this.gdv_room_area_unit.OptionsDetail.ShowDetailTabs = false;
            this.gdv_room_area_unit.OptionsMenu.EnableColumnMenu = false;
            this.gdv_room_area_unit.OptionsMenu.EnableFooterMenu = false;
            this.gdv_room_area_unit.OptionsMenu.EnableGroupPanelMenu = false;
            this.gdv_room_area_unit.OptionsMenu.ShowAddNewSummaryItem = DevExpress.Utils.DefaultBoolean.False;
            this.gdv_room_area_unit.OptionsMenu.ShowDateTimeGroupIntervalItems = false;
            this.gdv_room_area_unit.OptionsMenu.ShowGroupSortSummaryItems = false;
            this.gdv_room_area_unit.OptionsMenu.ShowSplitItem = false;
            this.gdv_room_area_unit.OptionsPrint.PrintFooter = false;
            this.gdv_room_area_unit.OptionsPrint.PrintGroupFooter = false;
            this.gdv_room_area_unit.OptionsView.ShowColumnHeaders = false;
            this.gdv_room_area_unit.OptionsView.ShowGroupExpandCollapseButtons = false;
            this.gdv_room_area_unit.OptionsView.ShowGroupPanel = false;
            this.gdv_room_area_unit.OptionsView.ShowIndicator = false;
            this.gdv_room_area_unit.Tag = "room_area_unit";
            this.gdv_room_area_unit.VertScrollVisibility = DevExpress.XtraGrid.Views.Base.ScrollVisibility.Never;
            this.gdv_room_area_unit.MasterRowGetLevelDefaultView += new DevExpress.XtraGrid.Views.Grid.MasterRowGetLevelDefaultViewEventHandler(this.gdv_room_area_unit_MasterRowGetLevelDefaultView);
            // 
            // gridControl_room
            // 
            this.gridControl_room.Dock = System.Windows.Forms.DockStyle.Fill;
            gridLevelNode1.LevelTemplate = this.gdv_room_area_unit;
            gridLevelNode1.RelationName = "Level1";
            gridLevelNode2.LevelTemplate = this.gdv_room_area_frame;
            gridLevelNode2.RelationName = "Level2";
            this.gridControl_room.LevelTree.Nodes.AddRange(new DevExpress.XtraGrid.GridLevelNode[] {
            gridLevelNode1,
            gridLevelNode2});
            this.gridControl_room.Location = new System.Drawing.Point(0, 0);
            this.gridControl_room.MainView = this.gdv_room;
            this.gridControl_room.Name = "gridControl_room";
            this.gridControl_room.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemHyperLinkEdit1,
            this.repositoryItemComboBox_coefficient,
            this.repositoryItemComboBox_farcoefficient,
            this.repositoryItemHyperLinkEdit2});
            this.gridControl_room.Size = new System.Drawing.Size(829, 467);
            this.gridControl_room.TabIndex = 0;
            this.gridControl_room.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gdv_room_area_frame,
            this.gdv_room,
            this.gdv_room_area_unit});
            // 
            // gdv_room_area_frame
            // 
            this.gdv_room_area_frame.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.room_are_frame_id,
            this.room_area_frame_coefficient,
            this.room_area_frame_far_coefficient,
            this.room_area_frame_area,
            this.room_area_frame_pick});
            this.gdv_room_area_frame.GridControl = this.gridControl_room;
            this.gdv_room_area_frame.HorzScrollVisibility = DevExpress.XtraGrid.Views.Base.ScrollVisibility.Never;
            this.gdv_room_area_frame.Name = "gdv_room_area_frame";
            this.gdv_room_area_frame.OptionsBehavior.AutoExpandAllGroups = true;
            this.gdv_room_area_frame.OptionsBehavior.EditorShowMode = DevExpress.Utils.EditorShowMode.Click;
            this.gdv_room_area_frame.OptionsBehavior.SmartVertScrollBar = false;
            this.gdv_room_area_frame.OptionsDetail.ShowDetailTabs = false;
            this.gdv_room_area_frame.OptionsMenu.EnableColumnMenu = false;
            this.gdv_room_area_frame.OptionsMenu.EnableFooterMenu = false;
            this.gdv_room_area_frame.OptionsMenu.EnableGroupPanelMenu = false;
            this.gdv_room_area_frame.OptionsMenu.ShowAddNewSummaryItem = DevExpress.Utils.DefaultBoolean.False;
            this.gdv_room_area_frame.OptionsMenu.ShowDateTimeGroupIntervalItems = false;
            this.gdv_room_area_frame.OptionsMenu.ShowGroupSortSummaryItems = false;
            this.gdv_room_area_frame.OptionsMenu.ShowSplitItem = false;
            this.gdv_room_area_frame.OptionsPrint.PrintGroupFooter = false;
            this.gdv_room_area_frame.OptionsSelection.MultiSelect = true;
            this.gdv_room_area_frame.OptionsView.AllowHtmlDrawGroups = false;
            this.gdv_room_area_frame.OptionsView.ShowColumnHeaders = false;
            this.gdv_room_area_frame.OptionsView.ShowFilterPanelMode = DevExpress.XtraGrid.Views.Base.ShowFilterPanelMode.Never;
            this.gdv_room_area_frame.OptionsView.ShowGroupPanel = false;
            this.gdv_room_area_frame.OptionsView.ShowIndicator = false;
            this.gdv_room_area_frame.Tag = "room_area_frame";
            this.gdv_room_area_frame.VertScrollVisibility = DevExpress.XtraGrid.Views.Base.ScrollVisibility.Never;
            this.gdv_room_area_frame.RowClick += new DevExpress.XtraGrid.Views.Grid.RowClickEventHandler(this.gdv_room_area_frame_RowClick);
            this.gdv_room_area_frame.CustomRowCellEdit += new DevExpress.XtraGrid.Views.Grid.CustomRowCellEditEventHandler(this.gdv_room_area_frame_CustomRowCellEdit);
            this.gdv_room_area_frame.PopupMenuShowing += new DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventHandler(this.gdv_room_area_frame_PopupMenuShowing);
            this.gdv_room_area_frame.RowUpdated += new DevExpress.XtraGrid.Views.Base.RowObjectEventHandler(this.gdv_room_area_frame_RowUpdated);
            this.gdv_room_area_frame.CustomUnboundColumnData += new DevExpress.XtraGrid.Views.Base.CustomColumnDataEventHandler(this.gdv_room_area_frame_CustomUnboundColumnData);
            this.gdv_room_area_frame.CustomColumnDisplayText += new DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventHandler(this.gdv_room_area_frame_CustomColumnDisplayText);
            this.gdv_room_area_frame.DoubleClick += new System.EventHandler(this.gdv_room_area_frame_DoubleClick);
            this.gdv_room_area_frame.ValidatingEditor += new DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventHandler(this.gdv_room_area_frame_ValidatingEditor);
            // 
            // room_are_frame_id
            // 
            this.room_are_frame_id.Caption = "编号";
            this.room_are_frame_id.FieldName = "Number";
            this.room_are_frame_id.Name = "room_are_frame_id";
            this.room_are_frame_id.OptionsColumn.AllowEdit = false;
            this.room_are_frame_id.OptionsColumn.AllowFocus = false;
            this.room_are_frame_id.Visible = true;
            this.room_are_frame_id.VisibleIndex = 0;
            // 
            // room_area_frame_coefficient
            // 
            this.room_area_frame_coefficient.Caption = "计算系数";
            this.room_area_frame_coefficient.ColumnEdit = this.repositoryItemComboBox_coefficient;
            this.room_area_frame_coefficient.FieldName = "Coefficient";
            this.room_area_frame_coefficient.Name = "room_area_frame_coefficient";
            this.room_area_frame_coefficient.Visible = true;
            this.room_area_frame_coefficient.VisibleIndex = 1;
            // 
            // repositoryItemComboBox_coefficient
            // 
            this.repositoryItemComboBox_coefficient.AutoHeight = false;
            this.repositoryItemComboBox_coefficient.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.repositoryItemComboBox_coefficient.Items.AddRange(new object[] {
            "0.0",
            "0.5",
            "1.0"});
            this.repositoryItemComboBox_coefficient.Name = "repositoryItemComboBox_coefficient";
            // 
            // room_area_frame_far_coefficient
            // 
            this.room_area_frame_far_coefficient.Caption = "计容系数";
            this.room_area_frame_far_coefficient.ColumnEdit = this.repositoryItemComboBox_farcoefficient;
            this.room_area_frame_far_coefficient.FieldName = "FARCoefficient";
            this.room_area_frame_far_coefficient.Name = "room_area_frame_far_coefficient";
            this.room_area_frame_far_coefficient.Visible = true;
            this.room_area_frame_far_coefficient.VisibleIndex = 2;
            // 
            // repositoryItemComboBox_farcoefficient
            // 
            this.repositoryItemComboBox_farcoefficient.AutoHeight = false;
            this.repositoryItemComboBox_farcoefficient.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.repositoryItemComboBox_farcoefficient.Items.AddRange(new object[] {
            "0.0",
            "0.5",
            "1.0"});
            this.repositoryItemComboBox_farcoefficient.Name = "repositoryItemComboBox_farcoefficient";
            // 
            // room_area_frame_area
            // 
            this.room_area_frame_area.Caption = "面积";
            this.room_area_frame_area.FieldName = "Area";
            this.room_area_frame_area.Name = "room_area_frame_area";
            this.room_area_frame_area.OptionsColumn.AllowEdit = false;
            this.room_area_frame_area.OptionsColumn.AllowFocus = false;
            this.room_area_frame_area.Visible = true;
            this.room_area_frame_area.VisibleIndex = 3;
            // 
            // room_area_frame_pick
            // 
            this.room_area_frame_pick.Caption = "选择";
            this.room_area_frame_pick.FieldName = "room_area_frame_pick";
            this.room_area_frame_pick.Name = "room_area_frame_pick";
            this.room_area_frame_pick.OptionsColumn.AllowEdit = false;
            this.room_area_frame_pick.OptionsColumn.AllowFocus = false;
            this.room_area_frame_pick.UnboundType = DevExpress.Data.UnboundColumnType.String;
            this.room_area_frame_pick.Visible = true;
            this.room_area_frame_pick.VisibleIndex = 4;
            // 
            // gdv_room
            // 
            this.gdv_room.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.room_name,
            this.room_identifier,
            this.room_area_dwelling,
            this.room_area_balcony,
            this.room_area_baywindow,
            this.room_area_miscellaneous,
            this.room_area_aggregation,
            this.room_area_adjustment});
            this.gdv_room.GridControl = this.gridControl_room;
            this.gdv_room.Name = "gdv_room";
            this.gdv_room.OptionsBehavior.AutoExpandAllGroups = true;
            this.gdv_room.OptionsBehavior.SmartVertScrollBar = false;
            this.gdv_room.OptionsDetail.ShowDetailTabs = false;
            this.gdv_room.OptionsMenu.EnableColumnMenu = false;
            this.gdv_room.OptionsMenu.EnableFooterMenu = false;
            this.gdv_room.OptionsMenu.EnableGroupPanelMenu = false;
            this.gdv_room.OptionsMenu.ShowAddNewSummaryItem = DevExpress.Utils.DefaultBoolean.False;
            this.gdv_room.OptionsMenu.ShowDateTimeGroupIntervalItems = false;
            this.gdv_room.OptionsMenu.ShowGroupSortSummaryItems = false;
            this.gdv_room.OptionsMenu.ShowSplitItem = false;
            this.gdv_room.OptionsSelection.MultiSelect = true;
            this.gdv_room.OptionsView.EnableAppearanceEvenRow = true;
            this.gdv_room.OptionsView.EnableAppearanceOddRow = true;
            this.gdv_room.OptionsView.ShowFilterPanelMode = DevExpress.XtraGrid.Views.Base.ShowFilterPanelMode.Never;
            this.gdv_room.OptionsView.ShowGroupPanel = false;
            this.gdv_room.OptionsView.ShowIndicator = false;
            this.gdv_room.Tag = "room";
            this.gdv_room.RowClick += new DevExpress.XtraGrid.Views.Grid.RowClickEventHandler(this.gdv_room_RowClick);
            this.gdv_room.MasterRowGetLevelDefaultView += new DevExpress.XtraGrid.Views.Grid.MasterRowGetLevelDefaultViewEventHandler(this.gdv_room_MasterRowGetLevelDefaultView);
            this.gdv_room.PopupMenuShowing += new DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventHandler(this.gdv_room_PopupMenuShowing);
            this.gdv_room.CustomUnboundColumnData += new DevExpress.XtraGrid.Views.Base.CustomColumnDataEventHandler(this.gdv_room_CustomUnboundColumnData);
            this.gdv_room.CustomColumnDisplayText += new DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventHandler(this.gdv_room_CustomColumnDisplayText);
            // 
            // room_name
            // 
            this.room_name.Caption = "户型名";
            this.room_name.FieldName = "Name";
            this.room_name.Name = "room_name";
            this.room_name.Visible = true;
            this.room_name.VisibleIndex = 0;
            // 
            // room_identifier
            // 
            this.room_identifier.Caption = "户型标识";
            this.room_identifier.FieldName = "Identifier";
            this.room_identifier.Name = "room_identifier";
            this.room_identifier.Visible = true;
            this.room_identifier.VisibleIndex = 1;
            // 
            // room_area_dwelling
            // 
            this.room_area_dwelling.Caption = "套内";
            this.room_area_dwelling.FieldName = "DwellingArea";
            this.room_area_dwelling.Name = "room_area_dwelling";
            this.room_area_dwelling.OptionsColumn.AllowEdit = false;
            this.room_area_dwelling.OptionsColumn.AllowFocus = false;
            this.room_area_dwelling.OptionsColumn.ReadOnly = true;
            this.room_area_dwelling.Visible = true;
            this.room_area_dwelling.VisibleIndex = 2;
            // 
            // room_area_balcony
            // 
            this.room_area_balcony.Caption = "阳台";
            this.room_area_balcony.FieldName = "BalconyArea";
            this.room_area_balcony.Name = "room_area_balcony";
            this.room_area_balcony.OptionsColumn.AllowEdit = false;
            this.room_area_balcony.OptionsColumn.AllowFocus = false;
            this.room_area_balcony.OptionsColumn.ReadOnly = true;
            this.room_area_balcony.Visible = true;
            this.room_area_balcony.VisibleIndex = 3;
            // 
            // room_area_baywindow
            // 
            this.room_area_baywindow.Caption = "飘窗";
            this.room_area_baywindow.FieldName = "BaywindowArea";
            this.room_area_baywindow.Name = "room_area_baywindow";
            this.room_area_baywindow.OptionsColumn.AllowEdit = false;
            this.room_area_baywindow.OptionsColumn.AllowFocus = false;
            this.room_area_baywindow.OptionsColumn.ReadOnly = true;
            this.room_area_baywindow.Visible = true;
            this.room_area_baywindow.VisibleIndex = 4;
            // 
            // room_area_miscellaneous
            // 
            this.room_area_miscellaneous.Caption = "其他";
            this.room_area_miscellaneous.FieldName = "MiscellaneousArea";
            this.room_area_miscellaneous.Name = "room_area_miscellaneous";
            this.room_area_miscellaneous.OptionsColumn.AllowEdit = false;
            this.room_area_miscellaneous.OptionsColumn.AllowFocus = false;
            this.room_area_miscellaneous.OptionsColumn.ReadOnly = true;
            this.room_area_miscellaneous.Visible = true;
            this.room_area_miscellaneous.VisibleIndex = 5;
            // 
            // room_area_aggregation
            // 
            this.room_area_aggregation.Caption = "合计";
            this.room_area_aggregation.FieldName = "AggregationArea";
            this.room_area_aggregation.Name = "room_area_aggregation";
            this.room_area_aggregation.OptionsColumn.AllowEdit = false;
            this.room_area_aggregation.OptionsColumn.AllowFocus = false;
            this.room_area_aggregation.OptionsColumn.ReadOnly = true;
            this.room_area_aggregation.Visible = true;
            this.room_area_aggregation.VisibleIndex = 6;
            // 
            // room_area_adjustment
            // 
            this.room_area_adjustment.Caption = "规整";
            this.room_area_adjustment.ColumnEdit = this.repositoryItemHyperLinkEdit2;
            this.room_area_adjustment.FieldName = "room_area_adjustment";
            this.room_area_adjustment.Name = "room_area_adjustment";
            this.room_area_adjustment.OptionsColumn.AllowEdit = false;
            this.room_area_adjustment.UnboundType = DevExpress.Data.UnboundColumnType.String;
            this.room_area_adjustment.Visible = true;
            this.room_area_adjustment.VisibleIndex = 7;
            // 
            // repositoryItemHyperLinkEdit2
            // 
            this.repositoryItemHyperLinkEdit2.AutoHeight = false;
            this.repositoryItemHyperLinkEdit2.LinkColor = System.Drawing.Color.Blue;
            this.repositoryItemHyperLinkEdit2.Name = "repositoryItemHyperLinkEdit2";
            // 
            // repositoryItemHyperLinkEdit1
            // 
            this.repositoryItemHyperLinkEdit1.AutoHeight = false;
            this.repositoryItemHyperLinkEdit1.LinkColor = System.Drawing.Color.Blue;
            this.repositoryItemHyperLinkEdit1.Name = "repositoryItemHyperLinkEdit1";
            // 
            // xtraTabControl1
            // 
            this.xtraTabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.xtraTabControl1.Location = new System.Drawing.Point(0, 0);
            this.xtraTabControl1.Name = "xtraTabControl1";
            this.xtraTabControl1.SelectedTabPage = this.xtraTabPage1;
            this.xtraTabControl1.Size = new System.Drawing.Size(835, 496);
            this.xtraTabControl1.TabIndex = 0;
            this.xtraTabControl1.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] {
            this.xtraTabPage1});
            this.xtraTabControl1.SelectedPageChanged += new DevExpress.XtraTab.TabPageChangedEventHandler(this.xtraTabControl1_SelectedPageChanged);
            this.xtraTabControl1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.xtraTabControl1_MouseUp);
            // 
            // xtraTabPage1
            // 
            this.xtraTabPage1.Controls.Add(this.gridControl_room);
            this.xtraTabPage1.Name = "xtraTabPage1";
            this.xtraTabPage1.Size = new System.Drawing.Size(829, 467);
            this.xtraTabPage1.Text = "c1";
            // 
            // barManager1
            // 
            this.barManager1.DockControls.Add(this.barDockControlTop);
            this.barManager1.DockControls.Add(this.barDockControlBottom);
            this.barManager1.DockControls.Add(this.barDockControlLeft);
            this.barManager1.DockControls.Add(this.barDockControlRight);
            this.barManager1.Form = this;
            this.barManager1.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
            this.barButtonItem_add,
            this.barButtonItem_modify,
            this.barButtonItem_delete});
            this.barManager1.MaxItemId = 3;
            // 
            // barDockControlTop
            // 
            this.barDockControlTop.CausesValidation = false;
            this.barDockControlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.barDockControlTop.Location = new System.Drawing.Point(0, 0);
            this.barDockControlTop.Manager = this.barManager1;
            this.barDockControlTop.Size = new System.Drawing.Size(835, 0);
            // 
            // barDockControlBottom
            // 
            this.barDockControlBottom.CausesValidation = false;
            this.barDockControlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.barDockControlBottom.Location = new System.Drawing.Point(0, 496);
            this.barDockControlBottom.Manager = this.barManager1;
            this.barDockControlBottom.Size = new System.Drawing.Size(835, 0);
            // 
            // barDockControlLeft
            // 
            this.barDockControlLeft.CausesValidation = false;
            this.barDockControlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.barDockControlLeft.Location = new System.Drawing.Point(0, 0);
            this.barDockControlLeft.Manager = this.barManager1;
            this.barDockControlLeft.Size = new System.Drawing.Size(0, 496);
            // 
            // barDockControlRight
            // 
            this.barDockControlRight.CausesValidation = false;
            this.barDockControlRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.barDockControlRight.Location = new System.Drawing.Point(835, 0);
            this.barDockControlRight.Manager = this.barManager1;
            this.barDockControlRight.Size = new System.Drawing.Size(0, 496);
            // 
            // barButtonItem_add
            // 
            this.barButtonItem_add.Caption = "添加";
            this.barButtonItem_add.Id = 0;
            this.barButtonItem_add.Name = "barButtonItem_add";
            this.barButtonItem_add.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonItem_add_ItemClick);
            // 
            // barButtonItem_modify
            // 
            this.barButtonItem_modify.Caption = "修改";
            this.barButtonItem_modify.Id = 1;
            this.barButtonItem_modify.Name = "barButtonItem_modify";
            this.barButtonItem_modify.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonItem_modify_ItemClick);
            // 
            // barButtonItem_delete
            // 
            this.barButtonItem_delete.Caption = "删除";
            this.barButtonItem_delete.Id = 2;
            this.barButtonItem_delete.Name = "barButtonItem_delete";
            this.barButtonItem_delete.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonItem_delete_ItemClick);
            // 
            // popupMenu_storey
            // 
            this.popupMenu_storey.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.barButtonItem_add),
            new DevExpress.XtraBars.LinkPersistInfo(this.barButtonItem_modify),
            new DevExpress.XtraBars.LinkPersistInfo(this.barButtonItem_delete)});
            this.popupMenu_storey.Manager = this.barManager1;
            this.popupMenu_storey.Name = "popupMenu_storey";
            // 
            // ThResidentialRoomControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.xtraTabControl1);
            this.Controls.Add(this.barDockControlLeft);
            this.Controls.Add(this.barDockControlRight);
            this.Controls.Add(this.barDockControlBottom);
            this.Controls.Add(this.barDockControlTop);
            this.Name = "ThResidentialRoomControl";
            this.Size = new System.Drawing.Size(835, 496);
            ((System.ComponentModel.ISupportInitialize)(this.gdv_room_area_unit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl_room)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gdv_room_area_frame)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBox_coefficient)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBox_farcoefficient)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gdv_room)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemHyperLinkEdit2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemHyperLinkEdit1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.xtraTabControl1)).EndInit();
            this.xtraTabControl1.ResumeLayout(false);
            this.xtraTabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.popupMenu_storey)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraTab.XtraTabControl xtraTabControl1;
        private DevExpress.XtraTab.XtraTabPage xtraTabPage1;
        private DevExpress.XtraGrid.GridControl gridControl_room;
        private DevExpress.XtraGrid.Views.Grid.GridView gdv_room;
        private DevExpress.XtraGrid.Views.Grid.GridView gdv_room_area_unit;
        private DevExpress.XtraGrid.Views.Grid.GridView gdv_room_area_frame;
        private DevExpress.XtraGrid.Columns.GridColumn room_name;
        private DevExpress.XtraGrid.Columns.GridColumn room_identifier;
        private DevExpress.XtraGrid.Columns.GridColumn room_area_dwelling;
        private DevExpress.XtraGrid.Columns.GridColumn room_area_balcony;
        private DevExpress.XtraGrid.Columns.GridColumn room_area_baywindow;
        private DevExpress.XtraGrid.Columns.GridColumn room_area_miscellaneous;
        private DevExpress.XtraGrid.Columns.GridColumn room_area_aggregation;
        private DevExpress.XtraGrid.Columns.GridColumn room_are_frame_id;
        private DevExpress.XtraGrid.Columns.GridColumn room_area_frame_coefficient;
        private DevExpress.XtraGrid.Columns.GridColumn room_area_frame_far_coefficient;
        private DevExpress.XtraGrid.Columns.GridColumn room_area_frame_area;
        private DevExpress.XtraGrid.Columns.GridColumn room_area_frame_pick;
        private DevExpress.XtraEditors.Repository.RepositoryItemHyperLinkEdit repositoryItemHyperLinkEdit1;
        private DevExpress.XtraEditors.Repository.RepositoryItemComboBox repositoryItemComboBox_coefficient;
        private DevExpress.XtraEditors.Repository.RepositoryItemComboBox repositoryItemComboBox_farcoefficient;
        private DevExpress.XtraBars.BarManager barManager1;
        private DevExpress.XtraBars.BarDockControl barDockControlTop;
        private DevExpress.XtraBars.BarDockControl barDockControlBottom;
        private DevExpress.XtraBars.BarDockControl barDockControlLeft;
        private DevExpress.XtraBars.BarDockControl barDockControlRight;
        private DevExpress.XtraBars.PopupMenu popupMenu_storey;
        private DevExpress.XtraBars.BarButtonItem barButtonItem_add;
        private DevExpress.XtraBars.BarButtonItem barButtonItem_modify;
        private DevExpress.XtraBars.BarButtonItem barButtonItem_delete;
        private DevExpress.XtraGrid.Columns.GridColumn room_area_adjustment;
        private DevExpress.XtraEditors.Repository.RepositoryItemHyperLinkEdit repositoryItemHyperLinkEdit2;
    }
}
