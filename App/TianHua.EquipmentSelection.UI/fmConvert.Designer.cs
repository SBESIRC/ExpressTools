namespace TianHua.FanSelection.UI
{
    partial class fmConvert
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(fmConvert));
            this.layoutControl1 = new DevExpress.XtraLayout.LayoutControl();
            this.CheckAxialFanPara = new DevExpress.XtraEditors.CheckEdit();
            this.CheckAxialFan = new DevExpress.XtraEditors.CheckEdit();
            this.CheckFanParameters = new DevExpress.XtraEditors.CheckEdit();
            this.CheckFanSelection = new DevExpress.XtraEditors.CheckEdit();
            this.BtnCancel = new DevExpress.XtraEditors.SimpleButton();
            this.BtnOK = new DevExpress.XtraEditors.SimpleButton();
            this.layoutControlGroup1 = new DevExpress.XtraLayout.LayoutControlGroup();
            this.layoutControlItem3 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem4 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem2 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem6 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem7 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem5 = new DevExpress.XtraLayout.LayoutControlItem();
            this.BGWorker = new System.ComponentModel.BackgroundWorker();
            this.Panel = new DevExpress.XtraEditors.PanelControl();
            this.labelExcelFile = new System.Windows.Forms.Label();
            this.layoutControlItem1 = new DevExpress.XtraLayout.LayoutControlItem();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).BeginInit();
            this.layoutControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CheckAxialFanPara.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CheckAxialFan.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CheckFanParameters.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CheckFanSelection.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem7)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Panel)).BeginInit();
            this.Panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).BeginInit();
            this.SuspendLayout();
            // 
            // layoutControl1
            // 
            this.layoutControl1.Controls.Add(this.CheckAxialFanPara);
            this.layoutControl1.Controls.Add(this.CheckAxialFan);
            this.layoutControl1.Controls.Add(this.CheckFanParameters);
            this.layoutControl1.Controls.Add(this.CheckFanSelection);
            this.layoutControl1.Controls.Add(this.BtnCancel);
            this.layoutControl1.Controls.Add(this.BtnOK);
            this.layoutControl1.Controls.Add(this.Panel);
            this.layoutControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutControl1.Location = new System.Drawing.Point(0, 0);
            this.layoutControl1.Name = "layoutControl1";
            this.layoutControl1.OptionsCustomizationForm.DesignTimeCustomizationFormPositionAndSize = new System.Drawing.Rectangle(822, 263, 650, 400);
            this.layoutControl1.Root = this.layoutControlGroup1;
            this.layoutControl1.Size = new System.Drawing.Size(281, 242);
            this.layoutControl1.TabIndex = 0;
            this.layoutControl1.Text = "layoutControl1";
            // 
            // CheckAxialFanPara
            // 
            this.CheckAxialFanPara.Enabled = false;
            this.CheckAxialFanPara.Location = new System.Drawing.Point(142, 185);
            this.CheckAxialFanPara.Name = "CheckAxialFanPara";
            this.CheckAxialFanPara.Properties.AllowFocused = false;
            this.CheckAxialFanPara.Properties.Caption = "轴流风机参数";
            this.CheckAxialFanPara.Size = new System.Drawing.Size(127, 19);
            this.CheckAxialFanPara.StyleController = this.layoutControl1;
            this.CheckAxialFanPara.TabIndex = 9;
            // 
            // CheckAxialFan
            // 
            this.CheckAxialFan.Enabled = false;
            this.CheckAxialFan.Location = new System.Drawing.Point(12, 185);
            this.CheckAxialFan.Name = "CheckAxialFan";
            this.CheckAxialFan.Properties.AllowFocused = false;
            this.CheckAxialFan.Properties.Caption = "轴流风机选型";
            this.CheckAxialFan.Size = new System.Drawing.Size(126, 19);
            this.CheckAxialFan.StyleController = this.layoutControl1;
            this.CheckAxialFan.TabIndex = 8;
            // 
            // CheckFanParameters
            // 
            this.CheckFanParameters.Enabled = false;
            this.CheckFanParameters.Location = new System.Drawing.Point(142, 162);
            this.CheckFanParameters.Name = "CheckFanParameters";
            this.CheckFanParameters.Properties.AllowFocused = false;
            this.CheckFanParameters.Properties.Caption = "风机箱参数";
            this.CheckFanParameters.Size = new System.Drawing.Size(127, 19);
            this.CheckFanParameters.StyleController = this.layoutControl1;
            this.CheckFanParameters.TabIndex = 7;
            // 
            // CheckFanSelection
            // 
            this.CheckFanSelection.Enabled = false;
            this.CheckFanSelection.Location = new System.Drawing.Point(12, 162);
            this.CheckFanSelection.Name = "CheckFanSelection";
            this.CheckFanSelection.Properties.AllowFocused = false;
            this.CheckFanSelection.Properties.Caption = "风机箱选型";
            this.CheckFanSelection.Size = new System.Drawing.Size(126, 19);
            this.CheckFanSelection.StyleController = this.layoutControl1;
            this.CheckFanSelection.TabIndex = 1;
            // 
            // BtnCancel
            // 
            this.BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.BtnCancel.Location = new System.Drawing.Point(142, 208);
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.Size = new System.Drawing.Size(127, 22);
            this.BtnCancel.StyleController = this.layoutControl1;
            this.BtnCancel.TabIndex = 6;
            this.BtnCancel.Text = "取消";
            this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // BtnOK
            // 
            this.BtnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.BtnOK.Location = new System.Drawing.Point(12, 208);
            this.BtnOK.Name = "BtnOK";
            this.BtnOK.Size = new System.Drawing.Size(126, 22);
            this.BtnOK.StyleController = this.layoutControl1;
            this.BtnOK.TabIndex = 5;
            this.BtnOK.Text = "确定";
            this.BtnOK.Click += new System.EventHandler(this.BtnOK_Click);
            // 
            // layoutControlGroup1
            // 
            this.layoutControlGroup1.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.layoutControlGroup1.GroupBordersVisible = false;
            this.layoutControlGroup1.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem1,
            this.layoutControlItem3,
            this.layoutControlItem4,
            this.layoutControlItem2,
            this.layoutControlItem6,
            this.layoutControlItem7,
            this.layoutControlItem5});
            this.layoutControlGroup1.Name = "Root";
            this.layoutControlGroup1.Size = new System.Drawing.Size(281, 242);
            this.layoutControlGroup1.TextVisible = false;
            // 
            // layoutControlItem3
            // 
            this.layoutControlItem3.Control = this.BtnOK;
            this.layoutControlItem3.Location = new System.Drawing.Point(0, 196);
            this.layoutControlItem3.Name = "layoutControlItem3";
            this.layoutControlItem3.Size = new System.Drawing.Size(130, 26);
            this.layoutControlItem3.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem3.TextVisible = false;
            // 
            // layoutControlItem4
            // 
            this.layoutControlItem4.Control = this.BtnCancel;
            this.layoutControlItem4.Location = new System.Drawing.Point(130, 196);
            this.layoutControlItem4.Name = "layoutControlItem4";
            this.layoutControlItem4.Size = new System.Drawing.Size(131, 26);
            this.layoutControlItem4.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem4.TextVisible = false;
            // 
            // layoutControlItem2
            // 
            this.layoutControlItem2.Control = this.CheckFanSelection;
            this.layoutControlItem2.Location = new System.Drawing.Point(0, 150);
            this.layoutControlItem2.Name = "layoutControlItem2";
            this.layoutControlItem2.Size = new System.Drawing.Size(130, 23);
            this.layoutControlItem2.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem2.TextVisible = false;
            // 
            // layoutControlItem6
            // 
            this.layoutControlItem6.Control = this.CheckAxialFan;
            this.layoutControlItem6.Location = new System.Drawing.Point(0, 173);
            this.layoutControlItem6.Name = "layoutControlItem6";
            this.layoutControlItem6.Size = new System.Drawing.Size(130, 23);
            this.layoutControlItem6.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem6.TextVisible = false;
            // 
            // layoutControlItem7
            // 
            this.layoutControlItem7.Control = this.CheckAxialFanPara;
            this.layoutControlItem7.Location = new System.Drawing.Point(130, 173);
            this.layoutControlItem7.Name = "layoutControlItem7";
            this.layoutControlItem7.Size = new System.Drawing.Size(131, 23);
            this.layoutControlItem7.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem7.TextVisible = false;
            // 
            // layoutControlItem5
            // 
            this.layoutControlItem5.Control = this.CheckFanParameters;
            this.layoutControlItem5.Location = new System.Drawing.Point(130, 150);
            this.layoutControlItem5.Name = "layoutControlItem5";
            this.layoutControlItem5.Size = new System.Drawing.Size(131, 23);
            this.layoutControlItem5.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem5.TextVisible = false;
            // 
            // BGWorker
            // 
            this.BGWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.BGWorker_DoWork);
            this.BGWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.BGWorker_RunWorkerCompleted);
            // 
            // Panel
            // 
            this.Panel.ContentImage = ((System.Drawing.Image)(resources.GetObject("Panel.ContentImage")));
            this.Panel.Controls.Add(this.labelExcelFile);
            this.Panel.Location = new System.Drawing.Point(12, 12);
            this.Panel.Name = "Panel";
            this.Panel.Size = new System.Drawing.Size(257, 146);
            this.Panel.TabIndex = 4;
            this.Panel.DoubleClick += new System.EventHandler(this.Panel_DoubleClick);
            // 
            // labelExcelFile
            // 
            this.labelExcelFile.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.labelExcelFile.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.labelExcelFile.ForeColor = System.Drawing.Color.White;
            this.labelExcelFile.Location = new System.Drawing.Point(2, 115);
            this.labelExcelFile.Name = "labelExcelFile";
            this.labelExcelFile.Size = new System.Drawing.Size(253, 29);
            this.labelExcelFile.TabIndex = 0;
            this.labelExcelFile.Text = "Open you .xlsx file here! ";
            this.labelExcelFile.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // layoutControlItem1
            // 
            this.layoutControlItem1.Control = this.Panel;
            this.layoutControlItem1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlItem1.Name = "layoutControlItem1";
            this.layoutControlItem1.Size = new System.Drawing.Size(261, 150);
            this.layoutControlItem1.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem1.TextVisible = false;
            // 
            // fmConvert
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(281, 242);
            this.Controls.Add(this.layoutControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "fmConvert";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "模型转换";
            this.Load += new System.EventHandler(this.fmConvert_Load);
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).EndInit();
            this.layoutControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.CheckAxialFanPara.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CheckAxialFan.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CheckFanParameters.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CheckFanSelection.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem7)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Panel)).EndInit();
            this.Panel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraLayout.LayoutControl layoutControl1;
        private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup1;
        private DevExpress.XtraEditors.SimpleButton BtnCancel;
        private DevExpress.XtraEditors.SimpleButton BtnOK;
        private DevExpress.XtraEditors.PanelControl Panel;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem1;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem3;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem4;
        private System.ComponentModel.BackgroundWorker BGWorker;
        private System.Windows.Forms.Label labelExcelFile;
        private DevExpress.XtraEditors.CheckEdit CheckAxialFanPara;
        private DevExpress.XtraEditors.CheckEdit CheckAxialFan;
        private DevExpress.XtraEditors.CheckEdit CheckFanParameters;
        private DevExpress.XtraEditors.CheckEdit CheckFanSelection;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem2;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem6;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem7;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem5;
    }
}