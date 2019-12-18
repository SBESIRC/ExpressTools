using System;
using System.Windows.Forms;
using ThAreaFrameConfig.View;
using DevExpress.XtraBars.Navigation;

namespace ThAreaFrameConfig.WinForms
{
    public partial class ThResidentialRoomControlDialog : DevExpress.XtraEditors.XtraForm
    {
        public ThResidentialRoomControlDialog()
        {
            InitializeComponent();
        }

        private void ThResidentialRoomControlDialog_Load(object sender, EventArgs e)
        {
            //
        }

        private void tabPane1_SelectedPageChanged(object sender, DevExpress.XtraBars.Navigation.SelectedPageChangedEventArgs e)
        {
            EnableAreaFrameModifiedEvent(e.OldPage, false);
            EnableAreaFrameModifiedEvent(e.Page, true);

            EnableAreaFrameErasedEvent(e.OldPage, false);
            EnableAreaFrameErasedEvent(e.Page, true);

            EnableAreaFrameCommandEvent(e.OldPage, false);
            EnableAreaFrameCommandEvent(e.Page, true);

            // 刷新当前页面
            Reload(e.Page);
        }

        private void ThResidentialRoomControlDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            EnableAreaFrameModifiedEvent(this.tabPane1.SelectedPage, false);
            EnableAreaFrameErasedEvent(this.tabPane1.SelectedPage, false);
            EnableAreaFrameCommandEvent(this.tabPane1.SelectedPage, false);
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

        private void EnableAreaFrameCommandEvent(INavigationPageBase pageBase, bool enable)
        {
            if (pageBase is TabNavigationPage page)
            {
                if (page.Controls[0] is IAreaFrameDocumentCollectionReactor reactor)
                {
                    if (enable)
                    {
                        reactor.RegisterDocumentLockModeChangedEvent();
                    }
                    else
                    {
                        reactor.UnRegisterDocumentLockModeChangedEvent();
                    }
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
