using System.Collections.Generic;
using ThColumnInfo.Validate.Model;

namespace ThColumnInfo.Validate.Rules
{
    /// <summary>
    /// 剪跨比(截面)
    /// </summary>
    public class ShearSpanRatioRule : IRule
    {
        private ShearSpanRatioModel ssrm = null;
        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();
        private string rule = "（《砼规》11.4.11-2）";

        public ShearSpanRatioRule(ShearSpanRatioModel shearSRM)
        {
            this.ssrm = shearSRM;
        }

        public void Validate()
        {
            if(ssrm==null || ssrm.ValidateProperty()==false)
            {
                return;
            }
            if (ssrm.ShearSpanRatio <= 2.0)
            {
                this.ValidateResults.Add("剪跨比小于等于2 ["+ ssrm.ShearSpanRatio+" <= 2.0]，"+this.rule);
            }
            else
            {
                this.CorrectResults.Add("剪跨比大于2"+this.rule);
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            steps.Add("类别：剪跨比（截面）");
            steps.Add("条目编号：12， 强制性：宜，适用构件：KZ、ZHZ");
            steps.Add("适用功能：图纸校核，条文编号：砼规 11.4.11-2，条文页数：175");
            steps.Add("条文：柱的剪跨比宜大于2");
            steps.Add("柱号 = " + this.ssrm.Text);
            steps.Add("if(剪跨比[" + ssrm.ShearSpanRatio + "] <= 2.0 )");
            steps.Add("  {");
            steps.Add("        Err：剪跨比小于等于2（《砼规》11.4.11-2）");
            steps.Add("  }");
            steps.Add("else");
            steps.Add("  {");
            steps.Add("        Debugprint：剪跨比大于2（《砼规》11.4.11-2）");
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
