using DevExpress.XtraEditors;
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
    public partial class fmSceneScreening : DevExpress.XtraEditors.XtraForm
    {

        public List<string> m_List { get; set; }

        public fmSceneScreening()
        {
            InitializeComponent();
        }


        public void Init(List<string> _List)
        {
            m_List = _List;
            if (_List == null) { return; }
            foreach (Control _Ctrl in this.layoutControl1.Controls)
            {
                if (_Ctrl is CheckEdit)
                {
                    var _Edit = _Ctrl as CheckEdit;
                    if(m_List.Contains( _Edit.Text))
                    {
                        _Edit.Checked = true;
                    }
                }
            }
        }


        private void CheckAll_CheckedChanged(object sender, EventArgs e)
        {
            foreach (Control _Ctrl in this.layoutControl1.Controls)
            {
                if (_Ctrl is CheckEdit)
                {
                    var _Edit = _Ctrl as CheckEdit;
                    _Edit.Checked = CheckAll.Checked;
                }
            }
        }

        private void Check_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            m_List = new List<string>();
            foreach (Control _Ctrl in this.layoutControl1.Controls)
            {
                if (_Ctrl is CheckEdit)
                {
                    var _Edit = _Ctrl as CheckEdit;
                    m_List.Add(_Edit.Text);
                }
            }
        }
    }
}
