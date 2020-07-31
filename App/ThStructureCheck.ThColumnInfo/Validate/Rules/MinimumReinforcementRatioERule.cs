using System;
using System.Collections.Generic;
using System.Linq;
using ThColumnInfo.Validate.Model;

namespace ThColumnInfo.Validate.Rules
{
    public class MinimumReinforcementRatioERule : IRule
    {
        private MinimumReinforceRatioEModel mrre=null;
        public MinimumReinforcementRatioERule(MinimumReinforceRatioEModel mrre)
        {
            this.mrre = mrre;
        }
        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();
        private string rule = "（《砼规》11.4.13）";
        public void Validate()
        {
            if (this.mrre == null || !this.mrre.ValidateProperty())
            {
                return;
            }            
            if(this.mrre.AntiSeismicGrade.Contains("一") && this.mrre.Jkb<=2)
            {
                if (mrre.Cdm.DblYP > 0.012 || mrre.Cdm.DblXP > 0.012)
                {
                    ValidateResults.Add("柱每侧纵向钢筋的配筋率不宜大于1.2% [" + Math.Min(mrre.Cdm.DblXP, mrre.Cdm.DblYP)
                        + " > " + 0.012 + "] " + this.rule);
                }
                else
                {
                    CorrectResults.Add("柱每侧纵向钢筋的配筋率都不大于1.2%" + this.rule);
                }
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            steps.Add("类别：最小配筋率E（侧面纵筋）");
            steps.Add("强制性：宜，适用构件：LZ、KZ、ZHZ");
            steps.Add("条文编号：砼规 11.4.13");
            steps.Add("条文：柱每侧纵向钢筋的配筋率不宜大于1.2%。");
            steps.Add("柱号 = " + this.mrre.Text);
            steps.Add(this.mrre.Cdm.GetDblxpCalculation());
            steps.Add(this.mrre.Cdm.GetDblypCalculation());
            steps.Add("if ((抗震等级[" + mrre.Cdm.Antiseismic + "].Contains(\"特一级\") || 抗震等级[" + mrre.Cdm.Antiseismic + "].Contains(\"特一级\")) && 剪跨比[" + mrre.Jkb + "] <=2)");
            steps.Add("  {");
            steps.Add("      if (DblYP[" + mrre.Cdm.DblYP + "] > 0.012 || DblXP[" + mrre.Cdm.DblXP + "] > 0.012)");
            steps.Add("        {");
            steps.Add("            Err：柱每侧纵向钢筋的配筋率不宜大于1.2% " + this.rule);
            steps.Add("        }");
            steps.Add("      else");
            steps.Add("        {");
            steps.Add("            Debugprint：柱每侧纵向钢筋的配筋率都不大于1.2% " + this.rule);
            steps.Add("        }");
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
