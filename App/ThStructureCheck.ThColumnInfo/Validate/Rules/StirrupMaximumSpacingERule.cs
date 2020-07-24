using System.Collections.Generic;
using ThColumnInfo.Validate.Model;

namespace ThColumnInfo.Validate.Rules
{
    public class StirrupMaximumSpacingERule : IRule
    {
        private StirrupMaximumSpacingEModel smse = null;
        private string rule = "（《砼规》9.3.2-5）";
        public StirrupMaximumSpacingERule(StirrupMaximumSpacingEModel smse)
        {
            this.smse = smse;
        }

        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();

        public void Validate()
        {
            if(this.smse.Cdm == null || !this.smse.ValidateProperty())
            {
                return;
            }            
            if(this.smse.Cdm.DblP>0.03)
            {
                if(smse.Cdm.IntStirrupSpacing0>200)
                {
                    this.ValidateResults.Add("（3%）箍筋间距大于200 ["+ smse.Cdm.IntStirrupSpacing0+" > 200]，"+this.rule);
                }
                else
                {
                    this.CorrectResults.Add("（3%）箍筋间距不大于200" + this.rule);
                }
            }            
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            steps.Add("类别：箍筋最大间距E（箍筋）");
            steps.Add("条目编号：59， 强制性：应，适用构件：LZ、KZ、ZHZ");
            steps.Add("适用功能：智能识图，图纸校核，条文编号：砼规 9.3.2-5，条文页数：P124");
            steps.Add("条文：柱中全部纵向受力钢筋的配筋率大于3% 时，箍筋直径不应小于8mm ，间距不应大于10d ，且不应大于200mm，d为纵向受力钢筋的最小直径。");
            steps.Add("柱号 = " + this.smse.Text);
            steps.Add(this.smse.Cdm.GetDblAsCalculation());
            steps.Add(this.smse.Cdm.GetDblpCalculation());
            steps.Add("if (dblP [" + this.smse.Cdm.DblP + "] > 0.03)");
            steps.Add("   if (IntStirrupSpacing0[" + smse.Cdm.IntStirrupSpacing0 + "] > 200)");
            steps.Add("      {");
            steps.Add("             Err:（3%）箍筋间距大于200" + this.rule);
            steps.Add("      }");
            steps.Add("   else");
            steps.Add("      {");
            steps.Add("             Debugprint：（3%）箍筋间距不大于200" + this.rule);
            steps.Add("      }");
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
