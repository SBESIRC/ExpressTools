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
                if (instance == null)
                {
                    instance = new ThProgressDialog();
                }

                return instance;
            }
        }

        public static void ShowProgress()
        {
            var progress = ThProgressDialog.Instance;
            progress.Show();
        }

        /// <summary>
        /// 进度条显示
        /// </summary>
        /// <param name="value"></param>
        public static void SetValue(int value)
        {
            var progress = ThProgressDialog.Instance;
            progress.m_progressBar.Value = value;
            progress.Refresh();
        }

        public static void HideProgress()
        {
            var progress = ThProgressDialog.Instance;
            progress.Hide();
        }
    }
}
