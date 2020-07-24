using System.Collections.Generic;
using ThColumnInfo.Validate.Model;

namespace ThColumnInfo.Validate.Rules
{
    /// <summary>
    /// 最大配筋率(侧面纵筋)
    /// </summary>
    public class MaximumReinforcementRatioRule : IRule
    {
        private MaximumReinforcementRatioModel ruleModel;
        private string rule = "（《砼规》9.3.1-1）";
        public MaximumReinforcementRatioRule(MaximumReinforcementRatioModel ruleModel)
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
                this.rule = "高规 10.2.11-7";
                if((ruleModel.AntiSeismicGrade.Contains("一级") ||
                    ruleModel.AntiSeismicGrade.Contains("二级") ||
                    ruleModel.AntiSeismicGrade.Contains("三级") ||
                    ruleModel.AntiSeismicGrade.Contains("二四")) &&
                    !ruleModel.AntiSeismicGrade.Contains("特"))
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
            else
            {
                if (this.ruleModel.Cdm.DblP > 0.05)
                {
                    ValidateResults.Add("全部纵向钢筋的配筋率大于5% [" + this.ruleModel.Cdm.DblP + " > 0.05]，" + this.rule);
                }
                else
                {
                    CorrectResults.Add("全部纵向钢筋的配筋率小于等于5%" + this.rule);
                }
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            steps.Add("类别：最大配筋率（侧面纵筋）");
            steps.Add("条目编号：42， 强制性：宜，适用构件：LZ、KZ、ZHZ");
            steps.Add("适用功能：智能识图，图纸校核，条文编号：砼规 9.3.1-1，条文页数：P123");
            steps.Add("条文：全部纵向钢筋的配筋率不宜大于5%");
            steps.Add("柱号 = " + this.ruleModel.Text);
            steps.Add(this.ruleModel.Cdm.GetDblAsCalculation());
            steps.Add(this.ruleModel.Cdm.GetDblpCalculation());
            if (ruleModel.Code.ToUpper().Contains("ZHZ"))
            {
                this.rule = "高规 10.2.11-7";
                if ((ruleModel.AntiSeismicGrade.Contains("一级") ||
                    ruleModel.AntiSeismicGrade.Contains("二级") ||
                    ruleModel.AntiSeismicGrade.Contains("三级") ||
                    ruleModel.AntiSeismicGrade.Contains("二四")) &&
                    !ruleModel.AntiSeismicGrade.Contains("特"))
                {
                    if (this.ruleModel.Cdm.DblP > 0.04)
                    {
                        ValidateResults.Add("全部纵向钢筋的配筋率大于4% [" + this.ruleModel.Cdm.DblP + " > 0.04]，" + this.rule);
                    }
                    else
                    {
                        CorrectResults.Add("全部纵向钢筋的配筋率小于等于4%" + this.rule);
                    }
                }
            }
            else
            {
                if (this.ruleModel.Cdm.DblP > 0.05)
                {
                    ValidateResults.Add("全部纵向钢筋的配筋率大于5% [" + this.ruleModel.Cdm.DblP + " > 0.05]，" + this.rule);
                }
                else
                {
                    CorrectResults.Add("全部纵向钢筋的配筋率小于等于5%" + this.rule);
                }
            }
            steps.Add("if ("+ ruleModel.Code+".Contains(ZHZ))");
            steps.Add("  {");
            steps.Add("    if (" + ruleModel.AntiSeismicGrade + ".Contains(\"一级\") || " +
                ruleModel.AntiSeismicGrade + ".Contains(\"二级\") || " +
                ruleModel.AntiSeismicGrade + ".Contains(\"三级\") || " +
                ruleModel.AntiSeismicGrade + ".Contains(\"四级\") && " +
                "!" + ruleModel.AntiSeismicGrade + ".Contains(\"特\")" + ")");
            steps.Add("        {");
            steps.Add("          if( DblP["+ this.ruleModel.Cdm.DblP+"] > 0.04)");
            steps.Add("             {");
            steps.Add("                Error: 全部纵向钢筋的配筋率小于等于4%（高规 10.2.11-7）");
            steps.Add("             }");
            steps.Add("          else");
            steps.Add("             {");
            steps.Add("                Debugprint: 全部纵向钢筋的配筋率小于等于4%（高规 10.2.11-7）");
            steps.Add("             }");
            steps.Add("        }");
            steps.Add("  }");
            steps.Add("else");
            steps.Add("     {");
            steps.Add("       if (dblP[" + this.ruleModel.Cdm.DblP + "] > 0.05)");
            steps.Add("        {");
            steps.Add("         Err: 全部纵向钢筋的配筋率大于5%（《砼规》9.3.1-1）");
            steps.Add("        }");
            steps.Add("        else");
            steps.Add("        {");
            steps.Add("            Debugprint: 全部纵向钢筋的配筋率小于等于5%（《砼规》9.3.1-1）");
            steps.Add("        }");
            steps.Add("     }");
            steps.Add("");
            return steps;
        }
    }
}
