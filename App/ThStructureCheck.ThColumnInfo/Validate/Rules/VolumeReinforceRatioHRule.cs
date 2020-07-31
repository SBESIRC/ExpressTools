using System.Collections.Generic;
using ThColumnInfo.Validate.Model;

namespace ThColumnInfo.Validate.Rules
{
    public class VolumeReinforceRatioHRule : IRule
    {
        private VolumeReinforceRatioHModel vrrh = null;
        private string rule = "（《高规》3.10.4-3）";
        public VolumeReinforceRatioHRule(VolumeReinforceRatioHModel volumeRRH)
        {
            this.vrrh = volumeRRH;
        }
        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();

        private double volumnStirrupRatio = 0.0;

        public void Validate()
        {
            if (this.vrrh == null || !this.vrrh.ValidateProperty())
            {
                return;
            }
            if (this.vrrh.AntiSeismicGrade.Contains("特") &&
               this.vrrh.AntiSeismicGrade.Contains("一级"))
            {
                if (this.vrrh.Code.ToUpper().Contains("ZHZ"))
                {
                    this.volumnStirrupRatio = vrrh.Cdm.GetVolumeStirrupRatio(this.vrrh.ProtectLayerThickness);
                    //全截面
                    if (this.volumnStirrupRatio < 0.016)
                    {
                        ValidateResults.Add("箍筋体积配箍率不应小于1.6% [" + this.volumnStirrupRatio
                            + " < 0.016]，" + this.rule);
                    }
                    else
                    {
                        CorrectResults.Add("箍筋体积配箍率不小于1.6%" + this.rule);
                    }
                }
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            steps.Add("类别：体积配箍率H    （箍筋）");
            steps.Add("强制性：应，适用构件：ZHZ");
            steps.Add("条文：特一级、框支柱、箍筋体积配箍率不应小于1.6%");
            steps.Add("柱号 = " + this.vrrh.Text);
            steps.Add("cover= " + this.vrrh.ProtectLayerThickness + "//保护层厚度");
            steps.Add(this.vrrh.Cdm.GetVolumeStirrupRatioCalculation(this.vrrh.ProtectLayerThickness) + this.volumnStirrupRatio);

            steps.Add("if (抗震等级[" + this.vrrh.AntiSeismicGrade + "].Contains(\"特一级\") && " +
                "柱号[" + this.vrrh.Text + "].Contains(\"ZHZ\"))");
            steps.Add("  {");
            steps.Add("      if (体积配箍率[" + this.volumnStirrupRatio + "] < 0.016)");
            steps.Add("        {");
            steps.Add("            Err: 箍筋体积配箍率不应小于1.6% [" + this.volumnStirrupRatio
                            + " < 0.016]，" + this.rule);
            steps.Add("        }");
            steps.Add("      else");
            steps.Add("        {");
            steps.Add("            Debugprint: 箍筋体积配箍率不小于1.6% [" + this.volumnStirrupRatio
                            + " < 0.016]，" + this.rule);
            steps.Add("        }");
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
