namespace ThSpray
{
    partial class SprayParam
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
            this.label1 = new System.Windows.Forms.Label();
            this.comboSprayType = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtSprayMin = new System.Windows.Forms.TextBox();
            this.txtSprayMax = new System.Windows.Forms.TextBox();
            this.txtWallMax = new System.Windows.Forms.TextBox();
            this.txtWallMin = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.txtBeamMax = new System.Windows.Forms.TextBox();
            this.txtBeamMin = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnPlace = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(26, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "喷头类型：";
            // 
            // comboSprayType
            // 
            this.comboSprayType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboSprayType.FormattingEnabled = true;
            this.comboSprayType.Items.AddRange(new object[] {
            "上喷",
            "下喷"});
            this.comboSprayType.Location = new System.Drawing.Point(95, 23);
            this.comboSprayType.Name = "comboSprayType";
            this.comboSprayType.Size = new System.Drawing.Size(64, 20);
            this.comboSprayType.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 27);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "喷头间距：";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(144, 30);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 12);
            this.label3.TabIndex = 3;
            this.label3.Text = "mm -";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(241, 30);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(17, 12);
            this.label4.TabIndex = 4;
            this.label4.Text = "mm";
            // 
            // txtSprayMin
            // 
            this.txtSprayMin.Location = new System.Drawing.Point(80, 24);
            this.txtSprayMin.Name = "txtSprayMin";
            this.txtSprayMin.Size = new System.Drawing.Size(59, 21);
            this.txtSprayMin.TabIndex = 5;
            this.txtSprayMin.Text = "1800";
            this.txtSprayMin.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtSprayMax
            // 
            this.txtSprayMax.Location = new System.Drawing.Point(180, 27);
            this.txtSprayMax.Name = "txtSprayMax";
            this.txtSprayMax.Size = new System.Drawing.Size(59, 21);
            this.txtSprayMax.TabIndex = 6;
            this.txtSprayMax.Text = "3400";
            this.txtSprayMax.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtWallMax
            // 
            this.txtWallMax.Location = new System.Drawing.Point(180, 68);
            this.txtWallMax.Name = "txtWallMax";
            this.txtWallMax.Size = new System.Drawing.Size(59, 21);
            this.txtWallMax.TabIndex = 11;
            this.txtWallMax.Text = "1700";
            this.txtWallMax.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtWallMin
            // 
            this.txtWallMin.Location = new System.Drawing.Point(80, 65);
            this.txtWallMin.Name = "txtWallMin";
            this.txtWallMin.Size = new System.Drawing.Size(59, 21);
            this.txtWallMin.TabIndex = 10;
            this.txtWallMin.Text = "100";
            this.txtWallMin.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(241, 71);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(17, 12);
            this.label5.TabIndex = 9;
            this.label5.Text = "mm";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(144, 71);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(29, 12);
            this.label6.TabIndex = 8;
            this.label6.Text = "mm -";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(14, 68);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(65, 12);
            this.label7.TabIndex = 7;
            this.label7.Text = "距墙距离：";
            // 
            // txtBeamMax
            // 
            this.txtBeamMax.Location = new System.Drawing.Point(180, 106);
            this.txtBeamMax.Name = "txtBeamMax";
            this.txtBeamMax.Size = new System.Drawing.Size(59, 21);
            this.txtBeamMax.TabIndex = 16;
            this.txtBeamMax.Text = "1800";
            this.txtBeamMax.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtBeamMin
            // 
            this.txtBeamMin.Location = new System.Drawing.Point(80, 103);
            this.txtBeamMin.Name = "txtBeamMin";
            this.txtBeamMin.Size = new System.Drawing.Size(59, 21);
            this.txtBeamMin.TabIndex = 15;
            this.txtBeamMin.Text = "150";
            this.txtBeamMin.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(241, 109);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(17, 12);
            this.label8.TabIndex = 14;
            this.label8.Text = "mm";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(144, 109);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(29, 12);
            this.label9.TabIndex = 13;
            this.label9.Text = "mm -";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(14, 106);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(65, 12);
            this.label10.TabIndex = 12;
            this.label10.Text = "距梁距离：";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.txtBeamMax);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.txtBeamMin);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.txtSprayMin);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.txtSprayMax);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.txtWallMax);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.txtWallMin);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Location = new System.Drawing.Point(12, 72);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(284, 151);
            this.groupBox1.TabIndex = 17;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "距离约束";
            // 
            // btnPlace
            // 
            this.btnPlace.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnPlace.Location = new System.Drawing.Point(126, 241);
            this.btnPlace.Name = "btnPlace";
            this.btnPlace.Size = new System.Drawing.Size(75, 23);
            this.btnPlace.TabIndex = 18;
            this.btnPlace.Text = "布置";
            this.btnPlace.UseVisualStyleBackColor = true;
            this.btnPlace.Click += new System.EventHandler(this.btnPlace_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(221, 241);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 19;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // SprayParam
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(302, 276);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnPlace);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.comboSprayType);
            this.Controls.Add(this.label1);
            this.Name = "SprayParam";
            this.Text = "喷头布置";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboSprayType;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtSprayMin;
        private System.Windows.Forms.TextBox txtSprayMax;
        private System.Windows.Forms.TextBox txtWallMax;
        private System.Windows.Forms.TextBox txtWallMin;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtBeamMax;
        private System.Windows.Forms.TextBox txtBeamMin;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnPlace;
        private System.Windows.Forms.Button btnCancel;
    }
}