namespace TianHua.AutoCAD.ThCui
{
    partial class ThToolPalette
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.lstCurrent = new System.Windows.Forms.ListBox();
            this.lstSource = new System.Windows.Forms.ListBox();
            this.btnUpLoad = new System.Windows.Forms.Button();
            this.btnDownLoad = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnAllOn = new System.Windows.Forms.Button();
            this.btnAllDown = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // lstCurrent
            // 
            this.lstCurrent.FormattingEnabled = true;
            this.lstCurrent.ItemHeight = 18;
            this.lstCurrent.Location = new System.Drawing.Point(25, 27);
            this.lstCurrent.Name = "lstCurrent";
            this.lstCurrent.Size = new System.Drawing.Size(217, 184);
            this.lstCurrent.TabIndex = 4;
            this.lstCurrent.SelectedIndexChanged += new System.EventHandler(this.lstCurrent_SelectedIndexChanged);
            // 
            // lstSource
            // 
            this.lstSource.FormattingEnabled = true;
            this.lstSource.ItemHeight = 18;
            this.lstSource.Location = new System.Drawing.Point(20, 27);
            this.lstSource.Name = "lstSource";
            this.lstSource.Size = new System.Drawing.Size(211, 184);
            this.lstSource.TabIndex = 5;
            this.lstSource.SelectedIndexChanged += new System.EventHandler(this.lstSource_SelectedIndexChanged);
            // 
            // btnUpLoad
            // 
            this.btnUpLoad.Location = new System.Drawing.Point(347, 86);
            this.btnUpLoad.Name = "btnUpLoad";
            this.btnUpLoad.Size = new System.Drawing.Size(75, 46);
            this.btnUpLoad.TabIndex = 6;
            this.btnUpLoad.Text = ">>";
            this.btnUpLoad.UseVisualStyleBackColor = true;
            this.btnUpLoad.Click += new System.EventHandler(this.btnUpLoad_Click);
            // 
            // btnDownLoad
            // 
            this.btnDownLoad.Location = new System.Drawing.Point(347, 154);
            this.btnDownLoad.Name = "btnDownLoad";
            this.btnDownLoad.Size = new System.Drawing.Size(75, 46);
            this.btnDownLoad.TabIndex = 8;
            this.btnDownLoad.Text = "<<";
            this.btnDownLoad.UseVisualStyleBackColor = true;
            this.btnDownLoad.Click += new System.EventHandler(this.btnDownLoad_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lstSource);
            this.groupBox1.Location = new System.Drawing.Point(46, 59);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(253, 231);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "可加载天华图块集：";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.lstCurrent);
            this.groupBox2.Location = new System.Drawing.Point(475, 59);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(261, 231);
            this.groupBox2.TabIndex = 10;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "已加载天华图块集：";
            // 
            // btnAllOn
            // 
            this.btnAllOn.Location = new System.Drawing.Point(250, 310);
            this.btnAllOn.Name = "btnAllOn";
            this.btnAllOn.Size = new System.Drawing.Size(108, 51);
            this.btnAllOn.TabIndex = 11;
            this.btnAllOn.Text = "全部加载";
            this.btnAllOn.UseVisualStyleBackColor = true;
            this.btnAllOn.Click += new System.EventHandler(this.btnAllOn_Click);
            // 
            // btnAllDown
            // 
            this.btnAllDown.Location = new System.Drawing.Point(417, 310);
            this.btnAllDown.Name = "btnAllDown";
            this.btnAllDown.Size = new System.Drawing.Size(108, 51);
            this.btnAllDown.TabIndex = 12;
            this.btnAllDown.Text = "全部卸载";
            this.btnAllDown.UseVisualStyleBackColor = true;
            this.btnAllDown.Click += new System.EventHandler(this.btnAllDown_Click);
            // 
            // btnExit
            // 
            this.btnExit.Location = new System.Drawing.Point(334, 378);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(108, 51);
            this.btnExit.TabIndex = 13;
            this.btnExit.Text = "退出";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // btnSave
            // 
            this.btnSave.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnSave.Location = new System.Drawing.Point(334, 224);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(107, 46);
            this.btnSave.TabIndex = 14;
            this.btnSave.Text = "保存配置";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // FrmTuKu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.btnAllDown);
            this.Controls.Add(this.btnAllOn);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnDownLoad);
            this.Controls.Add(this.btnUpLoad);
            this.Name = "FrmTuKu";
            this.Text = "天华图块集配置工具";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox lstCurrent;
        private System.Windows.Forms.ListBox lstSource;
        private System.Windows.Forms.Button btnUpLoad;
        private System.Windows.Forms.Button btnDownLoad;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnAllOn;
        private System.Windows.Forms.Button btnAllDown;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Button btnSave;
    }
}

