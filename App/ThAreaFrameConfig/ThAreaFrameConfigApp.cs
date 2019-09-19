using System.Collections.Generic;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using ThAreaFrameConfig.WinForms;
using ThAreaFrameConfig.Model;
using ThAreaFrameConfig.ViewModel;
using ThAreaFrameConfig.Presenter;
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
                        Number = 1,
                        Storey = 2,
                        Subkey = 13,
                        Frames = new List<ThFireCompartmentAreaFrame>()
                    }
                }
            };

            // 创建防火分区
            ThFireCompartmentHelper.PickFireCompartmentFrames(settings.Compartments[0], settings.Layers["OUTERFRAME"]);

            // 创建防火分区填充
            if (settings.GenerateHatch)
            {
                foreach(var compartment in settings.Compartments)
                {
                    // TODO: 创建Hatch对象
                    //  https://www.keanw.com/2010/06/creating-transparent-hatches-in-autocad-using-net.html
                    Hatch hatch = new Hatch()
                    {
                        // Set our transparency to 50% (=127)
                        // Alpha value is Truncate(255 * (100-n)/100)
                        Transparency = new Transparency(127)
                    };
                    hatch.SetHatchPattern(HatchPatternType.PreDefined, "ANSI31");
                    ThFireCompartmentHelper.FillFireCompartment(compartment, hatch);
                }
            }
        }
    }
}