using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Validate
{
    public class StirrupMaximumSpacingFRule : IRule
    {
        private StirrupMaximumSpacingFModel smsfm=null;
        public StirrupMaximumSpacingFRule(StirrupMaximumSpacingFModel smsfModel)
        {
            this.smsfm = smsfModel;
        }

        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();

        public void Validate()
        {
            if(this.smsfm == null || smsfm.ValidateProperty()==false)
            {
                return;
            }
            if (smsfm.IntStirrupSpacing > smsfm.IntStirrupSpacingLimited)
            {
                this.ValidateResults.Add("箍筋间距不满足抗震构造 (" + this.smsfm.IntStirrupSpacing +
                ">"+ smsfm.IntStirrupSpacingLimited+ ") (砼规 11.4.12-2)");
            }
            else
            {
                this.CorrectResults.Add("箍筋间距满足抗震构造");
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            steps.Add("类别：箍筋最大间距F（箍筋）");
            steps.Add("条目编号：511， 强制性：应，适用构件：KZ、ZHZ");
            steps.Add("适用功能：智能识图，图纸校核，条文编号：砼规 11.4.12-2，条文页数：P176");
            steps.Add("条文：见图");

            steps.Add("if (箍筋间距[" + smsfm.IntStirrupSpacing + "] > 箍筋间距限值[" + smsfm.IntStirrupSpacingLimited + "])");
            steps.Add("  {");
            steps.Add("      Err：箍筋间距不满足抗震构造");
            steps.Add("  }");
            steps.Add("else");
            steps.Add("  {");
            steps.Add("      OK：箍筋间距满足抗震构造");
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
