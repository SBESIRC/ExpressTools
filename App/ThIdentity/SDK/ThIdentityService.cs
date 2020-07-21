
namespace ThIdentity.SDK
{
    public class ThIdentityService
    {
        public static UserDetails UserProfile
        {
            get
            {
                return THRecordingService.m_UserDetails;
            }
        }

        public static bool IsLogged()
        {
            return true; 
            //return THRecordingService.SignIn(Properties.Settings.Default.Token);
        }

        // 使用Token登录
        public static void Login()
        {
            THRecordingService.SignIn(Properties.Settings.Default.Token);
        }

        // 使用天华内网账号和密码登录
        public static bool Login(string user, string password)
        {
            // 若已经登录，直接返回
            if (IsLogged())
            {
                return true;
            }

            if (!THRecordingService.SignIn(user, password))
            {
                return false;
            }

            // 开启会话
            THRecordingService.SessionBegin();

            // 保存口令
            Properties.Settings.Default.Token = THRecordingService.m_ToKen;
            Properties.Settings.Default.Save();

            return true;
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
