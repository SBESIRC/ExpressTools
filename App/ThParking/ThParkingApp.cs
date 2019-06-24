using Autodesk.AutoCAD.ApplicationServices;
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

[assembly: ExtensionApplication(typeof(TianHua.AutoCAD.Parking.ThParkingApp))]

namespace TianHua.AutoCAD.Parking
{
    public class ThParkingApp : IExtensionApplication
    {
        public void Initialize()
        {
        }

        public void Terminate()
        {
        }

        [CommandMethod("TIANHUACAD", "THCNU", CommandFlags.Modal)]
        public void ShowNumberDialog()
        {
            //将程序有效期验证为3个月，一旦超过时限，要求用户更新，不进行命令注册
            var usualDate = new DateTime(2019, 6, 1);
            var dateTime = DateTime.Today;
            if ((dateTime - usualDate).Days > 62)
            {
                return;
            }

            ThParkingDialog parkingDialog = ThParkingDialog.GetInstance();
            AcadApp.ShowModalWindow(parkingDialog);
        }
    }
}
