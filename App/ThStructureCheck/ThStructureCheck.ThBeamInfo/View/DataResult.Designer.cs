namespace ThStructureCheck.ThBeamInfo.View
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tpDistinguishRes = new System.Windows.Forms.TabPage();
            this.dgvDistinguishRes = new System.Windows.Forms.DataGridView();
            this.tabControl1.SuspendLayout();
            this.tpDistinguishRes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDistinguishRes)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tpDistinguishRes);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(800, 250);
            this.tabControl1.TabIndex = 0;
            this.tabControl1.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.tabControl1_DrawItem);
            // 
            // tpDistinguishRes
            // 
            this.tpDistinguishRes.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tpDistinguishRes.Controls.Add(this.dgvDistinguishRes);
            this.tpDistinguishRes.Location = new System.Drawing.Point(4, 22);
            this.tpDistinguishRes.Name = "tpDistinguishRes";
            this.tpDistinguishRes.Size = new System.Drawing.Size(792, 224);
            this.tpDistinguishRes.TabIndex = 2;
            this.tpDistinguishRes.Text = "图纸识别结果";
            this.tpDistinguishRes.UseVisualStyleBackColor = true;
            // 
            // dgvDistinguishRes
            // 
            this.dgvDistinguishRes.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dgvDistinguishRes.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.dgvDistinguishRes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDistinguishRes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvDistinguishRes.EnableHeadersVisualStyles = false;
            this.dgvDistinguishRes.Location = new System.Drawing.Point(0, 0);
            this.dgvDistinguishRes.MultiSelect = false;
            this.dgvDistinguishRes.Name = "dgvDistinguishRes";
            this.dgvDistinguishRes.ReadOnly = true;
            this.dgvDistinguishRes.RowTemplate.Height = 23;
            this.dgvDistinguishRes.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvDistinguishRes.Size = new System.Drawing.Size(790, 222);
            this.dgvDistinguishRes.TabIndex = 0;
            this.dgvDistinguishRes.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvDistinguishRes_CellDoubleClick);
            this.dgvDistinguishRes.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvDistinguishRes_CellMouseClick);
            this.dgvDistinguishRes.Paint += new System.Windows.Forms.PaintEventHandler(this.dgvDistinguishRes_Paint);
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
            this.tpDistinguishRes.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvDistinguishRes)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tpDistinguishRes;
        private System.Windows.Forms.DataGridView dgvDistinguishRes;
    }
}
