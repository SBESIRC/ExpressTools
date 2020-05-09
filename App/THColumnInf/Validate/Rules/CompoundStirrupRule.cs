using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Validate
{
    public class CompoundStirrupRule:IRule
    {
        private CompoundStirrupModel csm = null;
        public CompoundStirrupRule(CompoundStirrupModel csm)
        {
            this.csm = csm;
        }
        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();
        public void Validate()
        {
           if(this.csm == null || csm.ValidateProperty()==false)
            {
                return;
            }
           if((this.csm.Cdm.B>400 && this.csm.Cdm.IntXBarCount>1) || (this.csm.Cdm.B<=400 && this.csm.Cdm.IntXBarCount > 2))
            {
                if(this.csm.Cdm.IntYStirrupCount<=2)
                {
                    this.ValidateResults.Add("应设置复合箍筋 ("+ this.csm.Cdm.IntYStirrupCount+ "<=2) (砼规 9.3.2-4)");
                }
                else
                {
                    this.CorrectResults.Add("设置复合箍筋");
                }
            }
           if((this.csm.Cdm.H>400 && this.csm.Cdm.IntYBarCount>1) || (this.csm.Cdm.H<=400 && this.csm.Cdm.IntYBarCount>2))
            {
                if(this.csm.Cdm.IntXStirrupCount<=2)
                {
                    this.ValidateResults.Add("应设置复合箍筋 (" + this.csm.Cdm.IntXStirrupCount + "<=2) (砼规 9.3.2-4)");
                }
                else
                {
                    this.CorrectResults.Add("设置复合箍筋");
                }
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            steps.Add("类别：复合箍筋（箍筋）");
            steps.Add("条目编号：56， 强制性：应，适用构件：LZ、KZ、ZHZ");
            steps.Add("适用功能：智能识图，图纸校核，条文编号：砼规 9.3.2-4，条文页数：P123");
            steps.Add("条文：当柱截面短边尺寸大于400mm 且各边纵向钢筋多于3根时，或当柱截面短边尺寸不大于400mm 但各边纵向钢筋多于4根时，应设置复合箍筋");


            steps.Add("if ((B[" + this.csm.Cdm.B + "] > 400 && IntXBarCount[" +
               this.csm.Cdm.IntXBarCount + "] > 1 ) || (B["+ this.csm.Cdm.B+ "] <= 400 && IntXBarCount[" + this.csm.Cdm.IntXBarCount+"] > 2))");
            steps.Add("  {");
            steps.Add("    if (IntYStirrupCount[" + this.csm.Cdm.IntYStirrupCount  + "] <= 2 )");
            steps.Add("      {");
            steps.Add("          Err：应设置复合箍筋");
            steps.Add("      }");
            steps.Add("    else");
            steps.Add("      {");
            steps.Add("          OK:设置复合箍筋");
            steps.Add("      }");
            steps.Add("  }");

            steps.Add("if ((H[" + this.csm.Cdm.H + "] > 400 && IntYBarCount[" +
               this.csm.Cdm.IntYBarCount + "] > 1 ) || (H[" + this.csm.Cdm.H + "] <= 400 && IntYBarCount[" + this.csm.Cdm.IntYBarCount + "] > 2))");
            steps.Add("  {");
            steps.Add("    if (IntXStirrupCount[" + this.csm.Cdm.IntXStirrupCount + "] <= 2 )");
            steps.Add("      {");
            steps.Add("          Err：应设置复合箍筋");
            steps.Add("      }");
            steps.Add("    else");
            steps.Add("      {");
            steps.Add("          OK:设置复合箍筋");
            steps.Add("      }");
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
