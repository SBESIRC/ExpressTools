using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Validate
{
    public class StirrupMinimumDiameterCRule:IRule
    {
        private StirrupMinimumDiameterCModel smdc;
        private string rule = "（《砼规》9.3.2-5）";
        public StirrupMinimumDiameterCRule(StirrupMinimumDiameterCModel smdc)
        {
            this.smdc = smdc;
        }
        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();
        public void Validate()
        {
            if(this.smdc == null || smdc.ValidateProperty()==false)
            {
                return;
            }            
            if(this.smdc.Cdm.DblP>0.03)
            {
                if(this.smdc.Cdm.IntStirrupDia<8)
                {
                    this.ValidateResults.Add("（3%）箍筋直径小于8 ["+ this.smdc.Cdm.IntStirrupDia+" < 8]，"+this.rule);
                }      
                else
                {
                    this.CorrectResults.Add("（3%）箍筋直径不小于8" + this.rule);
                }
            }            
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            steps.Add("类别：箍筋最小直径C（箍筋）");
            steps.Add("条目编号：57， 强制性：应，适用构件：LZ、KZ、ZHZ");
            steps.Add("适用功能：智能识图，图纸校核，条文编号：砼规 9.3.2-5，条文页数：P124");
            steps.Add("条文：柱中全部纵向受力钢筋的配筋率大于3% 时，箍筋直径不应小于8mm ，间距不应大于10d ，且不应大于200mm，d为纵向受力钢筋的最小直径。");
            steps.Add("柱号 = " + this.smdc.Text);
            steps.Add(this.smdc.Cdm.GetDblAsCalculation());
            steps.Add(this.smdc.Cdm.GetDblpCalculation());
            steps.Add("if (dblP [" + this.smdc.Cdm.DblP + "] > 0.03)");
            steps.Add("  if (IntStirrupDia[" + smdc.Cdm.IntStirrupDia + "] < 8 )");
            steps.Add("    {");
            steps.Add("      Err:（3%）箍筋直径小于8" + this.rule);
            steps.Add("    }");
            steps.Add("   else");
            steps.Add("    {");
            steps.Add("      Debugprint：（3%）箍筋直径不小于8" + this.rule);
            steps.Add("    }");
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
