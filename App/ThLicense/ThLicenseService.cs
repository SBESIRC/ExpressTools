using ThIdentity;

namespace ThLicense
{
    public class ThLicenseService
    {
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
    }
}
