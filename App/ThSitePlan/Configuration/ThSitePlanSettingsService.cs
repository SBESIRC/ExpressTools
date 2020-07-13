using System;
using System.Collections.Generic;
using System.IO;
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
                    string mydoc = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    string filepath = Path.Combine(mydoc, ThSitePlanCommon.ThSitePlan_File_Save_Path);
                    Directory.CreateDirectory(filepath);
                    return filepath;
                }
                else
                {
                    return Properties.Settings.Default.FileSavePath;
                }
            }
            set
            {
                Properties.Settings.Default.FileSavePath = value;
            }
        }

        public double ShadowLengthScale
        {
            get
            {
                return Properties.Settings.Default.shadowLengthScale;
            }
            set
            {
                Properties.Settings.Default.shadowLengthScale = value;
            }
        }

        public double ShadowAngle
        {
            get
            {
                return Properties.Settings.Default.shadowAngle;
            }
            set
            {
                Properties.Settings.Default.shadowAngle = value;
            }
        }

        public double PlantRadius
        {
            get
            {
                return Properties.Settings.Default.PlantRadius;
            }
            set
            {
                Properties.Settings.Default.PlantRadius = value;
            }
        }

        public double PlantDensity
        {
            get
            {
                return Properties.Settings.Default.PlantDensity;
            }
            set
            {
                Properties.Settings.Default.PlantDensity = value;
            }
        }

        public void SaveProperties()
        {
            Properties.Settings.Default.Save();
        }


        public void DefaultReset()
        {
            Properties.Settings.Default.Reset();
        }
    }
}
