using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Validate
{
    public class StirrupMaximumSpacingERule : IRule
    {
        private ColumnDataModel cdm=null;
        public StirrupMaximumSpacingERule(ColumnDataModel columnDataModel)
        {
            this.cdm = columnDataModel;
        }

        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();

        private  bool ValidateProperty()
        {
            if (this.cdm.Code.Contains("LZ") || this.cdm.Code.Contains("KZ") || this.cdm.Code.Contains("ZHZ"))
            {
                return true;
            }
            return false;
        }
        public void Validate()
        {
            if(this.cdm == null || ValidateProperty()==false)
            {
                return;
            }
            double intBardiamax = Math.Max(this.cdm.IntXBarDia, 
                this.cdm.IntYBarDia);
            intBardiamax = Math.Max(this.cdm.IntCBarDia,intBardiamax);
            if(this.cdm.DblP>3)
            {
                if(cdm.IntStirrupSpacing0>200)
                {
                    this.ValidateResults.Add("（3%）箍筋间距大于200 (" + this.cdm.IntStirrupSpacing0 +
                    ">200) (砼规 9.3.2-5)");
                }
                else
                {
                    this.CorrectResults.Add("（3%）箍筋间距小于200 ");
                }
            }            
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            steps.Add("类别：箍筋最大间距E（箍筋）");
            steps.Add("条目编号：59， 强制性：应，适用构件：LZ、KZ、ZHZ");
            steps.Add("适用功能：智能识图，图纸校核，条文编号：砼规 9.3.2-5，条文页数：P124");
            steps.Add("条文：柱中全部纵向受力钢筋的配筋率大于3% 时，箍筋直径不应小于8mm ，间距不应大于10d ，且不应大于200mm，d为纵向受力钢筋的最小直径。");

            double intBardiamax = Math.Max(this.cdm.IntXBarDia,this.cdm.IntYBarDia);
            intBardiamax = Math.Max(this.cdm.IntCBarDia, intBardiamax);
            steps.Add("intBardiamax=Math.Max(IntCBarDia[" + this.cdm.IntCBarDia + "],IntXBarDia[" + this.cdm.IntXBarDia +
                "],IntYBarDia[" + this.cdm.IntYBarDia + "]) =" + intBardiamax);
            steps.Add(this.cdm.GetDblAsCalculation());
            steps.Add(this.cdm.GetDblpCalculation());
            steps.Add("if (dblP [" + this.cdm.DblP + "] > 3)");
            steps.Add("   if (IntStirrupSpacing0[" + cdm.IntStirrupSpacing0 + "] > 200 )");
            steps.Add("      {");
            steps.Add("         （3%）箍筋间距大于200");
            steps.Add("      }");
            steps.Add("   else");
            steps.Add("      {");
            steps.Add("         （3%）箍筋间距小于200 ");
            steps.Add("      }");
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
