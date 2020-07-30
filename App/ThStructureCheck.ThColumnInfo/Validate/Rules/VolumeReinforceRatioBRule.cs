using System.Collections.Generic;
using System.Linq;
using ThColumnInfo.Validate.Model;

namespace ThColumnInfo.Validate.Rules
{
    public class VolumeReinforceRatioBRule : IRule
    {
        private VolumeReinforceRatioBModel vrra = null;
        private string rule = "（《砼规》11.4.17-4）";
        public VolumeReinforceRatioBRule(VolumeReinforceRatioBModel volumeRRA)
        {
            this.vrra = volumeRRA;
        }
        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();

        private double calVolumnReinforceRatio = 0.0;
        private double volumeReinforceRatioLimited = 0.0;
        public void Validate()
        {
            if (this.vrra == null || !this.vrra.ValidateProperty())
            {
                return;
            }
            if(this.vrra.IsNonAntiseismic)
            {
                return;
            }
            this.calVolumnReinforceRatio = this.vrra.Cdm.GetVolumeStirrupRatio(this.vrra.ProtectLayerThickness);

            //体积配箍率限值
            if (this.vrra.ShearSpanRatio<=2)
            {
                this.volumeReinforceRatioLimited = 0.012;
            }
            else if(this.vrra.FortificationIntensity== 9 && 
                (this.vrra.AntiSeismicGrade.Contains("一级")))
            {
                this.volumeReinforceRatioLimited = 0.015;
            }
            else
            {
                return;
            }
            if (calVolumnReinforceRatio < volumeReinforceRatioLimited)
            {
                this.ValidateResults.Add("加密区箍筋体积配箍率不足 [" + calVolumnReinforceRatio + " < " +
                    volumeReinforceRatioLimited + "]，"+this.rule);
            }
            else
            {
                this.CorrectResults.Add("加密区箍筋体积配箍率满足抗震构造" + this.rule);
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            if (this.vrra.IsNonAntiseismic)
            {
                return steps;
            }
            steps.Add("类别：体积配箍率B（箍筋）");
            steps.Add("条目编号：517， 强制性：应，适用构件：KZ、ZHZ");
            steps.Add("适用功能：图纸校核，条文编号：11.4.17-4，条文页数：P179");
            steps.Add("条文：当剪跨比A 不大于2 时，宜采用复合螺旋箍或井字复合箍，其箍筋体积配筋率不应小于1. 2%; 9 度设防烈度→级抗震等级时，不应小于1. 5% 。");

            steps.Add("柱号 = " + this.vrra.Text);
            steps.Add("intStirrupDia= " + (int)this.vrra.Cdm.IntStirrupDia);
            steps.Add("intStirrupDiaArea= " + this.vrra.Cdm.IntStirrupDiaArea);
            steps.Add("cover= " + this.vrra.ProtectLayerThickness + "//保护层厚度");
            steps.Add("抗震等级 = " + this.vrra.AntiSeismicGrade);

            steps.Add(this.vrra.Cdm.GetVolumeStirrupRatioCalculation(
                this.vrra.ProtectLayerThickness) + this.calVolumnReinforceRatio);

            steps.Add("if(剪跨比[" + this.vrra.ShearSpanRatio + "] <= 2");
            steps.Add("  {");
            steps.Add("      体积配箍率限值=0.012");
            steps.Add("  }");
            steps.Add("else if(设防烈度[" + this.vrra.FortificationIntensity + "] == 9 && (抗震等级[" + this.vrra.AntiSeismicGrade
                + "].Contains(\"特一级\") || 抗震等级[" + this.vrra.AntiSeismicGrade + "].Contains(\"一级\"))");
            steps.Add("  {");
            steps.Add("      体积配箍率限值=0.015");
            steps.Add("  }");
            steps.Add("else");
            steps.Add("  {");
            steps.Add("      return;");
            steps.Add("  }");

            steps.Add("if (体积配箍率计算[" + this.calVolumnReinforceRatio + "] < 体积配筋率限值[" + this.volumeReinforceRatioLimited + "])");
            steps.Add("  {");
            steps.Add("      Err: 加密区箍筋体积配箍率不足"+this.rule);
            steps.Add("  }");
            steps.Add("else");
            steps.Add("  {");
            steps.Add("      Debugprint: 加密区箍筋体积配箍率满足抗震构造" + this.rule);
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
