using System;
using ThAreaFrameConfig.Model;
using DevExpress.XtraEditors;

namespace ThAreaFrameConfig.WinForms
{
    public partial class ThFireProofingDialog : XtraForm
    {
        public ThFireProofingDialog()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            // TODO: 
            //  暂时移除“地下车库”TabPage
            this.tabControl1.TabPages.Remove(this.tabPage_underground_parking);
            base.OnLoad(e);
        }
    }
}