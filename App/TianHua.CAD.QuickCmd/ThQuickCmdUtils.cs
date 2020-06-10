using System.Configuration;

namespace TianHua.CAD.QuickCmd
{
    public class ThQuickCmdUtils
    {
        public static string Profile()
        {
            var section = (ClientSettingsSection)ConfigurationManager.GetSection("userSettings/TianHua.AutoCAD.ThCui.Properties.Settings");
            var element = section.Settings.Get("Profile");
            switch(element.Value.ValueXml.InnerText)
            {
                case "0":
                    return "方案";
                case "1":
                    return "建筑";
                case "2":
                    return "结构";
                case "3":
                    return "暖通";
                case "4":
                    return "电气";
                case "5":
                    return "给排水";
                default:
                    return "";
            }
        }
    }
}