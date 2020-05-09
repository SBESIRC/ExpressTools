using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Validate
{
    public class StirrupMaximumSpacingARule : IRule
    {
        private StirrupMaximumSpacingAModel smsa =null;
        public StirrupMaximumSpacingARule(StirrupMaximumSpacingAModel smsa)
        {
            this.smsa = smsa;
        }

        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();
        public void Validate()
        {
            if(this.smsa == null || smsa.ValidateProperty()==false)
            {
                return;
            }            
            if(this.smsa.Cdm.IntStirrupSpacing0>400)
            {
                this.ValidateResults.Add("箍筋间距大于400 (" + this.smsa.Cdm.IntStirrupSpacing0 +
                    ">400) (砼规 9.3.2-2)");
            }
            else
            {
                this.CorrectResults.Add("箍筋间距小于400");
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            steps.Add("类别：箍筋最大间距A（箍筋）");
            steps.Add("条目编号：53， 强制性：应，适用构件：LZ、KZ、ZHZ");
            steps.Add("适用功能：智能识图，图纸校核，条文编号：砼规 9.3.2-2，条文页数：P123");
            steps.Add("条文：箍筋间距不应大于400mm 及构件截面的短边尺寸，且不应大于15d, d 为纵向钢筋的最小直径");
            steps.Add("if (IntStirrupSpacing0[" + this.smsa.Cdm.IntStirrupSpacing0 + "] > 400 )");
            steps.Add("  {");
            steps.Add("      Err：箍筋间距大于400");
            steps.Add("  }");
            steps.Add("else");
            steps.Add("  {");
            steps.Add("      OK：箍筋间距小于400");
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
