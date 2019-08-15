﻿namespace ThAreaFrameConfig.WinForms
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
            this.gridColumn_area = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn_floors = new DevExpress.XtraGrid.Columns.GridColumn();
            ((System.ComponentModel.ISupportInitialize)(this.xtraTabControl1)).BeginInit();
            this.xtraTabControl1.SuspendLayout();
            this.xtraTabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl_aoccupancy)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView_aoccupancy)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBox_component)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBox_category)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBox_coefficient)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBox_farcoefficient)).BeginInit();
            this.SuspendLayout();
            // 
            // xtraTabControl1
            // 
            this.xtraTabControl1.Location = new System.Drawing.Point(4, 4);
            this.xtraTabControl1.Name = "xtraTabControl1";
            this.xtraTabControl1.SelectedTabPage = this.xtraTabPage1;
            this.xtraTabControl1.Size = new System.Drawing.Size(663, 470);
            this.xtraTabControl1.TabIndex = 0;
            this.xtraTabControl1.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] {
            this.xtraTabPage1});
            this.xtraTabControl1.SelectedPageChanged += new DevExpress.XtraTab.TabPageChangedEventHandler(this.xtraTabControl1_SelectedPageChanged);
            // 
            // xtraTabPage1
            // 
            this.xtraTabPage1.Controls.Add(this.gridControl_aoccupancy);
            this.xtraTabPage1.Name = "xtraTabPage1";
            this.xtraTabPage1.Size = new System.Drawing.Size(657, 441);
            this.xtraTabPage1.Text = "xtraTabPage1";
            // 
            // gridControl_aoccupancy
            // 
            this.gridControl_aoccupancy.Location = new System.Drawing.Point(4, 0);
            this.gridControl_aoccupancy.MainView = this.gridView_aoccupancy;
            this.gridControl_aoccupancy.Name = "gridControl_aoccupancy";
            this.gridControl_aoccupancy.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemComboBox_component,
            this.repositoryItemComboBox_category,
            this.repositoryItemComboBox_coefficient,
            this.repositoryItemComboBox_farcoefficient});
            this.gridControl_aoccupancy.Size = new System.Drawing.Size(650, 438);
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
            this.gridColumn_area});
            this.gridView_aoccupancy.GridControl = this.gridControl_aoccupancy;
            this.gridView_aoccupancy.Name = "gridView_aoccupancy";
            this.gridView_aoccupancy.CustomColumnDisplayText += new DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventHandler(this.gridView_aoccupancy_CustomColumnDisplayText);
            // 
            // gridColumn_number
            // 
            this.gridColumn_number.Caption = "序号";
            this.gridColumn_number.FieldName = "Number";
            this.gridColumn_number.Name = "gridColumn_number";
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
            // gridColumn_area
            // 
            this.gridColumn_area.Caption = "面积";
            this.gridColumn_area.FieldName = "Area";
            this.gridColumn_area.Name = "gridColumn_area";
            this.gridColumn_area.Visible = true;
            this.gridColumn_area.VisibleIndex = 5;
            // 
            // gridColumn_floors
            // 
            this.gridColumn_floors.Caption = "车位层数";
            this.gridColumn_floors.Name = "gridColumn_floors";
            this.gridColumn_floors.Visible = true;
            this.gridColumn_floors.VisibleIndex = 6;
            // 
            // ThAOccupancyControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.xtraTabControl1);
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
            this.ResumeLayout(false);

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
    }
}
