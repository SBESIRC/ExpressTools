using System;

namespace ThAnalytics.SDK
{
    public class THRecordingService
    {
        public static string m_ToKen = null;
        public static UserDetails m_UserDetails = null;
        public static string m_Guid = Guid.NewGuid().ToString();

        public static void InitCfg(string _ServerUrl, string _SSOURL, string _AppVersion)
        {
            APIMessage.m_Config.ServerUrl = _ServerUrl;
            APIMessage.m_Config.SSOUrl = _SSOURL;
            APIMessage.m_Config.AppVersion = _AppVersion;
        }

        public static void InitCfg(THConfig _Config) { APIMessage.m_Config = _Config; }

        public static bool SignIn(string _Username, string _Password)
        {
            try
            {
                var _Token = APIMessage.SignIn(_Username, _Password);
                if (string.IsNullOrEmpty(_Token))
                {
                    return false;
                }

                m_ToKen = _Token;
                m_UserDetails = APIMessage.CADUserInfo(m_ToKen);
                return (m_ToKen != null) && (m_UserDetails != null);
            }
            catch
            {
                return false;
            }
        }

        public static bool SignIn(string _Token)
        {
            try
            {
                if (string.IsNullOrEmpty(_Token))
                {
                    return false;
                }

                m_ToKen = _Token;
                m_UserDetails = APIMessage.CADUserInfo(m_ToKen);
                return (m_ToKen != null) && (m_UserDetails != null);
            }
            catch
            {
                return false;
            }
        }

        public static void SessionBegin()
        {
            APIMessage.CADSession(m_ToKen, new Sessions()
            {
                session = m_Guid,
                operation = "Begin",
                ip_address = FuncMac.GetIpAddress(),
                mac_address = FuncMac.GetNetCardMacAddress(),
            });
        }

        public static void SessionEnd()
        {
            APIMessage.CADSession(m_ToKen, new Sessions()
            {
                session = m_Guid,
                operation = "End",
                ip_address = FuncMac.GetIpAddress(),
                mac_address = FuncMac.GetNetCardMacAddress(),
            });
        }

        public static void RecordEvent(string CmdName, int Duration, Segmentation _Segmentation)
        {
            APIMessage.CADOperation(m_ToKen, new InitiConnection()
            {
                cmd_name = CmdName,
                cmd_seconds = Duration,
                session_id = m_Guid,
                cmd_data = _Segmentation
            });
        }
    }
}
