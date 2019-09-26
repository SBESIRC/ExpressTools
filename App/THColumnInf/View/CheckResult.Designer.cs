namespace THColumnInfo.View
{
    partial class CheckResult
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

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.tvCheckRes = new System.Windows.Forms.TreeView();
            this.btnCheck = new System.Windows.Forms.Button();
            this.btnExport = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnSet = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tvCheckRes
            // 
            this.tvCheckRes.BackColor = System.Drawing.SystemColors.ControlDark;
            this.tvCheckRes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvCheckRes.Location = new System.Drawing.Point(0, 0);
            this.tvCheckRes.Name = "tvCheckRes";
            this.tvCheckRes.Size = new System.Drawing.Size(227, 386);
            this.tvCheckRes.TabIndex = 0;
            this.tvCheckRes.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.tvCheckRes_NodeMouseDoubleClick);
            // 
            // btnCheck
            // 
            this.btnCheck.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCheck.BackColor = System.Drawing.SystemColors.ControlDark;
            this.btnCheck.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnCheck.Location = new System.Drawing.Point(95, 102);
            this.btnCheck.Name = "btnCheck";
            this.btnCheck.Size = new System.Drawing.Size(60, 25);
            this.btnCheck.TabIndex = 1;
            this.btnCheck.Text = "检  查";
            this.btnCheck.UseVisualStyleBackColor = false;
            this.btnCheck.Click += new System.EventHandler(this.btnCheck_Click);
            // 
            // btnExport
            // 
            this.btnExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExport.BackColor = System.Drawing.SystemColors.ControlDark;
            this.btnExport.Location = new System.Drawing.Point(161, 102);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(60, 25);
            this.btnExport.TabIndex = 2;
            this.btnExport.Text = "导出";
            this.btnExport.UseVisualStyleBackColor = false;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.tvCheckRes);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(227, 386);
            this.panel1.TabIndex = 3;
            this.panel1.ControlAdded += new System.Windows.Forms.ControlEventHandler(this.panel1_ControlAdded);
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.SystemColors.ControlDark;
            this.panel2.Controls.Add(this.btnSet);
            this.panel2.Controls.Add(this.btnCheck);
            this.panel2.Controls.Add(this.btnExport);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 386);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(227, 131);
            this.panel2.TabIndex = 4;
            // 
            // btnSet
            // 
            this.btnSet.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSet.BackColor = System.Drawing.SystemColors.ControlDark;
            this.btnSet.Location = new System.Drawing.Point(29, 102);
            this.btnSet.Name = "btnSet";
            this.btnSet.Size = new System.Drawing.Size(60, 25);
            this.btnSet.TabIndex = 3;
            this.btnSet.Text = "设置";
            this.btnSet.UseVisualStyleBackColor = false;
            this.btnSet.Click += new System.EventHandler(this.btnSet_Click);
            // 
            // CheckResult
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "CheckResult";
            this.Size = new System.Drawing.Size(227, 517);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.TreeView tvCheckRes;
        private System.Windows.Forms.Button btnCheck;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btnSet;
    }
}
