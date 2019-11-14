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

        // Expire start date
        public static int Global_Expire_Duration = 90;
        public static DateTime Global_Expire_Start_Date = new DateTime(2019, 10, 15);

        // RegAppName
        public static readonly string RegAppName = "THCAD";

        // Area Frame
        public static readonly string RegAppName_AreaFrame = "THCAD_AF";
        public static readonly string RegAppName_AreaFrame_Version = "V2.2";
        public static readonly string RegAppName_AreaFrame_Version_Legacy = "V2.1";

        // Fire Compartment
        public static readonly string RegAppName_AreaFrame_FireCompartment_Fill = "THCAD_FCFill";
        public static readonly string RegAppName_AreaFrame_FireCompartment_Parking = "THCAD_FC_P";
        public static readonly string RegAppName_AreaFrame_FireCompartment_Commerce = "THCAD_FC_C";

        // Support 路径
        public static string SupportPath()
        {
            return Path.Combine(ContentsPath(), "Support");
        }

        // Standard style 路径
        public static string StandardStylePath()
        {
            return Path.Combine(ContentsPath(), "Standards", "Style");
        }

        // Resources 路径
        public static string ResourcesPath()
        {
            return Path.Combine(ContentsPath(), "Resources");
        }

        // Plotters 路径
        public static string PlottersPath()
        {
            return Path.Combine(ContentsPath(), "Plotters");
        }

        // PrinterDescPath 路径
        public static string PrinterDescPath()
        {
            return Path.Combine(ContentsPath(), "Plotters", "PMP Files");
        }

        // PrinterStyleSheetPath 路径
        public static string PrinterStyleSheetPath()
        {
            return Path.Combine(ContentsPath(), "Plotters", "Plot Styles");
        }

        // Contents 路径
        private static string ContentsPath()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                @"Autodesk\ApplicationPlugins\ThCADPlugin.bundle\Contents");
        }
    }
}
