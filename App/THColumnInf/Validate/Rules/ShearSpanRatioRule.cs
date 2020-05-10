using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Validate
{
    /// <summary>
    /// 剪跨比(截面)
    /// </summary>
    public class ShearSpanRatioRule : IRule
    {
        private ShearSpanRatioModel ssrm = null;
        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();
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
            if (ssrm.ShearSpanRatio < 2.0)
            {
                this.ValidateResults.Add("剪跨比小于2 ("+ ssrm.ShearSpanRatio+ "<2) (砼规 11.4.11-2)");
            }
            else
            {
                this.CorrectResults.Add("剪跨比大于2");
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
            steps.Add("if(剪跨比[" + ssrm.ShearSpanRatio + "] < 2.0 )");
            steps.Add("  {");
            steps.Add("        Err：剪跨比小于2");
            steps.Add("  }");
            steps.Add("else");
            steps.Add("  {");
            steps.Add("        OK：剪跨比大于2");
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
