namespace ThAreaFrameConfig.WinForms
{
    partial class ThPlotAreaControl
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
            this.gridView_plot_area = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.gridColumn_number = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn_household = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn_area = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn_pick = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repositoryItemHyperLinkEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemHyperLinkEdit();
            this.gridControl_plot_area = new DevExpress.XtraGrid.GridControl();
            ((System.ComponentModel.ISupportInitialize)(this.gridView_plot_area)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemHyperLinkEdit1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl_plot_area)).BeginInit();
            this.SuspendLayout();
            // 
            // gridView_plot_area
            // 
            this.gridView_plot_area.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.gridColumn_number,
            this.gridColumn_household,
            this.gridColumn_area,
            this.gridColumn_pick});
            this.gridView_plot_area.GridControl = this.gridControl_plot_area;
            this.gridView_plot_area.Name = "gridView_plot_area";
            this.gridView_plot_area.OptionsView.ShowGroupPanel = false;
            this.gridView_plot_area.RowClick += new DevExpress.XtraGrid.Views.Grid.RowClickEventHandler(this.gridView_plot_area_RowClick);
            this.gridView_plot_area.PopupMenuShowing += new DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventHandler(this.gridView_plot_area_PopupMenuShowing);
            this.gridView_plot_area.RowUpdated += new DevExpress.XtraGrid.Views.Base.RowObjectEventHandler(this.gridView_plot_area_RowUpdated);
            this.gridView_plot_area.CustomUnboundColumnData += new DevExpress.XtraGrid.Views.Base.CustomColumnDataEventHandler(this.gridView_plot_area_CustomUnboundColumnData);
            this.gridView_plot_area.CustomColumnDisplayText += new DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventHandler(this.gridView_plot_area_CustomColumnDisplayText);
            this.gridView_plot_area.DoubleClick += new System.EventHandler(this.gridView_plot_area_DoubleClick);
            this.gridView_plot_area.ValidatingEditor += new DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventHandler(this.gridView_plot_area_ValidatingEditor);
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
            // gridColumn_household
            // 
            this.gridColumn_household.Caption = "输入户数";
            this.gridColumn_household.FieldName = "HouseHold";
            this.gridColumn_household.Name = "gridColumn_household";
            this.gridColumn_household.Visible = true;
            this.gridColumn_household.VisibleIndex = 1;
            // 
            // gridColumn_area
            // 
            this.gridColumn_area.Caption = "面积";
            this.gridColumn_area.FieldName = "Area";
            this.gridColumn_area.Name = "gridColumn_area";
            this.gridColumn_area.OptionsColumn.AllowEdit = false;
            this.gridColumn_area.Visible = true;
            this.gridColumn_area.VisibleIndex = 2;
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
            this.gridColumn_pick.VisibleIndex = 3;
            // 
            // repositoryItemHyperLinkEdit1
            // 
            this.repositoryItemHyperLinkEdit1.AutoHeight = false;
            this.repositoryItemHyperLinkEdit1.Name = "repositoryItemHyperLinkEdit1";
            // 
            // gridControl_plot_area
            // 
            this.gridControl_plot_area.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControl_plot_area.Location = new System.Drawing.Point(0, 0);
            this.gridControl_plot_area.MainView = this.gridView_plot_area;
            this.gridControl_plot_area.Name = "gridControl_plot_area";
            this.gridControl_plot_area.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemHyperLinkEdit1});
            this.gridControl_plot_area.Size = new System.Drawing.Size(549, 342);
            this.gridControl_plot_area.TabIndex = 0;
            this.gridControl_plot_area.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView_plot_area});
            // 
            // ThPlotAreaControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gridControl_plot_area);
            this.Name = "ThPlotAreaControl";
            this.Size = new System.Drawing.Size(549, 342);
            ((System.ComponentModel.ISupportInitialize)(this.gridView_plot_area)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemHyperLinkEdit1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl_plot_area)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraGrid.Views.Grid.GridView gridView_plot_area;
        private DevExpress.XtraGrid.GridControl gridControl_plot_area;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_number;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_household;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_area;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_pick;
        private DevExpress.XtraEditors.Repository.RepositoryItemHyperLinkEdit repositoryItemHyperLinkEdit1;
    }
}
