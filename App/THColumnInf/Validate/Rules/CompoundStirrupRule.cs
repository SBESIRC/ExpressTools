using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Validate
{
    public class CompoundStirrupRule:IRule
    {
        private ColumnDataModel cdm = null;
        public CompoundStirrupRule(ColumnDataModel columnDataModel)
        {
            this.cdm = columnDataModel;
        }
        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();
        private bool ValidateProperty()
        {
            if (cdm.Code.Contains("LZ") || cdm.Code.Contains("KZ") || cdm.Code.Contains("ZHZ"))
            {
                return true;
            }
            return false;
        }
        public void Validate()
        {
           if(this.cdm==null || ValidateProperty()==false)
            {
                return;
            }
           if((cdm.B>400 && cdm.IntXBarCount>1) || (cdm.B<=400 && cdm.IntXBarCount > 2))
            {
                if(cdm.IntYStirrupCount<=2)
                {
                    this.ValidateResults.Add("应设置复合箍筋 ("+ cdm.IntYStirrupCount+ "<=2) (砼规 9.3.2-4)");
                }
                else
                {
                    this.CorrectResults.Add("设置复合箍筋");
                }
            }
           if((cdm.H>400 && cdm.IntYBarCount>1) || (cdm.H<=400 && cdm.IntYBarCount>2))
            {
                if(cdm.IntXStirrupCount<=2)
                {
                    this.ValidateResults.Add("应设置复合箍筋 (" + cdm.IntXStirrupCount + "<=2) (砼规 9.3.2-4)");
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


            steps.Add("if ((B[" + cdm.B + "] > 400 && IntXBarCount[" +
               cdm.IntXBarCount + "] > 1 ) || (B["+ cdm.B+ "] <= 400 && IntXBarCount[" + cdm.IntXBarCount+"] > 2))");
            steps.Add("  {");
            steps.Add("    if (IntYStirrupCount[" + cdm.IntYStirrupCount  + "] <= 2 )");
            steps.Add("      {");
            steps.Add("          Err：应设置复合箍筋");
            steps.Add("      }");
            steps.Add("    else");
            steps.Add("      {");
            steps.Add("          OK:设置复合箍筋");
            steps.Add("      }");
            steps.Add("  }");

            steps.Add("if ((H[" + cdm.H + "] > 400 && IntYBarCount[" +
               cdm.IntYBarCount + "] > 1 ) || (H[" + cdm.H + "] <= 400 && IntYBarCount[" + cdm.IntYBarCount + "] > 2))");
            steps.Add("  {");
            steps.Add("    if (IntXStirrupCount[" + cdm.IntXStirrupCount + "] <= 2 )");
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
