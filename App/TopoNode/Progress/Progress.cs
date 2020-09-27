using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TopoNode.Progress
{
    public partial class Progress : Form
    {
        public Progress()
        {
            InitializeComponent();
        }

        private static Progress instance = null;

        public static Progress Instance
        {
            get
            {
                if (instance == null || instance.Handle == null)
                {
                    instance = new Progress();
                }

                return instance;
            }
        }

        public static void ShowProgress()
        {
            try
            {
                var progress = Progress.Instance;
                progress.m_progressBar.Value = 1;
                progress.Show();
            }
            catch
            {
                // 避免出现某些操作，异常崩溃，用户体验差 
            }
        }

        /// <summary>
        /// 进度条显示
        /// </summary>
        /// <param name="value"></param>
        public static void SetValue(int value)
        {
            try
            {
                var progress = Progress.Instance;
                progress.m_progressBar.Value = value;
                var valueShow = string.Format("{0}%", value / 100.0);
                progress.m_lblProgresss.Text = valueShow;
                progress.m_lblProgresss.Refresh();
                progress.Show();
            }
            catch
            {
                // 避免出现某些操作，异常崩溃， 
            }
        }

        public static void SetTip(string tip)
        {
            try
            {
                var progress = Progress.Instance;
                progress.m_lblTip.Text = tip;
                progress.m_lblTip.Refresh();
            }
            catch
            {
                // 避免出现某些操作，异常崩溃， 
            }
        }

        public static void Reset()
        {
            try
            {
                var progress = Progress.Instance;
                progress.m_lblTip.Text = string.Empty;
                progress.m_lblProgresss.Text = string.Empty;
                progress.Refresh();
            }
            catch
            {
                // 避免出现某些操作，异常崩溃， 
            }
        }

        public static void HideProgress()
        {
            try
            {
                var progress = Progress.Instance;
                progress.Hide();
            }
            catch
            {
                // 避免出现某些操作，异常崩溃， 
            }

        }

        private void ProgressDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }
    }
}
