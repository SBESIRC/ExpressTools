namespace ThAreaFrameConfig.WinForms
{
    partial class ThFireProofingDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ThFireProofingDialog));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage_commerce = new System.Windows.Forms.TabPage();
            this.commerceFireProofControl = new ThAreaFrameConfig.WinForms.ThCommerceFireProofControl();
            this.tabPage_underground_parking = new System.Windows.Forms.TabPage();
            this.undergroundParkingFireProofControl = new ThAreaFrameConfig.WinForms.ThUndergroundParkingFireProofControl();
            this.tabControl1.SuspendLayout();
            this.tabPage_commerce.SuspendLayout();
            this.tabPage_underground_parking.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage_commerce);
            this.tabControl1.Controls.Add(this.tabPage_underground_parking);
            this.tabControl1.Location = new System.Drawing.Point(15, 15);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(517, 565);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage_commerce
            // 
            this.tabPage_commerce.Controls.Add(this.commerceFireProofControl);
            this.tabPage_commerce.Location = new System.Drawing.Point(4, 23);
            this.tabPage_commerce.Name = "tabPage_commerce";
            this.tabPage_commerce.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_commerce.Size = new System.Drawing.Size(509, 538);
            this.tabPage_commerce.TabIndex = 0;
            this.tabPage_commerce.Text = "商业";
            this.tabPage_commerce.UseVisualStyleBackColor = true;
            // 
            // commerceFireProofControl
            // 
            this.commerceFireProofControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.commerceFireProofControl.Location = new System.Drawing.Point(3, 3);
            this.commerceFireProofControl.Name = "commerceFireProofControl";
            this.commerceFireProofControl.Size = new System.Drawing.Size(503, 532);
            this.commerceFireProofControl.TabIndex = 0;
            // 
            // tabPage_underground_parking
            // 
            this.tabPage_underground_parking.Controls.Add(this.undergroundParkingFireProofControl);
            this.tabPage_underground_parking.Location = new System.Drawing.Point(4, 23);
            this.tabPage_underground_parking.Name = "tabPage_underground_parking";
            this.tabPage_underground_parking.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_underground_parking.Size = new System.Drawing.Size(509, 538);
            this.tabPage_underground_parking.TabIndex = 1;
            this.tabPage_underground_parking.Text = "地下车库";
            this.tabPage_underground_parking.UseVisualStyleBackColor = true;
            // 
            // undergroundParkingFireProofControl
            // 
            this.undergroundParkingFireProofControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.undergroundParkingFireProofControl.Location = new System.Drawing.Point(3, 3);
            this.undergroundParkingFireProofControl.Name = "undergroundParkingFireProofControl";
            this.undergroundParkingFireProofControl.Size = new System.Drawing.Size(503, 532);
            this.undergroundParkingFireProofControl.TabIndex = 0;
            // 
            // ThFireProofingDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(539, 589);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ThFireProofingDialog";
            this.Text = "防火分区疏散表";
            this.tabControl1.ResumeLayout(false);
            this.tabPage_commerce.ResumeLayout(false);
            this.tabPage_underground_parking.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage_underground_parking;
        private System.Windows.Forms.TabPage tabPage_commerce;
        private ThCommerceFireProofControl commerceFireProofControl;
        private ThUndergroundParkingFireProofControl undergroundParkingFireProofControl;
    }
}