using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using AcHelper;
using AcHelper.Wrappers;
using System;
using System.IO;
using Linq2Acad;
using System.Collections.Generic;
using System.Linq;
using THColumnInfo.View;
using THColumnInfo.Controller;

[assembly: CommandClass(typeof(THColumnInfo.ThColumnInfoCommands))]
[assembly: ExtensionApplication(typeof(THColumnInfo.ThColumnInfoApp))]
namespace THColumnInfo
{
    public class ThColumnInfoApp : IExtensionApplication
    {
        public void Initialize()
        {
            //throw new NotImplementedException();
        }

        public void Terminate()
        {
            //throw new NotImplementedException();
        }
    }
    public class ThColumnInfoCommands
    {
        [CommandMethod("TIANHUACAD","ThColumnInfCheckWindow", CommandFlags.Modal)]
        public void THColumnInfCheckWindow()
        {
            CheckPalette.Instance.Show();
            DataPalette.Instance.Show();
        }
    }
}
