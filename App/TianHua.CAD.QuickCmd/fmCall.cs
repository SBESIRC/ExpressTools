using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TianHua.Publics.BaseCode;

namespace TianHua.CAD.QuickCmd
{
    public partial class fmCall : DevExpress.XtraEditors.XtraForm
    {
        public fmCall()
        {
            InitializeComponent();
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            if (FuncStr.NullToStr(textEdit1.Text) == string.Empty || FuncStr.NullToStr(textEdit2.Text) == string.Empty) { return; }
            fmQuickCmd _fmQuickCmd = new fmQuickCmd();
            _fmQuickCmd.m_PgpPath = FuncStr.NullToStr(textEdit1.Text);
            _fmQuickCmd.m_Profession = FuncStr.NullToStr(textEdit2.Text);
            _fmQuickCmd.ShowDialog();
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void textEdit1_DoubleClick(object sender, EventArgs e)
        {
            var _OpenFileDialog = new OpenFileDialog();
            _OpenFileDialog.Filter = "Pgp Files (*.Pgp)|*.Pgp";
            var _Result = _OpenFileDialog.ShowDialog();
            if (_Result == DialogResult.OK)
            {
                textEdit1.Text =_OpenFileDialog.FileName;

            }

        }
 
    }
}
