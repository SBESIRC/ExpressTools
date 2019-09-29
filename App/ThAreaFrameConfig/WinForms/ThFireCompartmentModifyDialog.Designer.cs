namespace ThAreaFrameConfig.WinForms
{
    partial class ThFireCompartmentModifyDialog
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
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_storey = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBox_self_extinguishing_system = new System.Windows.Forms.ComboBox();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.button_cancel = new System.Windows.Forms.Button();
            this.button_ok = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 14);
            this.label1.TabIndex = 0;
            this.label1.Text = "楼层：";
            // 
            // textBox_storey
            // 
            this.textBox_storey.Location = new System.Drawing.Point(70, 12);
            this.textBox_storey.Name = "textBox_storey";
            this.textBox_storey.Size = new System.Drawing.Size(159, 22);
            this.textBox_storey.TabIndex = 1;
            this.textBox_storey.Validating += new System.ComponentModel.CancelEventHandler(this.textBox_storey_Validating);
            this.textBox_storey.Validated += new System.EventHandler(this.textBox_storey_Validated);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(17, 44);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(115, 14);
            this.label2.TabIndex = 2;
            this.label2.Text = "是否自动灭火系统：";
            // 
            // comboBox_self_extinguishing_system
            // 
            this.comboBox_self_extinguishing_system.FormattingEnabled = true;
            this.comboBox_self_extinguishing_system.Items.AddRange(new object[] {
            "是",
            "否"});
            this.comboBox_self_extinguishing_system.Location = new System.Drawing.Point(156, 41);
            this.comboBox_self_extinguishing_system.Name = "comboBox_self_extinguishing_system";
            this.comboBox_self_extinguishing_system.Size = new System.Drawing.Size(73, 22);
            this.comboBox_self_extinguishing_system.TabIndex = 3;
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            // 
            // button_cancel
            // 
            this.button_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_cancel.Location = new System.Drawing.Point(169, 69);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(60, 23);
            this.button_cancel.TabIndex = 4;
            this.button_cancel.Text = "取消";
            this.button_cancel.UseVisualStyleBackColor = true;
            // 
            // button_ok
            // 
            this.button_ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button_ok.Location = new System.Drawing.Point(103, 69);
            this.button_ok.Name = "button_ok";
            this.button_ok.Size = new System.Drawing.Size(60, 23);
            this.button_ok.TabIndex = 5;
            this.button_ok.Text = "确定";
            this.button_ok.UseVisualStyleBackColor = true;
            // 
            // ThFireCompartmentModifyDialog
            // 
            this.AcceptButton = this.button_ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_cancel;
            this.ClientSize = new System.Drawing.Size(242, 102);
            this.Controls.Add(this.button_ok);
            this.Controls.Add(this.button_cancel);
            this.Controls.Add(this.comboBox_self_extinguishing_system);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_storey);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ThFireCompartmentModifyDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "批量修改";
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_storey;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBox_self_extinguishing_system;
        private System.Windows.Forms.ErrorProvider errorProvider1;
        private System.Windows.Forms.Button button_ok;
        private System.Windows.Forms.Button button_cancel;
    }
}