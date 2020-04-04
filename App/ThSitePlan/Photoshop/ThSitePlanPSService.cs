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

        /// <summary>
        /// 初始化PS程序实例
        /// </summary>
        public void Initialize()
        {
            //
        }

        /// <summary>
        /// 结束PS程序实例
        /// </summary>
        public void Terminate()
        {
            //
        }

        public void ExportToFile(string path)
        {
            Application.ActiveDocument.SaveAs(path);
        }
    }
}