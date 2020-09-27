using System;
using System.Collections.Generic;
using System.Linq;
using ThColumnInfo.Validate.Model;

namespace ThColumnInfo.Validate.Rules
{
    public class JointCoreReinforceAreaRule : IRule
    {
        private JointCoreReinforceModel jcrm = null;
        private double xCoreStirupArea = 0.0;
        private double yCoreStirupArea = 0.0;
        private double jointCoreReinforceArea = 0.0;
        public JointCoreReinforceAreaRule(JointCoreReinforceModel jcrm)
        {
            this.jcrm = jcrm;
        }
        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();

        public void Validate()
        {
            if (this.jcrm == null || !this.jcrm.ValidateProperty())
            {
                return;
            }
            var coluJoinCore = this.jcrm.ColuJoinCoreData;
            if (coluJoinCore == null)
            {
                return;
            }
            this.xCoreStirupArea = this.jcrm.Cdm.GetXCoreReinforcementArea(coluJoinCore);
            this.yCoreStirupArea = this.jcrm.Cdm.GetYCoreReinforcementArea(coluJoinCore);
            this.jointCoreReinforceArea = Math.Min(xCoreStirupArea, yCoreStirupArea);
            if(this.jointCoreReinforceArea< jcrm.CoreJointReinforceArea)
            {
                this.ValidateResults.Add("节点核心区配筋面积不满足计算书结果");
            }
            else
            {
                this.CorrectResults.Add("节点核心区配筋面积满足计算书结果"); 
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            if (this.jointCoreReinforceArea == 0.0)
            {
                return steps;
            }
            steps.Add("类别：节点核心区配筋面积 (箍筋)");
            steps.Add("强制性：，适用构件：LZ、KZ、ZHZ");
            steps.Add("条文：实配钢筋应满足计算值");
            steps.Add("柱号 = " + this.jcrm.Text);

            steps.Add(this.jcrm.Cdm.GetXCoreReinAreaCalculation(this.jcrm.ColuJoinCoreData) +this.xCoreStirupArea);
            steps.Add(this.jcrm.Cdm.GetYCoreReinAreaCalculation(this.jcrm.ColuJoinCoreData) + this.yCoreStirupArea);
            steps.Add("节点核心区配筋面积=Math.Min(m向核芯区箍筋截面面积["+ this.xCoreStirupArea+
                "],n向核芯区箍筋截面面积["+ this.yCoreStirupArea+"]) = "+ this.jointCoreReinforceArea);
            steps.Add("if (节点核心区配筋面积[" + this.jointCoreReinforceArea + 
                "] < 计算书节点核芯区配筋面积["+this.jcrm.CoreJointReinforceArea+"])");
            steps.Add("  {");
            steps.Add("    Err: 节点核心区配筋面积不满足计算书结果");
            steps.Add("  }");
            steps.Add("else ");
            steps.Add("  {");
            steps.Add("    Debug: 节点核心区配筋面积满足计算书结果");
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
