using System;
using System.Windows.Forms;
using ThAreaFrameConfig.View;
using DevExpress.XtraBars.Navigation;

namespace ThAreaFrameConfig.WinForms
{
    public partial class ThSitePlanDialog : DevExpress.XtraEditors.XtraForm
    {
        public ThSitePlanDialog()
        {
            InitializeComponent();
        }

        private void tabPane1_SelectedPageChanged(object sender, DevExpress.XtraBars.Navigation.SelectedPageChangedEventArgs e)
        {
            EnableAreaFrameModifiedEvent(e.OldPage, false);
            EnableAreaFrameModifiedEvent(e.Page, true);

            EnableAreaFrameErasedEvent(e.OldPage, false);
            EnableAreaFrameErasedEvent(e.Page, true);

            // 刷新当前页面
            Reload(e.Page);
        }

        private void ThSitePlanDialog_Load(object sender, EventArgs e)
        {

        }

        private void ThSitePlanDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            EnableAreaFrameModifiedEvent(this.tabPane1.SelectedPage, false);
            EnableAreaFrameErasedEvent(this.tabPane1.SelectedPage, false);
        }

        private void Reload(INavigationPageBase pageBase)
        {
            if (pageBase is TabNavigationPage page)
            {
                if (page.Controls[0] is IThAreaFrameView view)
                {
                    view.Reload();
                }
            }
        }

        private void EnableAreaFrameModifiedEvent(INavigationPageBase pageBase, bool enable)
        {
            if (pageBase is TabNavigationPage page)
            {
                if (page.Controls[0] is IAreaFrameDatabaseReactor reactor)
                {
                    if (enable)
                    {
                        reactor.RegisterAreaFrameModifiedEvent();
                    }
                    else
                    {
                        reactor.UnRegisterAreaFrameModifiedEvent();
                    }
                }
            }
        }

        private void EnableAreaFrameErasedEvent(INavigationPageBase pageBase, bool enable)
        {
            if (pageBase is TabNavigationPage page)
            {
                if (page.Controls[0] is IAreaFrameDatabaseReactor reactor)
                {
                    if (enable)
                    {
                        reactor.RegisterAreaFrameErasedEvent();
                    }
                    else
                    {
                        reactor.UnRegisterAreaFrameErasedEvent();
                    }
                }
            }
        }
    }
}
