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
            // 登录界面
            {"THHLP", "帮助"},

            // 检查更新
            {"THUPT", "检查更新"},

            // 切换专业
            {"THPROFILE", "专业切换"},
            
            // 图块图库
            {"THBLI", "图块集"},
            {"THBLS", "图块集配置"},
            {"THBEE", "提电气块转换"},
            {"THBBR", "插块断线"},
            {"THBBE", "选块断线"},
            {"THBBS", "全选断线"},
            
            // 标注工具
            {"THCNU", "车位编号"},
            {"THDTA", "尺寸避让"},
            
            // 图层工具
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
            
            // 计算工具
            {"THBPS", "天华单体规整"},
            {"THSPS", "天华总体规整"},
            {"THBAC", "单体面积总汇"},
            {"THTET", "综合经济技术指标表"},
            {"THFET", "消防疏散表"},
            {"THABC", "房间面积框线"},

            // 平面绘图
            {"THSPC", "喷头布置"},

            // 文字表格
            {"THMTC", "文字内容刷"},
            
            // 辅助工具
            {"THQS",        "快选命令集"},
            {"THAL",        "对齐命令集"},
            {"THMSC",       "批量缩放"},
            {"THZ0",        "Z值归零"},
            {"DGNPURGE",    "DGN清理"},
            {"THBPT",       "批量打印PDF"},
            {"THBPD",       "批量打印DWF"},
            {"THBPP",       "批量打印PPT"},
            {"THSVM",       "版次信息修改"},
            {"THLTR",       "管线断线"},
            {"THMIR",       "文字块镜像"},

            // 第三方支持
            {"T20V4", "获取天正看图T20V4.0插件"},
            {"T20V5", "获取天正看图T20V5.0插件"},
        };

        public void Initialize()
        {
            //create the Countly init object
            CountlyConfig cc = new CountlyConfig()
            {
                serverUrl = "https://asia-try.count.ly",
                appKey      = "b179dc3c7e08f3aab6ceff7d0cf8e2304c196390",
                appVersion  = "1.0.0"
            };

            //initiate the SDK with your preferences
            Countly.Instance.Init(cc);

            //initiate the user profile
            UpdateUserProfile();
        }

        public void UpdateUserProfile()
        {
            try
            {
                using (IThUserProfile userProfile = new ThCybrosUserProfile())
                {
                    if (userProfile.IsDomainUser())
                    {
                        Countly.UserDetails.Name = userProfile.Name;
                        Countly.UserDetails.Email = userProfile.Mail;
                        Countly.UserDetails.Custom.Add("accountname", userProfile.Accountname);
                        Countly.UserDetails.Custom.Add("title", userProfile.Title); 
                        Countly.UserDetails.Custom.Add("company", userProfile.Company);
                        Countly.UserDetails.Custom.Add("department", userProfile.Department);
                    }
                }
            }
            catch
            {
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

        public void RecordTHCommandEvent(string cmdName, double duration)
        {
            if (thcommanfunctiondict.ContainsKey(cmdName))
            {
                Segmentation thsegmentation = new Segmentation();
                thsegmentation.Add("名称", cmdName);
                thsegmentation.Add("功能", thcommanfunctiondict[cmdName]);
                Countly.RecordEvent("天华命令使用", 1, null, duration, thsegmentation);
            }
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
