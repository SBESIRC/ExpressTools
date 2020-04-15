using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Validate
{
    public class VolumeReinforceRatioBRule : IRule
    {
        private VolumeReinforceRatioBModel vrra = null;
        public VolumeReinforceRatioBRule(VolumeReinforceRatioBModel volumeRRA)
        {
            this.vrra = volumeRRA;
        }
        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();

        private double intStirrupDiaArea = 0.0;
        private double calVolumnReinforceRatio = 0.0;
        private double volumeReinforceRatioLimited = 0.0;
        public void Validate()
        {
            if (this.vrra == null || this.vrra.ValidateProperty() == false)
            {
                return;
            }
            this.intStirrupDiaArea=ThValidate.GetIronSectionArea((int)this.vrra.Cdm.IntStirrupDia);
            //体积配箍率计算
            double value1 = this.vrra.Cdm.IntXStirrupCount * intStirrupDiaArea *
               (this.vrra.Cdm.B - 2 * this.vrra.ProtectLayerThickness);
            double value2 = this.vrra.Cdm.IntYStirrupCount * intStirrupDiaArea *
                (this.vrra.Cdm.H - 2 * this.vrra.ProtectLayerThickness);
            double value3 = this.vrra.Cdm.B * this.vrra.Cdm.H * this.vrra.Cdm.IntStirrupSpacing;
            this.calVolumnReinforceRatio = (value1 + value2) / value3;

            //体积配箍率限值
            if (this.vrra.ShearSpanRatio<=2)
            {
                this.volumeReinforceRatioLimited = 0.012;
            }
            else if(this.vrra.FortificationIntensity== 30206 && 
                (this.vrra.Antiseismic.Contains("一级") && !this.vrra.Antiseismic.Contains("特")))
            {
                this.volumeReinforceRatioLimited = 0.015;
            }
            else
            {
                return;
            }
            if (calVolumnReinforceRatio < volumeReinforceRatioLimited)
            {
                this.ValidateResults.Add("加密区箍筋体积配箍率不足 (" + calVolumnReinforceRatio + "<[" +
                    volumeReinforceRatioLimited + "]) (11.4.17-4)");
            }
            else
            {
                this.CorrectResults.Add("加密区箍筋体积配箍率满足抗震构造");
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            steps.Add("类别：体积配箍率B（箍筋）");
            steps.Add("条目编号：517， 强制性：应，适用构件：KZ、ZHZ");
            steps.Add("适用功能：图纸校核，条文编号：11.4.17-4，条文页数：P179");
            steps.Add("条文：当剪跨比A 不大于2 时，宜采用复合螺旋箍或井字复合箍，其箍筋体积配筋率不应小于1. 2%; 9 度设防烈度→级抗震等级时，不应小于1. 5% 。");

            steps.Add("intStirrupDia= " + (int)this.vrra.Cdm.IntStirrupDia);
            steps.Add("intStirrupDiaArea= " + this.intStirrupDiaArea);
            steps.Add("cover= " + this.vrra.ProtectLayerThickness + "//保护层厚度");
            steps.Add("体积配箍率计算= (intXStirrupCount[" + this.vrra.Cdm.IntXStirrupCount +
                "]  *  intStirrupDiaArea[" + this.intStirrupDiaArea + "] * (B[" + this.vrra.Cdm.B + "] - 2 * cover[" +
                this.vrra.ProtectLayerThickness + "] + intYStirrupCount[" + this.vrra.Cdm.IntYStirrupCount + "] * intStirrupDiaArea[" +
                this.intStirrupDiaArea + "] * (H[" + this.vrra.Cdm.H + "] - 2 * cover[" + this.vrra.ProtectLayerThickness + "])) / (B[" +
                this.vrra.Cdm.B + "] * H[" + this.vrra.Cdm.H + "] * intStirrupSpacing[" +
                this.vrra.Cdm.IntStirrupSpacing + "]) = " + this.calVolumnReinforceRatio);

            steps.Add("if(剪跨比[" + this.vrra.ShearSpanRatio + "] <= 2");
            steps.Add("  {");
            steps.Add("      体积配箍率限值=0.012");
            steps.Add("  }");
            steps.Add("else if(设防烈度[" + this.vrra.FortificationIntensity + "] == 30206 && 抗震等级[" + this.vrra.Antiseismic + "] == 一级");
            steps.Add("  {");
            steps.Add("      体积配箍率限值=0.015");
            steps.Add("  }");

            steps.Add("if (体积配箍率计算[" + this.calVolumnReinforceRatio + "] < 体积配筋率限值[" + this.volumeReinforceRatioLimited + "])");
            steps.Add("  {");
            steps.Add("      Err: 加密区箍筋体积配箍率不足");
            steps.Add("  }");
            steps.Add("else");
            steps.Add("  {");
            steps.Add("      Ok: 加密区箍筋体积配箍率满足抗震构造");
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
