namespace ThAreaFrameConfig.WinForms
{
    partial class ThResidentialStoreyDialog
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
            this.radioButton_common = new System.Windows.Forms.RadioButton();
            this.radioButton_odd = new System.Windows.Forms.RadioButton();
            this.radioButton_even = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_storey = new System.Windows.Forms.TextBox();
            this.button_ok = new System.Windows.Forms.Button();
            this.button_cancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // radioButton_common
            // 
            this.radioButton_common.AutoSize = true;
            this.radioButton_common.Checked = true;
            this.radioButton_common.Location = new System.Drawing.Point(15, 15);
            this.radioButton_common.Name = "radioButton_common";
            this.radioButton_common.Size = new System.Drawing.Size(61, 18);
            this.radioButton_common.TabIndex = 0;
            this.radioButton_common.TabStop = true;
            this.radioButton_common.Text = "普通层";
            this.radioButton_common.UseVisualStyleBackColor = true;
            // 
            // radioButton_odd
            // 
            this.radioButton_odd.AutoSize = true;
            this.radioButton_odd.Location = new System.Drawing.Point(91, 15);
            this.radioButton_odd.Name = "radioButton_odd";
            this.radioButton_odd.Size = new System.Drawing.Size(61, 18);
            this.radioButton_odd.TabIndex = 1;
            this.radioButton_odd.Text = "奇数层";
            this.radioButton_odd.UseVisualStyleBackColor = true;
            // 
            // radioButton_even
            // 
            this.radioButton_even.AutoSize = true;
            this.radioButton_even.Location = new System.Drawing.Point(167, 15);
            this.radioButton_even.Name = "radioButton_even";
            this.radioButton_even.Size = new System.Drawing.Size(61, 18);
            this.radioButton_even.TabIndex = 2;
            this.radioButton_even.Text = "偶数层";
            this.radioButton_even.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 48);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 14);
            this.label1.TabIndex = 3;
            this.label1.Text = "层名：";
            // 
            // textBox_storey
            // 
            this.textBox_storey.Location = new System.Drawing.Point(68, 44);
            this.textBox_storey.Name = "textBox_storey";
            this.textBox_storey.Size = new System.Drawing.Size(167, 22);
            this.textBox_storey.TabIndex = 4;
            // 
            // button_ok
            // 
            this.button_ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button_ok.Location = new System.Drawing.Point(15, 89);
            this.button_ok.Name = "button_ok";
            this.button_ok.Size = new System.Drawing.Size(87, 27);
            this.button_ok.TabIndex = 5;
            this.button_ok.Text = "确定";
            this.button_ok.UseVisualStyleBackColor = true;
            // 
            // button_cancel
            // 
            this.button_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_cancel.Location = new System.Drawing.Point(148, 89);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(87, 27);
            this.button_cancel.TabIndex = 6;
            this.button_cancel.Text = "取消";
            this.button_cancel.UseVisualStyleBackColor = true;
            // 
            // ThResidentialStoreyDialog
            // 
            this.AcceptButton = this.button_ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_cancel;
            this.ClientSize = new System.Drawing.Size(259, 132);
            this.Controls.Add(this.button_cancel);
            this.Controls.Add(this.button_ok);
            this.Controls.Add(this.textBox_storey);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.radioButton_even);
            this.Controls.Add(this.radioButton_odd);
            this.Controls.Add(this.radioButton_common);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ThResidentialStoreyDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "增加层";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton radioButton_common;
        private System.Windows.Forms.RadioButton radioButton_odd;
        private System.Windows.Forms.RadioButton radioButton_even;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_storey;
        private System.Windows.Forms.Button button_ok;
        private System.Windows.Forms.Button button_cancel;
    }
}