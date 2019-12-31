
namespace ThIdentity.SDK
{
    public class ThIdentityService
    {
        public static bool IsLogged()
        {
            return THRecordingService.SignIn(Properties.Settings.Default.Token);
        }

        // 使用Token登录
        public static void Login()
        {
            THRecordingService.SignIn(Properties.Settings.Default.Token);
        }

        // 使用AD域账号和密码登录
        public static void Login(string user, string password)
        {
            if (!IsLogged())
            {
                if (THRecordingService.SignIn(user, password))
                {
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
