using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Validate
{
    public class VolumeReinforceRatioCRule : IRule
    {
        private VolumeReinforceRatioCModel vrrc = null;
        public VolumeReinforceRatioCRule(VolumeReinforceRatioCModel volumeRRC)
        {
            this.vrrc = volumeRRC;
        }
        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();

        private double intStirrupDiaArea;
        private double calVolumnReinforceRatio = 0.0;

        public void Validate()
        {
            if (this.vrrc == null || this.vrrc.ValidateProperty() == false)
            {
                return;
            }
            //体积配箍率计算
            this.intStirrupDiaArea = ThValidate.GetIronSectionArea((int)this.vrrc.Cdm.IntStirrupDia);
            double value1 = this.vrrc.Cdm.IntXStirrupCount * intStirrupDiaArea *
                (this.vrrc.Cdm.B - 2 * this.vrrc.ProtectLayerThickness);
            double value2 = this.vrrc.Cdm.IntYStirrupCount * intStirrupDiaArea *
                (this.vrrc.Cdm.H - 2 * this.vrrc.ProtectLayerThickness);
            double value3 = this.vrrc.Cdm.B * this.vrrc.Cdm.H * this.vrrc.Cdm.IntStirrupSpacing;

            this.calVolumnReinforceRatio = (value1 + value2) / value3;

            
            if (calVolumnReinforceRatio < this.vrrc.VolumnReinforceRatioLimited)
            {
                this.ValidateResults.Add("加密区箍筋体积配箍率不足 (" + calVolumnReinforceRatio + "<[" +
                    this.vrrc.VolumnReinforceRatioLimited + "]) (配筋规则)");
            }
            else
            {
                this.CorrectResults.Add("加密区箍筋体积配箍率满足计算要求");
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            steps.Add("类别：体积配箍率C    （箍筋）");
            steps.Add("条目编号：518， 强制性：应，适用构件：LZ、KZ、ZHZ");
            steps.Add("适用功能：图纸校核，条文编号：配筋规则，条文页数：-");
            steps.Add("条文：实配钢筋应满足计算值");

            steps.Add("intStirrupDia= " + (int)this.vrrc.Cdm.IntStirrupDia);
            steps.Add("intStirrupDiaArea= " + this.intStirrupDiaArea);
            steps.Add("cover= " + this.vrrc.ProtectLayerThickness + "//保护层厚度");
            steps.Add("体积配箍率计算= (intXStirrupCount[" + this.vrrc.Cdm.IntXStirrupCount +
                "]  *  intStirrupDiaArea[" + this.intStirrupDiaArea + "] * (B[" + this.vrrc.Cdm.B + "] - 2 * cover[" +
                this.vrrc.ProtectLayerThickness + "] + intYStirrupCount[" + this.vrrc.Cdm.IntYStirrupCount + "] * intStirrupDiaArea[" +
                this.intStirrupDiaArea + "] * (H[" + this.vrrc.Cdm.H + "] - 2 * cover[" + this.vrrc.ProtectLayerThickness + "])) / (B[" +
                this.vrrc.Cdm.B + "] * H[" + this.vrrc.Cdm.H + "] * intStirrupSpacing[" +
                this.vrrc.Cdm.IntStirrupSpacing + "]) = " + this.calVolumnReinforceRatio);

            steps.Add("if (体积配箍率计算[" + this.calVolumnReinforceRatio + "] < 体积配筋率限值[" + this.vrrc.VolumnReinforceRatioLimited + "])");
            steps.Add("  {");
            steps.Add("      Err: 加密区箍筋体积配箍率不足");
            steps.Add("  }");
            steps.Add("else");
            steps.Add("  {");
            steps.Add("      Ok: 加密区箍筋体积配箍率满足计算要求");
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
