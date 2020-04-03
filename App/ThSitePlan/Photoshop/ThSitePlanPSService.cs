using PsApplication = Photoshop.Application;

namespace ThSitePlan.Photoshop
{
    public class ThSitePlanPSService
    {
        //==============SINGLETON============
        //fourth version from:
        //http://csharpindepth.com/Articles/General/Singleton.aspx
        private static readonly ThSitePlanPSService instance = new ThSitePlanPSService();
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit    
        static ThSitePlanPSService() { }
        internal ThSitePlanPSService() { }
        public static ThSitePlanPSService Instance { get { return instance; } }
        //-------------SINGLETON-----------------

        private PsApplication application;
        public PsApplication Application
        {
            get
            {
                if (application == null)
                {
                    application = new PsApplication();
                }
                return application;
            }
        }
    }
}
