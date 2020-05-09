using System.Collections.Generic;

namespace ThColumnInfo.Validate
{
    /// <summary>
    /// 配筋面积,实配钢筋应满足计算值
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
            if(!this.angularReinforcementDiaModel.IsCornerColumn)
            {
                //不是角柱，不需要验证
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
                ValidateResults.Add("角筋直径不满足双偏压验算");
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

            steps.Add("if(!是否需要核对角筋["+ this.angularReinforcementDiaModel.IsCornerColumn+"])");
            steps.Add("  {");
            steps.Add("     Debugprint：柱按单偏压计算");
            steps.Add("  }");

            steps.Add("if (角筋直径[" + angularReinforcementDiaModel.AngularReinforcementDia + "] < 角筋直径限值[" +
                angularReinforcementDiaModel.AngularReinforcementDiaLimited + "])");
            steps.Add("  {");
            steps.Add("    Err: 角筋直径不满足双偏压验算限值要求");
            steps.Add("  }");
            steps.Add("else");
            steps.Add("  {");
            steps.Add("    Debugprint:角筋直径满足双偏压验算");
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
