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
            this.addColumnTableTsmi = new System.Windows.Forms.ToolStripMenuItem();
            this.importCalculationTsmi = new System.Windows.Forms.ToolStripMenuItem();
            this.checkTsmi = new System.Windows.Forms.ToolStripMenuItem();
            this.detailDataTsmi = new System.Windows.Forms.ToolStripMenuItem();
            this.panelUp = new System.Windows.Forms.Panel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.btnExportExcel = new System.Windows.Forms.Button();
            this.btnCheckAll = new System.Windows.Forms.Button();
            this.btnParameterSet = new System.Windows.Forms.Button();
            this.btnShowDetailData = new System.Windows.Forms.Button();
            this.btnComponentDefinition = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.toolTip2 = new System.Windows.Forms.ToolTip(this.components);
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panelTree = new System.Windows.Forms.Panel();
            this.contextMenuStrip1.SuspendLayout();
            this.panelUp.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panelTree.SuspendLayout();
            this.SuspendLayout();
            // 
            // tvCheckRes
            // 
            this.tvCheckRes.BackColor = System.Drawing.SystemColors.ControlDark;
            this.tvCheckRes.ContextMenuStrip = this.contextMenuStrip1;
            this.tvCheckRes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvCheckRes.Location = new System.Drawing.Point(0, 0);
            this.tvCheckRes.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.tvCheckRes.Name = "tvCheckRes";
            this.tvCheckRes.Size = new System.Drawing.Size(244, 555);
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
            this.addColumnTableTsmi,
            this.importCalculationTsmi,
            this.checkTsmi,
            this.detailDataTsmi});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(181, 114);
            // 
            // addColumnTableTsmi
            // 
            this.addColumnTableTsmi.Name = "addColumnTableTsmi";
            this.addColumnTableTsmi.Size = new System.Drawing.Size(180, 22);
            this.addColumnTableTsmi.Text = "附加柱表";
            this.addColumnTableTsmi.Click += new System.EventHandler(this.addColumnTableTsmi_Click);
            // 
            // importCalculationTsmi
            // 
            this.importCalculationTsmi.Name = "importCalculationTsmi";
            this.importCalculationTsmi.Size = new System.Drawing.Size(180, 22);
            this.importCalculationTsmi.Text = "导入计算书";
            this.importCalculationTsmi.Click += new System.EventHandler(this.importCalculationTsmi_Click);
            // 
            // checkTsmi
            // 
            this.checkTsmi.Name = "checkTsmi";
            this.checkTsmi.Size = new System.Drawing.Size(180, 22);
            this.checkTsmi.Text = "校核";
            this.checkTsmi.Click += new System.EventHandler(this.checkTsmi_Click);
            // 
            // detailDataTsmi
            // 
            this.detailDataTsmi.Name = "detailDataTsmi";
            this.detailDataTsmi.Size = new System.Drawing.Size(180, 22);
            this.detailDataTsmi.Text = "详细数据";
            this.detailDataTsmi.Click += new System.EventHandler(this.detailDataTsmi_Click);
            // 
            // panelUp
            // 
            this.panelUp.Controls.Add(this.tableLayoutPanel2);
            this.panelUp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelUp.Location = new System.Drawing.Point(3, 0);
            this.panelUp.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.panelUp.Name = "panelUp";
            this.panelUp.Size = new System.Drawing.Size(244, 32);
            this.panelUp.TabIndex = 3;
            this.panelUp.ControlAdded += new System.Windows.Forms.ControlEventHandler(this.panel1_ControlAdded);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 5;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel2.Controls.Add(this.btnExportExcel, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnCheckAll, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnParameterSet, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnShowDetailData, 4, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnComponentDefinition, 2, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(244, 32);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // btnExportExcel
            // 
            this.btnExportExcel.BackColor = System.Drawing.Color.Transparent;
            this.btnExportExcel.BackgroundImage = global::ThColumnInfo.Properties.Resources.ComponentPropModifyPng;
            this.btnExportExcel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnExportExcel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnExportExcel.FlatAppearance.BorderColor = System.Drawing.SystemColors.Control;
            this.btnExportExcel.FlatAppearance.BorderSize = 0;
            this.btnExportExcel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnExportExcel.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnExportExcel.ForeColor = System.Drawing.SystemColors.Window;
            this.btnExportExcel.Location = new System.Drawing.Point(127, 0);
            this.btnExportExcel.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.btnExportExcel.Name = "btnExportExcel";
            this.btnExportExcel.Padding = new System.Windows.Forms.Padding(0, 0, 5, 0);
            this.btnExportExcel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.btnExportExcel.Size = new System.Drawing.Size(24, 32);
            this.btnExportExcel.TabIndex = 7;
            this.toolTip1.SetToolTip(this.btnExportExcel, "构件属性定义");
            this.btnExportExcel.UseVisualStyleBackColor = false;
            this.btnExportExcel.Click += new System.EventHandler(this.btnExportExcel_Click);
            // 
            // btnCheckAll
            // 
            this.btnCheckAll.BackColor = System.Drawing.Color.Transparent;
            this.btnCheckAll.BackgroundImage = global::ThColumnInfo.Properties.Resources.ComponentPropModifyPng;
            this.btnCheckAll.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnCheckAll.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnCheckAll.FlatAppearance.BorderColor = System.Drawing.SystemColors.Control;
            this.btnCheckAll.FlatAppearance.BorderSize = 0;
            this.btnCheckAll.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCheckAll.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnCheckAll.ForeColor = System.Drawing.SystemColors.Window;
            this.btnCheckAll.Location = new System.Drawing.Point(187, 0);
            this.btnCheckAll.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.btnCheckAll.Name = "btnCheckAll";
            this.btnCheckAll.Padding = new System.Windows.Forms.Padding(0, 0, 5, 0);
            this.btnCheckAll.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.btnCheckAll.Size = new System.Drawing.Size(24, 32);
            this.btnCheckAll.TabIndex = 6;
            this.toolTip1.SetToolTip(this.btnCheckAll, "构件属性定义");
            this.btnCheckAll.UseVisualStyleBackColor = false;
            this.btnCheckAll.Click += new System.EventHandler(this.btnCheckAll_Click);
            // 
            // btnParameterSet
            // 
            this.btnParameterSet.BackColor = System.Drawing.Color.Transparent;
            this.btnParameterSet.BackgroundImage = global::ThColumnInfo.Properties.Resources.ComponentPropModifyPng;
            this.btnParameterSet.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnParameterSet.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnParameterSet.FlatAppearance.BorderColor = System.Drawing.SystemColors.Control;
            this.btnParameterSet.FlatAppearance.BorderSize = 0;
            this.btnParameterSet.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnParameterSet.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnParameterSet.ForeColor = System.Drawing.SystemColors.Window;
            this.btnParameterSet.Location = new System.Drawing.Point(97, 0);
            this.btnParameterSet.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.btnParameterSet.Name = "btnParameterSet";
            this.btnParameterSet.Padding = new System.Windows.Forms.Padding(0, 0, 5, 0);
            this.btnParameterSet.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.btnParameterSet.Size = new System.Drawing.Size(24, 32);
            this.btnParameterSet.TabIndex = 5;
            this.toolTip1.SetToolTip(this.btnParameterSet, "构件属性定义");
            this.btnParameterSet.UseVisualStyleBackColor = false;
            this.btnParameterSet.Click += new System.EventHandler(this.btnParameterSet_Click);
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
            this.btnShowDetailData.Location = new System.Drawing.Point(217, 0);
            this.btnShowDetailData.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.btnShowDetailData.Name = "btnShowDetailData";
            this.btnShowDetailData.Size = new System.Drawing.Size(24, 32);
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
            this.btnComponentDefinition.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnComponentDefinition.FlatAppearance.BorderColor = System.Drawing.SystemColors.Control;
            this.btnComponentDefinition.FlatAppearance.BorderSize = 0;
            this.btnComponentDefinition.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnComponentDefinition.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnComponentDefinition.ForeColor = System.Drawing.SystemColors.Window;
            this.btnComponentDefinition.Location = new System.Drawing.Point(157, 0);
            this.btnComponentDefinition.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.btnComponentDefinition.Name = "btnComponentDefinition";
            this.btnComponentDefinition.Padding = new System.Windows.Forms.Padding(0, 0, 5, 0);
            this.btnComponentDefinition.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.btnComponentDefinition.Size = new System.Drawing.Size(24, 32);
            this.btnComponentDefinition.TabIndex = 2;
            this.toolTip1.SetToolTip(this.btnComponentDefinition, "构件属性定义");
            this.btnComponentDefinition.UseVisualStyleBackColor = false;
            this.btnComponentDefinition.Click += new System.EventHandler(this.btnComponentDefinition_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.panelUp, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panelTree, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(250, 593);
            this.tableLayoutPanel1.TabIndex = 5;
            // 
            // panelTree
            // 
            this.panelTree.Controls.Add(this.tvCheckRes);
            this.panelTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelTree.Location = new System.Drawing.Point(3, 35);
            this.panelTree.Name = "panelTree";
            this.panelTree.Size = new System.Drawing.Size(244, 555);
            this.panelTree.TabIndex = 4;
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
            this.panelUp.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panelTree.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.TreeView tvCheckRes;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem importCalculationTsmi;
        private System.Windows.Forms.ToolStripMenuItem checkTsmi;
        private System.Windows.Forms.ToolStripMenuItem detailDataTsmi;
        public System.Windows.Forms.Button btnComponentDefinition;
        public System.Windows.Forms.Panel panelUp;
        public System.Windows.Forms.Button btnShowDetailData;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ToolTip toolTip2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Panel panelTree;
        private System.Windows.Forms.Button btnExportExcel;
        private System.Windows.Forms.Button btnCheckAll;
        private System.Windows.Forms.Button btnParameterSet;
        private System.Windows.Forms.ToolStripMenuItem addColumnTableTsmi;
    }
}
