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
            //TODO: add code here 
            base.OnLoad(e);
        }

        public ThCommerceFireProofSettings CommerceFireProofSettings
        {
            get
            {
                return commerceFireProofControl.Settings;
            }
        }
    }
}
