using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CountlySDK;
using CountlySDK.Entities;

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
                serverUrl = "http://159.65.140.241",
                appKey = "0622be43c254fb306a277941b8e2f2842f807b43",
                appVersion = "1.0.0"
            };

            //initiate the SDK with your preferences
            Countly.Instance.Init(cc);
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
