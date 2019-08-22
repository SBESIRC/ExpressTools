using Autodesk.AutoCAD.Runtime;
using ThAreaFrameConfig.WinForms;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace ThAreaFrameConfig
{
    public class ThAreaFrameConfigApp : IExtensionApplication
    {
        public void Initialize()
        {
        }

        public void Terminate()
        {
        }

        [CommandMethod("TIANHUACAD", "THAFC", CommandFlags.Modal)]
        public void AreaFrameConfig()
        {
            ThResidentialRoomControlDialog dlg = new ThResidentialRoomControlDialog();
            AcadApp.ShowModelessDialog(dlg);
        }

        [CommandMethod("TIANHUACAD", "THAFC2", CommandFlags.Modal)]
        public void AreaFrameConfig2()
        {
            ThSitePlanDialog dlg = new ThSitePlanDialog();
            AcadApp.ShowModelessDialog(dlg);
        }
    }
}