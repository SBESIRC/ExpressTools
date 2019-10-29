using System;
using Newtonsoft.Json;


namespace ThAnalytics.SDK
{
    public class THRecordingService
    {
        public static string m_ToKen = string.Empty;
        public static string m_Guid = Guid.NewGuid().ToString();
        public static UserDetails m_UserDetails = new UserDetails();

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
                var _Str = APIMessage.SignIn(_Username, _Password);
                if (_Str == string.Empty) { throw new InvalidOperationException(" 初始化信息失败！'SignIn'"); }
                m_ToKen = _Str;
                var _UserInfo = APIMessage.CADUserInfo(m_ToKen);
                m_UserDetails = JsonConvert.DeserializeObject<UserDetails>(_UserInfo);
                return true;
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
                var _UserInfo = APIMessage.CADUserInfo(m_ToKen);
                m_UserDetails = JsonConvert.DeserializeObject<UserDetails>(_UserInfo);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void SessionBegin()
        {
            try
            {
                if (m_ToKen == string.Empty) { throw new InvalidOperationException(" 初始化信息失败！'SessionBegin'"); }
                Sessions Ssessions = new Sessions();
                Ssessions.ip_address = FuncMac.GetIpAddress();
                Ssessions.mac_address = FuncMac.GetNetCardMacAddress();
                Ssessions.operation = "Begin";
                Ssessions.session = m_Guid;
                APIMessage.CADSession(m_ToKen, JsonConvert.SerializeObject(Ssessions));
            }
            catch
            {
                //
            }
        }

        public static void SessionEnd()
        {
            try
            {
                if (m_ToKen == string.Empty) { throw new InvalidOperationException(" 初始化信息失败！'SessionEnd'"); }
                Sessions Ssessions = new Sessions();
                Ssessions.ip_address = FuncMac.GetIpAddress();
                Ssessions.mac_address = FuncMac.GetNetCardMacAddress();
                Ssessions.operation = "End";
                Ssessions.session = m_Guid;
                APIMessage.CADSession(m_ToKen, JsonConvert.SerializeObject(Ssessions));
            }
            catch
            {
                //
            }
        }

        public static void RecordEvent(string CmdName, int Duration, Segmentation _Segmentation)
        {
            try
            {
                if (m_ToKen == string.Empty) { throw new InvalidOperationException(" 事件调用失败！'RecordEvent'"); }
                InitiConnection _InitiConnection = new InitiConnection();
                _InitiConnection.cmd_name = CmdName;
                _InitiConnection.cmd_seconds = Duration;
                _InitiConnection.session_id = m_Guid;
                _InitiConnection.cmd_data = _Segmentation;
                APIMessage.CADOperation(m_ToKen, JsonConvert.SerializeObject(_InitiConnection));
            }
            catch
            {
                //
            }
        }
    }
}
