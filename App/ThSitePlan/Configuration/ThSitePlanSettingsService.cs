using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThSitePlan.Configuration
{
    public class ThSitePlanSettingsService
    {
        //==============SINGLETON============
        //fourth version from:
        //http://csharpindepth.com/Articles/General/Singleton.aspx
        private static readonly ThSitePlanSettingsService instance = new ThSitePlanSettingsService();
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit    
        static ThSitePlanSettingsService() { }
        internal ThSitePlanSettingsService() { }
        public static ThSitePlanSettingsService Instance { get { return instance; } }
        //-------------SINGLETON-----------------

        public string OutputPath
        {
            get
            {
                //
                if (Properties.Settings.Default.FileSavePath.IsNullOrEmpty())
                {
                    return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                }
                else
                {
                    return Properties.Settings.Default.FileSavePath;
                }
            }
        }
    }
}
