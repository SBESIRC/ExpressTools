﻿namespace ThAreaFrameConfig.WinForms
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
            this.gridView_roof = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.gridColumn_number = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn_Category = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repositoryItemComboBox_category = new DevExpress.XtraEditors.Repository.RepositoryItemComboBox();
            this.gridColumn_Coefficient = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repositoryItemComboBox_coefficient = new DevExpress.XtraEditors.Repository.RepositoryItemComboBox();
            this.gridColumn_FARCoefficient = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repositoryItemComboBox_farcoefficient = new DevExpress.XtraEditors.Repository.RepositoryItemComboBox();
            this.gridColumn_area = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn_pick = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repositoryItemHyperLinkEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemHyperLinkEdit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl_roof)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView_roof)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBox_category)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBox_coefficient)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBox_farcoefficient)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemHyperLinkEdit1)).BeginInit();
            this.SuspendLayout();
            // 
            // gridControl_roof
            // 
            this.gridControl_roof.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControl_roof.Location = new System.Drawing.Point(0, 0);
            this.gridControl_roof.MainView = this.gridView_roof;
            this.gridControl_roof.Name = "gridControl_roof";
            this.gridControl_roof.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemHyperLinkEdit1,
            this.repositoryItemComboBox_coefficient,
            this.repositoryItemComboBox_farcoefficient,
            this.repositoryItemComboBox_category});
            this.gridControl_roof.Size = new System.Drawing.Size(764, 562);
            this.gridControl_roof.TabIndex = 0;
            this.gridControl_roof.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView_roof});
            // 
            // gridView_roof
            // 
            this.gridView_roof.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.gridColumn_number,
            this.gridColumn_Category,
            this.gridColumn_Coefficient,
            this.gridColumn_FARCoefficient,
            this.gridColumn_area,
            this.gridColumn_pick});
            this.gridView_roof.GridControl = this.gridControl_roof;
            this.gridView_roof.Name = "gridView_roof";
            this.gridView_roof.OptionsBehavior.EditorShowMode = DevExpress.Utils.EditorShowMode.Click;
            this.gridView_roof.OptionsCustomization.AllowColumnMoving = false;
            this.gridView_roof.OptionsCustomization.AllowFilter = false;
            this.gridView_roof.OptionsCustomization.AllowGroup = false;
            this.gridView_roof.OptionsCustomization.AllowQuickHideColumns = false;
            this.gridView_roof.OptionsCustomization.AllowSort = false;
            this.gridView_roof.OptionsSelection.MultiSelect = true;
            this.gridView_roof.OptionsView.ShowGroupPanel = false;
            this.gridView_roof.RowClick += new DevExpress.XtraGrid.Views.Grid.RowClickEventHandler(this.gridView1_RowClick);
            this.gridView_roof.CustomRowCellEdit += new DevExpress.XtraGrid.Views.Grid.CustomRowCellEditEventHandler(this.gridView_roof_CustomRowCellEdit);
            this.gridView_roof.PopupMenuShowing += new DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventHandler(this.gridView_roof_PopupMenuShowing);
            this.gridView_roof.CellValueChanged += new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(this.gridView_roof_CellValueChanged);
            this.gridView_roof.CustomUnboundColumnData += new DevExpress.XtraGrid.Views.Base.CustomColumnDataEventHandler(this.gridView1_CustomUnboundColumnData);
            this.gridView_roof.CustomColumnDisplayText += new DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventHandler(this.gridView1_CustomColumnDisplayText);
            this.gridView_roof.DoubleClick += new System.EventHandler(this.gridView_roof_DoubleClick);
            this.gridView_roof.ValidatingEditor += new DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventHandler(this.gridView_roof_ValidatingEditor);
            // 
            // gridColumn_number
            // 
            this.gridColumn_number.Caption = "序号";
            this.gridColumn_number.FieldName = "Number";
            this.gridColumn_number.Name = "gridColumn_number";
            this.gridColumn_number.OptionsColumn.AllowEdit = false;
            this.gridColumn_number.OptionsColumn.AllowFocus = false;
            this.gridColumn_number.Visible = true;
            this.gridColumn_number.VisibleIndex = 0;
            // 
            // gridColumn_Category
            // 
            this.gridColumn_Category.Caption = "所属类型";
            this.gridColumn_Category.ColumnEdit = this.repositoryItemComboBox_category;
            this.gridColumn_Category.FieldName = "Category";
            this.gridColumn_Category.Name = "gridColumn_Category";
            this.gridColumn_Category.Visible = true;
            this.gridColumn_Category.VisibleIndex = 1;
            // 
            // repositoryItemComboBox_category
            // 
            this.repositoryItemComboBox_category.AutoHeight = false;
            this.repositoryItemComboBox_category.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.repositoryItemComboBox_category.Items.AddRange(new object[] {
            "住宅",
            "公建"});
            this.repositoryItemComboBox_category.Name = "repositoryItemComboBox_category";
            // 
            // gridColumn_Coefficient
            // 
            this.gridColumn_Coefficient.Caption = "计算系数";
            this.gridColumn_Coefficient.ColumnEdit = this.repositoryItemComboBox_coefficient;
            this.gridColumn_Coefficient.FieldName = "Coefficient";
            this.gridColumn_Coefficient.Name = "gridColumn_Coefficient";
            this.gridColumn_Coefficient.Visible = true;
            this.gridColumn_Coefficient.VisibleIndex = 2;
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
            // gridColumn_FARCoefficient
            // 
            this.gridColumn_FARCoefficient.Caption = "计容系数";
            this.gridColumn_FARCoefficient.ColumnEdit = this.repositoryItemComboBox_farcoefficient;
            this.gridColumn_FARCoefficient.FieldName = "FARCoefficient";
            this.gridColumn_FARCoefficient.Name = "gridColumn_FARCoefficient";
            this.gridColumn_FARCoefficient.Visible = true;
            this.gridColumn_FARCoefficient.VisibleIndex = 3;
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
            this.gridColumn_area.OptionsColumn.AllowEdit = false;
            this.gridColumn_area.OptionsColumn.AllowFocus = false;
            this.gridColumn_area.Visible = true;
            this.gridColumn_area.VisibleIndex = 4;
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
            this.gridColumn_pick.VisibleIndex = 5;
            // 
            // repositoryItemHyperLinkEdit1
            // 
            this.repositoryItemHyperLinkEdit1.AutoHeight = false;
            this.repositoryItemHyperLinkEdit1.LinkColor = System.Drawing.Color.Blue;
            this.repositoryItemHyperLinkEdit1.Name = "repositoryItemHyperLinkEdit1";
            // 
            // ThRoofControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gridControl_roof);
            this.Name = "ThRoofControl";
            this.Size = new System.Drawing.Size(764, 562);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl_roof)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView_roof)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBox_category)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBox_coefficient)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBox_farcoefficient)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemHyperLinkEdit1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraGrid.GridControl gridControl_roof;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView_roof;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_number;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_Coefficient;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_FARCoefficient;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_area;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_pick;
        private DevExpress.XtraEditors.Repository.RepositoryItemHyperLinkEdit repositoryItemHyperLinkEdit1;
        private DevExpress.XtraEditors.Repository.RepositoryItemComboBox repositoryItemComboBox_coefficient;
        private DevExpress.XtraEditors.Repository.RepositoryItemComboBox repositoryItemComboBox_farcoefficient;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_Category;
        private DevExpress.XtraEditors.Repository.RepositoryItemComboBox repositoryItemComboBox_category;
    }
}
