using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Internal;
using Autodesk.AutoCAD.Runtime;
using DotNetARX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;


namespace TianHua.AutoCAD.Parking
{
    public class ThParkingApp : IExtensionApplication
    {
        const string CMD_GROUPNAME = "TIANHUACAD";
        const string CMD_THCNU_GLOBAL_NAME = "THCNU";

        public void Initialize()
        {
            RegisterCommands();

        }

        public void Terminate()
        {
            UnregisterCommands();

        }

        public void RegisterCommands()
        {
            //注册车位编号命令
            Utils.AddCommand(CMD_GROUPNAME, CMD_THCNU_GLOBAL_NAME, CMD_THCNU_GLOBAL_NAME, CommandFlags.Modal, new CommandCallback(ShowNumberDialog));
        }

        public void UnregisterCommands()
        {
            //反注册车位编号命令
            Utils.RemoveCommand(CMD_GROUPNAME, CMD_THCNU_GLOBAL_NAME);
        }

        public void ShowNumberDialog()
        {
            ThParkingDialog parkingDialog = ThParkingDialog.GetInstance();
            AcadApp.ShowModalWindow(parkingDialog);
            //if (parkingDialog.ResultState == true)
            //{
            //    var numberManager = new ThNumberingManager();
            //    numberManager.Numbering(parkingDialog);
            //}
        }



    }
}
