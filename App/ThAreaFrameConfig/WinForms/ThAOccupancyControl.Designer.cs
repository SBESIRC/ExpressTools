namespace ThAreaFrameConfig.WinForms
{
    partial class ThAOccupancyControl
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
            this.xtraTabControl1 = new DevExpress.XtraTab.XtraTabControl();
            this.xtraTabPage1 = new DevExpress.XtraTab.XtraTabPage();
            this.gridControl_aoccupancy = new DevExpress.XtraGrid.GridControl();
            this.gridView_aoccupancy = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.gridColumn_number = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn_component = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repositoryItemComboBox_component = new DevExpress.XtraEditors.Repository.RepositoryItemComboBox();
            this.gridColumn_category = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repositoryItemComboBox_category = new DevExpress.XtraEditors.Repository.RepositoryItemComboBox();
            this.gridColumn_coefficient = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repositoryItemComboBox_coefficient = new DevExpress.XtraEditors.Repository.RepositoryItemComboBox();
            this.gridColumn_farcoefficient = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repositoryItemComboBox_farcoefficient = new DevExpress.XtraEditors.Repository.RepositoryItemComboBox();
            this.gridColumn_floors = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn_area = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn_pick = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repositoryItemHyperLinkEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemHyperLinkEdit();
            this.barManager1 = new DevExpress.XtraBars.BarManager(this.components);
            this.barDockControlTop = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlBottom = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlLeft = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlRight = new DevExpress.XtraBars.BarDockControl();
            this.barButtonItem_add = new DevExpress.XtraBars.BarButtonItem();
            this.barButtonItem_modify = new DevExpress.XtraBars.BarButtonItem();
            this.barButtonItem_delete = new DevExpress.XtraBars.BarButtonItem();
            this.popupMenu_storey = new DevExpress.XtraBars.PopupMenu(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.xtraTabControl1)).BeginInit();
            this.xtraTabControl1.SuspendLayout();
            this.xtraTabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl_aoccupancy)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView_aoccupancy)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBox_component)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBox_category)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBox_coefficient)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBox_farcoefficient)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemHyperLinkEdit1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.popupMenu_storey)).BeginInit();
            this.SuspendLayout();
            // 
            // xtraTabControl1
            // 
            this.xtraTabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.xtraTabControl1.Location = new System.Drawing.Point(0, 0);
            this.xtraTabControl1.Name = "xtraTabControl1";
            this.xtraTabControl1.SelectedTabPage = this.xtraTabPage1;
            this.xtraTabControl1.Size = new System.Drawing.Size(670, 477);
            this.xtraTabControl1.TabIndex = 0;
            this.xtraTabControl1.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] {
            this.xtraTabPage1});
            this.xtraTabControl1.SelectedPageChanged += new DevExpress.XtraTab.TabPageChangedEventHandler(this.xtraTabControl1_SelectedPageChanged);
            this.xtraTabControl1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.xtraTabControl1_MouseUp);
            // 
            // xtraTabPage1
            // 
            this.xtraTabPage1.Controls.Add(this.gridControl_aoccupancy);
            this.xtraTabPage1.Name = "xtraTabPage1";
            this.xtraTabPage1.Size = new System.Drawing.Size(664, 448);
            this.xtraTabPage1.Text = "c1";
            // 
            // gridControl_aoccupancy
            // 
            this.gridControl_aoccupancy.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControl_aoccupancy.Location = new System.Drawing.Point(0, 0);
            this.gridControl_aoccupancy.MainView = this.gridView_aoccupancy;
            this.gridControl_aoccupancy.Name = "gridControl_aoccupancy";
            this.gridControl_aoccupancy.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemComboBox_component,
            this.repositoryItemComboBox_category,
            this.repositoryItemComboBox_coefficient,
            this.repositoryItemComboBox_farcoefficient,
            this.repositoryItemHyperLinkEdit1});
            this.gridControl_aoccupancy.Size = new System.Drawing.Size(664, 448);
            this.gridControl_aoccupancy.TabIndex = 0;
            this.gridControl_aoccupancy.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView_aoccupancy});
            // 
            // gridView_aoccupancy
            // 
            this.gridView_aoccupancy.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.gridColumn_number,
            this.gridColumn_component,
            this.gridColumn_category,
            this.gridColumn_coefficient,
            this.gridColumn_farcoefficient,
            this.gridColumn_floors,
            this.gridColumn_area,
            this.gridColumn_pick});
            this.gridView_aoccupancy.GridControl = this.gridControl_aoccupancy;
            this.gridView_aoccupancy.Name = "gridView_aoccupancy";
            this.gridView_aoccupancy.OptionsSelection.MultiSelect = true;
            this.gridView_aoccupancy.OptionsView.ShowGroupPanel = false;
            this.gridView_aoccupancy.RowClick += new DevExpress.XtraGrid.Views.Grid.RowClickEventHandler(this.gridView_aoccupancy_RowClick);
            this.gridView_aoccupancy.CustomRowCellEdit += new DevExpress.XtraGrid.Views.Grid.CustomRowCellEditEventHandler(this.gridView_aoccupancy_CustomRowCellEdit);
            this.gridView_aoccupancy.PopupMenuShowing += new DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventHandler(this.gridView_aoccupancy_PopupMenuShowing);
            this.gridView_aoccupancy.ShowingEditor += new System.ComponentModel.CancelEventHandler(this.gridView_aoccupancy_ShowingEditor);
            this.gridView_aoccupancy.CellValueChanged += new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(this.gridView_aoccupancy_CellValueChanged);
            this.gridView_aoccupancy.RowUpdated += new DevExpress.XtraGrid.Views.Base.RowObjectEventHandler(this.gridView_aoccupancy_RowUpdated);
            this.gridView_aoccupancy.CustomUnboundColumnData += new DevExpress.XtraGrid.Views.Base.CustomColumnDataEventHandler(this.gridView_aoccupancy_CustomUnboundColumnData);
            this.gridView_aoccupancy.CustomColumnDisplayText += new DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventHandler(this.gridView_aoccupancy_CustomColumnDisplayText);
            this.gridView_aoccupancy.ValidatingEditor += new DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventHandler(this.gridView_aoccupancy_ValidatingEditor);
            // 
            // gridColumn_number
            // 
            this.gridColumn_number.Caption = "序号";
            this.gridColumn_number.FieldName = "Number";
            this.gridColumn_number.Name = "gridColumn_number";
            this.gridColumn_number.OptionsColumn.AllowEdit = false;
            this.gridColumn_number.OptionsColumn.AllowFocus = false;
            this.gridColumn_number.UnboundType = DevExpress.Data.UnboundColumnType.String;
            this.gridColumn_number.Visible = true;
            this.gridColumn_number.VisibleIndex = 0;
            // 
            // gridColumn_component
            // 
            this.gridColumn_component.Caption = "构件";
            this.gridColumn_component.ColumnEdit = this.repositoryItemComboBox_component;
            this.gridColumn_component.FieldName = "Component";
            this.gridColumn_component.Name = "gridColumn_component";
            this.gridColumn_component.Visible = true;
            this.gridColumn_component.VisibleIndex = 1;
            // 
            // repositoryItemComboBox_component
            // 
            this.repositoryItemComboBox_component.AutoHeight = false;
            this.repositoryItemComboBox_component.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.repositoryItemComboBox_component.Items.AddRange(new object[] {
            "主体",
            "阳台",
            "飘窗",
            "架空",
            "雨棚",
            "附属其他构件"});
            this.repositoryItemComboBox_component.Name = "repositoryItemComboBox_component";
            this.repositoryItemComboBox_component.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            // 
            // gridColumn_category
            // 
            this.gridColumn_category.Caption = "类型";
            this.gridColumn_category.ColumnEdit = this.repositoryItemComboBox_category;
            this.gridColumn_category.FieldName = "Category";
            this.gridColumn_category.Name = "gridColumn_category";
            this.gridColumn_category.Visible = true;
            this.gridColumn_category.VisibleIndex = 2;
            // 
            // repositoryItemComboBox_category
            // 
            this.repositoryItemComboBox_category.AutoHeight = false;
            this.repositoryItemComboBox_category.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.repositoryItemComboBox_category.Items.AddRange(new object[] {
            "商业",
            "办公",
            "旅游",
            "科教文卫",
            "通讯",
            "交通运输",
            "室内停车库"});
            this.repositoryItemComboBox_category.Name = "repositoryItemComboBox_category";
            this.repositoryItemComboBox_category.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            // 
            // gridColumn_coefficient
            // 
            this.gridColumn_coefficient.Caption = "计算系数";
            this.gridColumn_coefficient.ColumnEdit = this.repositoryItemComboBox_coefficient;
            this.gridColumn_coefficient.FieldName = "Coefficient";
            this.gridColumn_coefficient.Name = "gridColumn_coefficient";
            this.gridColumn_coefficient.Visible = true;
            this.gridColumn_coefficient.VisibleIndex = 3;
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
            // gridColumn_farcoefficient
            // 
            this.gridColumn_farcoefficient.Caption = "计容系数";
            this.gridColumn_farcoefficient.ColumnEdit = this.repositoryItemComboBox_farcoefficient;
            this.gridColumn_farcoefficient.FieldName = "FARCoefficient";
            this.gridColumn_farcoefficient.Name = "gridColumn_farcoefficient";
            this.gridColumn_farcoefficient.Visible = true;
            this.gridColumn_farcoefficient.VisibleIndex = 4;
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
            // gridColumn_floors
            // 
            this.gridColumn_floors.Caption = "车位层数";
            this.gridColumn_floors.FieldName = "Floors";
            this.gridColumn_floors.Name = "gridColumn_floors";
            this.gridColumn_floors.Visible = true;
            this.gridColumn_floors.VisibleIndex = 6;
            // 
            // gridColumn_area
            // 
            this.gridColumn_area.Caption = "面积";
            this.gridColumn_area.FieldName = "Area";
            this.gridColumn_area.Name = "gridColumn_area";
            this.gridColumn_area.OptionsColumn.AllowEdit = false;
            this.gridColumn_area.OptionsColumn.AllowFocus = false;
            this.gridColumn_area.Visible = true;
            this.gridColumn_area.VisibleIndex = 5;
            // 
            // gridColumn_pick
            // 
            this.gridColumn_pick.Caption = "选择";
            this.gridColumn_pick.FieldName = "gridColumn_pick";
            this.gridColumn_pick.Name = "gridColumn_pick";
            this.gridColumn_pick.OptionsColumn.AllowEdit = false;
            this.gridColumn_pick.OptionsColumn.AllowFocus = false;
            this.gridColumn_pick.UnboundType = DevExpress.Data.UnboundColumnType.String;
            this.gridColumn_pick.Visible = true;
            this.gridColumn_pick.VisibleIndex = 7;
            // 
            // repositoryItemHyperLinkEdit1
            // 
            this.repositoryItemHyperLinkEdit1.AutoHeight = false;
            this.repositoryItemHyperLinkEdit1.LinkColor = System.Drawing.Color.Blue;
            this.repositoryItemHyperLinkEdit1.Name = "repositoryItemHyperLinkEdit1";
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
            this.barDockControlTop.Size = new System.Drawing.Size(670, 0);
            // 
            // barDockControlBottom
            // 
            this.barDockControlBottom.CausesValidation = false;
            this.barDockControlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.barDockControlBottom.Location = new System.Drawing.Point(0, 477);
            this.barDockControlBottom.Manager = this.barManager1;
            this.barDockControlBottom.Size = new System.Drawing.Size(670, 0);
            // 
            // barDockControlLeft
            // 
            this.barDockControlLeft.CausesValidation = false;
            this.barDockControlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.barDockControlLeft.Location = new System.Drawing.Point(0, 0);
            this.barDockControlLeft.Manager = this.barManager1;
            this.barDockControlLeft.Size = new System.Drawing.Size(0, 477);
            // 
            // barDockControlRight
            // 
            this.barDockControlRight.CausesValidation = false;
            this.barDockControlRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.barDockControlRight.Location = new System.Drawing.Point(670, 0);
            this.barDockControlRight.Manager = this.barManager1;
            this.barDockControlRight.Size = new System.Drawing.Size(0, 477);
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
            // ThAOccupancyControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.xtraTabControl1);
            this.Controls.Add(this.barDockControlLeft);
            this.Controls.Add(this.barDockControlRight);
            this.Controls.Add(this.barDockControlBottom);
            this.Controls.Add(this.barDockControlTop);
            this.Name = "ThAOccupancyControl";
            this.Size = new System.Drawing.Size(670, 477);
            ((System.ComponentModel.ISupportInitialize)(this.xtraTabControl1)).EndInit();
            this.xtraTabControl1.ResumeLayout(false);
            this.xtraTabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl_aoccupancy)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView_aoccupancy)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBox_component)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBox_category)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBox_coefficient)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBox_farcoefficient)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemHyperLinkEdit1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.popupMenu_storey)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraTab.XtraTabControl xtraTabControl1;
        private DevExpress.XtraTab.XtraTabPage xtraTabPage1;
        private DevExpress.XtraGrid.GridControl gridControl_aoccupancy;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView_aoccupancy;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_number;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_component;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_category;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_coefficient;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_farcoefficient;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_area;
        private DevExpress.XtraEditors.Repository.RepositoryItemComboBox repositoryItemComboBox_category;
        private DevExpress.XtraEditors.Repository.RepositoryItemComboBox repositoryItemComboBox_component;
        private DevExpress.XtraEditors.Repository.RepositoryItemComboBox repositoryItemComboBox_coefficient;
        private DevExpress.XtraEditors.Repository.RepositoryItemComboBox repositoryItemComboBox_farcoefficient;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_floors;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_pick;
        private DevExpress.XtraEditors.Repository.RepositoryItemHyperLinkEdit repositoryItemHyperLinkEdit1;
        private DevExpress.XtraBars.BarManager barManager1;
        private DevExpress.XtraBars.BarDockControl barDockControlTop;
        private DevExpress.XtraBars.BarDockControl barDockControlBottom;
        private DevExpress.XtraBars.BarDockControl barDockControlLeft;
        private DevExpress.XtraBars.BarDockControl barDockControlRight;
        private DevExpress.XtraBars.PopupMenu popupMenu_storey;
        private DevExpress.XtraBars.BarButtonItem barButtonItem_add;
        private DevExpress.XtraBars.BarButtonItem barButtonItem_modify;
        private DevExpress.XtraBars.BarButtonItem barButtonItem_delete;
    }
}
