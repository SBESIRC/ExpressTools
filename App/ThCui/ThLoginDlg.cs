using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TianHua.AutoCAD.ThCui
{
    public partial class ThLoginDlg : Form
    {
        public string User
        {
            get
            {
                return textBox_user_name.Text;
            }
        }

        public string Password
        {
            get
            {
                return textBox_password.Text;
            }
        }

        public ThLoginDlg()
        {
            InitializeComponent();
        }
    }
}
