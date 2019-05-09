using Autodesk.AutoCAD.Runtime;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace TianHua.AutoCAD.ThCui
{
    public class ThCuiManager
    {
        /// <summary>
        /// 图块配置
        /// </summary>
        [CommandMethod("TIANHUACAD", "THBLS", CommandFlags.Modal)]
        public void ShowToolPalette()
        {
            ThToolPalette toolPalette = new ThToolPalette();
            AcadApp.ShowModalDialog(toolPalette);
        }

    }
}
