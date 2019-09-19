using System.Collections.Generic;
using AcHelper.Wrappers;
using Autodesk.AutoCAD.Runtime;
using ThAreaFrameConfig.WinForms;
using ThAreaFrameConfig.Model;
using ThAreaFrameConfig.ViewModel;
using ThAreaFrameConfig.Presenter;
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
        [CommandMethod("TIANHUACAD", "THFETCLI", CommandFlags.Modal)]
        public void THFETCLI()
        {
            // 初始化防火分区设置
            var settings = new ThCommerceFireProofSettings()
            {
                Info = new ThCommerceFireProofSettings.BuildingInfo()
                {
                    subKey = 13,
                    AboveGroundStoreys = 1,
                    fireResistance = ThCommerceFireProofSettings.FireResistance.Level1
                },
                Layers = new Dictionary<string, string>()
                {
                    { "INNERFRAME", "AD-INDX"},
                    { "OUTERFRAME", "AD-AREA-DIVD" }
                },
                Compartments = new List<ThFireCompartment>()
                {
                    new ThFireCompartment()
                    {
                        Number = 1,
                        Storey = 2,
                        Subkey = 13,
                        Frames = new List<ThFireCompartmentAreaFrame>()
                    }
                }
            };

            // 创建防火分区
            ThFireCompartmentHelper.PickFireCompartmentFrames(settings.Compartments[0], settings.Layers["OUTERFRAME"]);
        }
    }
}