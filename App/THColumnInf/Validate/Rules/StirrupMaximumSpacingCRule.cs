using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Validate
{
    public class StirrupMaximumSpacingCRule : IRule
    {
        private StirrupMaximumSpacingCModel smsc = null;
        public StirrupMaximumSpacingCRule(StirrupMaximumSpacingCModel smsc)
        {
            this.smsc = smsc;
        }
        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();
        public void Validate()
        {
            if(this.smsc == null || this.smsc.ValidateProperty()== false)
            {
                return;
            }
            double intBardiamax = Math.Max(this.smsc.Cdm.IntXBarDia, 
                this.smsc.Cdm.IntYBarDia);
            intBardiamax = Math.Max(this.smsc.Cdm.IntCBarDia,intBardiamax);
            if (this.smsc.Cdm.IntStirrupSpacing0 > (15 * intBardiamax))
            {
                this.ValidateResults.Add("箍筋间距大于15倍纵筋最大直径 (" + this.smsc.Cdm.IntStirrupSpacing0 +
                    ">"+ 15 * intBardiamax + ") (砼规 9.3.2-2)");
            }
            else
            {
                this.CorrectResults.Add("箍筋间距小于15倍纵筋最大直径");
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            double intBardiamax = Math.Max(this.smsc.Cdm.IntXBarDia,
                this.smsc.Cdm.IntYBarDia);
            intBardiamax = Math.Max(this.smsc.Cdm.IntCBarDia, intBardiamax);
            steps.Add("类别：箍筋最大间距C（箍筋）");
            steps.Add("条目编号：55， 强制性：应，适用构件：LZ、KZ、ZHZ");
            steps.Add("适用功能：智能识图，图纸校核，条文编号：砼规 9.3.2-2，条文页数：P123");
            steps.Add("条文：箍筋间距不应大于400mm 及构件截面的短边尺寸，且不应大于15d, d 为纵向钢筋的最小直径");

            steps.Add("intBardiamax=Math.min(IntCBarDia["+ this.smsc.Cdm.IntXBarDia+ "],IntXBarDia[" +
                this.smsc.Cdm.IntXBarDia + "],IntYBarDia["+this.smsc.Cdm.IntYBarDia+ "]) = " + intBardiamax);
            steps.Add("if (IntStirrupSpacing0 [" + this.smsc.Cdm.IntStirrupSpacing0 + "] > (15 * intBardiamax[" + intBardiamax + "]))");
            steps.Add("  {");
            steps.Add("      Err：箍筋间距大于15倍纵筋最大直径");
            steps.Add("  }");
            steps.Add("else");
            steps.Add("  {");
            steps.Add("      OK：箍筋间距小于15倍纵筋最大直径");
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
