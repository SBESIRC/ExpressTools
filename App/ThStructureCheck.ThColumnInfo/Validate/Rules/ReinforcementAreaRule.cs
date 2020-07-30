using System.Collections.Generic;
using ThColumnInfo.Validate.Model;

namespace ThColumnInfo.Validate.Rules
{
    public class ReinforcementAreaRule:IRule
    {
        private ReinforcementAreaModel reinforcementAreaModel = null;
        public ReinforcementAreaRule(ReinforcementAreaModel reinforcementAreaModel)
        {
            this.reinforcementAreaModel = reinforcementAreaModel;
        }
        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();

        public void Validate()
        {
            if (reinforcementAreaModel == null || 
                reinforcementAreaModel.ValidateProperty()==false)
            {
                return;
            }
            if(reinforcementAreaModel.Cdm.DblXAs< reinforcementAreaModel.DblXAsCal)
            {
                this.ValidateResults.Add("X方向配筋不足 [" + reinforcementAreaModel.Cdm.DblXAs + " < " + reinforcementAreaModel.DblXAsCal + "]");
            }
            else
            {
                this.CorrectResults.Add("X方向配筋满足计算");
            }
            if (reinforcementAreaModel.Cdm.DblYAs < reinforcementAreaModel.DblYAsCal)
            {
                this.ValidateResults.Add("Y方向配筋不足 [" + reinforcementAreaModel.Cdm.DblYAs + " < " + reinforcementAreaModel.DblYAsCal + "]");
            }
            else
            {
                this.CorrectResults.Add("Y方向配筋满足计算");
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            steps.Add("类别：配筋面积");
            steps.Add("条目编号：46， 强制性：应，适用构件：LZ、KZ、ZHZ");
            steps.Add("适用功能：图纸校核，条文编号：配筋规则，条文页数：-");
            steps.Add("条文：实配钢筋应满足计算值");
            steps.Add("柱号 = " + this.reinforcementAreaModel.Text);
            steps.Add(reinforcementAreaModel.Cdm.GetDblXAsCalculation());
            steps.Add("if (DblXAs[" + reinforcementAreaModel.Cdm.DblXAs + "] < DblXAsCal[" +
                reinforcementAreaModel.DblXAsCal + "] )");
            steps.Add("  {");
            steps.Add("      Err：X方向配筋小于计算书结果");
            steps.Add("  }");
            steps.Add("else");
            steps.Add("  {");
            steps.Add("      Debugprint：X方向配筋满足计算书结果");
            steps.Add("  }");

            steps.Add(reinforcementAreaModel.Cdm.GetDblYAsCalculation());
            steps.Add("if (DblYAs[" + reinforcementAreaModel.Cdm.DblYAs + "] < DblYAsCal[" + reinforcementAreaModel.DblYAsCal + "])");
            steps.Add("  {");
            steps.Add("      Err：Y方向配筋小于计算书结果");
            steps.Add("  }");
            steps.Add("else");
            steps.Add("  {");
            steps.Add("      Debugprint：Y方向配筋满足计算书结果");
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
