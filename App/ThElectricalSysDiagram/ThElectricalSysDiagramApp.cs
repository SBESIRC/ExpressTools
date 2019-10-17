using System;
using Autodesk.AutoCAD.Runtime;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
using TianHua.AutoCAD.Utility.ExtensionTools;

[assembly: CommandClass(typeof(ThElectricalSysDiagram.ThElectricalSysDiagramCommands))]
[assembly: ExtensionApplication(typeof(ThElectricalSysDiagram.ThElectricalSysDiagramApp))]

namespace ThElectricalSysDiagram
{    
    public class ThElectricalSysDiagramApp : IExtensionApplication
    {
        public void Initialize()
        {
        }

        public void Terminate()
        {
        }
    }

    public class ThElectricalSysDiagramCommands
    {
        [CommandMethod("TIANHUACAD","THBEE", CommandFlags.Modal)]
        public void BlockExchangeCmd()
        {
            //将程序有效期验证为3个月，一旦超过时限，要求用户更新，不进行命令注册
            if ((DateTime.Today - ThCADCommon.Global_Expire_Start_Date).Days > ThCADCommon.Global_Expire_Duration)
            {
                return;
            }

            var view = new ThSysDiagramView();
            AcadApp.ShowModalWindow(view);
        }
    }
}
