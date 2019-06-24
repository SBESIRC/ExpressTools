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
        [CommandMethod("THBEE", CommandFlags.Modal)]
        public void BlockExchangeCmd()
        {
            var view = new ThSysDiagramView();
            AcadApp.ShowModalWindow(view);
        }
    }
}
