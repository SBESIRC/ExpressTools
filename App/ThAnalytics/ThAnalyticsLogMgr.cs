using System.Windows.Forms;
using ThAnalytics.SDK;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace ThAnalytics.Identity
{
    public class ThAnalyticsLogMgr
    {
        public static bool IsLogged()
        {
            return THRecordingService.SignIn(Properties.Settings.Default.Token);
        }

        public static void Login()
        {
            if (!IsLogged())
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

                    // 开启会话
                    THRecordingService.SessionBegin();

                    // 保存口令
                    Properties.Settings.Default.Token = THRecordingService.m_ToKen;
                    Properties.Settings.Default.Save();
                }
            }
        }

        public static void Logout()
        {
            if (IsLogged())
            {
                // 结束会话
                THRecordingService.SessionEnd();

                // 清除口令
                Properties.Settings.Default.Token = null;
                Properties.Settings.Default.Save();
            }
        }
    }
}
