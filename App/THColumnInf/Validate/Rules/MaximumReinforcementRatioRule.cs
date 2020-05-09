using System.Collections.Generic;

namespace ThColumnInfo.Validate
{
    /// <summary>
    /// 最大配筋率(侧面纵筋)
    /// </summary>
    public class MaximumReinforcementRatioRule : IRule
    {
        private MaximumReinforcementRatioModel ruleModel;
        public MaximumReinforcementRatioRule(MaximumReinforcementRatioModel ruleModel)
        {
            this.ruleModel = ruleModel;
        }
        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();

        public void Validate()
        {
            if(ruleModel==null || ruleModel.Cdm==null)
            {
                return;
            }
            if (this.ruleModel.Cdm.DblP > 0.05)
            {
                ValidateResults.Add("全部纵向钢筋的配筋率不宜大于5% ");
            }
            else
            {
                CorrectResults.Add("全部纵向钢筋的配筋率小于5%");
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            steps.Add("类别：最大配筋率（侧面纵筋）");
            steps.Add("条目编号：42， 强制性：宜，适用构件：LZ、KZ、ZHZ");
            steps.Add("适用功能：智能识图，图纸校核，条文编号：砼规 9.3.1-1，条文页数：P123");
            steps.Add("条文：全部纵向钢筋的配筋率不宜大于5%");
            steps.Add(this.ruleModel.Cdm.GetDblAsCalculation());
            steps.Add(this.ruleModel.Cdm.GetDblpCalculation());
            steps.Add("if (dblP[" + this.ruleModel.Cdm.DblP + "] > 0.05)");
            steps.Add("  {");
            steps.Add("     Err: 全部纵向钢筋的配筋率不宜大于5%");
            steps.Add("  }");
            steps.Add("else");
            steps.Add("  {");
            steps.Add("     Debugprint: 全部纵向钢筋的配筋率小于5%");
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
