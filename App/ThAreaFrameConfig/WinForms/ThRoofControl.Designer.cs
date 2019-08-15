namespace ThAreaFrameConfig.WinForms
{
    partial class ThRoofControl
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
            this.gridControl_roof = new DevExpress.XtraGrid.GridControl();
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.gridColumn_number = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn_Coefficient = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn_FARCoefficient = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn_area = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn_pick = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repositoryItemHyperLinkEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemHyperLinkEdit();
            this.repositoryItemComboBox_coefficient = new DevExpress.XtraEditors.Repository.RepositoryItemComboBox();
            this.repositoryItemComboBox_farcoefficient = new DevExpress.XtraEditors.Repository.RepositoryItemComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl_roof)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemHyperLinkEdit1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBox_coefficient)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBox_farcoefficient)).BeginInit();
            this.SuspendLayout();
            // 
            // gridControl_roof
            // 
            this.gridControl_roof.Location = new System.Drawing.Point(0, 3);
            this.gridControl_roof.MainView = this.gridView1;
            this.gridControl_roof.Name = "gridControl_roof";
            this.gridControl_roof.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemHyperLinkEdit1,
            this.repositoryItemComboBox_coefficient,
            this.repositoryItemComboBox_farcoefficient});
            this.gridControl_roof.Size = new System.Drawing.Size(764, 556);
            this.gridControl_roof.TabIndex = 0;
            this.gridControl_roof.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
            // 
            // gridView1
            // 
            this.gridView1.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.gridColumn_number,
            this.gridColumn_Coefficient,
            this.gridColumn_FARCoefficient,
            this.gridColumn_area,
            this.gridColumn_pick});
            this.gridView1.GridControl = this.gridControl_roof;
            this.gridView1.Name = "gridView1";
            this.gridView1.RowClick += new DevExpress.XtraGrid.Views.Grid.RowClickEventHandler(this.gridView1_RowClick);
            this.gridView1.CustomUnboundColumnData += new DevExpress.XtraGrid.Views.Base.CustomColumnDataEventHandler(this.gridView1_CustomUnboundColumnData);
            this.gridView1.CustomColumnDisplayText += new DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventHandler(this.gridView1_CustomColumnDisplayText);
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
            // gridColumn_Coefficient
            // 
            this.gridColumn_Coefficient.Caption = "计算系数";
            this.gridColumn_Coefficient.ColumnEdit = this.repositoryItemComboBox_coefficient;
            this.gridColumn_Coefficient.FieldName = "Coefficient";
            this.gridColumn_Coefficient.Name = "gridColumn_Coefficient";
            this.gridColumn_Coefficient.Visible = true;
            this.gridColumn_Coefficient.VisibleIndex = 1;
            // 
            // gridColumn_FARCoefficient
            // 
            this.gridColumn_FARCoefficient.Caption = "计容系数";
            this.gridColumn_FARCoefficient.ColumnEdit = this.repositoryItemComboBox_farcoefficient;
            this.gridColumn_FARCoefficient.FieldName = "FARCoefficient";
            this.gridColumn_FARCoefficient.Name = "gridColumn_FARCoefficient";
            this.gridColumn_FARCoefficient.Visible = true;
            this.gridColumn_FARCoefficient.VisibleIndex = 2;
            // 
            // gridColumn_area
            // 
            this.gridColumn_area.Caption = "面积";
            this.gridColumn_area.FieldName = "Area";
            this.gridColumn_area.Name = "gridColumn_area";
            this.gridColumn_area.OptionsColumn.AllowEdit = false;
            this.gridColumn_area.Visible = true;
            this.gridColumn_area.VisibleIndex = 3;
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
            this.gridColumn_pick.VisibleIndex = 4;
            // 
            // repositoryItemHyperLinkEdit1
            // 
            this.repositoryItemHyperLinkEdit1.AutoHeight = false;
            this.repositoryItemHyperLinkEdit1.Name = "repositoryItemHyperLinkEdit1";
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
            // ThRoofControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gridControl_roof);
            this.Name = "ThRoofControl";
            this.Size = new System.Drawing.Size(764, 562);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl_roof)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemHyperLinkEdit1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBox_coefficient)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBox_farcoefficient)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraGrid.GridControl gridControl_roof;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_number;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_Coefficient;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_FARCoefficient;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_area;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_pick;
        private DevExpress.XtraEditors.Repository.RepositoryItemHyperLinkEdit repositoryItemHyperLinkEdit1;
        private DevExpress.XtraEditors.Repository.RepositoryItemComboBox repositoryItemComboBox_coefficient;
        private DevExpress.XtraEditors.Repository.RepositoryItemComboBox repositoryItemComboBox_farcoefficient;
    }
}
