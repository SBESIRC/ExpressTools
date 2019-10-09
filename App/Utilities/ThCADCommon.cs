using System;
using System.IO;
using Autodesk.AutoCAD.Geometry;

namespace TianHua.AutoCAD.Utility.ExtensionTools
{
    public class ThCADCommon
    {
        // Tolerance
        // Tolerance.Global默认值：new Tolerance(1e-10, 1e-12)
        public static Tolerance Global_Tolerance = new Tolerance(1e-4, 1e-4);

        // RegAppName
        public static readonly string RegAppName = "THCAD";

        // Area Frame
        public static readonly string RegAppName_AreaFrame = "THCAD_AF";
        public static readonly string RegAppName_AreaFrame_Version = "V2.2";
        public static readonly string RegAppName_AreaFrame_Version_Legacy = "V2.1";

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

        // Resources 路径
        public static string ResourcePath()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                @"Autodesk\ApplicationPlugins\ThCADPlugin.bundle\Contents\Resources");
        }
    }
}
