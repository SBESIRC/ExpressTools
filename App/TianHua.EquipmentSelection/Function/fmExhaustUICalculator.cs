using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TianHua.FanSelection.Model;
using TianHua.Publics.BaseCode;


namespace TianHua.FanSelection.Function
{
    public static class fmExhaustUICalculator
    {
        //计算风口-当量直径
        public static double GetSmokeDiameter(ExhaustCalcModel model)
        {
            return Math.Round(2 * model.SmokeWidth.NullToDouble() * model.SmokeLength.NullToDouble() / (model.SmokeLength.NullToDouble() + model.SmokeWidth.NullToDouble()) , 0) ;
        }

        //获取Z值
        public static double GetZValue(ExhaustCalcModel model)
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
            return z;
        }

        //获取Z1值
        public static double GetZ1Value(ExhaustCalcModel model)
        {
            double qc = 0.7 * model.HeatReleaseRate.NullToDouble();
            return 0.166 * Math.Pow(qc, 0.4);
        }

        //Z值是否大于Z1
        public static bool IfZBiggerThanZ1(ExhaustCalcModel model)
        {
            return GetZValue(model) > GetZ1Value(model);
        }

        //获取Hq值
        public static double GetHqValue(ExhaustCalcModel model)
        {
            double hsp = model.SpaceHeight.NullToDouble() < 3 ? 0.5 * model.SpaceHeight.NullToDouble() : model.Axial_HighestHeight.NullToDouble();
            return 1.6 + 0.1 * hsp;
        }

        //计算轴对称型Mp值
        public static double GetAxialMpValue(ExhaustCalcModel model)
        {
            double hq = GetHqValue(model);
            double z = GetZValue(model);
            double mp = 0;
            double qc = 0.7 * model.HeatReleaseRate.NullToDouble();
            double z1 = GetZ1Value(model);
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
        public static double GetOverfloorMpValue(ExhaustCalcModel model)
        {
            double W = model.Spill_FireOpening.NullToDouble() + model.Spill_OpenBalcony.NullToDouble();
            double mp = 0.36 * (Math.Pow(model.HeatReleaseRate.NullToDouble() * Math.Pow(W, 2), 1.0 / 3.0) * (model.Spill_BalconySmokeBottom.NullToDouble() + 0.25 * model.Spill_FuelBalcony.NullToDouble()));
            return mp;
        }

        //计算窗口型Mp值
        public static double GetWindowMpValue(ExhaustCalcModel model)
        {
            double aw = 2.4 * Math.Pow(model.Window_WindowArea.NullToDouble(), 0.4) * Math.Pow(model.Window_WindowHeight.NullToDouble(), 0.2) - 2.1 * model.Window_WindowHeight.NullToDouble();
            double mp = 0.68 * Math.Pow(model.Window_WindowArea.NullToDouble() * Math.Pow(model.Window_WindowHeight.NullToDouble(), 0.5), 1.0 / 3.0) * Math.Pow(model.Window_SmokeBottom.NullToDouble() + aw, 5.0 / 3.0) + 1.59 * model.Window_WindowArea.NullToDouble() * Math.Pow(model.Window_WindowHeight.NullToDouble(), 0.5);
            return mp;
        }

        //计算dt
        public static double GetDtValue(ExhaustCalcModel model)
        {
            double qc = 0.7 * model.HeatReleaseRate.NullToDouble();
            double mp = 0;
            switch (model.PlumeSelection)
            {
                case "轴对称型":
                    mp = GetAxialMpValue(model);
                    break;
                case "阳台溢出型":
                    mp = GetOverfloorMpValue(model);
                    break;
                case "窗口型":
                    mp = GetWindowMpValue(model);
                    break;
                default:
                    break;
            }
            return qc / (mp / 1.01);
        }

        //计算aw值
        public static double GetawValue(ExhaustCalcModel model)
        {
            double aw = 2.4 * Math.Pow(model.Window_WindowArea.NullToDouble(), 0.4) * Math.Pow(model.Window_WindowHeight.NullToDouble(), 0.2) - 2.1 * model.Window_WindowHeight.NullToDouble();
            return aw;
        }


        public static double GeTValue(ExhaustCalcModel model)
        {
            return 293.15 + GetDtValue(model);
        }

    }
}
