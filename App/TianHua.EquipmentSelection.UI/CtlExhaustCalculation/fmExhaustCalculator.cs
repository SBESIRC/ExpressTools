using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TianHua.AutoCAD.Utility.ExtensionTools;
using TianHua.FanSelection.Model;
using TianHua.Publics.BaseCode;
namespace TianHua.FanSelection.UI.CtlExhaustCalculation
{
    public static class fmExhaustCalculator
    {
        //计算空间净高小于6的场所最小风量
        public static double GetMinVolumeForLess6(ExhaustCalcModel model)
        {
            double textArea = model.CoveredArea.NullToDouble();
            double txtUnitVolume = model.UnitVolume.NullToDouble();
            return Math.Max(textArea * txtUnitVolume, 15000.0);
        }

        //计算空间净高大于6的场所最小风量
        public static double GetMinVolumeForGreater6(ExhaustCalcModel model)
        {
            if (model.SpatialTypes.IsNullOrEmptyOrWhiteSpace() || model.SpaceHeight.IsNullOrEmptyOrWhiteSpace())
            {
                return 0;
            }
            List<ExhaustSpaceInfo> largerAndLowerModel = new List<ExhaustSpaceInfo>();
            if (model.SpatialTypes.Contains("办公室"))
            {
                largerAndLowerModel = FindLargerAndLowerModel(model, "办公室");
                return 10000 * GetLinearInterpolation(largerAndLowerModel, model.SpaceHeight.NullToDouble());
            }
            else if (model.SpatialTypes.Contains("商店"))
            {
                largerAndLowerModel = FindLargerAndLowerModel(model, "商店");
                return 10000 * GetLinearInterpolation(largerAndLowerModel, model.SpaceHeight.NullToDouble());
            }
            else if (model.SpatialTypes.Contains("仓库"))
            {
                largerAndLowerModel = FindLargerAndLowerModel(model, "仓库");
                return 10000 * GetLinearInterpolation(largerAndLowerModel, model.SpaceHeight.NullToDouble());
            }
            else
            {
                largerAndLowerModel = FindLargerAndLowerModel(model, "厂房");
                return 10000 * GetLinearInterpolation(largerAndLowerModel, model.SpaceHeight.NullToDouble());
            }
        }

        public static List<ExhaustSpaceInfo> FindLargerAndLowerModel(ExhaustCalcModel model, string spacetype)
        {
            var exhaustSpaceJson = ReadTxt(Path.Combine(ThCADCommon.SupportPath(), "最小排烟量.json"));
            var exhaustSpaceInfo = FuncJson.Deserialize<List<ExhaustSpaceInfo>>(exhaustSpaceJson);

            var largerNetHeightItems = exhaustSpaceInfo.Where(e => e.SpaceNetHeight >= model.SpaceHeight.NullToDouble() && e.SpaceType.Contains(spacetype) && e.HasSprinkler == model.IsSpray).ToList();
            var lowerNetHeightItems = exhaustSpaceInfo.Where(e => e.SpaceNetHeight < model.SpaceHeight.NullToDouble() && e.SpaceType.Contains(spacetype) && e.HasSprinkler == model.IsSpray).ToList();
            if (largerNetHeightItems.Count != 0)
            {
                if (lowerNetHeightItems.Count != 0)
                {
                    var largerModel = largerNetHeightItems.OrderBy(e => e.SpaceNetHeight).First();
                    var lowerModel = lowerNetHeightItems.OrderBy(e => e.SpaceNetHeight).Last();
                    return new List<ExhaustSpaceInfo>() { largerModel, lowerModel };
                }
                else
                {
                    var largerModel = largerNetHeightItems.OrderBy(e => e.SpaceNetHeight).First();
                    return new List<ExhaustSpaceInfo>() { largerModel };
                }
            }
            else
            {
                return new List<ExhaustSpaceInfo>();
            }

        }

        public static double GetLinearInterpolation(List<ExhaustSpaceInfo> models, double interx)
        {
            if (models.Count == 0)
            {
                return 0;
            }
            else
            {
                if (models.Count == 1)
                {
                    return models.First().MinVolume;
                }
                else
                {
                    return Math.Round(((interx - models[0].SpaceNetHeight) * (models[1].MinVolume - models[0].MinVolume) / (models[1].SpaceNetHeight - models[0].SpaceNetHeight)) + models[0].MinVolume, 2);
                }
            }
        }

