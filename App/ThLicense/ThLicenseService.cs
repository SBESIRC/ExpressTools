using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThIdentity;

namespace ThLicense
{
    public class ThLicenseService
    {
        // Expire start date
        public static int Global_Expire_Duration = 90;
        public static DateTime Global_Expire_Start_Date = new DateTime(2019, 10, 15);

        //==============SINGLETON============
        //fourth version from:
        //http://csharpindepth.com/Articles/General/Singleton.aspx
        private static readonly ThLicenseService instance = new ThLicenseService();
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit    
        static ThLicenseService() { }
        internal ThLicenseService() { }
        public static ThLicenseService Instance { get { return instance; } }
        //-------------SINGLETON-----------------

        public bool IsLicensed()
        {
            ThUserProfile identity = new ThUserProfile();
            return identity.IsDomainUser();
        }

        public bool IsExpired()
        {
            TimeSpan timeSpan = DateTime.Today - Global_Expire_Start_Date;
            return timeSpan.Days > Global_Expire_Duration;
        }
    }
}