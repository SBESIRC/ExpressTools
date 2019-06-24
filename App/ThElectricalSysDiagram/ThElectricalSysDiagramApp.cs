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
        //const string CMD_GROUPNAME = "TIANHUACAD";
        //const string CMD_THCADLOGIN_GLOBAL_NAME = "THLIXUYIN";

        public void Initialize()
        {
            //RegisterCommands();
        }

        public void Terminate()
        {
        }

        public void RegisterCommands()
        {
            ////注册登录命令
            //Utils.AddCommand(CMD_GROUPNAME, CMD_THCADLOGIN_GLOBAL_NAME, CMD_THCADLOGIN_GLOBAL_NAME, CommandFlags.Modal, new CommandCallback(Show));
        }


    }

    public class ThElectricalSysDiagramCommands
    {
        [CommandMethod("THLIXUYIN")]
        public void Show()
        {
            var view = new ThSysDiagramView();
            AcadApp.ShowModalWindow(view);
        }
    }
}
