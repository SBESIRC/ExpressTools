using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThAnalytics
{
    public class ThADPUserProfile
    {
        //==============SINGLETON============
        //fourth version from:
        //http://csharpindepth.com/Articles/General/Singleton.aspx
        private static readonly ThADPUserProfile instance = new ThADPUserProfile();
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit    
        static ThADPUserProfile() { }
        internal ThADPUserProfile() { }
        public static ThADPUserProfile CurrentUser { get { return instance; } }
        //-------------SINGLETON-----------------

        public static void InitializeWithAD()
        {
            //
        }


        // 用户名
        private string username;
        public string Username { get => username; set => username = value; }
    }
}
