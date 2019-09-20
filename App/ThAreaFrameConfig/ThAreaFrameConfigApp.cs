using System.Collections.Generic;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using ThAreaFrameConfig.WinForms;
using ThAreaFrameConfig.Model;
using ThAreaFrameConfig.ViewModel;
using ThAreaFrameConfig.Presenter;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
using System.Windows.Forms;

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
                // 初始化防火分区设置
                dlg.CommerceFireProofSettings = new ThCommerceFireProofSettings()
                {
                    GenerateHatch = true,
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
                            Subkey = 13,
                            Storey = 1,
                            Index = 1,
                            SelfExtinguishingSystem = true,
                            Frames = new List<ThFireCompartmentAreaFrame>()
                        }
                    }
                };

                // 显示模态对话框
                if (AcadApp.ShowModalDialog(dlg) != DialogResult.OK)
                {
                    return;
                }
            }
        }

        // 测试命令
        [CommandMethod("TIANHUACAD", "THFETCLI", CommandFlags.Modal)]
        public void THFETCLI()
        {
            // 创建防火分区
            ThFireCompartmentHelper.PickFireCompartmentFrames(13, 1, 1);
        }
    }
}