namespace TopoNode.Progress
{
    partial class Progress
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Progress));
            this.m_progressBar = new System.Windows.Forms.ProgressBar();
            this.m_lblTip = new System.Windows.Forms.Label();
            this.m_lblProgresss = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // m_progressBar
            // 
            this.m_progressBar.ForeColor = System.Drawing.SystemColors.Control;
            this.m_progressBar.Location = new System.Drawing.Point(55, 37);
            this.m_progressBar.Maximum = 1000;
            this.m_progressBar.Name = "m_progressBar";
            this.m_progressBar.Size = new System.Drawing.Size(341, 20);
            this.m_progressBar.Step = 1;
            this.m_progressBar.TabIndex = 0;
            // 
            // m_lblTip
            // 
            this.m_lblTip.AutoSize = true;
            this.m_lblTip.Location = new System.Drawing.Point(54, 13);
            this.m_lblTip.Name = "m_lblTip";
            this.m_lblTip.Size = new System.Drawing.Size(83, 12);
            this.m_lblTip.TabIndex = 1;
            this.m_lblTip.Text = "图纸预处理...";
            // 
            // m_lblProgresss
            // 
            this.m_lblProgresss.AutoSize = true;
            this.m_lblProgresss.BackColor = System.Drawing.SystemColors.Control;
            this.m_lblProgresss.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.m_lblProgresss.Font = new System.Drawing.Font("楷体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.m_lblProgresss.ForeColor = System.Drawing.SystemColors.ControlText;
            this.m_lblProgresss.Location = new System.Drawing.Point(337, 13);
            this.m_lblProgresss.Name = "m_lblProgresss";
            this.m_lblProgresss.Size = new System.Drawing.Size(56, 16);
            this.m_lblProgresss.TabIndex = 2;
            this.m_lblProgresss.Text = "10%100";
            this.m_lblProgresss.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Progress
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(440, 87);
            this.Controls.Add(this.m_lblProgresss);
            this.Controls.Add(this.m_lblTip);
            this.Controls.Add(this.m_progressBar);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Progress";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "正在处理...";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar m_progressBar;
        private System.Windows.Forms.Label m_lblTip;
        public System.Windows.Forms.Label m_lblProgresss;
    }
}