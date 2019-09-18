using System;
using ThAreaFrameConfig.ViewModel;
using DevExpress.XtraEditors;

namespace ThAreaFrameConfig.WinForms
{
    public partial class ThFireProofingDialog : XtraForm
    {
        public ThCommerceFireProofSettings CommerceFireProofSettings { get; set; }
        public ThUndergroundGarageFireProofSettings UndergroundGarageFireProofSettings { get; set; }

        public ThFireProofingDialog()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            //TODO: add code here 
            base.OnLoad(e);
        }
    }
}
