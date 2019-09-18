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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage_commerce = new System.Windows.Forms.TabPage();
            this.commerceFireProofControl = new ThAreaFrameConfig.WinForms.ThCommerceFireProofControl();
            this.tabPage_underground_parking = new System.Windows.Forms.TabPage();
            this.undergroundParkingFireProofControl = new ThAreaFrameConfig.WinForms.ThUndergroundParkingFireProofControl();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabPage_commerce.SuspendLayout();
            this.tabPage_underground_parking.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage_commerce);
            this.tabControl1.Controls.Add(this.tabPage_underground_parking);
            this.tabControl1.Location = new System.Drawing.Point(13, 13);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(511, 644);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage_commerce
            // 
            this.tabPage_commerce.Controls.Add(this.commerceFireProofControl);
            this.tabPage_commerce.Location = new System.Drawing.Point(4, 22);
            this.tabPage_commerce.Name = "tabPage_commerce";
            this.tabPage_commerce.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_commerce.Size = new System.Drawing.Size(503, 618);
            this.tabPage_commerce.TabIndex = 0;
            this.tabPage_commerce.Text = "商业";
            this.tabPage_commerce.UseVisualStyleBackColor = true;
            // 
            // commerceFireProofControl
            // 
            this.commerceFireProofControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.commerceFireProofControl.Location = new System.Drawing.Point(3, 3);
            this.commerceFireProofControl.Name = "commerceFireProofControl";
            this.commerceFireProofControl.Size = new System.Drawing.Size(497, 612);
            this.commerceFireProofControl.TabIndex = 0;
            // 
            // tabPage_underground_parking
            // 
            this.tabPage_underground_parking.Controls.Add(this.undergroundParkingFireProofControl);
            this.tabPage_underground_parking.Location = new System.Drawing.Point(4, 22);
            this.tabPage_underground_parking.Name = "tabPage_underground_parking";
            this.tabPage_underground_parking.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_underground_parking.Size = new System.Drawing.Size(503, 618);
            this.tabPage_underground_parking.TabIndex = 1;
            this.tabPage_underground_parking.Text = "地下车库";
            this.tabPage_underground_parking.UseVisualStyleBackColor = true;
            // 
            // undergroundParkingFireProofControl
            // 
            this.undergroundParkingFireProofControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.undergroundParkingFireProofControl.Location = new System.Drawing.Point(3, 3);
            this.undergroundParkingFireProofControl.Name = "undergroundParkingFireProofControl";
            this.undergroundParkingFireProofControl.Size = new System.Drawing.Size(497, 612);
            this.undergroundParkingFireProofControl.TabIndex = 0;
            // 
            // button_Cancel
            // 
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(430, 663);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(94, 23);
            this.button_Cancel.TabIndex = 1;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            // 
            // button_OK
            // 
            this.button_OK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button_OK.Location = new System.Drawing.Point(330, 663);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(94, 23);
            this.button_OK.TabIndex = 2;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            // 
            // ThFireProofingDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(531, 696);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.tabControl1);
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
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.TabPage tabPage_commerce;
        private ThCommerceFireProofControl commerceFireProofControl;
        private ThUndergroundParkingFireProofControl undergroundParkingFireProofControl;
    }
}