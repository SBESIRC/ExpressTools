namespace ThAreaFrameConfig.WinForms
{
    partial class ThOutdoorParkingSpaceControl
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
            this.gridControl_outdoor_parking_space = new DevExpress.XtraGrid.GridControl();
            this.gridView_outdoor_parking_space = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.gridColumn_number = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn_category = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn_parking_category = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn_storey = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn_area = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn_pick = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repositoryItemHyperLinkEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemHyperLinkEdit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl_outdoor_parking_space)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView_outdoor_parking_space)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemHyperLinkEdit1)).BeginInit();
            this.SuspendLayout();
            // 
            // gridControl_outdoor_parking_space
            // 
            this.gridControl_outdoor_parking_space.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControl_outdoor_parking_space.Location = new System.Drawing.Point(0, 0);
            this.gridControl_outdoor_parking_space.MainView = this.gridView_outdoor_parking_space;
            this.gridControl_outdoor_parking_space.Name = "gridControl_outdoor_parking_space";
            this.gridControl_outdoor_parking_space.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemHyperLinkEdit1});
            this.gridControl_outdoor_parking_space.Size = new System.Drawing.Size(552, 346);
            this.gridControl_outdoor_parking_space.TabIndex = 0;
            this.gridControl_outdoor_parking_space.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView_outdoor_parking_space});
            // 
            // gridView_outdoor_parking_space
            // 
            this.gridView_outdoor_parking_space.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.gridColumn_number,
            this.gridColumn_category,
            this.gridColumn_parking_category,
            this.gridColumn_storey,
            this.gridColumn_area,
            this.gridColumn_pick});
            this.gridView_outdoor_parking_space.GridControl = this.gridControl_outdoor_parking_space;
            this.gridView_outdoor_parking_space.Name = "gridView_outdoor_parking_space";
            this.gridView_outdoor_parking_space.OptionsView.ShowGroupPanel = false;
            this.gridView_outdoor_parking_space.RowClick += new DevExpress.XtraGrid.Views.Grid.RowClickEventHandler(this.gridView_outdoor_parking_space_RowClick);
            this.gridView_outdoor_parking_space.PopupMenuShowing += new DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventHandler(this.gridView_outdoor_parking_space_PopupMenuShowing);
            this.gridView_outdoor_parking_space.RowUpdated += new DevExpress.XtraGrid.Views.Base.RowObjectEventHandler(this.gridView_outdoor_parking_space_RowUpdated);
            this.gridView_outdoor_parking_space.CustomUnboundColumnData += new DevExpress.XtraGrid.Views.Base.CustomColumnDataEventHandler(this.gridView_outdoor_parking_space_CustomUnboundColumnData);
            this.gridView_outdoor_parking_space.CustomColumnDisplayText += new DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventHandler(this.gridView_outdoor_parking_space_CustomColumnDisplayText);
            this.gridView_outdoor_parking_space.DoubleClick += new System.EventHandler(this.gridView_outdoor_parking_space_DoubleClick);
            this.gridView_outdoor_parking_space.ValidatingEditor += new DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventHandler(this.gridView_outdoor_parking_space_ValidatingEditor);
            // 
            // gridColumn_number
            // 
            this.gridColumn_number.Caption = "序号";
            this.gridColumn_number.FieldName = "Number";
            this.gridColumn_number.Name = "gridColumn_number";
            this.gridColumn_number.OptionsColumn.AllowEdit = false;
            this.gridColumn_number.Visible = true;
            this.gridColumn_number.VisibleIndex = 0;
            // 
            // gridColumn_category
            // 
            this.gridColumn_category.Caption = "车场类型";
            this.gridColumn_category.FieldName = "Category";
            this.gridColumn_category.Name = "gridColumn_category";
            this.gridColumn_category.OptionsColumn.AllowEdit = false;
            this.gridColumn_category.Visible = true;
            this.gridColumn_category.VisibleIndex = 1;
            // 
            // gridColumn_parking_category
            // 
            this.gridColumn_parking_category.Caption = "停车类型";
            this.gridColumn_parking_category.FieldName = "ParkingCategory";
            this.gridColumn_parking_category.Name = "gridColumn_parking_category";
            this.gridColumn_parking_category.OptionsColumn.AllowEdit = false;
            this.gridColumn_parking_category.Visible = true;
            this.gridColumn_parking_category.VisibleIndex = 2;
            // 
            // gridColumn_storey
            // 
            this.gridColumn_storey.Caption = "车场/车位层数";
            this.gridColumn_storey.FieldName = "Storey";
            this.gridColumn_storey.Name = "gridColumn_storey";
            this.gridColumn_storey.Visible = true;
            this.gridColumn_storey.VisibleIndex = 3;
            // 
            // gridColumn_area
            // 
            this.gridColumn_area.Caption = "面积";
            this.gridColumn_area.FieldName = "Area";
            this.gridColumn_area.Name = "gridColumn_area";
            this.gridColumn_area.OptionsColumn.AllowEdit = false;
            this.gridColumn_area.Visible = true;
            this.gridColumn_area.VisibleIndex = 4;
            // 
            // gridColumn_pick
            // 
            this.gridColumn_pick.Caption = "选择";
            this.gridColumn_pick.ColumnEdit = this.repositoryItemHyperLinkEdit1;
            this.gridColumn_pick.FieldName = "gridColumn_pick";
            this.gridColumn_pick.Name = "gridColumn_pick";
            this.gridColumn_pick.OptionsColumn.AllowEdit = false;
            this.gridColumn_pick.UnboundType = DevExpress.Data.UnboundColumnType.String;
            this.gridColumn_pick.Visible = true;
            this.gridColumn_pick.VisibleIndex = 5;
            // 
            // repositoryItemHyperLinkEdit1
            // 
            this.repositoryItemHyperLinkEdit1.AutoHeight = false;
            this.repositoryItemHyperLinkEdit1.Name = "repositoryItemHyperLinkEdit1";
            // 
            // ThOutdoorParkingSpaceControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gridControl_outdoor_parking_space);
            this.Name = "ThOutdoorParkingSpaceControl";
            this.Size = new System.Drawing.Size(552, 346);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl_outdoor_parking_space)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView_outdoor_parking_space)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemHyperLinkEdit1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraGrid.GridControl gridControl_outdoor_parking_space;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView_outdoor_parking_space;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_number;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_category;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_parking_category;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_storey;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_area;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_pick;
        private DevExpress.XtraEditors.Repository.RepositoryItemHyperLinkEdit repositoryItemHyperLinkEdit1;
    }
}
