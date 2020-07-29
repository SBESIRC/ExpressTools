using System;
using Autodesk.AutoCAD.Runtime;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace TianHua.FanSelection.UI
{
    public class EquipmentSelectionApp : IExtensionApplication
    {
        public void Initialize()
        {
            //
        }

        public void Terminate()
        {
            //
        }

        [CommandMethod("TIANHUACAD", "THFJ", CommandFlags.Modal)]
        public void ThEquipmentSelection()
        {
            var dwgName = Convert.ToInt32(AcadApp.GetSystemVariable("DWGTITLED"));
            if (dwgName == 0)
            {
                AcadApp.ShowAlertDialog("请先保存当前图纸!");
                return;
            }
            AcadApp.ShowModelessDialog(fmFanSelection.GetInstance());
        }
    }
}