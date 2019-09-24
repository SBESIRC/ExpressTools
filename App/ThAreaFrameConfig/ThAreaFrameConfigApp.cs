using System.Windows.Forms;
using System.Collections.Generic;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using ThAreaFrameConfig.WinForms;
using ThAreaFrameConfig.Model;
using ThAreaFrameConfig.ViewModel;
using AcHelper;
using Linq2Acad;
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

        // 天华单体规划
        [CommandMethod("TIANHUACAD", "THBPS", CommandFlags.Modal)]
        public void THBPS()
        {
            ThResidentialRoomControlDialog dlg = new ThResidentialRoomControlDialog();
            AcadApp.ShowModelessDialog(dlg);
        }

        // 天华总平规划
        [CommandMethod("TIANHUACAD", "THSPS", CommandFlags.Modal)]
        public void THSPS()
        {
            ThSitePlanDialog dlg = new ThSitePlanDialog();
            AcadApp.ShowModelessDialog(dlg);
        }

        // 消防疏散表
        [CommandMethod("TIANHUACAD", "THFET", CommandFlags.Modal)]
        public void THFET()
        {
            ThFireProofingDialog dlg = new ThFireProofingDialog();
            AcadApp.ShowModelessDialog(dlg);
        }
    }
}