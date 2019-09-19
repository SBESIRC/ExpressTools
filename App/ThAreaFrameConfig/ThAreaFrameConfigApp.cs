using AcHelper;
using AcHelper.Wrappers;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.EditorInput;
using ThAreaFrameConfig.WinForms;
using ThAreaFrameConfig.Model;
using TianHua.AutoCAD.Utility.ExtensionTools;
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
            using (var dlg = new ThFireProofingDialog())
            {
                AcadApp.ShowModalDialog(dlg);
            }
        }

        // 测试命令
        [CommandMethod("TIANHUACAD", "THXXX", CommandFlags.Modal)]
        public void THXXX()
        {
            using (AcTransaction tr = new AcTransaction())
            {
                var pline = SelectionTool.ChooseEntity<Polyline>();
                Active.Editor.WriteLine(pline.ObjectId.OldIdPtr.AreaEx());
            }
        }
    }
}