using System.Collections.Generic;

namespace ThColumnInfo.Validate
{
    /// <summary>
    /// 配筋面积
    /// </summary>
    public class AngularReinforcementDiaRule : IRule
    {
        private AngularReinforcementDiaModel angularReinforcementDiaModel;
        public AngularReinforcementDiaRule(AngularReinforcementDiaModel angularReinforcementDiaModel)
        {
            this.angularReinforcementDiaModel = angularReinforcementDiaModel;
        }
        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();
        public void Validate()
        {
            if (angularReinforcementDiaModel == null)
            {
                return;
            }
            if (angularReinforcementDiaModel .AngularReinforcementDia<= 0.0 || 
                angularReinforcementDiaModel.AngularReinforcementDiaLimited <= 0.0)
            {
                return;
            }
            if(angularReinforcementDiaModel.AngularReinforcementDia <
                angularReinforcementDiaModel.AngularReinforcementDiaLimited)
            {
                ValidateResults.Add("角筋直径不满足双偏压验算 (" +
                    angularReinforcementDiaModel.AngularReinforcementDia+"<"+
                    angularReinforcementDiaModel.AngularReinforcementDiaLimited +")");
            }
            else
            {
                CorrectResults.Add("角筋直径满足双偏压验算");
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            steps.Add("类别：配筋面积（角筋）");
            steps.Add("条目编号：32， 强制性：应，适用构件：LZ、KZ、ZHZ");
            steps.Add("适用功能：图纸校核，条文编号：配筋规则，条文页数：-");
            steps.Add("条文：实配钢筋应满足计算值");
            steps.Add("if (角筋直径[" + angularReinforcementDiaModel.AngularReinforcementDia + "] < 角筋直径限值[" +
                angularReinforcementDiaModel.AngularReinforcementDiaLimited + "])");
            steps.Add("  {");
            steps.Add("    Err: 角筋直径不满足双偏压验算");
            steps.Add("  }");
            steps.Add("else");
            steps.Add("  {");
            steps.Add("    OK: 角筋直径满足双偏压验算");
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
