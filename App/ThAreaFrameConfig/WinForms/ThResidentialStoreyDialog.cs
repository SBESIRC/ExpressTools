using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ThAreaFrameConfig.WinForms
{
    public partial class ThResidentialStoreyDialog : DevExpress.XtraEditors.XtraForm
    {
        public string Storey
        {
            get
            {
                if (radioButton_common.Checked)
                    return "c" + textBox_storey.Text;
                else if (radioButton_odd.Checked)
                    return "j" + textBox_storey.Text;
                else if (radioButton_even.Checked)
                    return "o" + textBox_storey.Text;
                else
                    return "";
            }
        }

        public ThResidentialStoreyDialog()
        {
            InitializeComponent();
        }
    }
}
