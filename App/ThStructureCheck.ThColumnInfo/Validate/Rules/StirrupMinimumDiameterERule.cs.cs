using System.Collections.Generic;
using System.Linq;
using ThColumnInfo.Validate.Model;

namespace ThColumnInfo.Validate.Rules
{
    public class StirrupMinimumDiameterERule:IRule
    {
        private StirrupMinimumDiameterEModel smde;
        private string rule = "（高规 10.2.11-8）";
        public StirrupMinimumDiameterERule(StirrupMinimumDiameterEModel smde)
        {
            this.smde = smde;
        }
        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();

        public void Validate()
        {
            if (this.smde == null || !this.smde.ValidateProperty())
            {
                return;
            }
            if(this.smde.Code.ToUpper().Contains("ZHZ") && this.smde.AntiSeismicGrade.Contains("非"))
            {
                if (this.smde.Cdm.IntStirrupDia < 10)
                {
                    this.ValidateResults.Add("箍筋最小直径小于10 [" + this.smde.Cdm.IntStirrupDia + " < 10]，" + this.rule);
                }
                else
                {
                    this.CorrectResults.Add("箍筋最小直径不宜小于10" + this.rule);
                }
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            steps.Add("类别：箍筋最小直径E（箍筋）");
            steps.Add("条文：转换柱、非抗震时，箍筋最小直径不宜小于10mm");
            steps.Add("柱号 = " + this.smde.Text);
            steps.Add("if (柱号["+ this.smde.Text+"].Contains(\"ZHZ\") && 抗震等级[" + this.smde.AntiSeismicGrade+"].Contains(\"非抗震\"))");
            steps.Add("  {");
            steps.Add("    if (箍筋最小直径[" + this.smde.Cdm.IntStirrupDia + "] < 10)");
            steps.Add("      {");
            steps.Add("         Err：箍筋最小直径小于10" + this.rule);
            steps.Add("      }");
            steps.Add("    else");
            steps.Add("      {");
            steps.Add("         Debugprint：箍筋最小直径不小于10" + this.rule);
            steps.Add("      }");
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
