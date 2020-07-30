using System.Collections.Generic;
using ThColumnInfo.Validate.Model;

namespace ThColumnInfo.Validate.Rules
{
    /// <summary>
    /// 最大配筋率(侧面纵筋)
    /// </summary>
    public class MaximumReinforcementRatioBRule : IRule
    {
        private MaximumReinforcementRatioModel ruleModel;
        private string rule = "（《高规》10.2.11-7）";
        public MaximumReinforcementRatioBRule(MaximumReinforcementRatioModel ruleModel)
        {
            this.ruleModel = ruleModel;
        }
        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();

        public void Validate()
        {
            if(ruleModel==null || !ruleModel.ValidateProperty())
            {
                return;
            }
            if(ruleModel.Code.ToUpper().Contains("ZHZ"))
            {
                if(ruleModel.AntiSeismicGrade.Contains("一级") ||
                    ruleModel.AntiSeismicGrade.Contains("二级") ||
                    ruleModel.AntiSeismicGrade.Contains("三级") ||
                    ruleModel.AntiSeismicGrade.Contains("二四"))
                {
                    if(this.ruleModel.Cdm.DblP > 0.04)
                    {
                        ValidateResults.Add("全部纵向钢筋的配筋率大于4% [" + this.ruleModel.Cdm.DblP + " > 0.04]，" + this.rule);
                    }
                    else
                    {
                        CorrectResults.Add("全部纵向钢筋的配筋率小于等于4%" + this.rule);
                    }
                }
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            steps.Add("类别：最大配筋率（侧面纵筋）");
            steps.Add("强制性：宜，适用构件：ZHZ");
            steps.Add("条文编号：《高规》10.2.11-7)");
            steps.Add("条文：全部纵向钢筋的配筋率小于等于4%");
            steps.Add("柱号 = " + this.ruleModel.Text);
            steps.Add(this.ruleModel.Cdm.GetDblAsCalculation());
            steps.Add(this.ruleModel.Cdm.GetDblpCalculation());
            steps.Add("if (柱号["+ ruleModel.Code+"].Contains(ZHZ))");
            steps.Add("  {");
            steps.Add("    if (抗震等级[" + ruleModel.AntiSeismicGrade + "].Contains(\"特一级\") || " +
                "抗震等级[" + ruleModel.AntiSeismicGrade + "].Contains(\"一级\") || " +
                "抗震等级[" + ruleModel.AntiSeismicGrade + "].Contains(\"二级\") || " +
                "抗震等级[" + ruleModel.AntiSeismicGrade + "].Contains(\"三级\") || " +
                "抗震等级[" + ruleModel.AntiSeismicGrade + "].Contains(\"四级\"))");
            steps.Add("        {");
            steps.Add("          if( DblP["+ this.ruleModel.Cdm.DblP+"] > 0.04)");
            steps.Add("             {");
            steps.Add("                Error: 全部纵向钢筋的配筋率小于等于4% " + this.rule);
            steps.Add("             }");
            steps.Add("          else");
            steps.Add("             {");
            steps.Add("                Debugprint: 全部纵向钢筋的配筋率小于等于4% " + this.rule);
            steps.Add("             }");
            steps.Add("        }");
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
