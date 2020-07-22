using System.Collections.Generic;
using System.Linq;
using ThColumnInfo.Validate.Model;

namespace ThColumnInfo.Validate.Rules
{
    public class StirrupMinimumDiameterFRule:IRule
    {
        private StirrupMinimumDiameterFModel smdf;
        private string rule = "（高规 10.2.10-2）";
        public StirrupMinimumDiameterFRule(StirrupMinimumDiameterFModel smdf)
        {
            this.smdf = smdf;
        }
        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();

        public void Validate()
        {
            if (this.smdf == null || this.smdf.ValidateProperty() == false)
            {
                return;
            }
            if (this.smdf.AntiSeismicGrade.Contains("一") ||
                   this.smdf.AntiSeismicGrade.Contains("二") ||
                   this.smdf.AntiSeismicGrade.Contains("三") ||
                  this.smdf.AntiSeismicGrade.Contains("四") &&
                  !this.smdf.AntiSeismicGrade.Contains("特"))
            {
                if (this.smdf.IntStirrupDia < 10)
                {
                    this.ValidateResults.Add("箍筋直径小于10 [" + this.smdf.IntStirrupDia + " < 10]，" + this.rule);
                }
                else
                {
                    this.CorrectResults.Add("箍筋直径不小于10" + this.rule);
                }
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            steps.Add("类别：箍筋最小直径（箍筋）");
            steps.Add("条文：转换柱、抗震设计时(一二三四级)，箍筋直径不应小于10mm");
            steps.Add("柱号 = " + this.smdf.Text);
            steps.Add("if (抗震等级["+ this.smdf.AntiSeismicGrade+"].Contains(\"一级\") || " 
                +"抗震等级["+ this.smdf.AntiSeismicGrade+"].Contains(\"二级\") || " +
                "抗震等级[" + this.smdf.AntiSeismicGrade + "].Contains(\"三级\") || "+
                "抗震等级[" + this.smdf.AntiSeismicGrade + "].Contains(\"四级\"))");
            steps.Add("  {");
            steps.Add("    if (IntStirrupDia[" + this.smdf.IntStirrupDia + "] < 10)");
            steps.Add("      {");
            steps.Add("         Err：箍筋直径小于10" + this.rule);
            steps.Add("      }");
            steps.Add("    else");
            steps.Add("      {");
            steps.Add("         Debugprint：箍筋直径不小于10" + this.rule);
            steps.Add("      }");
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
