using System;
using Autodesk.AutoCAD.Runtime;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
using TianHua.AutoCAD.Utility.ExtensionTools;

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
            ThParkingDialog parkingDialog = ThParkingDialog.GetInstance();
            AcadApp.ShowModalWindow(parkingDialog);
        }
    }
}
