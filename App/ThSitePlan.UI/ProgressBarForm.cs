using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ThSitePlan.UI
{
    public partial class ProgressBarForm : Form
    {
        public ProgressBarForm(int maxprogressbar)
        {
            InitializeComponent();

            progressBar1.Maximum = maxprogressbar;
            Confirm_Btn.Enabled = false;
        }

        public void SetProgressBar(string appruninfo, string progressbarinfo, int stepno)
        {
            this.ProgressInfo.Text = progressbarinfo;
            this.progressBar1.Value += stepno;
            this.AppInfoBox.AppendText(appruninfo + Environment.NewLine);
        }

        public void SetProgressInfo(string progressbarinfo)
        {
            this.ProgressInfo.Text = progressbarinfo;
        }

        public void SetAppInfoBox(string appruninfo)
        {
            this.AppInfoBox.AppendText(appruninfo + Environment.NewLine);
        }

        public void setconfirmbtn()
        {
            Confirm_Btn.Enabled = true;
        }

        private void Confirm_Btn_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }
    }
}
