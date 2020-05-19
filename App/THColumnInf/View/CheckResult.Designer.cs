namespace ThColumnInfo.View
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
            this.components = new System.ComponentModel.Container();
            this.tvCheckRes = new System.Windows.Forms.TreeView();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.importCalculationTsmi = new System.Windows.Forms.ToolStripMenuItem();
            this.checkTsmi = new System.Windows.Forms.ToolStripMenuItem();
            this.detailDataTsmi = new System.Windows.Forms.ToolStripMenuItem();
            this.panelMiddle = new System.Windows.Forms.Panel();
            this.lblPaperDistinguishResult = new System.Windows.Forms.Label();
            this.panelDown = new System.Windows.Forms.Panel();
            this.btnShowDetailData = new System.Windows.Forms.Button();
            this.btnComponentDefinition = new System.Windows.Forms.Button();
            this.contextMenuStrip2 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.parameterSetTsmi = new System.Windows.Forms.ToolStripMenuItem();
            this.checkAllTsmi = new System.Windows.Forms.ToolStripMenuItem();
            this.panelUp = new System.Windows.Forms.Panel();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.toolTip2 = new System.Windows.Forms.ToolTip(this.components);
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.contextMenuStrip1.SuspendLayout();
            this.panelMiddle.SuspendLayout();
            this.panelDown.SuspendLayout();
            this.contextMenuStrip2.SuspendLayout();
            this.panelUp.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tvCheckRes
            // 
            this.tvCheckRes.BackColor = System.Drawing.SystemColors.ControlDark;
            this.tvCheckRes.ContextMenuStrip = this.contextMenuStrip1;
            this.tvCheckRes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvCheckRes.Location = new System.Drawing.Point(0, 0);
            this.tvCheckRes.Name = "tvCheckRes";
            this.tvCheckRes.Size = new System.Drawing.Size(244, 529);
            this.tvCheckRes.TabIndex = 0;
            this.tvCheckRes.BeforeCollapse += new System.Windows.Forms.TreeViewCancelEventHandler(this.tvCheckRes_BeforeCollapse);
            this.tvCheckRes.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.tvCheckRes_BeforeExpand);
            this.tvCheckRes.DrawNode += new System.Windows.Forms.DrawTreeNodeEventHandler(this.tvCheckRes_DrawNode);
            this.tvCheckRes.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.tvCheckRes_NodeMouseClick);
            this.tvCheckRes.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.tvCheckRes_NodeMouseDoubleClick);
            this.tvCheckRes.Leave += new System.EventHandler(this.tvCheckRes_Leave);
            this.tvCheckRes.MouseDown += new System.Windows.Forms.MouseEventHandler(this.tvCheckRes_MouseDown);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.importCalculationTsmi,
            this.checkTsmi,
            this.detailDataTsmi});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(137, 70);
            // 
            // importCalculationTsmi
            // 
            this.importCalculationTsmi.Name = "importCalculationTsmi";
            this.importCalculationTsmi.Size = new System.Drawing.Size(136, 22);
            this.importCalculationTsmi.Text = "导入计算书";
            this.importCalculationTsmi.Click += new System.EventHandler(this.importCalculationTsmi_Click);
            // 
            // checkTsmi
            // 
            this.checkTsmi.Name = "checkTsmi";
            this.checkTsmi.Size = new System.Drawing.Size(136, 22);
            this.checkTsmi.Text = "校核";
            this.checkTsmi.Click += new System.EventHandler(this.checkTsmi_Click);
            // 
            // detailDataTsmi
            // 
            this.detailDataTsmi.Name = "detailDataTsmi";
            this.detailDataTsmi.Size = new System.Drawing.Size(136, 22);
            this.detailDataTsmi.Text = "详细数据";
            this.detailDataTsmi.Click += new System.EventHandler(this.detailDataTsmi_Click);
            // 
            // panelMiddle
            // 
            this.panelMiddle.Controls.Add(this.tvCheckRes);
            this.panelMiddle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMiddle.Location = new System.Drawing.Point(3, 31);
            this.panelMiddle.Name = "panelMiddle";
            this.panelMiddle.Size = new System.Drawing.Size(244, 529);
            this.panelMiddle.TabIndex = 3;
            this.panelMiddle.ControlAdded += new System.Windows.Forms.ControlEventHandler(this.panel1_ControlAdded);
            // 
            // lblPaperDistinguishResult
            // 
            this.lblPaperDistinguishResult.AutoSize = true;
            this.lblPaperDistinguishResult.BackColor = System.Drawing.Color.Transparent;
            this.lblPaperDistinguishResult.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblPaperDistinguishResult.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblPaperDistinguishResult.Location = new System.Drawing.Point(0, 0);
            this.lblPaperDistinguishResult.Name = "lblPaperDistinguishResult";
            this.lblPaperDistinguishResult.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.lblPaperDistinguishResult.Size = new System.Drawing.Size(77, 17);
            this.lblPaperDistinguishResult.TabIndex = 1;
            this.lblPaperDistinguishResult.Text = "图纸识别结果";
            this.lblPaperDistinguishResult.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // panelDown
            // 
            this.panelDown.BackColor = System.Drawing.Color.Transparent;
            this.panelDown.Controls.Add(this.btnShowDetailData);
            this.panelDown.Controls.Add(this.btnComponentDefinition);
            this.panelDown.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelDown.Location = new System.Drawing.Point(3, 566);
            this.panelDown.MaximumSize = new System.Drawing.Size(0, 30);
            this.panelDown.MinimumSize = new System.Drawing.Size(0, 20);
            this.panelDown.Name = "panelDown";
            this.panelDown.Size = new System.Drawing.Size(244, 24);
            this.panelDown.TabIndex = 4;
            // 
            // btnShowDetailData
            // 
            this.btnShowDetailData.BackColor = System.Drawing.Color.Transparent;
            this.btnShowDetailData.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnShowDetailData.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnShowDetailData.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnShowDetailData.FlatAppearance.BorderSize = 0;
            this.btnShowDetailData.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnShowDetailData.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnShowDetailData.ForeColor = System.Drawing.SystemColors.Window;
            this.btnShowDetailData.Location = new System.Drawing.Point(220, 0);
            this.btnShowDetailData.Name = "btnShowDetailData";
            this.btnShowDetailData.Size = new System.Drawing.Size(24, 24);
            this.btnShowDetailData.TabIndex = 4;
            this.btnShowDetailData.UseVisualStyleBackColor = false;
            this.btnShowDetailData.Click += new System.EventHandler(this.btnShowDetailData_Click);
            this.btnShowDetailData.MouseHover += new System.EventHandler(this.btnShowDetailData_MouseHover);
            // 
            // btnComponentDefinition
            // 
            this.btnComponentDefinition.BackColor = System.Drawing.Color.Transparent;
            this.btnComponentDefinition.BackgroundImage = global::ThColumnInfo.Properties.Resources.ComponentPropModifyPng;
            this.btnComponentDefinition.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnComponentDefinition.FlatAppearance.BorderColor = System.Drawing.SystemColors.Control;
            this.btnComponentDefinition.FlatAppearance.BorderSize = 0;
            this.btnComponentDefinition.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnComponentDefinition.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnComponentDefinition.ForeColor = System.Drawing.SystemColors.Window;
            this.btnComponentDefinition.Location = new System.Drawing.Point(195, 1);
            this.btnComponentDefinition.Name = "btnComponentDefinition";
            this.btnComponentDefinition.Padding = new System.Windows.Forms.Padding(0, 0, 5, 0);
            this.btnComponentDefinition.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.btnComponentDefinition.Size = new System.Drawing.Size(24, 24);
            this.btnComponentDefinition.TabIndex = 2;
            this.toolTip1.SetToolTip(this.btnComponentDefinition, "构件属性定义");
            this.btnComponentDefinition.UseVisualStyleBackColor = false;
            this.btnComponentDefinition.Click += new System.EventHandler(this.btnComponentDefinition_Click);
            // 
            // contextMenuStrip2
            // 
            this.contextMenuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.parameterSetTsmi,
            this.checkAllTsmi});
            this.contextMenuStrip2.Name = "contextMenuStrip2";
            this.contextMenuStrip2.Size = new System.Drawing.Size(101, 48);
            // 
            // parameterSetTsmi
            // 
            this.parameterSetTsmi.Name = "parameterSetTsmi";
            this.parameterSetTsmi.Size = new System.Drawing.Size(100, 22);
            this.parameterSetTsmi.Text = "设置";
            this.parameterSetTsmi.Click += new System.EventHandler(this.parameterSetTsmi_Click);
            // 
            // checkAllTsmi
            // 
            this.checkAllTsmi.Name = "checkAllTsmi";
            this.checkAllTsmi.Size = new System.Drawing.Size(100, 22);
            this.checkAllTsmi.Text = "校核";
            this.checkAllTsmi.Click += new System.EventHandler(this.distinguishAllTsmi_Click);
            // 
            // panelUp
            // 
            this.panelUp.Controls.Add(this.lblPaperDistinguishResult);
            this.panelUp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelUp.Location = new System.Drawing.Point(3, 3);
            this.panelUp.Name = "panelUp";
            this.panelUp.Size = new System.Drawing.Size(244, 22);
            this.panelUp.TabIndex = 5;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.panelUp, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panelMiddle, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.panelDown, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(250, 593);
            this.tableLayoutPanel1.TabIndex = 5;
            // 
            // CheckResult
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "CheckResult";
            this.Size = new System.Drawing.Size(250, 593);
            this.contextMenuStrip1.ResumeLayout(false);
            this.panelMiddle.ResumeLayout(false);
            this.panelDown.ResumeLayout(false);
            this.contextMenuStrip2.ResumeLayout(false);
            this.panelUp.ResumeLayout(false);
            this.panelUp.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.TreeView tvCheckRes;
        public System.Windows.Forms.Panel panelMiddle;
        public System.Windows.Forms.Panel panelDown;
        private System.Windows.Forms.Label lblPaperDistinguishResult;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem importCalculationTsmi;
        private System.Windows.Forms.ToolStripMenuItem checkTsmi;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip2;
        private System.Windows.Forms.ToolStripMenuItem checkAllTsmi;
        private System.Windows.Forms.ToolStripMenuItem detailDataTsmi;
        private System.Windows.Forms.ToolStripMenuItem parameterSetTsmi;
        private System.Windows.Forms.Button btnComponentDefinition;
        public System.Windows.Forms.Panel panelUp;
        private System.Windows.Forms.Button btnShowDetailData;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ToolTip toolTip2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}
