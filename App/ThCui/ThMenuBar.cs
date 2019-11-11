using System.IO;
using AcHelper;
using DotNetARX;
using Autodesk.AutoCAD.Customization;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace TianHua.AutoCAD.ThCui
{
    public class ThMenuBar
    {
        private readonly string CUIXFILE = "THMenubar.cuix";
        private readonly string MENUGROUP = "TianHuaCustom";

        public void LoadThMenu()
        {
            try
            {
                string cuiFile = Path.Combine(ThCADCommon.ResourcesPath(), CUIXFILE);
                CustomizationSection cs = Active.Document.AddCui(cuiFile, MENUGROUP);
                cs.LoadCui();
            }
            catch
            {
                // 捕捉异常，可能是cuix文件不存在
            }
        }
    }
}
