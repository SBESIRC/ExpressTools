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

        [CommandMethod("TIANHUACAD", "THEQSEL", CommandFlags.Modal)]
        public void ThEquipmentSelection()
        {
            //using (var dlg = new Form1())
            //{
            //    AcadApp.ShowModelessDialog(dlg);
            //}
            var dlg = fmFanSelection.GetInstance();
            AcadApp.ShowModelessDialog(dlg);
        }
    }
}