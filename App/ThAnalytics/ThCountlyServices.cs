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
            Countly.RecordEvent("CAD命令使用", 1, null, duration, segmentation);
        }
    }
}
