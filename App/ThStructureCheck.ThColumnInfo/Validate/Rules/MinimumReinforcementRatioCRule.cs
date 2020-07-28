using System.Collections.Generic;
using System.Linq;
using ThColumnInfo.Validate.Model;

namespace ThColumnInfo.Validate.Rules
{
    public class MinimumReinforcementRatioCRule : IRule
    {
        private MinimumReinforceRatioAModel minimumReinforceRatioAModel;
        public MinimumReinforcementRatioCRule(MinimumReinforceRatioAModel minimumReinforceRatioAModel)
        {
            this.minimumReinforceRatioAModel = minimumReinforceRatioAModel;
        }
        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();
        private string rule = "（《高规》3.10.4-3）";
        public void Validate()
        {
            if (this.minimumReinforceRatioAModel == null || !this.minimumReinforceRatioAModel.ValidateProperty())
            {
                return;
            }
            if(this.minimumReinforceRatioAModel.AntiSeismicGrade.Contains("特")&&
               this.minimumReinforceRatioAModel.AntiSeismicGrade.Contains("一级"))
            {
                if(this.minimumReinforceRatioAModel.Code.ToUpper().Contains("ZHZ"))
                {
                    //全截面
                    if (minimumReinforceRatioAModel.Cdm.DblP < 0.016)
                    {
                        ValidateResults.Add("全部纵筋最小配筋率小于1.6% [" + minimumReinforceRatioAModel.Cdm.DblP
                            + " < 0.016]，" + this.rule);
                    }
                    else
                    {
                        CorrectResults.Add("全部纵筋最小配筋率不小于1.6%" + this.rule);
                    }
                }
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            steps.Add("类别：最小配筋率（侧面纵筋）");
            steps.Add("强制性：应，适用构件：ZHZ");
            steps.Add("条文编号：高规 3.10.4-3");
            steps.Add("条文：特一级、框支柱、全部纵筋最小配筋率1.6%");
            steps.Add("柱号 = " + this.minimumReinforceRatioAModel.Text);
            steps.Add("if(抗震等级 ["+ this.minimumReinforceRatioAModel.AntiSeismicGrade+"].Contains(\"特一级\"))");
            steps.Add("  {");
            steps.Add("    if (DblP[" + minimumReinforceRatioAModel.Cdm.DblP + "] < 0.016)");
            steps.Add("      {");
            steps.Add("          Err：全部纵筋最小配筋率小于1.6%  " + this.rule);
            steps.Add("      }");
            steps.Add("    else");
            steps.Add("      {");
            steps.Add("          Debugprint：全部纵筋最小配筋率不小于1.6% " + this.rule);
            steps.Add("      }");
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
