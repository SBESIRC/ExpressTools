using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcHelper;

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

        private string m_outPutPath = null;

        public string OutputPath
        {
            get
            {
                if(!string.IsNullOrEmpty(m_outPutPath))
                {
                    return m_outPutPath;
                }

                var filepath = Properties.Settings.Default.FileSavePath;
                //
                if (filepath.IsNullOrEmpty())
                {
                    string mydoc = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    filepath = Path.Combine(mydoc, ThSitePlanCommon.ThSitePlan_File_Save_Path);
                    Directory.CreateDirectory(filepath);
                }

                if(!string.IsNullOrEmpty(Active.DocumentName))
                {
                    filepath = Path.Combine(filepath, Active.DocumentName);
                    if(Directory.Exists(filepath))
                    {
                        var dirs = Directory.GetDirectories(filepath);
                        int iter = 1;
                        foreach(var dir in dirs)
                        {
                            var dirName = new DirectoryInfo(dir).Name;
                            int num = 0;
                            if(int.TryParse(dirName, out num))
                            {
                                if(num >= iter)
                                {
                                    iter = num + 1;
                                }
                            }
                        }

                        var newDirName = iter.ToString();
                        if(iter < 10)
                        {
                            newDirName = newDirName.Insert(0, "0");
                        }

                        filepath = Path.Combine(filepath, newDirName);
                        Directory.CreateDirectory(filepath);
                    }
                    else
                    {
                        Directory.CreateDirectory(filepath);
                        filepath = Path.Combine(filepath, "01");
                    }
                }

                m_outPutPath = filepath;

                return m_outPutPath;
            }
            set
            {
                Properties.Settings.Default.FileSavePath = value;
            }
        }
        
        public void ResetOutputPath()
        {
            m_outPutPath = null;
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
