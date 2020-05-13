namespace ThSitePlan.UI
{
    partial class fmMobilePanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(fmMobilePanel));
            this.layoutControl1 = new DevExpress.XtraLayout.LayoutControl();
            this.ColorPEdit = new DevExpress.XtraEditors.ColorPickEdit();
            this.PanBG = new System.Windows.Forms.Panel();
            this.PicEdit = new DevExpress.XtraEditors.PictureEdit();
            this.layoutControlGroup1 = new DevExpress.XtraLayout.LayoutControlGroup();
            this.layoutControlItem4 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem1 = new DevExpress.XtraLayout.LayoutControlItem();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).BeginInit();
            this.layoutControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ColorPEdit.Properties)).BeginInit();
            this.PanBG.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PicEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).BeginInit();
            this.SuspendLayout();
            // 
            // layoutControl1
            // 
            this.layoutControl1.BackColor = System.Drawing.Color.Transparent;
            this.layoutControl1.Controls.Add(this.ColorPEdit);
            this.layoutControl1.Controls.Add(this.PanBG);
            this.layoutControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutControl1.Location = new System.Drawing.Point(0, 0);
            this.layoutControl1.Name = "layoutControl1";
            this.layoutControl1.OptionsCustomizationForm.DesignTimeCustomizationFormPositionAndSize = new System.Drawing.Rectangle(486, 265, 650, 400);
            this.layoutControl1.Root = this.layoutControlGroup1;
            this.layoutControl1.Size = new System.Drawing.Size(123, 100);
            this.layoutControl1.TabIndex = 0;
            // 
            // ColorPEdit
            // 
            this.ColorPEdit.EditValue = System.Drawing.Color.Empty;
            this.ColorPEdit.Location = new System.Drawing.Point(2, 80);
            this.ColorPEdit.Name = "ColorPEdit";
            this.ColorPEdit.Properties.AllowFocused = false;
            this.ColorPEdit.Properties.Appearance.BackColor = System.Drawing.Color.White;
            this.ColorPEdit.Properties.Appearance.Options.UseBackColor = true;
            this.ColorPEdit.Properties.AutomaticColor = System.Drawing.Color.Black;
            this.ColorPEdit.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.ColorPEdit.Properties.ColorDialogOptions.AllowTransparency = false;
            this.ColorPEdit.Properties.ColorDialogOptions.ShowMakeWebSafeButton = false;
            this.ColorPEdit.Properties.ColorDialogOptions.ShowTabs = DevExpress.XtraEditors.ShowTabs.RGBModel;
            this.ColorPEdit.Properties.ShowPopupShadow = false;
            this.ColorPEdit.Properties.ShowSystemColors = false;
            this.ColorPEdit.Properties.ShowWebColors = false;
            this.ColorPEdit.Properties.StoreColorAsInteger = true;
            this.ColorPEdit.Size = new System.Drawing.Size(119, 18);
            this.ColorPEdit.StyleController = this.layoutControl1;
            this.ColorPEdit.TabIndex = 12;
            // 
            // PanBG
            // 
            this.PanBG.Controls.Add(this.PicEdit);
            this.PanBG.Location = new System.Drawing.Point(2, 2);
            this.PanBG.Name = "PanBG";
            this.PanBG.Size = new System.Drawing.Size(119, 74);
            this.PanBG.TabIndex = 7;
            // 
            // PicEdit
            // 
            this.PicEdit.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.PicEdit.Cursor = System.Windows.Forms.Cursors.Default;
            this.PicEdit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PicEdit.EditValue = ((object)(resources.GetObject("PicEdit.EditValue")));
            this.PicEdit.Location = new System.Drawing.Point(0, 0);
            this.PicEdit.Name = "PicEdit";
            this.PicEdit.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.PicEdit.Properties.Appearance.Options.UseBackColor = true;
            this.PicEdit.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.PicEdit.Properties.NullText = " ";
            this.PicEdit.Properties.ShowCameraMenuItem = DevExpress.XtraEditors.Controls.CameraMenuItemVisibility.Auto;
            this.PicEdit.Size = new System.Drawing.Size(119, 74);
            this.PicEdit.TabIndex = 4;
            // 
            // layoutControlGroup1
            // 
            this.layoutControlGroup1.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.layoutControlGroup1.GroupBordersVisible = false;
            this.layoutControlGroup1.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem4,
            this.layoutControlItem1});
            this.layoutControlGroup1.Name = "Root";
            this.layoutControlGroup1.Padding = new DevExpress.XtraLayout.Utils.Padding(0, 0, 0, 0);
            this.layoutControlGroup1.Size = new System.Drawing.Size(123, 100);
            this.layoutControlGroup1.StartNewLine = true;
            this.layoutControlGroup1.TextVisible = false;
            // 
            // layoutControlItem4
            // 
            this.layoutControlItem4.Control = this.PanBG;
            this.layoutControlItem4.Location = new System.Drawing.Point(0, 0);
            this.layoutControlItem4.Name = "layoutControlItem4";
            this.layoutControlItem4.Size = new System.Drawing.Size(123, 78);
            this.layoutControlItem4.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem4.TextVisible = false;
            // 
            // layoutControlItem1
            // 
            this.layoutControlItem1.Control = this.ColorPEdit;
            this.layoutControlItem1.Location = new System.Drawing.Point(0, 78);
            this.layoutControlItem1.Name = "layoutControlItem1";
            this.layoutControlItem1.Size = new System.Drawing.Size(123, 22);
            this.layoutControlItem1.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem1.TextVisible = false;
            // 
            // fmMobilePanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.White;
            this.BackgroundImage = global::ThSitePlan.UI.Properties.Resources.取色器;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.ClientSize = new System.Drawing.Size(123, 100);
            this.Controls.Add(this.layoutControl1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(123, 100);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(123, 100);
            this.Name = "fmMobilePanel";
            this.Text = "fmMobilePanel";
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).EndInit();
            this.layoutControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ColorPEdit.Properties)).EndInit();
            this.PanBG.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.PicEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraLayout.LayoutControl layoutControl1;
        private DevExpress.XtraEditors.PictureEdit PicEdit;
        private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup1;
        private System.Windows.Forms.Panel PanBG;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem4;
        private DevExpress.XtraEditors.ColorPickEdit ColorPEdit;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem1;
    }
}