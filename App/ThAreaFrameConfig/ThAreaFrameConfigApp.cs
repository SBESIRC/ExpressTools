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

        // 测试命令
        [CommandMethod("TIANHUACAD", "THFETCLI", CommandFlags.Modal)]
        public void THFETCLI()
        {
            // 创建防火分区
            ThFireCompartmentDbHelper.PickFireCompartmentFrames(13, 1, 1);

            // 获取防火分区
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                // 合并防火分区
                var compartments = acadDatabase.Database.CommerceFireCompartments();
                if (compartments.Count == 3)
                {
                    ThFireCompartmentDbHelper.MergeFireCompartment(compartments[0], compartments[1]);
                }

                // 规整防火分区
                var compartments2 = acadDatabase.Database.CommerceFireCompartments();
                if (compartments2.Count == 2)
                {
                    ThFireCompartmentDbHelper.NormalizeFireCompartments(compartments2);
                }
            }
        }
    }
}