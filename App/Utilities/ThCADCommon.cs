using System;
using System.IO;

namespace TianHua.AutoCAD.Utility.ExtensionTools
{
    public class ThCADCommon
    {
        // RegAppName
        public static readonly string RegAppName = "THCAD";

        // Area Frame
        public static readonly string RegAppName_AreaFrame = "THCAD_AF";
        public static readonly string RegAppName_AreaFrame_Version = "V2.2";

        // Fire Compartment
        public static readonly string RegAppName_AreaFrame_FireCompartment = "THCAD_FC";

        // Support 路径
        public static string SupportPath()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                @"Autodesk\ApplicationPlugins\ThCADPlugin.bundle\Contents\Support");
        }

        // Standard style 路径
        public static string StandardStylePath()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                @"Autodesk\ApplicationPlugins\ThCADPlugin.bundle\Contents\Standards\Style");
        }
    }
}