        //计算汽车库最小风量
        public static double GetMinVolumeForGarage(ExhaustCalcModel model)
        {
            double height = model.SpaceHeight.NullToDouble();
            if (height < 3)
            {
                return 30000;
            }
            else
            {
                if (height < 4)
                {
                    return 31500;
                }
                else
                {
                    if (height < 5)
                    {
                        return 33000;
                    }
                    else
                    {
                        if (height < 6)
                        {
                            return 34500;
                        }
                        else
                        {
                            return 36000;
                        }
                    }
                }
            }
        }

        //计算公共建筑房间内走道或回廊设置排烟时最小风量
        public static double GetMinVolumeForCtlCloistersRooms(ExhaustCalcModel model)
        {
            double calculatevalue = model.CoveredArea.NullToDouble() * model.UnitVolume.NullToDouble();
            return Math.Max(calculatevalue,13000);
        }

        //计算热释放速率
        //净高小于等于6的场所除外
        public static double GetHeatReleaseRate(ExhaustCalcModel model)
        {
            bool hassprinkler = model.IsSpray;
            if (model.SpatialTypes.IsNullOrEmptyOrWhiteSpace())
            {
                return 0;
            }
            else if (model.SpaceHeight.NullToDouble() >8)
            {
                hassprinkler = false;
            }
            var heatReleaseJson = ReadTxt(Path.Combine(ThCADCommon.SupportPath(), "火灾达到静态时的热释放速率.json"));
            var heatReleaseModels = FuncJson.Deserialize<List<HeatReleaseInfo>>(heatReleaseJson);
            var heatmodels = new List<HeatReleaseInfo>();
            if (model.SpatialTypes.Contains("办公室"))
            {
                heatmodels = heatReleaseModels.Where(h => h.BuildType.Contains("办公室") && h.HasSprinkler == hassprinkler).ToList();
            }
            else if (model.SpatialTypes.Contains("商店"))
            {
                heatmodels = heatReleaseModels.Where(h => h.BuildType.Contains("商店") && h.HasSprinkler == hassprinkler).ToList();
            }
            else if (model.SpatialTypes.Contains("仓库"))
            {
                heatmodels = heatReleaseModels.Where(h => h.BuildType.Contains("仓库") && h.HasSprinkler == hassprinkler).ToList();
            }
            else if (model.SpatialTypes.Contains("车库"))
            {
                heatmodels = heatReleaseModels.Where(h => h.BuildType.Contains("车库") && h.HasSprinkler == hassprinkler).ToList();
            }
            else
            {
                heatmodels = heatReleaseModels.Where(h => h.BuildType.Contains("厂房") && h.HasSprinkler == hassprinkler).ToList();
            }
            if (heatmodels.IsNull() || heatmodels.Count() == 0)
            {
                return 0;
            }
            return 1000*heatmodels.Select(h => h.ReleaseSpeed).First();
        }

        //计算风口-当量直径
        public static double GetSmokeDiameter(ExhaustCalcModel model)
        {
            return Math.Round(2 * model.SmokeWidth.NullToDouble() * model.SmokeLength.NullToDouble() / (model.SmokeLength.NullToDouble() + model.SmokeWidth.NullToDouble()) , 0) ;
        }

        //计算轴对称型Mp值
        public static double GetAxialCalcAirVolum(ExhaustCalcModel model)
        {
            double hsp = model.SpaceHeight.NullToDouble() < 3 ? 0.5 * model.SpaceHeight.NullToDouble() : model.Axial_HighestHeight.NullToDouble();
            double hq = 1.6 + 0.1 * hsp;
            double z = 0;
            if (model.Axial_HangingWallGround.IsNullOrEmptyOrWhiteSpace())
            {
                z = hq;
            }
            else
            {
                double hdd = model.Axial_HangingWallGround.NullToDouble() - model.Axial_FuelFloor.NullToDouble();
                z = Math.Max(hdd, hq);
            }

            double mp = 0;
            double qc = 0.7 * model.HeatReleaseRate.NullToDouble();
            double z1 = 0.166 * Math.Pow(qc, 0.4);
            if (z > z1)
            {
                mp = 0.071 * Math.Pow(qc, 1.0 / 3.0) * Math.Pow(z, 5.0 / 3.0) + 0.0018 * qc;
            }
            else
            {
                mp = 0.032 * Math.Pow(qc, 3.0 / 5.0) * z;
            }
            return mp;
        }

