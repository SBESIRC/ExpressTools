namespace ThSitePlan.UI
{
    partial class ProgressBarForm
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
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.label1 = new System.Windows.Forms.Label();
            this.ProgressInfo = new System.Windows.Forms.Label();
            this.AppInfoBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.Confirm_Btn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(10, 34);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(383, 23);
            this.progressBar1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "总进度：";
            // 
            // ProgressInfo
            // 
            this.ProgressInfo.AutoSize = true;
            this.ProgressInfo.Location = new System.Drawing.Point(12, 60);
            this.ProgressInfo.Name = "ProgressInfo";
            this.ProgressInfo.Size = new System.Drawing.Size(47, 12);
            this.ProgressInfo.TabIndex = 2;
            this.ProgressInfo.Text = "... ...";
            // 
            // AppInfoBox
            // 
            this.AppInfoBox.Location = new System.Drawing.Point(10, 118);
            this.AppInfoBox.Multiline = true;
            this.AppInfoBox.Name = "AppInfoBox";
            this.AppInfoBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.AppInfoBox.Size = new System.Drawing.Size(383, 85);
            this.AppInfoBox.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 94);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "运行信息：";
            // 
            // Confirm_Btn
            // 
            this.Confirm_Btn.Location = new System.Drawing.Point(155, 209);
            this.Confirm_Btn.Name = "Confirm_Btn";
            this.Confirm_Btn.Size = new System.Drawing.Size(75, 23);
            this.Confirm_Btn.TabIndex = 5;
            this.Confirm_Btn.Text = "完成";
            this.Confirm_Btn.UseVisualStyleBackColor = true;
            this.Confirm_Btn.Click += new System.EventHandler(this.Confirm_Btn_Click);
            // 
            // ProgressBarForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(404, 241);
            this.Controls.Add(this.Confirm_Btn);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.AppInfoBox);
            this.Controls.Add(this.ProgressInfo);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.progressBar1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "ProgressBarForm";
            this.Text = "一键彩总";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label ProgressInfo;
        private System.Windows.Forms.TextBox AppInfoBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button Confirm_Btn;
    }
}