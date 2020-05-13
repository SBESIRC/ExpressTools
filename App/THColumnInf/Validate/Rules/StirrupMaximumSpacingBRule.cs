using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Validate
{
    public class StirrupMaximumSpacingBRule : IRule
    {
        private StirrupMaximumSpacingBModel smsb;
        public StirrupMaximumSpacingBRule(StirrupMaximumSpacingBModel smsb)
        {
            this.smsb = smsb;
        }

        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();

        public void Validate()
        {
            if(this.smsb == null || smsb.ValidateProperty()==false)
            {
                return;
            }
            double dblSeclower = Math.Min(this.smsb.Cdm.B, this.smsb.Cdm.H);
            if(this.smsb.Cdm.IntStirrupSpacing0> dblSeclower)
            {
                this.ValidateResults.Add("箍筋间距大于构件截面的短边尺寸");
            }
            else
            {
                this.CorrectResults.Add("箍筋间距小于构件截面的短边尺寸");
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            steps.Add("类别：箍筋最大间距B（箍筋）");
            steps.Add("条目编号：54， 强制性：应，适用构件：LZ、KZ、ZHZ");
            steps.Add("适用功能：智能识图，图纸校核，条文编号：砼规 9.3.2-2，条文页数：P123");
            steps.Add("条文：箍筋间距不应大于400mm 及构件截面的短边尺寸，且不应大于15d, d 为纵向钢筋的最小直径");
            steps.Add("柱号 = " + this.smsb.Text);
            steps.Add("if (IntStirrupSpacing0[" + this.smsb.Cdm.IntStirrupSpacing0 + "] > Math.Min(B[" + this.smsb.Cdm.B+"] , H["+ this.smsb.Cdm.H + "])");
            steps.Add("  {");
            steps.Add("      Err：箍筋间距大于构件截面的短边尺寸");
            steps.Add("  }");
            steps.Add("else");
            steps.Add("  {");
            steps.Add("      Debugprint：箍筋间距小于构件截面的短边尺寸");
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
