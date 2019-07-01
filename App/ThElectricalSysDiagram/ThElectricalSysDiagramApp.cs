using Autodesk.AutoCAD.Internal;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

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
            var usualDate = new DateTime(2019, 7, 1);
            var dateTime = DateTime.Today;
            if ((dateTime - usualDate).Days > 62)
            {
                return;
            }

            var view = new ThSysDiagramView();
            AcadApp.ShowModalWindow(view);
        }
    }
}
