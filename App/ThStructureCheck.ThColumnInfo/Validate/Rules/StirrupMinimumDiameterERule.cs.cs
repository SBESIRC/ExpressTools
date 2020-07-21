using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            if (this.smde == null || this.smde.ValidateProperty() == false)
            {
                return;
            }
            if(this.smde.AntiSeismicGrade.Contains("非"))
            {
                if (this.smde.IntStirrupDiaLimited < 10)
                {
                    this.ValidateResults.Add("箍筋最小直径小于10 [" + this.smde.IntStirrupDiaLimited + " < 10]，" + this.rule);
                }
                else
                {
                    this.CorrectResults.Add("箍筋最小直径不小于10" + this.rule);
                }
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            steps.Add("类别：箍筋最小直径（箍筋）");
            steps.Add("条文：转换柱、非抗震时，箍筋最小直径不宜小于10mm");
            steps.Add("柱号 = " + this.smde.Text);
            steps.Add("if (抗震等级["+ this.smde.AntiSeismicGrade+"].Contains(\"非抗震\"))");
            steps.Add("  {");
            steps.Add("    if (IntStirrupDia[" + this.smde.IntStirrupDiaLimited + "] < 10)");
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
