namespace ThSitePlan
{
    partial class ThSitePlanForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ThSitePlanForm));
            this.ShadowUpdBt = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.ShadowAngleSetBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.ShadowLengthSetBox = new System.Windows.Forms.TextBox();
            this.ShadowGroup = new System.Windows.Forms.GroupBox();
            this.LandTreeGroup = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.LandTreeUpdBt = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.TreeDensitySetBox = new System.Windows.Forms.TextBox();
            this.TreeRadiusSetBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.ConfirmBt = new System.Windows.Forms.Button();
            this.CancelBt = new System.Windows.Forms.Button();
            this.HelpBt = new System.Windows.Forms.Button();
            this.BroseBt = new System.Windows.Forms.Button();
            this.WorkPathSetBox = new ThSitePlan.PlaceholderTextBox();
            this.ShadowGroup.SuspendLayout();
            this.LandTreeGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // ShadowUpdBt
            // 
            this.ShadowUpdBt.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.ShadowUpdBt.Location = new System.Drawing.Point(72, 32);
            this.ShadowUpdBt.Name = "ShadowUpdBt";
            this.ShadowUpdBt.Size = new System.Drawing.Size(48, 25);
            this.ShadowUpdBt.TabIndex = 4;
            this.ShadowUpdBt.Text = "刷新";
            this.ShadowUpdBt.UseVisualStyleBackColor = true;
            this.ShadowUpdBt.Click += new System.EventHandler(this.ShadowUpdBt_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(6, 38);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "阴影设置:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(6, 75);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "阴影角度:";
            // 
            // ShadowAngleSetBox
            // 
            this.ShadowAngleSetBox.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.ShadowAngleSetBox.Location = new System.Drawing.Point(72, 72);
            this.ShadowAngleSetBox.Name = "ShadowAngleSetBox";
            this.ShadowAngleSetBox.Size = new System.Drawing.Size(72, 21);
            this.ShadowAngleSetBox.TabIndex = 2;
            this.ShadowAngleSetBox.MouseEnter += new System.EventHandler(this.ShadowAngleHelp);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(167, 75);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(59, 12);
            this.label3.TabIndex = 1;
            this.label3.Text = "长度系数:";
            // 
            // ShadowLengthSetBox
            // 
            this.ShadowLengthSetBox.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.ShadowLengthSetBox.Location = new System.Drawing.Point(232, 72);
            this.ShadowLengthSetBox.Name = "ShadowLengthSetBox";
            this.ShadowLengthSetBox.Size = new System.Drawing.Size(73, 21);
            this.ShadowLengthSetBox.TabIndex = 3;
            this.ShadowLengthSetBox.MouseEnter += new System.EventHandler(this.ShadowLengthHelp);
            // 
            // ShadowGroup
            // 
            this.ShadowGroup.Controls.Add(this.label1);
            this.ShadowGroup.Controls.Add(this.ShadowLengthSetBox);
            this.ShadowGroup.Controls.Add(this.ShadowUpdBt);
            this.ShadowGroup.Controls.Add(this.ShadowAngleSetBox);
            this.ShadowGroup.Controls.Add(this.label2);
            this.ShadowGroup.Controls.Add(this.label3);
            this.ShadowGroup.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.ShadowGroup.Location = new System.Drawing.Point(12, 12);
            this.ShadowGroup.Name = "ShadowGroup";
            this.ShadowGroup.Size = new System.Drawing.Size(323, 107);
            this.ShadowGroup.TabIndex = 1;
            this.ShadowGroup.TabStop = false;
            this.ShadowGroup.Text = "阴影设置";
            // 
            // LandTreeGroup
            // 
            this.LandTreeGroup.Controls.Add(this.label4);
            this.LandTreeGroup.Controls.Add(this.LandTreeUpdBt);
            this.LandTreeGroup.Controls.Add(this.label5);
            this.LandTreeGroup.Controls.Add(this.TreeDensitySetBox);
            this.LandTreeGroup.Controls.Add(this.TreeRadiusSetBox);
            this.LandTreeGroup.Controls.Add(this.label6);
            this.LandTreeGroup.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.LandTreeGroup.Location = new System.Drawing.Point(12, 134);
            this.LandTreeGroup.Name = "LandTreeGroup";
            this.LandTreeGroup.Size = new System.Drawing.Size(323, 106);
            this.LandTreeGroup.TabIndex = 5;
            this.LandTreeGroup.TabStop = false;
            this.LandTreeGroup.Text = "行道树设置";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label4.Location = new System.Drawing.Point(6, 38);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(71, 12);
            this.label4.TabIndex = 1;
            this.label4.Text = "行道树设置:";
            // 
            // LandTreeUpdBt
            // 
            this.LandTreeUpdBt.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.LandTreeUpdBt.Location = new System.Drawing.Point(83, 32);
            this.LandTreeUpdBt.Name = "LandTreeUpdBt";
            this.LandTreeUpdBt.Size = new System.Drawing.Size(48, 25);
            this.LandTreeUpdBt.TabIndex = 8;
            this.LandTreeUpdBt.Text = "刷新";
            this.LandTreeUpdBt.UseVisualStyleBackColor = true;
            this.LandTreeUpdBt.Click += new System.EventHandler(this.LandTreeUpdBt_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label5.Location = new System.Drawing.Point(6, 75);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(59, 12);
            this.label5.TabIndex = 1;
            this.label5.Text = "树木半径:";
            // 
            // TreeDensitySetBox
            // 
            this.TreeDensitySetBox.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.TreeDensitySetBox.Location = new System.Drawing.Point(232, 72);
            this.TreeDensitySetBox.Name = "TreeDensitySetBox";
            this.TreeDensitySetBox.Size = new System.Drawing.Size(72, 21);
            this.TreeDensitySetBox.TabIndex = 7;
            this.TreeDensitySetBox.MouseEnter += new System.EventHandler(this.TreeDensityHelp);
            // 
            // TreeRadiusSetBox
            // 
            this.TreeRadiusSetBox.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.TreeRadiusSetBox.Location = new System.Drawing.Point(72, 72);
            this.TreeRadiusSetBox.Name = "TreeRadiusSetBox";
            this.TreeRadiusSetBox.Size = new System.Drawing.Size(72, 21);
            this.TreeRadiusSetBox.TabIndex = 6;
            this.TreeRadiusSetBox.MouseEnter += new System.EventHandler(this.TreeRadiusHelp);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label6.Location = new System.Drawing.Point(167, 75);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(59, 12);
            this.label6.TabIndex = 1;
            this.label6.Text = "密度系数:";
            // 
            // ConfirmBt
            // 
            this.ConfirmBt.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.ConfirmBt.Location = new System.Drawing.Point(192, 297);
            this.ConfirmBt.Name = "ConfirmBt";
            this.ConfirmBt.Size = new System.Drawing.Size(68, 21);
            this.ConfirmBt.TabIndex = 0;
            this.ConfirmBt.Text = "确定";
            this.ConfirmBt.UseVisualStyleBackColor = true;
            this.ConfirmBt.Click += new System.EventHandler(this.ConfirmBt_Click);
            // 
            // CancelBt
            // 
            this.CancelBt.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.CancelBt.Location = new System.Drawing.Point(118, 297);
            this.CancelBt.Name = "CancelBt";
            this.CancelBt.Size = new System.Drawing.Size(68, 21);
            this.CancelBt.TabIndex = 9;
            this.CancelBt.Text = "重置";
            this.CancelBt.UseVisualStyleBackColor = true;
            this.CancelBt.Click += new System.EventHandler(this.CancelBt_Click);
            // 
            // HelpBt
            // 
            this.HelpBt.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.HelpBt.Location = new System.Drawing.Point(266, 297);
            this.HelpBt.Name = "HelpBt";
            this.HelpBt.Size = new System.Drawing.Size(68, 21);
            this.HelpBt.TabIndex = 10;
            this.HelpBt.Text = "帮助";
            this.HelpBt.UseVisualStyleBackColor = true;
            // 
            // BroseBt
            // 
            this.BroseBt.Location = new System.Drawing.Point(278, 258);
            this.BroseBt.Name = "BroseBt";
            this.BroseBt.Size = new System.Drawing.Size(57, 21);
            this.BroseBt.TabIndex = 12;
            this.BroseBt.Text = "浏 览";
            this.BroseBt.UseVisualStyleBackColor = true;
            this.BroseBt.Click += new System.EventHandler(this.BroseBt_Click);
            // 
            // WorkPathSetBox
            // 
            this.WorkPathSetBox.Location = new System.Drawing.Point(12, 258);
            this.WorkPathSetBox.Name = "WorkPathSetBox";
            this.WorkPathSetBox.Size = new System.Drawing.Size(249, 21);
            this.WorkPathSetBox.TabIndex = 13;
            this.WorkPathSetBox.WatermarkText = "请指定文件保存路径";
            // 
            // ThSitePlanForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(346, 331);
            this.Controls.Add(this.WorkPathSetBox);
            this.Controls.Add(this.BroseBt);
            this.Controls.Add(this.HelpBt);
            this.Controls.Add(this.CancelBt);
            this.Controls.Add(this.ConfirmBt);
            this.Controls.Add(this.LandTreeGroup);
            this.Controls.Add(this.ShadowGroup);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "ThSitePlanForm";
            this.Text = "ThSitePlanForm";
            this.Load += new System.EventHandler(this.ThSitePlanForm_Load);
            this.ShadowGroup.ResumeLayout(false);
            this.ShadowGroup.PerformLayout();
            this.LandTreeGroup.ResumeLayout(false);
            this.LandTreeGroup.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button ShadowUpdBt;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox ShadowAngleSetBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox ShadowLengthSetBox;
        private System.Windows.Forms.GroupBox ShadowGroup;
        private System.Windows.Forms.GroupBox LandTreeGroup;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button ConfirmBt;
        private System.Windows.Forms.Button CancelBt;
        private System.Windows.Forms.Button HelpBt;
        private System.Windows.Forms.Button LandTreeUpdBt;
        private System.Windows.Forms.TextBox TreeDensitySetBox;
        private System.Windows.Forms.TextBox TreeRadiusSetBox;
        private System.Windows.Forms.Button BroseBt;
        private PlaceholderTextBox WorkPathSetBox;
    }
}