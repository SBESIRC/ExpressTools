using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Validate
{
    public class StirrupMaximumSpacingDRule : IRule
    {
        private StirrupMaximumSpacingDModel smsd = null;
        private string rule = "（《砼规》9.3.2-5）";
        public StirrupMaximumSpacingDRule(StirrupMaximumSpacingDModel smsd)
        {
            this.smsd = smsd;
        }

        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();

        public void Validate()
        {
            if(this.smsd == null || smsd.ValidateProperty()==false)
            {
                return;
            }
            double intBardiamin = Math.Min(this.smsd.Cdm.IntXBarDia,this.smsd.Cdm.IntYBarDia);
            intBardiamin = Math.Min(this.smsd.Cdm.IntCBarDia, intBardiamin);
            if(this.smsd.Cdm.DblP>0.03)
            {
                if (smsd.Cdm.IntStirrupSpacing0 > 10 * intBardiamin)
                {
                    this.ValidateResults.Add("（3%）箍筋间距大于10d [" + smsd.Cdm.IntStirrupSpacing0 +
                        " > " + 10 * intBardiamin+"]，"+this.rule);
                }
                else
                {
                    this.CorrectResults.Add("（3%）箍筋间距不大于10d"+this.rule);
                }
            }            
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            steps.Add("类别：箍筋最大间距D（箍筋）");
            steps.Add("条目编号：58， 强制性：应，适用构件：LZ、KZ、ZHZ");
            steps.Add("适用功能：智能识图，图纸校核，条文编号：砼规 9.3.2-5，条文页数：P124");
            steps.Add("条文：柱中全部纵向受力钢筋的配筋率大于3% 时，箍筋直径不应小于8mm ，间距不应大于10d ，且不应大于200mm，d为纵向受力钢筋的最小直径。");
            steps.Add("柱号 = " + this.smsd.Text);
            double intBardiamin = Math.Min(this.smsd.Cdm.IntXBarDia,this.smsd.Cdm.IntYBarDia);
            intBardiamin = Math.Min(this.smsd.Cdm.IntCBarDia, intBardiamin);
            steps.Add("intBardiamin=Math.Min(IntCBarDia[" + this.smsd.Cdm.IntCBarDia + "],IntXBarDia[" + this.smsd.Cdm.IntXBarDia +
                "],IntYBarDia[" + this.smsd.Cdm.IntYBarDia + "]) =" + intBardiamin);
            steps.Add(this.smsd.Cdm.GetDblAsCalculation());
            steps.Add(this.smsd.Cdm.GetDblpCalculation());
            steps.Add("if (dblP [" + this.smsd.Cdm.DblP + "] > 0.03)");
            steps.Add("    if (IntStirrupSpacing0["+ smsd.Cdm.IntStirrupSpacing0+ "] > 10 * intBardiamin[" + intBardiamin + "] )");
            steps.Add("        {");
            steps.Add("           Err:（3%）箍筋间距大于10d" + this.rule);
            steps.Add("        }");
            steps.Add("    else");
            steps.Add("        {");
            steps.Add("           Debugprint：（3%）箍筋间距不大于10d" + this.rule);
            steps.Add("        }");
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
