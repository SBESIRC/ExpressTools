using System.Collections.Generic;
using System.Diagnostics;
using CountlySDK;
using CountlySDK.Entities;
using ThIdentity;

namespace ThAnalytics
{
    public class ThCountlyServices : IADPServices
    {
        //==============SINGLETON============
        //fourth version from:
        //http://csharpindepth.com/Articles/General/Singleton.aspx
        private static readonly ThCountlyServices instance = new ThCountlyServices();
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit    
        static ThCountlyServices() { }
        internal ThCountlyServices() { }
        public static ThCountlyServices Instance { get { return instance; } }
        //-------------SINGLETON-----------------

        private readonly Dictionary<string, string> thcommanfunctiondict = new Dictionary<string, string>
        {
            {"THHLP", "帮助"},

            {"THUPT", "检查更新"},

            {"THBLI", "图块集"},
            {"THBLS", "图块集配置"},
            {"THBEE", "提电气块转换"},

            {"THCNU", "车位编号"},
            {"THDTA", "尺寸避让"},

            {"THALC", "建立建筑图层"},
            {"THSLC", "建立结构图层"},
            {"THMLC", "建立暖通图层"},
            {"THELC", "建立电气图层"},
            {"THPLC", "建立给排水图层"},
            {"THLPM", "暖通用"},
            {"THLPE", "电气用"},
            {"THLPP", "给排水用"},
            {"THMLK", "锁定暖通图层"},
            {"THMUK", "隔离暖通图层"},
            {"THUKA", "解锁所有图层"},
            {"THMOF", "关闭暖通图层"},
            {"THMON", "开启暖通图层"},

            {"THBPS", "天华单体规整"},
            {"THCSP", "天华总体规整"},
            {"THBAC", "单体面积总汇"},
            {"THTET", "综合经济技术指标表"},

            {"THLDC", "配电箱系统图修改"},

            {"THMSC", "批量缩放"},
            {"THZ0", "Z值归零"},
            {"DGNPURGE", "DGN清理"},
            {"THBPT", "批量打印PDF"},

            {"THMTC", "文字内容刷"}
        };

        public void Initialize()
        {
            ThUserProfile userProfile = new ThUserProfile();

            //create the Countly init object
            CountlyConfig cc = new CountlyConfig()
            {
                serverUrl = "https://asia-try.count.ly",
                appKey      = "b179dc3c7e08f3aab6ceff7d0cf8e2304c196390",
                appVersion  = "1.0.0"
            };

            if (userProfile.IsDomainUser())
            {
                cc.developerProvidedDeviceId = userProfile.Mail;
            }
            

            //initiate the SDK with your preferences
            Countly.Instance.Init(cc);

            //initiate the user profile
            InitializeUserProfile(userProfile);
        }

        private void InitializeUserProfile(ThUserProfile userProfile)
        {
            if (userProfile.IsDomainUser())
            {
                Countly.UserDetails.Name = userProfile.Name;
                Countly.UserDetails.Email = userProfile.Mail;
                //这个地方存在一个bug，大部分用户无法显示Custom中的第一项内容，为了显示需要的内容，特此添加一项accountname作为第一项
                Countly.UserDetails.Custom.Add("accountname", userProfile.Accountname);
                Countly.UserDetails.Custom.Add("title", userProfile.Title);
                Countly.UserDetails.Custom.Add("company", userProfile.Company);
                Countly.UserDetails.Custom.Add("department", userProfile.Department);
            }
        }

        public void UnInitialize()
        {
        }

        public void StartSession()
        {
            //start the user session
            Countly.Instance.SessionBegin();
        }

        public void EndSession()
        {
            //end the user session
            Countly.Instance.SessionEnd();
        }

        public void RecordCommandEvent(string cmdName, double duration)
        {
            Segmentation segmentation = new Segmentation();
            segmentation.Add("名称", cmdName);
            if (thcommanfunctiondict.ContainsKey(cmdName))
            {
                segmentation.Add("功能", thcommanfunctiondict[cmdName]);
            }
            Countly.RecordEvent("CAD命令使用", 1, null, duration, segmentation);
        }

        public void RecordSysVerEvent(string sysverName, string sysverValue)
        {
            Segmentation segmentation = new Segmentation();
            segmentation.Add("名称", sysverName);
            segmentation.Add("值", sysverValue);
            Countly.RecordEvent("CAD系统变量", 1, null, segmentation);
        }
    }
}
