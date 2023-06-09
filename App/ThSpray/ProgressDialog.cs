﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ThSpray
{
    public partial class ProgressDialog : Form
    {
        public ProgressDialog()
        {
            InitializeComponent();
        }

        private static ProgressDialog instance = null;

        public static ProgressDialog Instance
        {
            get
            {
                if (instance == null || instance.Handle == null)
                {
                    instance = new ProgressDialog();
                }

                return instance;
            }
        }

        public static void ShowProgress()
        {
            try
            {
                var progress = ProgressDialog.Instance;
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
                var progress = ProgressDialog.Instance;
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
                var progress = ProgressDialog.Instance;
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
