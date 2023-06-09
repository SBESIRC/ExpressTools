﻿namespace ThAreaFrameConfig.WinForms
{
    partial class ThUnderGroundParkingControl
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
            this.gridControl_parking = new DevExpress.XtraGrid.GridControl();
            this.gridView_parking = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.gridColumn_number = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn_category = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repositoryItemComboBox_category = new DevExpress.XtraEditors.Repository.RepositoryItemComboBox();
            this.gridColumn_Floor = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn_storey = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn_slot = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn_pick = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repositoryItemHyperLinkEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemHyperLinkEdit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl_parking)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView_parking)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBox_category)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemHyperLinkEdit1)).BeginInit();
            this.SuspendLayout();
            // 
            // gridControl_parking
            // 
            this.gridControl_parking.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControl_parking.Location = new System.Drawing.Point(0, 0);
            this.gridControl_parking.MainView = this.gridView_parking;
            this.gridControl_parking.Name = "gridControl_parking";
            this.gridControl_parking.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemHyperLinkEdit1,
            this.repositoryItemComboBox_category});
            this.gridControl_parking.Size = new System.Drawing.Size(775, 586);
            this.gridControl_parking.TabIndex = 0;
            this.gridControl_parking.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView_parking});
            // 
            // gridView_parking
            // 
            this.gridView_parking.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.gridColumn_number,
            this.gridColumn_category,
            this.gridColumn_Floor,
            this.gridColumn_storey,
            this.gridColumn_slot,
            this.gridColumn_pick});
            this.gridView_parking.GridControl = this.gridControl_parking;
            this.gridView_parking.Name = "gridView_parking";
            this.gridView_parking.OptionsCustomization.AllowColumnMoving = false;
            this.gridView_parking.OptionsCustomization.AllowFilter = false;
            this.gridView_parking.OptionsCustomization.AllowGroup = false;
            this.gridView_parking.OptionsCustomization.AllowQuickHideColumns = false;
            this.gridView_parking.OptionsCustomization.AllowSort = false;
            this.gridView_parking.OptionsDetail.EnableMasterViewMode = false;
            this.gridView_parking.OptionsSelection.MultiSelect = true;
            this.gridView_parking.OptionsView.ShowGroupPanel = false;
            this.gridView_parking.RowClick += new DevExpress.XtraGrid.Views.Grid.RowClickEventHandler(this.gridView1_RowClick);
            this.gridView_parking.CustomRowCellEdit += new DevExpress.XtraGrid.Views.Grid.CustomRowCellEditEventHandler(this.gridView_parking_CustomRowCellEdit);
            this.gridView_parking.PopupMenuShowing += new DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventHandler(this.gridView_parking_PopupMenuShowing);
            this.gridView_parking.CellValueChanged += new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(this.gridView_parking_CellValueChanged);
            this.gridView_parking.CustomUnboundColumnData += new DevExpress.XtraGrid.Views.Base.CustomColumnDataEventHandler(this.gridView1_CustomUnboundColumnData);
            this.gridView_parking.DoubleClick += new System.EventHandler(this.gridView_parking_DoubleClick);
            this.gridView_parking.ValidatingEditor += new DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventHandler(this.gridView_parking_ValidatingEditor);
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
            // gridColumn_category
            // 
            this.gridColumn_category.Caption = "停车类型";
            this.gridColumn_category.ColumnEdit = this.repositoryItemComboBox_category;
            this.gridColumn_category.FieldName = "Category";
            this.gridColumn_category.Name = "gridColumn_category";
            this.gridColumn_category.OptionsColumn.AllowEdit = false;
            this.gridColumn_category.OptionsColumn.AllowFocus = false;
            this.gridColumn_category.Visible = true;
            this.gridColumn_category.VisibleIndex = 1;
            // 
            // repositoryItemComboBox_category
            // 
            this.repositoryItemComboBox_category.AutoHeight = false;
            this.repositoryItemComboBox_category.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.repositoryItemComboBox_category.Items.AddRange(new object[] {
            "小型汽车",
            "微型汽车",
            "中型汽车",
            "大型汽车",
            "铰接车"});
            this.repositoryItemComboBox_category.Name = "repositoryItemComboBox_category";
            this.repositoryItemComboBox_category.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            // 
            // gridColumn_Floor
            // 
            this.gridColumn_Floor.Caption = "车场层数";
            this.gridColumn_Floor.FieldName = "Floors";
            this.gridColumn_Floor.Name = "gridColumn_Floor";
            this.gridColumn_Floor.Visible = true;
            this.gridColumn_Floor.VisibleIndex = 2;
            // 
            // gridColumn_storey
            // 
            this.gridColumn_storey.Caption = "所属层";
            this.gridColumn_storey.FieldName = "Storey";
            this.gridColumn_storey.Name = "gridColumn_storey";
            this.gridColumn_storey.Visible = true;
            this.gridColumn_storey.VisibleIndex = 3;
            // 
            // gridColumn_slot
            // 
            this.gridColumn_slot.Caption = "室内车位数";
            this.gridColumn_slot.FieldName = "Slots";
            this.gridColumn_slot.Name = "gridColumn_slot";
            this.gridColumn_slot.OptionsColumn.AllowEdit = false;
            this.gridColumn_slot.OptionsColumn.AllowFocus = false;
            this.gridColumn_slot.Visible = true;
            this.gridColumn_slot.VisibleIndex = 4;
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
            // ThUnderGroundParkingControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gridControl_parking);
            this.Name = "ThUnderGroundParkingControl";
            this.Size = new System.Drawing.Size(775, 586);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl_parking)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView_parking)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBox_category)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemHyperLinkEdit1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraGrid.GridControl gridControl_parking;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView_parking;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_number;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_category;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_Floor;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_storey;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_slot;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_pick;
        private DevExpress.XtraEditors.Repository.RepositoryItemHyperLinkEdit repositoryItemHyperLinkEdit1;
        private DevExpress.XtraEditors.Repository.RepositoryItemComboBox repositoryItemComboBox_category;
    }
}
