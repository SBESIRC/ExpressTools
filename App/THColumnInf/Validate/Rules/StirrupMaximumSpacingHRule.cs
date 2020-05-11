using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Validate
{
    public class StirrupMaximumSpacingHRule : IRule
    {
        private StirrupMaximumSpacingHModel smsh = null;
        public StirrupMaximumSpacingHRule(StirrupMaximumSpacingHModel smsh)
        {
            this.smsh = smsh;
        }
        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();

        public void Validate()
        {
            if(smsh == null || smsh.ValidateProperty()==false)
            {
                return;
            }
            if(smsh.Cdm.IntStirrupSpacing0> smsh.Cdm.IntStirrupSpacing*2)
            {
                this.ValidateResults.Add("非加密区箍筋体积配箍率小于加密区2倍");
            }
            else
            {
                this.CorrectResults.Add("非加密区箍筋体积配箍率不大于加密区2倍");
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            steps.Add("类别：箍筋最大间距H（箍筋）");
            steps.Add("条目编号：512， 强制性：应，适用构件：KZ、ZHZ");
            steps.Add("适用功能：智能识图，图纸校核，条文编号：砼规 11.4.19，条文页数：P179");
            steps.Add("条文：在箍筋加密区外，箍筋的体积配筋率不宜小于加密区配筋率的一半");
            steps.Add("柱号 = " + this.smsh.Text);
            steps.Add("if (IntStirrupSpacing0[" + smsh.Cdm.IntStirrupSpacing0 + "] > IntStirrupSpacing[" + smsh.Cdm.IntStirrupSpacing + "] * 2)");
            steps.Add("  {");
            steps.Add("     Err: 非加密区箍筋体积配箍率小于加密区的2倍");
            steps.Add("  }");
            steps.Add("else");
            steps.Add("  {");
            steps.Add("     Debugprint: 非加密区箍筋体积配箍率不大于加密区2倍");
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
