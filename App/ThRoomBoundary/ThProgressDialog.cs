using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ThRoomBoundary
{
    public partial class ThProgressDialog : Form
    {
        public ThProgressDialog()
        {
            InitializeComponent();
        }

        private static ThProgressDialog instance = null;

        public static ThProgressDialog Instance
        {
            get
            {
                if (instance == null || instance.Handle == null)
                {
                    instance = new ThProgressDialog();
                }

                return instance;
            }
        }

        public static void ShowProgress()
        {
            try
            {
                var progress = ThProgressDialog.Instance;
                progress.m_progressBar.Value = 5;
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
                var progress = ThProgressDialog.Instance;
                progress.m_progressBar.Value = value;
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
                var progress = ThProgressDialog.Instance;
                progress.Hide();
            }
            catch
            {
                // 避免出现某些操作，异常崩溃， 
            }

        }

        private void ThProgressDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }
    }
}
