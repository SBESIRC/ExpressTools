using System;
using System.Collections.Generic;
using ThColumnInfo.Validate.Model;

namespace ThColumnInfo.Validate.Rules
{
    public class StirrupMaximumSpacingIRule : IRule
    {
        private StirrupMaximumSpacingGModel smsg =null;
        private string rule = "（《高规》10.2.11-8）";
        public StirrupMaximumSpacingIRule(StirrupMaximumSpacingGModel smsg)
        {
            this.smsg = smsg;
        }
        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();
        public void Validate()
        {
            if(this.smsg == null || !smsg.ValidateProperty())
            {
                return;
            }            
            if(this.smsg.AntiSeismicGrade.Contains("非") &&
                this.smsg.Code.ToUpper().Contains("ZHZ"))
            {
                double intStirrupSpacing = Math.Max(this.smsg.Cdm.IntStirrupSpacing0,
                    this.smsg.Cdm.IntStirrupSpacing);
                if (intStirrupSpacing > 150)
                {
                    this.ValidateResults.Add("箍筋间距不宜大于150mm " + this.rule);
                }
                else
                {
                    this.CorrectResults.Add("箍筋间距不大于150mm " + this.rule);
                }
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            steps.Add("类别：箍筋最大间距（箍筋）");
            steps.Add("强制性：宜，适用构件：ZHZ");
            steps.Add("条文：转换柱、非抗震时、箍筋间距不宜大于150mm");
            steps.Add("柱号 = " + this.smsg.Text);
            double value1 = this.smsg.Cdm.IntStirrupSpacing;
            double value2 = this.smsg.Cdm.IntStirrupSpacing0;
            double intStirrupSpacing = Math.Max(value1,value2);
            steps.Add("if (抗震等级["+this.smsg.AntiSeismicGrade+"].Contains(\"非抗震\") && "+
                "柱号[" + this.smsg.Code + "].Contains(\"ZHZ\"))");
            steps.Add("  {");
            steps.Add("      double intStirrupSpacing = Math.Max(" + value1+","+value2+") = " + intStirrupSpacing);
            steps.Add("      if(intStirrupSpacing[" + intStirrupSpacing + "] > 150)");
            steps.Add("        {");
            steps.Add("           Error: 箍筋间距不宜大于150mm " + this.rule);
            steps.Add("        }");
            steps.Add("        else");
            steps.Add("        {");
            steps.Add("           DebugPrint: 箍筋间距不大于150mm " + this.rule);
            steps.Add("        }");
            steps.Add("  }");
            return steps;
        }
    }
}
