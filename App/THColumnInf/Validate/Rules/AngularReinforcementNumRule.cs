using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Validate
{
    /// <summary>
    /// 角筋根数
    /// </summary>
    public class AngularReinforcementNumRule : IRule
    {
        private AngularReinforcementNumModel ruleModel;
        public AngularReinforcementNumRule(AngularReinforcementNumModel ruleModel)
        {
            this.ruleModel = ruleModel;
        }
        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();
        public void Validate()
        {
            if(ruleModel==null)
            {
                return;
            }
            if(ruleModel.AngularReinforcementNum%4!=0)
            {
                ValidateResults.Add("角筋根数不是4的倍数");
            }
            else
            {
                CorrectResults.Add("角筋根数是4的倍数");
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            steps.Add("类别：角筋根数（角筋）");
            steps.Add("条目编号：31， 强制性：应，适用构件：LZ、KZ、ZHZ");
            steps.Add("适用功能：智能识图、图纸校核，条文编号：配筋规则，条文页数：-");
            steps.Add("条文：角筋根数（intCBarCount）应该是4的倍数");
            steps.Add("柱号 = " + this.ruleModel.Text);
            steps.Add("if (角筋根数[" + ruleModel.AngularReinforcementNum + "] % 4 !=0) ");
            steps.Add("  {");
            steps.Add("    Err: 角筋根数不是4的倍数");
            steps.Add("  }");
            steps.Add("else");
            steps.Add("  {");
            steps.Add("    Debugprint:角筋根数是4的倍数");
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