        //计算阳台溢出型Mp值
        public static double GetOverfloorCalcAirVolum(ExhaustCalcModel model)
        {
            double W = model.Spill_FireOpening.NullToDouble() + model.Spill_OpenBalcony.NullToDouble();
            double mp = 0.36 * (Math.Pow(model.HeatReleaseRate.NullToDouble() * Math.Pow(W, 2), 1.0 / 3.0) * (model.Spill_BalconySmokeBottom.NullToDouble() + 0.25 * model.Spill_FuelBalcony.NullToDouble()));
            return mp;
        }

        //计算窗口型Mp值
        public static double GetWindowCalcAirVolum(ExhaustCalcModel model)
        {
            double aw = 2.4 * Math.Pow(model.Window_WindowArea.NullToDouble(), 0.4) * Math.Pow(model.Window_WindowHeight.NullToDouble(), 0.2) - 2.1 * model.Window_WindowHeight.NullToDouble();
            double mp = 0.68 * Math.Pow(model.Window_WindowArea.NullToDouble() * Math.Pow(model.Window_WindowHeight.NullToDouble(), 0.5), 1.0 / 3.0) * Math.Pow(model.Window_SmokeBottom.NullToDouble() + aw, 5.0 / 3.0) + 1.59 * model.Window_WindowArea.NullToDouble() * Math.Pow(model.Window_WindowHeight.NullToDouble(), 0.5);
            return mp;
        }

        //计算排烟量
        public static double GetCalcAirVolum(ExhaustCalcModel model)
        {
            double qc = 0.7 * model.HeatReleaseRate.NullToDouble();
            double mp = 0;
            switch (model.PlumeSelection)
            {
                case "轴对称型":
                    mp = GetAxialCalcAirVolum(model);
                    break;
                case "阳台溢出型":
                    mp = GetOverfloorCalcAirVolum(model);
                    break;
                case "窗口型":
                    mp = GetWindowCalcAirVolum(model);
                    break;
                default:
                    break;
            }
            double t = 293.15 + qc / (mp / 1.01);
            return Math.Round(3600*mp * t / (1.2 * 293.15),0);
        }

        //计算排烟位置系数
        public static double GetSmokeFactor(ExhaustCalcModel model)
        {
            if (model.SmokeFactorOption.Contains("≥2"))
            {
                return 1;
            }
            else
            {
                return 0.5;
            }
        }

        //计算最大允许排烟
        public static double GetMaxSmoke(ExhaustCalcModel model)
        {
            double qc = 0.7 * model.HeatReleaseRate.NullToDouble();
            double dt = 0;
            switch (model.PlumeSelection)
            {
                case "轴对称型":
                    dt = qc / (GetAxialCalcAirVolum(model) / 1.01);
                    break;
                case "阳台溢出型":
                    dt = qc / (GetOverfloorCalcAirVolum(model) / 1.01);
                    break;
                case "窗口型":
                    dt = qc / (GetWindowCalcAirVolum(model) / 1.01);
                    break;
                default:
                    return 0;
            }
            return Math.Round(3600*4.16 * model.SmokeFactorValue.NullToDouble() * Math.Pow(model.SmokeThickness.NullToDouble(), 2.5) * Math.Pow(dt / 293.15, 0.5) , 1);
        }

        //判断选型系数
        public static string SelectionFactorCheck(string factor)
        {
            return factor.NullToDouble() < 1.2 ? "1.2" : factor;
        }

        public static string ReadTxt(string _Path)
        {
            try
            {
                using (StreamReader _StreamReader = File.OpenText(_Path))
                {
                    return _StreamReader.ReadToEnd();
                }
            }
            catch
            {
                XtraMessageBox.Show("数据文件读取时发生错误！");
                return string.Empty;

            }
        }

    }
}
