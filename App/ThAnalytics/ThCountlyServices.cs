using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            //create the Countly init object
            CountlyConfig cc = new CountlyConfig()
            {
                serverUrl   = "http://49.234.60.227",
                appKey      = "8a8a305d8fd6eef7a1f8b25843778ebd9f9417db",
                appVersion  = "1.0.0"
            };

            //initiate the SDK with your preferences
            Countly.Instance.Init(cc);

            //initiate the user profile
            InitializeUserProfile();
        }

        private void InitializeUserProfile()
        {
            ThUserProfile thuserprofile = new ThUserProfile();
            Countly.UserDetails.Name = thuserprofile.Name;
            Countly.UserDetails.Email = thuserprofile.Mail;
            Countly.UserDetails.Custom.Add("title", thuserprofile.Title);
            Countly.UserDetails.Custom.Add("company", thuserprofile.Company);
            Countly.UserDetails.Custom.Add("department", thuserprofile.Department);
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

        public void RecordCommandEvent(string cmdName)
        {
            Segmentation segmentation = new Segmentation();
            segmentation.Add("GlobalCommandName", cmdName);
            Countly.RecordEvent("InvokeCommand", 1, segmentation);
        }
    }
}
