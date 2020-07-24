namespace ThColumnInfo.View
{
    partial class DataResult
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.dgvColumnTable = new System.Windows.Forms.DataGridView();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.dgvIndicator = new System.Windows.Forms.DataGridView();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.dgvCheckRes = new System.Windows.Forms.DataGridView();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rbShowAll = new System.Windows.Forms.RadioButton();
            this.rbShowInvalid = new System.Windows.Forms.RadioButton();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvColumnTable)).BeginInit();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvIndicator)).BeginInit();
            this.tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCheckRes)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(800, 250);
            this.tabControl1.TabIndex = 0;
            this.tabControl1.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.tabControl1_DrawItem);
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.Color.Transparent;
            this.tabPage1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tabPage1.Controls.Add(this.dgvColumnTable);
            this.tabPage1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(792, 224);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "识别柱列表";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // dgvColumnTable
            // 
            this.dgvColumnTable.AllowUserToAddRows = false;
            this.dgvColumnTable.AllowUserToDeleteRows = false;
            this.dgvColumnTable.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dgvColumnTable.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvColumnTable.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.dgvColumnTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvColumnTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvColumnTable.EnableHeadersVisualStyles = false;
            this.dgvColumnTable.GridColor = System.Drawing.Color.Black;
            this.dgvColumnTable.Location = new System.Drawing.Point(3, 3);
            this.dgvColumnTable.MultiSelect = false;
            this.dgvColumnTable.Name = "dgvColumnTable";
            this.dgvColumnTable.ReadOnly = true;
            this.dgvColumnTable.RowTemplate.Height = 23;
            this.dgvColumnTable.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvColumnTable.Size = new System.Drawing.Size(784, 216);
            this.dgvColumnTable.TabIndex = 0;
            this.dgvColumnTable.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellDoubleClick);
            this.dgvColumnTable.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvColumnTable_CellMouseClick);
            this.dgvColumnTable.Paint += new System.Windows.Forms.PaintEventHandler(this.dgvColumnTable_Paint);
            // 
            // tabPage2
            // 
            this.tabPage2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tabPage2.Controls.Add(this.dgvIndicator);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(792, 224);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "规范校对结果";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // dgvIndicator
            // 
            this.dgvIndicator.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dgvIndicator.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.dgvIndicator.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvIndicator.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvIndicator.EnableHeadersVisualStyles = false;
            this.dgvIndicator.Location = new System.Drawing.Point(3, 3);
            this.dgvIndicator.MultiSelect = false;
            this.dgvIndicator.Name = "dgvIndicator";
            this.dgvIndicator.ReadOnly = true;
            this.dgvIndicator.RowTemplate.Height = 23;
            this.dgvIndicator.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvIndicator.Size = new System.Drawing.Size(784, 216);
            this.dgvIndicator.TabIndex = 0;
            this.dgvIndicator.Paint += new System.Windows.Forms.PaintEventHandler(this.dgvSpecificationRes_Paint);
            // 
            // tabPage3
            // 
            this.tabPage3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tabPage3.Controls.Add(this.tableLayoutPanel1);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(792, 224);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "计算书校对结果";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // dgvCheckRes
            // 
            this.dgvCheckRes.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dgvCheckRes.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.dgvCheckRes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCheckRes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvCheckRes.EnableHeadersVisualStyles = false;
            this.dgvCheckRes.Location = new System.Drawing.Point(3, 3);
            this.dgvCheckRes.MultiSelect = false;
            this.dgvCheckRes.Name = "dgvCheckRes";
            this.dgvCheckRes.ReadOnly = true;
            this.dgvCheckRes.RowTemplate.Height = 23;
            this.dgvCheckRes.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvCheckRes.Size = new System.Drawing.Size(665, 216);
            this.dgvCheckRes.TabIndex = 0;
            this.dgvCheckRes.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvCheckRes_CellDoubleClick);
            this.dgvCheckRes.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvCheckRes_CellMouseClick);
            this.dgvCheckRes.Paint += new System.Windows.Forms.PaintEventHandler(this.dgvCalculationRes_Paint);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 85F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel1.Controls.Add(this.dgvCheckRes, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(790, 222);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rbShowInvalid);
            this.groupBox1.Controls.Add(this.rbShowAll);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(674, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(113, 121);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "显示模式：";
            this.groupBox1.Paint += new System.Windows.Forms.PaintEventHandler(this.groupBox1_Paint);
            // 
            // rbShowAll
            // 
            this.rbShowAll.AutoSize = true;
            this.rbShowAll.Checked = true;
            this.rbShowAll.Location = new System.Drawing.Point(6, 24);
            this.rbShowAll.Name = "rbShowAll";
            this.rbShowAll.Size = new System.Drawing.Size(71, 16);
            this.rbShowAll.TabIndex = 0;
            this.rbShowAll.TabStop = true;
            this.rbShowAll.Text = "全部显示";
            this.rbShowAll.UseVisualStyleBackColor = true;
            this.rbShowAll.CheckedChanged += new System.EventHandler(this.rbShowAll_CheckedChanged);
            // 
            // rbShowInvalid
            // 
            this.rbShowInvalid.AutoSize = true;
            this.rbShowInvalid.Location = new System.Drawing.Point(6, 47);
            this.rbShowInvalid.Name = "rbShowInvalid";
            this.rbShowInvalid.Size = new System.Drawing.Size(95, 16);
            this.rbShowInvalid.TabIndex = 1;
            this.rbShowInvalid.Text = "仅显示不通过";
            this.rbShowInvalid.UseVisualStyleBackColor = true;
            this.rbShowInvalid.CheckedChanged += new System.EventHandler(this.rbShowInvalid_CheckedChanged);
            // 
            // DataResult
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControl1);
            this.MinimumSize = new System.Drawing.Size(800, 200);
            this.Name = "DataResult";
            this.Size = new System.Drawing.Size(800, 250);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvColumnTable)).EndInit();
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvIndicator)).EndInit();
            this.tabPage3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvCheckRes)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        public System.Windows.Forms.DataGridView dgvColumnTable;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.DataGridView dgvIndicator;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.DataGridView dgvCheckRes;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rbShowInvalid;
        private System.Windows.Forms.RadioButton rbShowAll;
    }
}
