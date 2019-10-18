namespace ThRoomBoundary
{
    partial class ThProgressDialog
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
            this.m_progressBar = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // m_progressBar
            // 
            this.m_progressBar.Location = new System.Drawing.Point(40, 26);
            this.m_progressBar.Name = "m_progressBar";
            this.m_progressBar.Size = new System.Drawing.Size(360, 23);
            this.m_progressBar.Step = 2;
            this.m_progressBar.TabIndex = 0;
            // 
            // ThProgressDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(454, 95);
            this.Controls.Add(this.m_progressBar);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ThProgressDialog";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "正在计算面积框线...";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ThProgressDialog_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.ProgressBar m_progressBar;
    }
}