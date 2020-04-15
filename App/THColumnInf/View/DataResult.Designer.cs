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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.dgvColumnTable = new System.Windows.Forms.DataGridView();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.dgvSpecificationRes = new System.Windows.Forms.DataGridView();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.dgvCalculationRes = new System.Windows.Forms.DataGridView();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvColumnTable)).BeginInit();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSpecificationRes)).BeginInit();
            this.tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCalculationRes)).BeginInit();
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
            this.tabControl1.Size = new System.Drawing.Size(647, 244);
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
            this.tabPage1.Size = new System.Drawing.Size(639, 218);
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
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvColumnTable.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
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
            this.dgvColumnTable.Size = new System.Drawing.Size(631, 210);
            this.dgvColumnTable.TabIndex = 0;
            this.dgvColumnTable.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellDoubleClick);
            this.dgvColumnTable.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvColumnTable_CellMouseClick);
            this.dgvColumnTable.Paint += new System.Windows.Forms.PaintEventHandler(this.dgvColumnTable_Paint);
            // 
            // tabPage2
            // 
            this.tabPage2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tabPage2.Controls.Add(this.dgvSpecificationRes);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(639, 218);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "规范校对结果";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // dgvSpecificationRes
            // 
            this.dgvSpecificationRes.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dgvSpecificationRes.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.dgvSpecificationRes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvSpecificationRes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvSpecificationRes.EnableHeadersVisualStyles = false;
            this.dgvSpecificationRes.Location = new System.Drawing.Point(3, 3);
            this.dgvSpecificationRes.Name = "dgvSpecificationRes";
            this.dgvSpecificationRes.ReadOnly = true;
            this.dgvSpecificationRes.RowTemplate.Height = 23;
            this.dgvSpecificationRes.Size = new System.Drawing.Size(631, 210);
            this.dgvSpecificationRes.TabIndex = 0;
            this.dgvSpecificationRes.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvSpecificationRes_CellDoubleClick);
            this.dgvSpecificationRes.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvSpecificationRes_CellMouseClick);
            this.dgvSpecificationRes.Paint += new System.Windows.Forms.PaintEventHandler(this.dgvSpecificationRes_Paint);
            // 
            // tabPage3
            // 
            this.tabPage3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tabPage3.Controls.Add(this.dgvCalculationRes);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(639, 218);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "计算书校对结果";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // dgvCalculationRes
            // 
            this.dgvCalculationRes.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dgvCalculationRes.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.dgvCalculationRes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCalculationRes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvCalculationRes.EnableHeadersVisualStyles = false;
            this.dgvCalculationRes.Location = new System.Drawing.Point(0, 0);
            this.dgvCalculationRes.Name = "dgvCalculationRes";
            this.dgvCalculationRes.ReadOnly = true;
            this.dgvCalculationRes.RowTemplate.Height = 23;
            this.dgvCalculationRes.Size = new System.Drawing.Size(637, 216);
            this.dgvCalculationRes.TabIndex = 0;
            this.dgvCalculationRes.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvCalculationRes_CellDoubleClick);
            this.dgvCalculationRes.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvCalculationRes_CellMouseClick);
            this.dgvCalculationRes.Paint += new System.Windows.Forms.PaintEventHandler(this.dgvCalculationRes_Paint);
            // 
            // DataResult
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControl1);
            this.Name = "DataResult";
            this.Size = new System.Drawing.Size(647, 244);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvColumnTable)).EndInit();
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvSpecificationRes)).EndInit();
            this.tabPage3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvCalculationRes)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        public System.Windows.Forms.DataGridView dgvColumnTable;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.DataGridView dgvSpecificationRes;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.DataGridView dgvCalculationRes;
    }
}
