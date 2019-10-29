using System.Windows.Forms;
using ThAnalytics.SDK;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace ThAnalytics.Identity
{
    public class ThAnalyticsLogMgr
    {
        public static bool IsLogged()
        {
            return !string.IsNullOrEmpty(Properties.Settings.Default.Token);
        }

        public static void Login()
        {
            using (var dlg = new ThAnalyticsLoginDlg())
            {
                if (AcadApp.ShowModalDialog(dlg) != DialogResult.OK)
                {
                    return;
                }

                if (!THRecordingService.SignIn(dlg.User, dlg.Password))
                {
                    return;
                }

                Properties.Settings.Default.Token = THRecordingService.m_ToKen;
                Properties.Settings.Default.Save();
            }
        }

        public static void Logout()
        {
            Properties.Settings.Default.Token = null;
            Properties.Settings.Default.Save();
        }
    }
}
