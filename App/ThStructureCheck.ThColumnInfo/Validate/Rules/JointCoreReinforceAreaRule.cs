using System;
using System.Collections.Generic;
using System.Linq;
using ThColumnInfo.Validate.Model;

namespace ThColumnInfo.Validate.Rules
{
    public class JointCoreReinforceAreaRule : IRule
    {
        private JointCoreReinforceModel jcrm = null;
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
            this.jointCoreReinforceArea = GetJointCoreReinArea();
            if(this.jointCoreReinforceArea< jcrm.CoreJointReinforceArea)
            {
                this.ValidateResults.Add("节点核心区配筋面积应满足计算值");
            }
            else
            {
                this.CorrectResults.Add("节点核心区配筋面积满足计算要求"); 
            }
        }
        private double GetJointCoreReinArea()
        {
            var coluJoinCore = this.jcrm.ColuJoinCoreData;
            if (coluJoinCore == null)
            {
                return 0.0;
            }
            return this.jcrm.Cdm.GetCoreReinforcementArea(coluJoinCore, this.jcrm.ProtectLayerThickness);
        }
        private string GetJointCoreReinAreaCalculation()
        {
            var coluJoinCore = this.jcrm.ColuJoinCoreData;
            if (coluJoinCore == null)
            {
                return "";
            }
            return this.jcrm.Cdm.GetCoreReinAreaCalculation(coluJoinCore, this.jcrm.ProtectLayerThickness);
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            steps.Add("类别：节点核心区配筋面积 (箍筋)");
            steps.Add("强制性：，适用构件：LZ、KZ、ZHZ");
            steps.Add("条文：实配钢筋应满足计算值");
            steps.Add("柱号 = " + this.jcrm.Text);
            steps.Add(GetJointCoreReinAreaCalculation()+this.jointCoreReinforceArea);
            steps.Add("if (节点核心区配筋面积[" + this.jointCoreReinforceArea + 
                "] < 计算书节点核芯区配筋面积["+this.jcrm.CoreJointReinforceArea+"])");
            steps.Add("  {");
            steps.Add("    Err: 节点核心区配筋面积应满足计算值");
            steps.Add("  }");
            steps.Add("else ");
            steps.Add("  {");
            steps.Add("    Debug: 节点核心区配筋面积满足计算要求");
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
