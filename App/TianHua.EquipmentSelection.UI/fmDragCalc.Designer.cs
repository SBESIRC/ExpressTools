namespace TianHua.FanSelection.UI
{
    partial class fmDragCalc
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.layoutControl1 = new DevExpress.XtraLayout.LayoutControl();
            this.BtnOK = new DevExpress.XtraEditors.SimpleButton();
            this.Gdc = new DevExpress.XtraGrid.GridControl();
            this.Gdv = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridView();
            this.gridBand2 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
            this.ColDuctLength = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.TxtDuctLength = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
            this.gridBand3 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
            this.ColFriction = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.TxtFriction = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
            this.gridBand4 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
            this.ColLocRes = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.TxtLocRes = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
            this.gridBand5 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
            this.ColDuctResistance = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.gridBand6 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
            this.ColDamper = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.TxtDamper = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
            this.gridBand7 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
            this.ColDynPress = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.TxtDynPress = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
            this.gridBand8 = new DevExpress.XtraGrid.Views.BandedGrid.GridBand();
            this.ColWindResis = new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn();
            this.layoutControlGroup1 = new DevExpress.XtraLayout.LayoutControlGroup();
            this.layoutControlItem1 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem2 = new DevExpress.XtraLayout.LayoutControlItem();
            this.emptySpaceItem1 = new DevExpress.XtraLayout.EmptySpaceItem();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).BeginInit();
            this.layoutControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Gdc)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Gdv)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TxtDuctLength)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TxtFriction)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TxtLocRes)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TxtDamper)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TxtDynPress)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem1)).BeginInit();
            this.SuspendLayout();
            // 
            // layoutControl1
            // 
            this.layoutControl1.Controls.Add(this.BtnOK);
            this.layoutControl1.Controls.Add(this.Gdc);
            this.layoutControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutControl1.Location = new System.Drawing.Point(0, 0);
            this.layoutControl1.Name = "layoutControl1";
            this.layoutControl1.OptionsCustomizationForm.DesignTimeCustomizationFormPositionAndSize = new System.Drawing.Rectangle(486, 429, 650, 400);
            this.layoutControl1.Root = this.layoutControlGroup1;
            this.layoutControl1.Size = new System.Drawing.Size(631, 146);
            this.layoutControl1.TabIndex = 0;
            this.layoutControl1.Text = "layoutControl1";
            // 
            // BtnOK
            // 
            this.BtnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.BtnOK.Location = new System.Drawing.Point(527, 112);
            this.BtnOK.Name = "BtnOK";
            this.BtnOK.Size = new System.Drawing.Size(92, 22);
            this.BtnOK.StyleController = this.layoutControl1;
            this.BtnOK.TabIndex = 32;
            this.BtnOK.Text = "确定";
            // 
            // Gdc
            // 
            this.Gdc.EmbeddedNavigator.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.Gdc.Location = new System.Drawing.Point(12, 12);
            this.Gdc.LookAndFeel.SkinName = "Office 2013";
            this.Gdc.MainView = this.Gdv;
            this.Gdc.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.Gdc.Name = "Gdc";
            this.Gdc.Padding = new System.Windows.Forms.Padding(8);
            this.Gdc.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.TxtDuctLength,
            this.TxtFriction,
            this.TxtLocRes,
            this.TxtDamper,
            this.TxtDynPress});
            this.Gdc.Size = new System.Drawing.Size(607, 96);
            this.Gdc.TabIndex = 31;
            this.Gdc.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.Gdv});
            // 
            // Gdv
            // 
            this.Gdv.Appearance.FocusedCell.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(102)))), ((int)(((byte)(124)))));
            this.Gdv.Appearance.FocusedCell.Options.UseBackColor = true;
            this.Gdv.Appearance.FocusedRow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(102)))), ((int)(((byte)(124)))));
            this.Gdv.Appearance.FocusedRow.Options.UseBackColor = true;
            this.Gdv.Appearance.SelectedRow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(102)))), ((int)(((byte)(124)))));
            this.Gdv.Appearance.SelectedRow.Options.UseBackColor = true;
            this.Gdv.Bands.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.GridBand[] {
            this.gridBand2,
            this.gridBand3,
            this.gridBand4,
            this.gridBand5,
            this.gridBand6,
            this.gridBand7,
            this.gridBand8});
            this.Gdv.Columns.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn[] {
            this.ColDuctLength,
            this.ColFriction,
            this.ColLocRes,
            this.ColDuctResistance,
            this.ColDamper,
            this.ColDynPress,
            this.ColWindResis});
            this.Gdv.GridControl = this.Gdc;
            this.Gdv.Name = "Gdv";
            this.Gdv.OptionsCustomization.AllowBandMoving = false;
            this.Gdv.OptionsCustomization.AllowBandResizing = false;
            this.Gdv.OptionsCustomization.AllowFilter = false;
            this.Gdv.OptionsCustomization.AllowGroup = false;
            this.Gdv.OptionsCustomization.AllowQuickHideColumns = false;
            this.Gdv.OptionsCustomization.ShowBandsInCustomizationForm = false;
            this.Gdv.OptionsDetail.AllowZoomDetail = false;
            this.Gdv.OptionsDetail.ShowDetailTabs = false;
            this.Gdv.OptionsMenu.EnableColumnMenu = false;
            this.Gdv.OptionsMenu.EnableFooterMenu = false;
            this.Gdv.OptionsMenu.EnableGroupPanelMenu = false;
            this.Gdv.OptionsSelection.EnableAppearanceFocusedRow = false;
            this.Gdv.OptionsSelection.MultiSelect = true;
            this.Gdv.OptionsView.BestFitUseErrorInfo = DevExpress.Utils.DefaultBoolean.True;
            this.Gdv.OptionsView.HeaderFilterButtonShowMode = DevExpress.XtraEditors.Controls.FilterButtonShowMode.Button;
            this.Gdv.OptionsView.ShowColumnHeaders = false;
            this.Gdv.OptionsView.ShowDetailButtons = false;
            this.Gdv.OptionsView.ShowFilterPanelMode = DevExpress.XtraGrid.Views.Base.ShowFilterPanelMode.Never;
            this.Gdv.OptionsView.ShowGroupExpandCollapseButtons = false;
            this.Gdv.OptionsView.ShowGroupPanel = false;
            this.Gdv.OptionsView.ShowIndicator = false;
            this.Gdv.RowHeight = 23;
            this.Gdv.CellValueChanged += new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(this.Gdv_CellValueChanged);
            this.Gdv.CustomColumnDisplayText += new DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventHandler(this.Gdv_CustomColumnDisplayText);
            // 
            // gridBand2
            // 
            this.gridBand2.AppearanceHeader.Options.UseTextOptions = true;
            this.gridBand2.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.gridBand2.Caption = "风管长\r\n（m）";
            this.gridBand2.Columns.Add(this.ColDuctLength);
            this.gridBand2.Name = "gridBand2";
            this.gridBand2.VisibleIndex = 0;
            this.gridBand2.Width = 66;
            // 
            // ColDuctLength
            // 
            this.ColDuctLength.AppearanceCell.Options.UseTextOptions = true;
            this.ColDuctLength.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.ColDuctLength.Caption = "风管长度";
            this.ColDuctLength.ColumnEdit = this.TxtDuctLength;
            this.ColDuctLength.FieldName = "DuctLength";
            this.ColDuctLength.Name = "ColDuctLength";
            this.ColDuctLength.Visible = true;
            this.ColDuctLength.Width = 66;
            // 
            // TxtDuctLength
            // 
            this.TxtDuctLength.AutoHeight = false;
            this.TxtDuctLength.Mask.EditMask = "([0-9]{1,})";
            this.TxtDuctLength.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.RegEx;
            this.TxtDuctLength.Name = "TxtDuctLength";
            // 
            // gridBand3
            // 
            this.gridBand3.AppearanceHeader.Options.UseTextOptions = true;
            this.gridBand3.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.gridBand3.Caption = "比摩阻1.0-3.0\r\n（Pa/m）";
            this.gridBand3.Columns.Add(this.ColFriction);
            this.gridBand3.Name = "gridBand3";
            this.gridBand3.VisibleIndex = 1;
            this.gridBand3.Width = 100;
            // 
            // ColFriction
            // 
            this.ColFriction.AppearanceCell.Options.UseTextOptions = true;
            this.ColFriction.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.ColFriction.Caption = "比摩阻";
            this.ColFriction.ColumnEdit = this.TxtFriction;
            this.ColFriction.FieldName = "Friction";
            this.ColFriction.Name = "ColFriction";
            this.ColFriction.Visible = true;
            this.ColFriction.Width = 100;
            // 
            // TxtFriction
            // 
            this.TxtFriction.AutoHeight = false;
            this.TxtFriction.Mask.EditMask = "\\d*\\.{0,1}\\d{0,1}";
            this.TxtFriction.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.RegEx;
            this.TxtFriction.Name = "TxtFriction";
            // 
            // gridBand4
            // 
            this.gridBand4.AppearanceHeader.Options.UseTextOptions = true;
            this.gridBand4.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.gridBand4.Caption = "局部阻力倍数\r\n1-5";
            this.gridBand4.Columns.Add(this.ColLocRes);
            this.gridBand4.Name = "gridBand4";
            this.gridBand4.VisibleIndex = 2;
            this.gridBand4.Width = 90;
            // 
            // ColLocRes
            // 
            this.ColLocRes.AppearanceCell.Options.UseTextOptions = true;
            this.ColLocRes.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.ColLocRes.Caption = "局部阻力倍数";
            this.ColLocRes.ColumnEdit = this.TxtLocRes;
            this.ColLocRes.FieldName = "LocRes";
            this.ColLocRes.Name = "ColLocRes";
            this.ColLocRes.Visible = true;
            this.ColLocRes.Width = 90;
            // 
            // TxtLocRes
            // 
            this.TxtLocRes.AutoHeight = false;
            this.TxtLocRes.Mask.EditMask = "\\d*\\.{0,1}\\d{0,1}";
            this.TxtLocRes.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.RegEx;
            this.TxtLocRes.Name = "TxtLocRes";
            // 
            // gridBand5
            // 
            this.gridBand5.AppearanceHeader.Options.UseTextOptions = true;
            this.gridBand5.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.gridBand5.Caption = "风管阻力\r\n（Pa）";
            this.gridBand5.Columns.Add(this.ColDuctResistance);
            this.gridBand5.Name = "gridBand5";
            this.gridBand5.VisibleIndex = 3;
            this.gridBand5.Width = 71;
            // 
            // ColDuctResistance
            // 
            this.ColDuctResistance.AppearanceCell.Options.UseTextOptions = true;
            this.ColDuctResistance.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.ColDuctResistance.Caption = "风管阻力";
            this.ColDuctResistance.FieldName = "DuctResistance";
            this.ColDuctResistance.Name = "ColDuctResistance";
            this.ColDuctResistance.OptionsColumn.AllowEdit = false;
            this.ColDuctResistance.Visible = true;
            this.ColDuctResistance.Width = 71;
            // 
            // gridBand6
            // 
            this.gridBand6.AppearanceHeader.Options.UseTextOptions = true;
            this.gridBand6.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.gridBand6.Caption = "消声器阻力\r\n20-70Pa/只（Pa）";
            this.gridBand6.Columns.Add(this.ColDamper);
            this.gridBand6.Name = "gridBand6";
            this.gridBand6.VisibleIndex = 4;
            this.gridBand6.Width = 131;
            // 
            // ColDamper
            // 
            this.ColDamper.AppearanceCell.Options.UseTextOptions = true;
            this.ColDamper.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.ColDamper.Caption = "消声器阻力";
            this.ColDamper.ColumnEdit = this.TxtDamper;
            this.ColDamper.FieldName = "Damper";
            this.ColDamper.Name = "ColDamper";
            this.ColDamper.Visible = true;
            this.ColDamper.Width = 131;
            // 
            // TxtDamper
            // 
            this.TxtDamper.AutoHeight = false;
            this.TxtDamper.Mask.EditMask = "([0-9]{1,})";
            this.TxtDamper.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.RegEx;
            this.TxtDamper.Name = "TxtDamper";
            // 
            // gridBand7
            // 
            this.gridBand7.AppearanceHeader.Options.UseTextOptions = true;
            this.gridBand7.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.gridBand7.Caption = "动压\r\n40-90Pa";
            this.gridBand7.Columns.Add(this.ColDynPress);
            this.gridBand7.Name = "gridBand7";
            this.gridBand7.VisibleIndex = 5;
            // 
            // ColDynPress
            // 
            this.ColDynPress.AppearanceCell.Options.UseTextOptions = true;
            this.ColDynPress.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.ColDynPress.Caption = "动压";
            this.ColDynPress.ColumnEdit = this.TxtDynPress;
            this.ColDynPress.FieldName = "DynPress";
            this.ColDynPress.Name = "ColDynPress";
            this.ColDynPress.Visible = true;
            this.ColDynPress.Width = 70;
            // 
            // TxtDynPress
            // 
            this.TxtDynPress.AutoHeight = false;
            this.TxtDynPress.Mask.EditMask = "([0-9]{1,})";
            this.TxtDynPress.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.RegEx;
            this.TxtDynPress.Name = "TxtDynPress";
            // 
            // gridBand8
            // 
            this.gridBand8.AppearanceHeader.Options.UseTextOptions = true;
            this.gridBand8.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.gridBand8.Caption = "总阻力\r\n（Pa）";
            this.gridBand8.Columns.Add(this.ColWindResis);
            this.gridBand8.Name = "gridBand8";
            this.gridBand8.RowCount = 2;
            this.gridBand8.VisibleIndex = 6;
            this.gridBand8.Width = 77;
            // 
            // ColWindResis
            // 
            this.ColWindResis.AppearanceCell.Options.UseTextOptions = true;
            this.ColWindResis.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.ColWindResis.Caption = "总阻力";
            this.ColWindResis.FieldName = "WindResis";
            this.ColWindResis.Name = "ColWindResis";
            this.ColWindResis.OptionsColumn.AllowEdit = false;
            this.ColWindResis.Visible = true;
            this.ColWindResis.Width = 77;
            // 
            // layoutControlGroup1
            // 
            this.layoutControlGroup1.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.layoutControlGroup1.GroupBordersVisible = false;
            this.layoutControlGroup1.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem1,
            this.layoutControlItem2,
            this.emptySpaceItem1});
            this.layoutControlGroup1.Name = "Root";
            this.layoutControlGroup1.Size = new System.Drawing.Size(631, 146);
            this.layoutControlGroup1.TextVisible = false;
            // 
            // layoutControlItem1
            // 
            this.layoutControlItem1.Control = this.Gdc;
            this.layoutControlItem1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlItem1.Name = "layoutControlItem1";
            this.layoutControlItem1.Size = new System.Drawing.Size(611, 100);
            this.layoutControlItem1.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem1.TextVisible = false;
            // 
            // layoutControlItem2
            // 
            this.layoutControlItem2.Control = this.BtnOK;
            this.layoutControlItem2.Location = new System.Drawing.Point(515, 100);
            this.layoutControlItem2.Name = "layoutControlItem2";
            this.layoutControlItem2.Size = new System.Drawing.Size(96, 26);
            this.layoutControlItem2.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem2.TextVisible = false;
            // 
            // emptySpaceItem1
            // 
            this.emptySpaceItem1.AllowHotTrack = false;
            this.emptySpaceItem1.Location = new System.Drawing.Point(0, 100);
            this.emptySpaceItem1.Name = "emptySpaceItem1";
            this.emptySpaceItem1.Size = new System.Drawing.Size(515, 26);
            this.emptySpaceItem1.TextSize = new System.Drawing.Size(0, 0);
            // 
            // fmDragCalc
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(631, 146);
            this.Controls.Add(this.layoutControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "fmDragCalc";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "阻力计算";
            this.Load += new System.EventHandler(this.fmDragCalc_Load);
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).EndInit();
            this.layoutControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.Gdc)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Gdv)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TxtDuctLength)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TxtFriction)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TxtLocRes)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TxtDamper)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TxtDynPress)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraLayout.LayoutControl layoutControl1;
        private DevExpress.XtraEditors.SimpleButton BtnOK;
        public DevExpress.XtraGrid.GridControl Gdc;
        private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup1;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem1;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem2;
        private DevExpress.XtraLayout.EmptySpaceItem emptySpaceItem1;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridView Gdv;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn ColDuctLength;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn ColFriction;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn ColLocRes;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn ColDuctResistance;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn ColDamper;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn ColDynPress;
        private DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn ColWindResis;
        private DevExpress.XtraGrid.Views.BandedGrid.GridBand gridBand2;
        private DevExpress.XtraGrid.Views.BandedGrid.GridBand gridBand3;
        private DevExpress.XtraGrid.Views.BandedGrid.GridBand gridBand4;
        private DevExpress.XtraGrid.Views.BandedGrid.GridBand gridBand5;
        private DevExpress.XtraGrid.Views.BandedGrid.GridBand gridBand6;
        private DevExpress.XtraGrid.Views.BandedGrid.GridBand gridBand7;
        private DevExpress.XtraGrid.Views.BandedGrid.GridBand gridBand8;
        private DevExpress.XtraEditors.Repository.RepositoryItemTextEdit TxtDuctLength;
        private DevExpress.XtraEditors.Repository.RepositoryItemTextEdit TxtFriction;
        private DevExpress.XtraEditors.Repository.RepositoryItemTextEdit TxtLocRes;
        private DevExpress.XtraEditors.Repository.RepositoryItemTextEdit TxtDamper;
        private DevExpress.XtraEditors.Repository.RepositoryItemTextEdit TxtDynPress;
    }
}