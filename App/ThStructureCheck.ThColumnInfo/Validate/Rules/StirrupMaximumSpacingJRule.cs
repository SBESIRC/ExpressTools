using System;
using System.Collections.Generic;
using System.Linq;
using ThColumnInfo.Validate.Model;

namespace ThColumnInfo.Validate.Rules
{
    public class StirrupMaximumSpacingJRule : IRule
    {
        private StirrupMaximumSpacingJModel smsj = null;
        private string rule = "（《砼规》11.4.18）";
        private double intBardiamin;
        private double stirrupSpaceingLimited = 0.0;
        public StirrupMaximumSpacingJRule(StirrupMaximumSpacingJModel stirrupMCJ)
        {
            smsj = stirrupMCJ;
        }
        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();

        public void Validate()
        {
            if(smsj == null || smsj.ValidateProperty()==false)
            {
                return;
            }
            this.intBardiamin = Math.Min(this.smsj.Cdm.IntXBarDia,
                this.smsj.Cdm.IntYBarDia);
            intBardiamin = Math.Min(this.smsj.Cdm.IntCBarDia, intBardiamin);
            
            if ((this.smsj.Antiseismic.Contains("一级") && !this.smsj.Antiseismic.Contains("特")) ||
                this.smsj.Antiseismic.Contains("二级"))
            {
                this.stirrupSpaceingLimited = 10 * intBardiamin;
            }
            else if(this.smsj.Antiseismic.Contains("三级") ||
                this.smsj.Antiseismic.Contains("四级"))
            {
                this.stirrupSpaceingLimited = 15 * intBardiamin;
            }
            else
            {
                return;
            }
            if (this.smsj.Cdm.IntStirrupSpacing0> stirrupSpaceingLimited)
            {
                this.ValidateResults.Add("非加密区箍筋间距过大 ["+ this.smsj.Cdm.IntStirrupSpacing0 +
                    " > " + stirrupSpaceingLimited+"]，"+this.rule);
            }
            else
            {
                this.CorrectResults.Add("非加密区箍筋间距满足抗震构造"+this.rule);
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();            
            steps.Add("类别：箍筋最大间距J（箍筋）");
            steps.Add("条目编号：512， 强制性：应，适用构件：KZ、ZHZ");
            steps.Add("适用功能：智能识图，图纸校核，条文编号：砼规 11.4.18，条文页数：P179");
            steps.Add("条文：对一、二级抗震等级，箍筋间距不应大于10d，对三、四级抗震等级，箍筋间距不应大于15d ，此处， d 为纵向钢筋直径。");
            steps.Add("柱号 = " + this.smsj.Text);

            steps.Add("intBardiamin=Math.Min(IntCBarDia[" + smsj.Cdm.IntCBarDia + "],IntXBarDia[" +
                smsj.Cdm.IntXBarDia + "],IntYBarDia[" + smsj.Cdm.IntYBarDia + "]) = " + this.intBardiamin);
            steps.Add("if (抗震等级[" + smsj.Antiseismic + "] == 一级 || 抗震等级["+ smsj.Antiseismic+ "] == 二级)");
            steps.Add("  {");
            steps.Add("      箍筋间距限值= 10 * intBardiamin["+ intBardiamin+"]");
            steps.Add("  }");
            steps.Add("else if (抗震等级[" + smsj.Antiseismic + "] == 三级 || 抗震等级[" + smsj.Antiseismic + "] == 四级) ");
            steps.Add("  {");
            steps.Add("      箍筋间距限值= 15 * intBardiamin[" + intBardiamin + "]");
            steps.Add("  }");

            steps.Add("if (IntStirrupSpacing0[" + smsj.Cdm.IntStirrupSpacing0 + "] > 箍筋间距限值[" +  this.stirrupSpaceingLimited + "])");
            steps.Add("  {");
            steps.Add("      Err: 非加密区箍筋间距过大" + this.rule);
            steps.Add("  }");
            steps.Add("else");
            steps.Add("  {");
            steps.Add("      Debugprint: 非加密区箍筋间距满足抗震构造" + this.rule);
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
