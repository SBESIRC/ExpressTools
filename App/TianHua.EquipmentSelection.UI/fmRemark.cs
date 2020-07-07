using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TianHua.FanSelection.UI
{
    public partial class fmRemark : DevExpress.XtraEditors.XtraForm
    {

        public string m_Remark = string.Empty;

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        public fmRemark()
        {
            InitializeComponent();
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            m_Remark = Memo.Text;
        }

        private void fmRemark_Load(object sender, EventArgs e)
        {
            Memo.Text = m_Remark;
        }
    }
}
