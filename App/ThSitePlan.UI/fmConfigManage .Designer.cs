using System;

namespace ThSitePlan.UI
{
    partial class fmConfigManage
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
            this.components = new System.ComponentModel.Container();
            DevExpress.XtraEditors.Controls.EditorButtonImageOptions editorButtonImageOptions2 = new DevExpress.XtraEditors.Controls.EditorButtonImageOptions();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(fmConfigManage));
            DevExpress.Utils.SerializableAppearanceObject serializableAppearanceObject5 = new DevExpress.Utils.SerializableAppearanceObject();
            DevExpress.Utils.SerializableAppearanceObject serializableAppearanceObject6 = new DevExpress.Utils.SerializableAppearanceObject();
            DevExpress.Utils.SerializableAppearanceObject serializableAppearanceObject7 = new DevExpress.Utils.SerializableAppearanceObject();
            DevExpress.Utils.SerializableAppearanceObject serializableAppearanceObject8 = new DevExpress.Utils.SerializableAppearanceObject();
            this.TxtName = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
            this.layoutControl1 = new DevExpress.XtraLayout.LayoutControl();
            this.label6 = new System.Windows.Forms.Label();
            this.BtnHelp = new System.Windows.Forms.Button();
            this.BtnCancel = new System.Windows.Forms.Button();
            this.BtnOK = new System.Windows.Forms.Button();
            this.BtnExport = new System.Windows.Forms.Button();
            this.BtnImport = new System.Windows.Forms.Button();
            this.BtnRestore = new System.Windows.Forms.Button();
            this.TreeList = new DevExpress.XtraTreeList.TreeList();
            this.ColName = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.TextName = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
            this.ColColor = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.ColorEdit = new DevExpress.XtraEditors.Repository.RepositoryItemColorPickEdit();
            this.ColTransparency = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.SpinEdit = new DevExpress.XtraEditors.Repository.RepositoryItemSpinEdit();
            this.ColFrame = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.ColLayer = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.PopupContainer = new DevExpress.XtraEditors.Repository.RepositoryItemPopupContainerEdit();
            this.PopCtl = new DevExpress.XtraEditors.PopupContainerControl();
            this.Gdc = new DevExpress.XtraGrid.GridControl();
            this.Gdv = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.gridColumn5 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn6 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.PicEdit = new DevExpress.XtraEditors.Repository.RepositoryItemPictureEdit();
            this.ColImg = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.PictureEdit = new DevExpress.XtraEditors.Repository.RepositoryItemPictureEdit();
            this.ColScript = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.ComBoxScript = new DevExpress.XtraEditors.Repository.RepositoryItemComboBox();
            this.ColType = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.images = new DevExpress.Utils.ImageCollection(this.components);
            this.TrackBar = new DevExpress.XtraEditors.Repository.RepositoryItemTrackBar();
            this.TxtEmpty = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
            this.ComBoxLayer = new DevExpress.XtraEditors.Repository.RepositoryItemComboBox();
            this.MEdit = new DevExpress.XtraEditors.Repository.RepositoryItemMRUEdit();
            this.layoutControlItem13 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlGroup1 = new DevExpress.XtraLayout.LayoutControlGroup();
            this.layoutControlItem1 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem2 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem3 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem4 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem5 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem6 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem8 = new DevExpress.XtraLayout.LayoutControlItem();
            this.emptySpaceItem1 = new DevExpress.XtraLayout.EmptySpaceItem();
            this.behaviorManager1 = new DevExpress.Utils.Behaviors.BehaviorManager(this.components);
            this.Tim = new System.Windows.Forms.Timer(this.components);
            this.ContMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.MenuItemNewPeerGroup = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItemNewGroup = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItemNewLayer = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItemRename = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItemCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItemDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.ContextMenuTransparency = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.MenuItemOpacity = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItemOpacity10 = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItemOpacity20 = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItemOpacity30 = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItemOpacity40 = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItemOpacity50 = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItemOpacity60 = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItemOpacity70 = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItemOpacity80 = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItemOpacity90 = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItemOpacity100 = new System.Windows.Forms.ToolStripMenuItem();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.BtnPick = new DevExpress.XtraEditors.SimpleButton();
            this.defaultLookAndFeel1 = new DevExpress.LookAndFeel.DefaultLookAndFeel(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.TxtName)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).BeginInit();
            this.layoutControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TreeList)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TextName)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ColorEdit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.SpinEdit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PopupContainer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PopCtl)).BeginInit();
            this.PopCtl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Gdc)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Gdv)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PicEdit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PictureEdit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ComBoxScript)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.images)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TxtEmpty)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ComBoxLayer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MEdit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem13)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem8)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.behaviorManager1)).BeginInit();
            this.ContMenu.SuspendLayout();
            this.ContextMenuTransparency.SuspendLayout();
            this.SuspendLayout();
            // 
            // TxtName
            // 
            this.TxtName.AutoHeight = false;
            this.TxtName.Name = "TxtName";
            // 
            // layoutControl1
            // 
            this.layoutControl1.AllowCustomization = false;
            this.layoutControl1.Controls.Add(this.label6);
            this.layoutControl1.Controls.Add(this.BtnHelp);
            this.layoutControl1.Controls.Add(this.BtnCancel);
            this.layoutControl1.Controls.Add(this.BtnOK);
            this.layoutControl1.Controls.Add(this.BtnExport);
            this.layoutControl1.Controls.Add(this.BtnImport);
            this.layoutControl1.Controls.Add(this.BtnRestore);
            this.layoutControl1.Controls.Add(this.TreeList);
            this.layoutControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutControl1.HiddenItems.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem13});
            this.layoutControl1.Location = new System.Drawing.Point(0, 0);
            this.layoutControl1.LookAndFeel.SkinName = "Metropolis";
            this.layoutControl1.Name = "layoutControl1";
            this.layoutControl1.OptionsCustomizationForm.DesignTimeCustomizationFormPositionAndSize = new System.Drawing.Rectangle(946, 252, 650, 533);
            this.layoutControl1.Root = this.layoutControlGroup1;
            this.layoutControl1.Size = new System.Drawing.Size(773, 478);
            this.layoutControl1.TabIndex = 0;
            this.layoutControl1.Text = "layoutControl1";
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(592, 12);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(108, 20);
            this.label6.TabIndex = 22;
            this.label6.Text = "CAD脚本程序：";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // BtnHelp
            // 
            this.BtnHelp.Location = new System.Drawing.Point(674, 445);
            this.BtnHelp.Name = "BtnHelp";
            this.BtnHelp.Size = new System.Drawing.Size(88, 22);
            this.BtnHelp.TabIndex = 16;
            this.BtnHelp.Text = "帮助";
            this.BtnHelp.UseVisualStyleBackColor = true;
            this.BtnHelp.Click += new System.EventHandler(this.BtnHelp_Click);
            // 
            // BtnCancel
            // 
            this.BtnCancel.Location = new System.Drawing.Point(580, 445);
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.Size = new System.Drawing.Size(92, 22);
            this.BtnCancel.TabIndex = 15;
            this.BtnCancel.Text = "取消";
            this.BtnCancel.UseVisualStyleBackColor = true;
            this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // BtnOK
            // 
            this.BtnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.BtnOK.Location = new System.Drawing.Point(485, 445);
            this.BtnOK.Name = "BtnOK";
            this.BtnOK.Size = new System.Drawing.Size(93, 22);
            this.BtnOK.TabIndex = 14;
            this.BtnOK.Text = "确定";
            this.BtnOK.UseVisualStyleBackColor = true;
            this.BtnOK.Click += new System.EventHandler(this.BtnOK_Click);
            // 
            // BtnExport
            // 
            this.BtnExport.Location = new System.Drawing.Point(189, 445);
            this.BtnExport.Name = "BtnExport";
            this.BtnExport.Size = new System.Drawing.Size(91, 22);
            this.BtnExport.TabIndex = 13;
            this.BtnExport.Text = "导出";
            this.BtnExport.UseVisualStyleBackColor = true;
            this.BtnExport.Click += new System.EventHandler(this.BtnExport_Click);
            // 
            // BtnImport
            // 
            this.BtnImport.Location = new System.Drawing.Point(102, 445);
            this.BtnImport.Name = "BtnImport";
            this.BtnImport.Size = new System.Drawing.Size(85, 22);
            this.BtnImport.TabIndex = 12;
            this.BtnImport.Text = "导入";
            this.BtnImport.UseVisualStyleBackColor = true;
            this.BtnImport.Click += new System.EventHandler(this.BtnImport_Click);
            // 
            // BtnRestore
            // 
            this.BtnRestore.Location = new System.Drawing.Point(11, 445);
            this.BtnRestore.Name = "BtnRestore";
            this.BtnRestore.Size = new System.Drawing.Size(89, 22);
            this.BtnRestore.TabIndex = 11;
            this.BtnRestore.Text = "还原";
            this.BtnRestore.UseVisualStyleBackColor = true;
            this.BtnRestore.Click += new System.EventHandler(this.BtnRestore_Click);
            // 
            // TreeList
            // 
            this.TreeList.Appearance.FocusedRow.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.TreeList.Appearance.FocusedRow.ForeColor = System.Drawing.Color.Black;
            this.TreeList.Appearance.FocusedRow.Options.UseBackColor = true;
            this.TreeList.Appearance.FocusedRow.Options.UseForeColor = true;
            this.TreeList.Columns.AddRange(new DevExpress.XtraTreeList.Columns.TreeListColumn[] {
            this.ColName,
            this.ColColor,
            this.ColTransparency,
            this.ColFrame,
            this.ColLayer,
            this.ColImg,
            this.ColScript,
            this.ColType});
            this.TreeList.ColumnsImageList = this.images;
            this.TreeList.Cursor = System.Windows.Forms.Cursors.Default;
            this.TreeList.DataSource = null;
            this.TreeList.HorzScrollStep = 1;
            this.TreeList.HtmlImages = this.images;
            this.TreeList.Location = new System.Drawing.Point(12, 12);
            this.TreeList.LookAndFeel.SkinName = "Office 2016 Colorful";
            this.TreeList.LookAndFeel.UseDefaultLookAndFeel = false;
            this.TreeList.Name = "TreeList";
            this.TreeList.BeginUnboundLoad();
            this.TreeList.AppendNode(new object[] {
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null}, -1);
            this.TreeList.EndUnboundLoad();
            this.TreeList.OptionsBehavior.AllowExpandOnDblClick = false;
            this.TreeList.OptionsCustomization.AllowColumnMoving = false;
            this.TreeList.OptionsCustomization.AllowFilter = false;
            this.TreeList.OptionsCustomization.AllowSort = false;
            this.TreeList.OptionsDragAndDrop.AcceptOuterNodes = true;
            this.TreeList.OptionsDragAndDrop.CanCloneNodesOnDrop = true;
            this.TreeList.OptionsFilter.ShowAllValuesInCheckedFilterPopup = false;
            this.TreeList.OptionsMenu.EnableColumnMenu = false;
            this.TreeList.OptionsMenu.EnableFooterMenu = false;
            this.TreeList.OptionsMenu.ShowAutoFilterRowItem = false;
            this.TreeList.OptionsPrint.PrintReportFooter = false;
            this.TreeList.OptionsSelection.EnableAppearanceFocusedCell = false;
            this.TreeList.OptionsView.FocusRectStyle = DevExpress.XtraTreeList.DrawFocusRectStyle.RowFullFocus;
            this.TreeList.OptionsView.ShowButtons = false;
            this.TreeList.OptionsView.ShowFilterPanelMode = DevExpress.XtraTreeList.ShowFilterPanelMode.Never;
            this.TreeList.OptionsView.ShowHierarchyIndentationLines = DevExpress.Utils.DefaultBoolean.True;
            this.TreeList.OptionsView.ShowIndentAsRowStyle = true;
            this.TreeList.OptionsView.ShowIndicator = false;
            this.TreeList.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.ColorEdit,
            this.TrackBar,
            this.SpinEdit,
            this.TxtEmpty,
            this.ComBoxLayer,
            this.ComBoxScript,
            this.PictureEdit,
            this.MEdit,
            this.PopupContainer,
            this.TextName});
            this.TreeList.RowHeight = 28;
            this.TreeList.SelectImageList = this.images;
            this.TreeList.Size = new System.Drawing.Size(749, 428);
            this.TreeList.TabIndex = 4;
            this.TreeList.TreeLevelWidth = 12;
            this.TreeList.TreeLineStyle = DevExpress.XtraTreeList.LineStyle.Solid;
            this.TreeList.GetSelectImage += new DevExpress.XtraTreeList.GetSelectImageEventHandler(this.TreeList_GetSelectImage);
            this.TreeList.CustomNodeCellEditForEditing += new DevExpress.XtraTreeList.GetCustomNodeCellEditEventHandler(this.TreeList_CustomNodeCellEditForEditing);
            this.TreeList.CustomColumnDisplayText += new DevExpress.XtraTreeList.CustomColumnDisplayTextEventHandler(this.TreeList_CustomColumnDisplayText);
            this.TreeList.ValidatingEditor += new DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventHandler(this.TreeList_ValidatingEditor);
            this.TreeList.InvalidValueException += new DevExpress.XtraEditors.Controls.InvalidValueExceptionEventHandler(this.TreeList_InvalidValueException);
            this.TreeList.ShownEditor += new System.EventHandler(this.TreeList_ShownEditor);
            this.TreeList.HiddenEditor += new System.EventHandler(this.TreeList_HiddenEditor);
            this.TreeList.CellValueChanged += new DevExpress.XtraTreeList.CellValueChangedEventHandler(this.TreeList_CellValueChanged);
            this.TreeList.ShowingEditor += new System.ComponentModel.CancelEventHandler(this.TreeList_ShowingEditor);
            this.TreeList.StartSorting += new System.EventHandler(this.TreeList_StartSorting);
            this.TreeList.Click += new System.EventHandler(this.TreeList_Click);
            this.TreeList.DragDrop += new System.Windows.Forms.DragEventHandler(this.TreeList_DragDrop);
            this.TreeList.DragEnter += new System.Windows.Forms.DragEventHandler(this.TreeList_DragEnte);
            this.TreeList.DragOver += new System.Windows.Forms.DragEventHandler(this.TreeList_DragOver);
            this.TreeList.DoubleClick += new System.EventHandler(this.TreeList_DoubleClick);
            this.TreeList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.TreeList_MouseDoubleClick);
            this.TreeList.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TreeList_MouseDown);
            // 
            // ColName
            // 
            this.ColName.AppearanceHeader.Options.UseTextOptions = true;
            this.ColName.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.ColName.Caption = "PSD图层结构";
            this.ColName.ColumnEdit = this.TextName;
            this.ColName.FieldName = "Name";
            this.ColName.Name = "ColName";
            this.ColName.OptionsColumn.AllowEdit = false;
            this.ColName.OptionsColumn.AllowMove = false;
            this.ColName.Visible = true;
            this.ColName.VisibleIndex = 0;
            this.ColName.Width = 175;
            // 
            // TextName
            // 
            this.TextName.AutoHeight = false;
            this.TextName.Name = "TextName";
            this.TextName.EditValueChanging += new DevExpress.XtraEditors.Controls.ChangingEventHandler(this.TextName_EditValueChanging);
            // 
            // ColColor
            // 
            this.ColColor.AppearanceCell.Options.UseTextOptions = true;
            this.ColColor.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.ColColor.AppearanceHeader.Options.UseTextOptions = true;
            this.ColColor.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.ColColor.Caption = "PSD颜色";
            this.ColColor.ColumnEdit = this.ColorEdit;
            this.ColColor.FieldName = "PSD_Color";
            this.ColColor.Name = "ColColor";
            this.ColColor.OptionsColumn.AllowMove = false;
            this.ColColor.Visible = true;
            this.ColColor.VisibleIndex = 1;
            this.ColColor.Width = 140;
            // 
            // ColorEdit
            // 
            this.ColorEdit.AutoHeight = false;
            this.ColorEdit.AutomaticColor = System.Drawing.Color.Black;
            editorButtonImageOptions2.Image = ((System.Drawing.Image)(resources.GetObject("editorButtonImageOptions2.Image")));
            this.ColorEdit.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo),
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Glyph, "", -1, true, true, false, editorButtonImageOptions2, new DevExpress.Utils.KeyShortcut(System.Windows.Forms.Keys.None), serializableAppearanceObject5, serializableAppearanceObject6, serializableAppearanceObject7, serializableAppearanceObject8, "", "Spectroscope", null, DevExpress.Utils.ToolTipAnchor.Default)});
            this.ColorEdit.ButtonsStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.ColorEdit.ColorDialogOptions.AllowTransparency = false;
            this.ColorEdit.ColorDialogOptions.ShowMakeWebSafeButton = false;
            this.ColorEdit.ColorDialogOptions.ShowTabs = DevExpress.XtraEditors.ShowTabs.RGBModel;
            this.ColorEdit.ColorDialogType = DevExpress.XtraEditors.Popup.ColorDialogType.Advanced;
            this.ColorEdit.Name = "ColorEdit";
            this.ColorEdit.ShowDropDown = DevExpress.XtraEditors.Controls.ShowDropDown.DoubleClick;
            this.ColorEdit.ShowPopupShadow = false;
            this.ColorEdit.ShowSystemColors = false;
            this.ColorEdit.ShowWebColors = false;
            this.ColorEdit.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.Standard;
            this.ColorEdit.ColorChanged += new System.EventHandler(this.ColorEdit_ColorChanged);
            this.ColorEdit.ButtonClick += new DevExpress.XtraEditors.Controls.ButtonPressedEventHandler(this.ColorEdit_ButtonClick);
            // 
            // ColTransparency
            // 
            this.ColTransparency.AppearanceCell.Options.UseTextOptions = true;
            this.ColTransparency.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.ColTransparency.AppearanceHeader.Options.UseTextOptions = true;
            this.ColTransparency.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.ColTransparency.Caption = "PSD透明";
            this.ColTransparency.ColumnEdit = this.SpinEdit;
            this.ColTransparency.FieldName = "PSD_Transparency";
            this.ColTransparency.Name = "ColTransparency";
            this.ColTransparency.OptionsColumn.AllowEdit = false;
            this.ColTransparency.OptionsColumn.AllowMove = false;
            this.ColTransparency.Visible = true;
            this.ColTransparency.VisibleIndex = 2;
            this.ColTransparency.Width = 105;
            // 
            // SpinEdit
            // 
            this.SpinEdit.AutoHeight = false;
            this.SpinEdit.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.SpinEdit.ExportMode = DevExpress.XtraEditors.Repository.ExportMode.Value;
            this.SpinEdit.Mask.EditMask = "(1|([1-9]\\d{0,1})|100)";
            this.SpinEdit.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.RegEx;
            this.SpinEdit.MaxValue = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.SpinEdit.Name = "SpinEdit";
            // 
            // ColFrame
            // 
            this.ColFrame.AppearanceCell.Options.UseTextOptions = true;
            this.ColFrame.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.ColFrame.AppearanceHeader.Options.UseTextOptions = true;
            this.ColFrame.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.ColFrame.Caption = "CAD图像框";
            this.ColFrame.FieldName = "CAD_Frame";
            this.ColFrame.Name = "ColFrame";
            this.ColFrame.OptionsColumn.AllowEdit = false;
            this.ColFrame.OptionsColumn.AllowMove = false;
            this.ColFrame.Visible = true;
            this.ColFrame.VisibleIndex = 3;
            this.ColFrame.Width = 99;
            // 
            // ColLayer
            // 
            this.ColLayer.AppearanceCell.Options.UseTextOptions = true;
            this.ColLayer.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.ColLayer.AppearanceHeader.Options.UseTextOptions = true;
            this.ColLayer.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.ColLayer.Caption = "CAD图层";
            this.ColLayer.ColumnEdit = this.PopupContainer;
            this.ColLayer.FieldName = "CAD_Layer_Value";
            this.ColLayer.Name = "ColLayer";
            this.ColLayer.OptionsColumn.AllowMove = false;
            this.ColLayer.Visible = true;
            this.ColLayer.VisibleIndex = 4;
            this.ColLayer.Width = 143;
            // 
            // PopupContainer
            // 
            this.PopupContainer.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.PopupContainer.Name = "PopupContainer";
            this.PopupContainer.PopupControl = this.PopCtl;
            this.PopupContainer.PopupFormMinSize = new System.Drawing.Size(110, 0);
            this.PopupContainer.PopupFormSize = new System.Drawing.Size(110, 0);
            this.PopupContainer.PopupWidthMode = DevExpress.XtraEditors.PopupWidthMode.UseEditorWidth;
            this.PopupContainer.ShowPopupCloseButton = false;
            // 
            // PopCtl
            // 
            this.PopCtl.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.PopCtl.Appearance.Options.UseBackColor = true;
            this.PopCtl.AutoSize = true;
            this.PopCtl.Controls.Add(this.Gdc);
            this.PopCtl.Location = new System.Drawing.Point(443, 163);
            this.PopCtl.Name = "PopCtl";
            this.PopCtl.Size = new System.Drawing.Size(111, 100);
            this.PopCtl.TabIndex = 17;
            // 
            // Gdc
            // 
            this.Gdc.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Gdc.Location = new System.Drawing.Point(0, 0);
            this.Gdc.LookAndFeel.SkinMaskColor = System.Drawing.Color.Transparent;
            this.Gdc.LookAndFeel.SkinName = "The Bezier";
            this.Gdc.LookAndFeel.UseDefaultLookAndFeel = false;
            this.Gdc.MainView = this.Gdv;
            this.Gdc.Name = "Gdc";
            this.Gdc.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.PicEdit});
            this.Gdc.Size = new System.Drawing.Size(111, 100);
            this.Gdc.TabIndex = 1;
            this.Gdc.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.Gdv});
            // 
            // Gdv
            // 
            this.Gdv.Appearance.FocusedRow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.Gdv.Appearance.FocusedRow.Options.UseBackColor = true;
            this.Gdv.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.gridColumn5,
            this.gridColumn6});
            this.Gdv.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.None;
            this.Gdv.GridControl = this.Gdc;
            this.Gdv.Name = "Gdv";
            this.Gdv.OptionsNavigation.AutoMoveRowFocus = false;
            this.Gdv.OptionsSelection.EnableAppearanceFocusedCell = false;
            this.Gdv.OptionsSelection.EnableAppearanceFocusedRow = false;
            this.Gdv.OptionsView.ShowColumnHeaders = false;
            this.Gdv.OptionsView.ShowDetailButtons = false;
            this.Gdv.OptionsView.ShowFilterPanelMode = DevExpress.XtraGrid.Views.Base.ShowFilterPanelMode.Never;
            this.Gdv.OptionsView.ShowGroupExpandCollapseButtons = false;
            this.Gdv.OptionsView.ShowGroupPanel = false;
            this.Gdv.OptionsView.ShowHorizontalLines = DevExpress.Utils.DefaultBoolean.False;
            this.Gdv.OptionsView.ShowIndicator = false;
            this.Gdv.OptionsView.ShowPreviewRowLines = DevExpress.Utils.DefaultBoolean.False;
            this.Gdv.OptionsView.ShowVerticalLines = DevExpress.Utils.DefaultBoolean.False;
            this.Gdv.RowCellClick += new DevExpress.XtraGrid.Views.Grid.RowCellClickEventHandler(this.Gdv_RowCellClick);
            // 
            // gridColumn5
            // 
            this.gridColumn5.Caption = "名称";
            this.gridColumn5.FieldName = "Name";
            this.gridColumn5.Name = "gridColumn5";
            this.gridColumn5.OptionsColumn.AllowEdit = false;
            this.gridColumn5.OptionsColumn.AllowSize = false;
            this.gridColumn5.Visible = true;
            this.gridColumn5.VisibleIndex = 0;
            this.gridColumn5.Width = 80;
            // 
            // gridColumn6
            // 
            this.gridColumn6.AppearanceCell.Options.UseTextOptions = true;
            this.gridColumn6.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.gridColumn6.Caption = "操作";
            this.gridColumn6.ColumnEdit = this.PicEdit;
            this.gridColumn6.FieldName = "DeleteImg";
            this.gridColumn6.Name = "gridColumn6";
            this.gridColumn6.OptionsColumn.AllowEdit = false;
            this.gridColumn6.OptionsColumn.AllowSize = false;
            this.gridColumn6.Visible = true;
            this.gridColumn6.VisibleIndex = 1;
            this.gridColumn6.Width = 30;
            // 
            // PicEdit
            // 
            this.PicEdit.AllowFocused = false;
            this.PicEdit.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.PicEdit.Name = "PicEdit";
            this.PicEdit.NullText = " ";
            this.PicEdit.ShowMenu = false;
            this.PicEdit.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Zoom;
            // 
            // ColImg
            // 
            this.ColImg.AppearanceCell.Options.UseTextOptions = true;
            this.ColImg.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.ColImg.AppearanceHeader.Options.UseTextOptions = true;
            this.ColImg.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.ColImg.Caption = " ";
            this.ColImg.ColumnEdit = this.PictureEdit;
            this.ColImg.FieldName = "CAD_SelectImg";
            this.ColImg.MaxWidth = 24;
            this.ColImg.MinWidth = 24;
            this.ColImg.Name = "ColImg";
            this.ColImg.OptionsColumn.AllowMove = false;
            this.ColImg.Visible = true;
            this.ColImg.VisibleIndex = 5;
            this.ColImg.Width = 24;
            // 
            // PictureEdit
            // 
            this.PictureEdit.AllowFocused = false;
            this.PictureEdit.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.PictureEdit.Name = "PictureEdit";
            this.PictureEdit.NullText = " ";
            this.PictureEdit.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Zoom;
            this.PictureEdit.Click += new System.EventHandler(this.PictureEdit_Click);
            // 
            // ColScript
            // 
            this.ColScript.AppearanceCell.Options.UseTextOptions = true;
            this.ColScript.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.ColScript.AppearanceHeader.Options.UseTextOptions = true;
            this.ColScript.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.ColScript.Caption = "CAD脚本";
            this.ColScript.ColumnEdit = this.ComBoxScript;
            this.ColScript.FieldName = "CAD_Script";
            this.ColScript.Name = "ColScript";
            this.ColScript.OptionsColumn.AllowMove = false;
            this.ColScript.Visible = true;
            this.ColScript.VisibleIndex = 6;
            this.ColScript.Width = 109;
            // 
            // ComBoxScript
            // 
            this.ComBoxScript.AutoHeight = false;
            this.ComBoxScript.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.ComBoxScript.Name = "ComBoxScript";
            this.ComBoxScript.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.ComBoxScript.EditValueChanging += new DevExpress.XtraEditors.Controls.ChangingEventHandler(this.ComBoxScript_EditValueChanging);
            // 
            // ColType
            // 
            this.ColType.AppearanceHeader.Options.UseTextOptions = true;
            this.ColType.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.ColType.Caption = "类型";
            this.ColType.FieldName = "Type";
            this.ColType.Name = "ColType";
            // 
            // images
            // 
            this.images.ImageStream = ((DevExpress.Utils.ImageCollectionStreamer)(resources.GetObject("images.ImageStream")));
            this.images.Images.SetKeyName(0, "图标-07.png");
            this.images.Images.SetKeyName(1, "空白48.png");
            this.images.Images.SetKeyName(2, "图标-12.png");
            // 
            // TrackBar
            // 
            this.TrackBar.LabelAppearance.Options.UseTextOptions = true;
            this.TrackBar.LabelAppearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.TrackBar.LookAndFeel.SkinName = "Valentine";
            this.TrackBar.LookAndFeel.UseDefaultLookAndFeel = false;
            this.TrackBar.Maximum = 100;
            this.TrackBar.Name = "TrackBar";
            this.TrackBar.ShowValueToolTip = true;
            // 
            // TxtEmpty
            // 
            this.TxtEmpty.AutoHeight = false;
            this.TxtEmpty.Name = "TxtEmpty";
            // 
            // ComBoxLayer
            // 
            this.ComBoxLayer.AutoHeight = false;
            this.ComBoxLayer.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.ComBoxLayer.Name = "ComBoxLayer";
            this.ComBoxLayer.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            // 
            // MEdit
            // 
            this.MEdit.AutoHeight = false;
            this.MEdit.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.MEdit.Name = "MEdit";
            this.MEdit.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            // 
            // layoutControlItem13
            // 
            this.layoutControlItem13.Control = this.label6;
            this.layoutControlItem13.Location = new System.Drawing.Point(580, 0);
            this.layoutControlItem13.Name = "layoutControlItem13";
            this.layoutControlItem13.Size = new System.Drawing.Size(112, 24);
            this.layoutControlItem13.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem13.TextVisible = false;
            // 
            // layoutControlGroup1
            // 
            this.layoutControlGroup1.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.layoutControlGroup1.GroupBordersVisible = false;
            this.layoutControlGroup1.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem1,
            this.layoutControlItem2,
            this.layoutControlItem3,
            this.layoutControlItem4,
            this.layoutControlItem5,
            this.layoutControlItem6,
            this.layoutControlItem8,
            this.emptySpaceItem1});
            this.layoutControlGroup1.Name = "Root";
            this.layoutControlGroup1.Size = new System.Drawing.Size(773, 478);
            this.layoutControlGroup1.TextVisible = false;
            this.layoutControlGroup1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.layoutControlGroup1_MouseDown);
            // 
            // layoutControlItem1
            // 
            this.layoutControlItem1.Control = this.TreeList;
            this.layoutControlItem1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlItem1.Name = "layoutControlItem1";
            this.layoutControlItem1.Padding = new DevExpress.XtraLayout.Utils.Padding(2, 2, 2, 4);
            this.layoutControlItem1.Size = new System.Drawing.Size(753, 434);
            this.layoutControlItem1.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem1.TextVisible = false;
            // 
            // layoutControlItem2
            // 
            this.layoutControlItem2.Control = this.BtnRestore;
            this.layoutControlItem2.Location = new System.Drawing.Point(0, 434);
            this.layoutControlItem2.Name = "layoutControlItem2";
            this.layoutControlItem2.Padding = new DevExpress.XtraLayout.Utils.Padding(1, 1, 1, 1);
            this.layoutControlItem2.Size = new System.Drawing.Size(91, 24);
            this.layoutControlItem2.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem2.TextVisible = false;
            // 
            // layoutControlItem3
            // 
            this.layoutControlItem3.Control = this.BtnImport;
            this.layoutControlItem3.Location = new System.Drawing.Point(91, 434);
            this.layoutControlItem3.Name = "layoutControlItem3";
            this.layoutControlItem3.Padding = new DevExpress.XtraLayout.Utils.Padding(1, 1, 1, 1);
            this.layoutControlItem3.Size = new System.Drawing.Size(87, 24);
            this.layoutControlItem3.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem3.TextVisible = false;
            // 
            // layoutControlItem4
            // 
            this.layoutControlItem4.Control = this.BtnExport;
            this.layoutControlItem4.Location = new System.Drawing.Point(178, 434);
            this.layoutControlItem4.Name = "layoutControlItem4";
            this.layoutControlItem4.Padding = new DevExpress.XtraLayout.Utils.Padding(1, 1, 1, 1);
            this.layoutControlItem4.Size = new System.Drawing.Size(93, 24);
            this.layoutControlItem4.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem4.TextVisible = false;
            // 
            // layoutControlItem5
            // 
            this.layoutControlItem5.Control = this.BtnOK;
            this.layoutControlItem5.Location = new System.Drawing.Point(474, 434);
            this.layoutControlItem5.Name = "layoutControlItem5";
            this.layoutControlItem5.Padding = new DevExpress.XtraLayout.Utils.Padding(1, 1, 1, 1);
            this.layoutControlItem5.Size = new System.Drawing.Size(95, 24);
            this.layoutControlItem5.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem5.TextVisible = false;
            // 
            // layoutControlItem6
            // 
            this.layoutControlItem6.Control = this.BtnCancel;
            this.layoutControlItem6.Location = new System.Drawing.Point(569, 434);
            this.layoutControlItem6.Name = "layoutControlItem6";
            this.layoutControlItem6.Padding = new DevExpress.XtraLayout.Utils.Padding(1, 1, 1, 1);
            this.layoutControlItem6.Size = new System.Drawing.Size(94, 24);
            this.layoutControlItem6.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem6.TextVisible = false;
            // 
            // layoutControlItem8
            // 
            this.layoutControlItem8.Control = this.BtnHelp;
            this.layoutControlItem8.Location = new System.Drawing.Point(663, 434);
            this.layoutControlItem8.Name = "layoutControlItem8";
            this.layoutControlItem8.Padding = new DevExpress.XtraLayout.Utils.Padding(1, 1, 1, 1);
            this.layoutControlItem8.Size = new System.Drawing.Size(90, 24);
            this.layoutControlItem8.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem8.TextVisible = false;
            // 
            // emptySpaceItem1
            // 
            this.emptySpaceItem1.AllowHotTrack = false;
            this.emptySpaceItem1.Location = new System.Drawing.Point(271, 434);
            this.emptySpaceItem1.Name = "emptySpaceItem1";
            this.emptySpaceItem1.Size = new System.Drawing.Size(203, 24);
            this.emptySpaceItem1.TextSize = new System.Drawing.Size(0, 0);
            // 
            // Tim
            // 
            this.Tim.Tick += new System.EventHandler(this.Tim_Tick);
            // 
            // ContMenu
            // 
            this.ContMenu.AutoSize = false;
            this.ContMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItemNewPeerGroup,
            this.MenuItemNewGroup,
            this.MenuItemNewLayer,
            this.toolStripSeparator1,
            this.MenuItemRename,
            this.MenuItemCopy,
            this.MenuItemDelete});
            this.ContMenu.Name = "ContMenu";
            this.ContMenu.ShowImageMargin = false;
            this.ContMenu.Size = new System.Drawing.Size(110, 145);
            // 
            // MenuItemNewPeerGroup
            // 
            this.MenuItemNewPeerGroup.Name = "MenuItemNewPeerGroup";
            this.MenuItemNewPeerGroup.Size = new System.Drawing.Size(123, 22);
            this.MenuItemNewPeerGroup.Text = "新建同级群组";
            this.MenuItemNewPeerGroup.Click += new System.EventHandler(this.MenuItemNewPeerGroup_Click);
            // 
            // MenuItemNewGroup
            // 
            this.MenuItemNewGroup.Name = "MenuItemNewGroup";
            this.MenuItemNewGroup.Size = new System.Drawing.Size(123, 22);
            this.MenuItemNewGroup.Text = "新建次级群组";
            this.MenuItemNewGroup.Click += new System.EventHandler(this.MenuItemNewGroup_Click);
            // 
            // MenuItemNewLayer
            // 
            this.MenuItemNewLayer.Name = "MenuItemNewLayer";
            this.MenuItemNewLayer.Size = new System.Drawing.Size(123, 22);
            this.MenuItemNewLayer.Text = "新建图层";
            this.MenuItemNewLayer.Click += new System.EventHandler(this.MenuItemNewLayer_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(106, 6);
            // 
            // MenuItemRename
            // 
            this.MenuItemRename.Name = "MenuItemRename";
            this.MenuItemRename.Size = new System.Drawing.Size(123, 22);
            this.MenuItemRename.Text = "重命名";
            this.MenuItemRename.Click += new System.EventHandler(this.MenuItemRename_Click);
            // 
            // MenuItemCopy
            // 
            this.MenuItemCopy.Name = "MenuItemCopy";
            this.MenuItemCopy.Size = new System.Drawing.Size(123, 22);
            this.MenuItemCopy.Text = "复制";
            this.MenuItemCopy.Click += new System.EventHandler(this.MenuItemCopy_Click);
            // 
            // MenuItemDelete
            // 
            this.MenuItemDelete.Name = "MenuItemDelete";
            this.MenuItemDelete.Size = new System.Drawing.Size(123, 22);
            this.MenuItemDelete.Text = "删除";
            this.MenuItemDelete.Click += new System.EventHandler(this.MenuItemDelete_Click);
            // 
            // ContextMenuTransparency
            // 
            this.ContextMenuTransparency.AutoSize = false;
            this.ContextMenuTransparency.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItemOpacity,
            this.MenuItemOpacity10,
            this.MenuItemOpacity20,
            this.MenuItemOpacity30,
            this.MenuItemOpacity40,
            this.MenuItemOpacity50,
            this.MenuItemOpacity60,
            this.MenuItemOpacity70,
            this.MenuItemOpacity80,
            this.MenuItemOpacity90,
            this.MenuItemOpacity100});
            this.ContextMenuTransparency.Name = "ContextMenuTransparency";
            this.ContextMenuTransparency.ShowImageMargin = false;
            this.ContextMenuTransparency.Size = new System.Drawing.Size(120, 247);
            // 
            // MenuItemOpacity
            // 
            this.MenuItemOpacity.Name = "MenuItemOpacity";
            this.MenuItemOpacity.Size = new System.Drawing.Size(143, 22);
            this.MenuItemOpacity.Tag = "0";
            this.MenuItemOpacity.Text = "不透明度：0%";
            this.MenuItemOpacity.Click += new System.EventHandler(this.MenuItemOpacity_Click);
            // 
            // MenuItemOpacity10
            // 
            this.MenuItemOpacity10.Name = "MenuItemOpacity10";
            this.MenuItemOpacity10.Size = new System.Drawing.Size(143, 22);
            this.MenuItemOpacity10.Tag = "10";
            this.MenuItemOpacity10.Text = "不透明度：10%";
            this.MenuItemOpacity10.Click += new System.EventHandler(this.MenuItemOpacity_Click);
            // 
            // MenuItemOpacity20
            // 
            this.MenuItemOpacity20.Name = "MenuItemOpacity20";
            this.MenuItemOpacity20.Size = new System.Drawing.Size(143, 22);
            this.MenuItemOpacity20.Tag = "20";
            this.MenuItemOpacity20.Text = "不透明度：20%";
            this.MenuItemOpacity20.Click += new System.EventHandler(this.MenuItemOpacity_Click);
            // 
            // MenuItemOpacity30
            // 
            this.MenuItemOpacity30.Name = "MenuItemOpacity30";
            this.MenuItemOpacity30.Size = new System.Drawing.Size(143, 22);
            this.MenuItemOpacity30.Tag = "30";
            this.MenuItemOpacity30.Text = "不透明度：30%";
            this.MenuItemOpacity30.Click += new System.EventHandler(this.MenuItemOpacity_Click);
            // 
            // MenuItemOpacity40
            // 
            this.MenuItemOpacity40.Name = "MenuItemOpacity40";
            this.MenuItemOpacity40.Size = new System.Drawing.Size(143, 22);
            this.MenuItemOpacity40.Tag = "40";
            this.MenuItemOpacity40.Text = "不透明度：40%";
            this.MenuItemOpacity40.Click += new System.EventHandler(this.MenuItemOpacity_Click);
            // 
            // MenuItemOpacity50
            // 
            this.MenuItemOpacity50.Name = "MenuItemOpacity50";
            this.MenuItemOpacity50.Size = new System.Drawing.Size(143, 22);
            this.MenuItemOpacity50.Tag = "50";
            this.MenuItemOpacity50.Text = "不透明度：50%";
            this.MenuItemOpacity50.Click += new System.EventHandler(this.MenuItemOpacity_Click);
            // 
            // MenuItemOpacity60
            // 
            this.MenuItemOpacity60.Name = "MenuItemOpacity60";
            this.MenuItemOpacity60.Size = new System.Drawing.Size(143, 22);
            this.MenuItemOpacity60.Tag = "60";
            this.MenuItemOpacity60.Text = "不透明度：60%";
            this.MenuItemOpacity60.Click += new System.EventHandler(this.MenuItemOpacity_Click);
            // 
            // MenuItemOpacity70
            // 
            this.MenuItemOpacity70.Name = "MenuItemOpacity70";
            this.MenuItemOpacity70.Size = new System.Drawing.Size(143, 22);
            this.MenuItemOpacity70.Tag = "70";
            this.MenuItemOpacity70.Text = "不透明度：70%";
            this.MenuItemOpacity70.Click += new System.EventHandler(this.MenuItemOpacity_Click);
            // 
            // MenuItemOpacity80
            // 
            this.MenuItemOpacity80.Name = "MenuItemOpacity80";
            this.MenuItemOpacity80.Size = new System.Drawing.Size(143, 22);
            this.MenuItemOpacity80.Tag = "80";
            this.MenuItemOpacity80.Text = "不透明度：80%";
            this.MenuItemOpacity80.Click += new System.EventHandler(this.MenuItemOpacity_Click);
            // 
            // MenuItemOpacity90
            // 
            this.MenuItemOpacity90.Name = "MenuItemOpacity90";
            this.MenuItemOpacity90.Size = new System.Drawing.Size(143, 22);
            this.MenuItemOpacity90.Tag = "90";
            this.MenuItemOpacity90.Text = "不透明度：90%";
            this.MenuItemOpacity90.Click += new System.EventHandler(this.MenuItemOpacity_Click);
            // 
            // MenuItemOpacity100
            // 
            this.MenuItemOpacity100.Name = "MenuItemOpacity100";
            this.MenuItemOpacity100.Size = new System.Drawing.Size(143, 22);
            this.MenuItemOpacity100.Tag = "100";
            this.MenuItemOpacity100.Text = "不透明度：100%";
            this.MenuItemOpacity100.Click += new System.EventHandler(this.MenuItemOpacity_Click);
            // 
            // BtnPick
            // 
            this.BtnPick.Location = new System.Drawing.Point(428, 446);
            this.BtnPick.Name = "BtnPick";
            this.BtnPick.Size = new System.Drawing.Size(16, 20);
            this.BtnPick.TabIndex = 23;
            this.BtnPick.Click += new System.EventHandler(this.BtnPick_Click);
            // 
            // fmConfigManage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(773, 478);
            this.Controls.Add(this.PopCtl);
            this.Controls.Add(this.layoutControl1);
            this.Controls.Add(this.BtnPick);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.LookAndFeel.UseDefaultLookAndFeel = false;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "fmConfigManage";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "映射配置管理";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.fmConfigManage_FormClosed);
            this.Load += new System.EventHandler(this.fmConfigManage_Load);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.fmConfigManage_MouseDown);
            ((System.ComponentModel.ISupportInitialize)(this.TxtName)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).EndInit();
            this.layoutControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.TreeList)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TextName)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ColorEdit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.SpinEdit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PopupContainer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PopCtl)).EndInit();
            this.PopCtl.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.Gdc)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Gdv)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PicEdit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PictureEdit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ComBoxScript)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.images)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TxtEmpty)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ComBoxLayer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MEdit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem13)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem8)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.behaviorManager1)).EndInit();
            this.ContMenu.ResumeLayout(false);
            this.ContextMenuTransparency.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraLayout.LayoutControl layoutControl1;
        private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup1;
        private DevExpress.XtraTreeList.TreeList TreeList;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem1;
        private DevExpress.XtraTreeList.Columns.TreeListColumn ColName;
        private DevExpress.XtraTreeList.Columns.TreeListColumn ColColor;
        private DevExpress.XtraTreeList.Columns.TreeListColumn ColTransparency;
        private DevExpress.XtraTreeList.Columns.TreeListColumn ColFrame;
        private DevExpress.XtraTreeList.Columns.TreeListColumn ColLayer;
        private DevExpress.XtraTreeList.Columns.TreeListColumn ColImg;
        private DevExpress.XtraTreeList.Columns.TreeListColumn ColScript;
        private DevExpress.Utils.ImageCollection images;
        private DevExpress.XtraTreeList.Columns.TreeListColumn ColType;
        private DevExpress.Utils.Behaviors.BehaviorManager behaviorManager1;
        private DevExpress.XtraEditors.Repository.RepositoryItemColorPickEdit ColorEdit;
        private System.Windows.Forms.Timer Tim;
        private DevExpress.XtraEditors.Repository.RepositoryItemTrackBar TrackBar;
        private DevExpress.XtraEditors.Repository.RepositoryItemSpinEdit SpinEdit;
        private DevExpress.XtraEditors.Repository.RepositoryItemTextEdit TxtEmpty;
        private DevExpress.XtraEditors.Repository.RepositoryItemComboBox ComBoxLayer;
        private DevExpress.XtraEditors.Repository.RepositoryItemComboBox ComBoxScript;
        private DevExpress.XtraEditors.Repository.RepositoryItemPictureEdit PictureEdit;
        private System.Windows.Forms.Button BtnHelp;
        private System.Windows.Forms.Button BtnCancel;
        private System.Windows.Forms.Button BtnOK;
        private System.Windows.Forms.Button BtnExport;
        private System.Windows.Forms.Button BtnImport;
        private System.Windows.Forms.Button BtnRestore;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem2;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem3;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem4;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem5;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem6;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem8;
        private DevExpress.XtraEditors.Repository.RepositoryItemMRUEdit MEdit;
        private System.Windows.Forms.ContextMenuStrip ContMenu;
        private System.Windows.Forms.ToolStripMenuItem MenuItemNewPeerGroup;
        private System.Windows.Forms.ToolStripMenuItem MenuItemNewGroup;
        private System.Windows.Forms.ToolStripMenuItem MenuItemNewLayer;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem MenuItemRename;
        private System.Windows.Forms.ToolStripMenuItem MenuItemCopy;
        private System.Windows.Forms.ToolStripMenuItem MenuItemDelete;
        private System.Windows.Forms.ContextMenuStrip ContextMenuTransparency;
        private System.Windows.Forms.ToolStripMenuItem MenuItemOpacity;
        private System.Windows.Forms.ToolStripMenuItem MenuItemOpacity10;
        private System.Windows.Forms.ToolStripMenuItem MenuItemOpacity20;
        private System.Windows.Forms.ToolStripMenuItem MenuItemOpacity30;
        private System.Windows.Forms.ToolStripMenuItem MenuItemOpacity40;
        private System.Windows.Forms.ToolStripMenuItem MenuItemOpacity50;
        private System.Windows.Forms.ToolStripMenuItem MenuItemOpacity60;
        private System.Windows.Forms.ToolStripMenuItem MenuItemOpacity70;
        private System.Windows.Forms.ToolStripMenuItem MenuItemOpacity80;
        private System.Windows.Forms.ToolStripMenuItem MenuItemOpacity90;
        private System.Windows.Forms.ToolStripMenuItem MenuItemOpacity100;
        private DevExpress.XtraEditors.Repository.RepositoryItemPopupContainerEdit PopupContainer;
        private DevExpress.XtraEditors.PopupContainerControl PopCtl;
        private DevExpress.XtraGrid.GridControl Gdc;
        private DevExpress.XtraGrid.Views.Grid.GridView Gdv;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn5;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn6;
        private DevExpress.XtraEditors.Repository.RepositoryItemPictureEdit PicEdit;
        private System.Windows.Forms.Label label6;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem13;
        private DevExpress.XtraLayout.EmptySpaceItem emptySpaceItem1;
        private DevExpress.XtraEditors.Repository.RepositoryItemTextEdit TxtName;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private DevExpress.XtraEditors.SimpleButton BtnPick;
        private DevExpress.XtraEditors.Repository.RepositoryItemTextEdit TextName;
        private DevExpress.LookAndFeel.DefaultLookAndFeel defaultLookAndFeel1;
    }
}