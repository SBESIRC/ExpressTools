using System.Collections.Generic;
using System.Linq;
using ThColumnInfo.Validate.Model;

namespace ThColumnInfo.Validate.Rules
{
    public class MinimumReinforcementRatioDRule : IRule
    {
        private MinimumReinforceRatioBModel minimumReinforceRatioBModel;
        private string rule = "（《高规》3.10.2-3）";
        public MinimumReinforcementRatioDRule(MinimumReinforceRatioBModel minimumReinforceRatioBModel)
        {
            this.minimumReinforceRatioBModel = minimumReinforceRatioBModel;
        }
        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();
        public void Validate()
        {
            if (this.minimumReinforceRatioBModel == null || 
                minimumReinforceRatioBModel.ValidateProperty()==false)
            {
                return;
            }
            if (this.minimumReinforceRatioBModel.AntiSeismicGrade.Contains("特") &&
               this.minimumReinforceRatioBModel.AntiSeismicGrade.Contains("一级"))
            {
                if (this.minimumReinforceRatioBModel.IsCornerColumn)
                {
                    //全截面
                    if (minimumReinforceRatioBModel.Cdm.DblP < 0.016)
                    {
                        ValidateResults.Add("角柱最小配筋率不应小于1.6% [" + minimumReinforceRatioBModel.Cdm.DblP
                            + " < 0.016]，" + this.rule);
                    }
                    else
                    {
                        CorrectResults.Add("角柱最小配筋率大于等于1.6% [" + minimumReinforceRatioBModel.Cdm.DblP
                            + " < 0.016]，" + this.rule);
                    }
                }
                else
                {
                    if (minimumReinforceRatioBModel.Cdm.DblP < 0.014)
                    {
                        ValidateResults.Add("中、边柱最小配筋率不应小于1.4% [" + minimumReinforceRatioBModel.Cdm.DblP
                            + " < 0.014]，" + this.rule);
                    }
                    else
                    {
                        CorrectResults.Add("中、边柱最小配筋率大于等于1.4% [" + minimumReinforceRatioBModel.Cdm.DblP
                            + " < 0.014]，" + this.rule);
                    }
                }
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            steps.Add("类别：最小配筋率（侧面纵筋）");
            steps.Add("强制性：，适用构件：LZ、KZ、ZHZ");
            steps.Add("条文编号：高规 3.10.2-3");
            steps.Add("条文：特一级、全部纵筋配筋率，中、边柱不应小于1.4%，角柱不应小于1.6%");
            steps.Add("柱号 = " + this.minimumReinforceRatioBModel.Text);
            steps.Add("抗震等级：" +this.minimumReinforceRatioBModel.AntiSeismicGrade);
            steps.Add("结构类型：" + this.minimumReinforceRatioBModel.StructureType);
            steps.Add("是否角柱：" + this.minimumReinforceRatioBModel.IsCornerColumn);
            steps.Add("柱子类型：" + this.minimumReinforceRatioBModel.ColumnType);
            steps.Add("if (抗震等级["+ minimumReinforceRatioBModel.AntiSeismicGrade+"].Contains(\"特一级\")");
            steps.Add("  {");
            steps.Add("      if(是否角柱["+ minimumReinforceRatioBModel.IsCornerColumn+"]");
            steps.Add("        {");
            steps.Add("            if (DblP[" + minimumReinforceRatioBModel.Cdm.DblP + "] < 0.016)");
            steps.Add("              {");
            steps.Add("                  Err：角柱最小配筋率不应小于1.6%  " + this.rule);
            steps.Add("              }");
            steps.Add("            else ");
            steps.Add("              {");
            steps.Add("                  Debugprint：角柱最小配筋率大于等于1.6%  " + this.rule);
            steps.Add("              }");
            steps.Add("        }");
            steps.Add("      else");
            steps.Add("        {");
            steps.Add("            if (DblP[" + minimumReinforceRatioBModel.Cdm.DblP + "] < 0.014)");
            steps.Add("              {");
            steps.Add("                  Err：中、边柱最小配筋率不应小于1.4%  " + this.rule);
            steps.Add("              }");
            steps.Add("            else");
            steps.Add("              {");
            steps.Add("                  Debugprint：中、边柱最小配筋率大于等于1.4%  " + this.rule);
            steps.Add("              }");
            steps.Add("        }");
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
