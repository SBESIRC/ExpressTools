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
            var view = new ThSysDiagramView();
            AcadApp.ShowModalWindow(view);
        }
    }
}
