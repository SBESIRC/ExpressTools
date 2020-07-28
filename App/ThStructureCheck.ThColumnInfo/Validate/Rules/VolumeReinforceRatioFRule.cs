using System.Collections.Generic;
using ThColumnInfo.Validate.Model;

namespace ThColumnInfo.Validate.Rules
{
    public class VolumeReinforceRatioFRule : IRule
    {
        private VolumeReinforceRatioAModel vrra = null;
        private string rule = "（《高规》 10.2.10-3）";
        public VolumeReinforceRatioFRule(VolumeReinforceRatioAModel volumeRRA)
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

            if(this.vrra.Code.ToUpper().Contains("ZHZ"))
            {
                if(this.vrra.AntiSeismicGrade.Contains("一级") ||
                    this.vrra.AntiSeismicGrade.Contains("二级") ||
                    this.vrra.AntiSeismicGrade.Contains("三级") ||
                    this.vrra.AntiSeismicGrade.Contains("四级"))
                {
                    if(this.calVolumnReinforceRatio < 0.015)
                    {
                        this.ValidateResults.Add("箍筋体积配箍率不应小于1.5% [" + calVolumnReinforceRatio +
                   " < " + 0.015 + "]，" + this.rule);
                    }
                    else
                    {
                        this.CorrectResults.Add("箍筋体积配箍率大于1.5%" + this.rule);
                    }
                }
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            steps.Add("类别：体积配箍率（箍筋）");
            steps.Add("强制性：强条，适用构件：ZHZ");
            steps.Add("条文：转换柱、抗震时、体积配箍率不宜小于1.5%");
            steps.Add("柱号 = " + this.vrra.Text);
            steps.Add("if (柱号[" + this.vrra.Code + "].Contains(\"ZHZ\") && " +
                "(抗震等级[" +this.vrra.AntiSeismicGrade+ "].Contains(\"一级\")) || " +
                "抗震等级[" + this.vrra.AntiSeismicGrade + "].Contains(\"二级\")) || " +
                "抗震等级[" + this.vrra.AntiSeismicGrade + "].Contains(\"三级\")) || " +
                "抗震等级[" + this.vrra.AntiSeismicGrade + "].Contains(\"四级\"))");
            steps.Add("  {");
            steps.Add("      if(体积配箍率["+ this.calVolumnReinforceRatio+"] < 0.015)");
            steps.Add("        {");
            steps.Add("           Err: 箍筋体积配箍率不应小于1.5%" + this.rule);
            steps.Add("        }");
            steps.Add("        else");
            steps.Add("        {");
            steps.Add("           Debugprint: 箍筋体积配箍率大于1.5%" + this.rule);
            steps.Add("        }");
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
