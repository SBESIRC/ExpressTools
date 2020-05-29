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
            this.SuspendLayout();
            // 
            // m_progressBar
            // 
            this.m_progressBar.Location = new System.Drawing.Point(55, 35);
            this.m_progressBar.Name = "m_progressBar";
            this.m_progressBar.Size = new System.Drawing.Size(341, 22);
            this.m_progressBar.Step = 1;
            this.m_progressBar.TabIndex = 0;
            // 
            // Progress
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(440, 87);
            this.Controls.Add(this.m_progressBar);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Progress";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "正在处理...";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ProgressBar m_progressBar;
    }
}