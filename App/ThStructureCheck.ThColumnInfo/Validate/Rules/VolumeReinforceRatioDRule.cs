using System.Collections.Generic;
using ThColumnInfo.Validate.Model;

namespace ThColumnInfo.Validate.Rules
{
    public class VolumeReinforceRatioDRule : IRule
    {
        private VolumeReinforceRatioAModel vrra = null;
        private string rule = "（《高规》 10.2.11-8）";
        public VolumeReinforceRatioDRule(VolumeReinforceRatioAModel volumeRRA)
        {
            this.vrra = volumeRRA;
        }
        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();

        private double calVolumnReinforceRatio = 0.0;
        public void Validate()
        {
            if (this.vrra == null || !this.vrra.ValidateProperty())
            {
                return;
            }
            this.calVolumnReinforceRatio = this.vrra.Cdm.GetVolumeStirrupRatio(this.vrra.ProtectLayerThickness);

            if(this.vrra.Code.ToUpper().Contains("ZHZ") && this.vrra.AntiSeismicGrade.Contains("非"))
            {
                if(this.calVolumnReinforceRatio<0.008)
                {
                    this.ValidateResults.Add("体积配箍率不宜小于0.8% [" + calVolumnReinforceRatio +
                    " < " + 0.008 + "]，" + this.rule);
                }
                else
                {
                    this.CorrectResults.Add("体积配箍率不小于0.8%" + this.rule);
                }
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            steps.Add("类别：体积配箍率D（箍筋）");
            steps.Add("强制性：宜，适用构件：ZHZ");
            steps.Add("条文：转换柱、非抗震时、体积配箍率不宜小于0.8%");
            steps.Add("柱号 = " + this.vrra.Text);
            steps.Add("intStirrupDia= " + (int)this.vrra.Cdm.IntStirrupDia);
            steps.Add("intStirrupDiaArea= " + this.vrra.Cdm.IntStirrupDiaArea);
            steps.Add("cover= " + this.vrra.ProtectLayerThickness + "//保护层厚度");
            steps.Add(this.vrra.Cdm.GetVolumeStirrupRatioCalculation(this.vrra.ProtectLayerThickness) + this.calVolumnReinforceRatio);
            steps.Add("if (柱号[" + this.vrra.Code + "].Contains(\"ZHZ\") && " +
                "抗震等级[" +this.vrra.AntiSeismicGrade+"].Contains(\"非抗震\"))");
            steps.Add("  {");
            steps.Add("      if(体积配箍率["+ this.calVolumnReinforceRatio+"] < 0.008)");
            steps.Add("        {");
            steps.Add("           Err: 体积配箍率不宜小于0.8%" + this.rule);
            steps.Add("        }");
            steps.Add("        else");
            steps.Add("        {");
            steps.Add("           Debugprint: 体积配箍率不小于0.8%" + this.rule);
            steps.Add("        }");
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
