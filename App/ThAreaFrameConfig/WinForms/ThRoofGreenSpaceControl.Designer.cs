namespace ThAreaFrameConfig.WinForms
{
    partial class ThRoofGreenSpaceControl
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
            this.gridControl_space = new DevExpress.XtraGrid.GridControl();
            this.gridView_space = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.gridColumn_number = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn_coefficient = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repositoryItemComboBox_coefficient = new DevExpress.XtraEditors.Repository.RepositoryItemComboBox();
            this.gridColumn_area = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn_pick = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repositoryItemHyperLinkEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemHyperLinkEdit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl_space)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView_space)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBox_coefficient)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemHyperLinkEdit1)).BeginInit();
            this.SuspendLayout();
            // 
            // gridControl_space
            // 
            this.gridControl_space.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControl_space.Location = new System.Drawing.Point(0, 0);
            this.gridControl_space.MainView = this.gridView_space;
            this.gridControl_space.Name = "gridControl_space";
            this.gridControl_space.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemHyperLinkEdit1,
            this.repositoryItemComboBox_coefficient});
            this.gridControl_space.Size = new System.Drawing.Size(689, 509);
            this.gridControl_space.TabIndex = 0;
            this.gridControl_space.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView_space});
            // 
            // gridView_space
            // 
            this.gridView_space.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.gridColumn_number,
            this.gridColumn_coefficient,
            this.gridColumn_area,
            this.gridColumn_pick});
            this.gridView_space.GridControl = this.gridControl_space;
            this.gridView_space.Name = "gridView_space";
            this.gridView_space.OptionsBehavior.EditorShowMode = DevExpress.Utils.EditorShowMode.Click;
            this.gridView_space.OptionsView.ShowGroupPanel = false;
            this.gridView_space.RowClick += new DevExpress.XtraGrid.Views.Grid.RowClickEventHandler(this.gridView1_RowClick);
            this.gridView_space.CustomRowCellEdit += new DevExpress.XtraGrid.Views.Grid.CustomRowCellEditEventHandler(this.gridView_space_CustomRowCellEdit);
            this.gridView_space.PopupMenuShowing += new DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventHandler(this.gridView_space_PopupMenuShowing);
            this.gridView_space.RowUpdated += new DevExpress.XtraGrid.Views.Base.RowObjectEventHandler(this.gridView_space_RowUpdated);
            this.gridView_space.CustomUnboundColumnData += new DevExpress.XtraGrid.Views.Base.CustomColumnDataEventHandler(this.gridView1_CustomUnboundColumnData);
            this.gridView_space.CustomColumnDisplayText += new DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventHandler(this.gridView1_CustomColumnDisplayText);
            this.gridView_space.DoubleClick += new System.EventHandler(this.gridView_space_DoubleClick);
            this.gridView_space.ValidatingEditor += new DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventHandler(this.gridView_space_ValidatingEditor);
            // 
            // gridColumn_number
            // 
            this.gridColumn_number.Caption = "编号";
            this.gridColumn_number.FieldName = "Number";
            this.gridColumn_number.Name = "gridColumn_number";
            this.gridColumn_number.OptionsColumn.AllowEdit = false;
            this.gridColumn_number.OptionsColumn.AllowFocus = false;
            this.gridColumn_number.Visible = true;
            this.gridColumn_number.VisibleIndex = 0;
            // 
            // gridColumn_coefficient
            // 
            this.gridColumn_coefficient.Caption = "折算系数";
            this.gridColumn_coefficient.ColumnEdit = this.repositoryItemComboBox_coefficient;
            this.gridColumn_coefficient.FieldName = "Coefficient";
            this.gridColumn_coefficient.Name = "gridColumn_coefficient";
            this.gridColumn_coefficient.Visible = true;
            this.gridColumn_coefficient.VisibleIndex = 1;
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
            // gridColumn_area
            // 
            this.gridColumn_area.Caption = "面积";
            this.gridColumn_area.FieldName = "Area";
            this.gridColumn_area.Name = "gridColumn_area";
            this.gridColumn_area.OptionsColumn.AllowEdit = false;
            this.gridColumn_area.OptionsColumn.AllowFocus = false;
            this.gridColumn_area.Visible = true;
            this.gridColumn_area.VisibleIndex = 2;
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
            this.gridColumn_pick.VisibleIndex = 3;
            // 
            // repositoryItemHyperLinkEdit1
            // 
            this.repositoryItemHyperLinkEdit1.AutoHeight = false;
            this.repositoryItemHyperLinkEdit1.LinkColor = System.Drawing.Color.Blue;
            this.repositoryItemHyperLinkEdit1.Name = "repositoryItemHyperLinkEdit1";
            // 
            // ThRoofGreenSpaceControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gridControl_space);
            this.Name = "ThRoofGreenSpaceControl";
            this.Size = new System.Drawing.Size(689, 509);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl_space)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView_space)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBox_coefficient)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemHyperLinkEdit1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraGrid.GridControl gridControl_space;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView_space;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_number;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_coefficient;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_area;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_pick;
        private DevExpress.XtraEditors.Repository.RepositoryItemHyperLinkEdit repositoryItemHyperLinkEdit1;
        private DevExpress.XtraEditors.Repository.RepositoryItemComboBox repositoryItemComboBox_coefficient;
    }
}
