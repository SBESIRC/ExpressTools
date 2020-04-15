using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Validate
{
    public class StirrupReinforcementAreaRule : IRule
    {
        private StirrupReinforcementAreaModel sram = null;
        public StirrupReinforcementAreaRule(StirrupReinforcementAreaModel stirrupRAM)
        {
            this.sram = stirrupRAM;
        }
        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();
        public void Validate()
        {
            if(sram==null || sram.ValidateProperty()==false)
            {
                return;
            }
            //配筋面积
            double intStirrupDiaArea = ThValidate.GetIronSectionArea((int)this.sram.Cdm.IntStirrupDia);
            double dblXStirrupAs = this.sram.Cdm.IntXStirrupCount * intStirrupDiaArea;
            double dblYStirrupAs = this.sram.Cdm.IntYStirrupCount * intStirrupDiaArea;
            double dblStirrupAsmin = Math.Min(dblXStirrupAs, dblYStirrupAs);

            double compareValue1 = this.sram.DblStirrupAsCal0 * this.sram.Cdm.IntStirrupSpacing0 / this.sram.IntStirrupSpacingCal;
            if (dblStirrupAsmin< compareValue1)
            {
                this.ValidateResults.Add("非加密区箍筋配筋不足 (" + dblStirrupAsmin+"<"+ compareValue1+ ") (配筋规则)");
            }
            else
            {
                this.CorrectResults.Add("非加密区箍筋配筋满足计算要求");
            }
            double compareValue2 = this.sram.DblStirrupAsCal * this.sram.Cdm.IntStirrupSpacing / this.sram.IntStirrupSpacingCal;
            if (dblStirrupAsmin< compareValue2)
            {
                this.ValidateResults.Add("加密区箍筋配筋不足 (" + dblStirrupAsmin + "<" + compareValue2 + ") (配筋规则)");
            }
            else
            {
                this.CorrectResults.Add("加密区箍筋配筋满足计算要求");
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            double intStirrupDiaArea = ThValidate.GetIronSectionArea((int)this.sram.Cdm.IntStirrupDia);
            double dblXStirrupAs = this.sram.Cdm.IntXStirrupCount * intStirrupDiaArea;
            double dblYStirrupAs = this.sram.Cdm.IntYStirrupCount * intStirrupDiaArea;
            double dblStirrupAsmin = Math.Min(dblXStirrupAs, dblYStirrupAs);
            steps.Add("IntStirrupDia= " + (int)this.sram.Cdm.IntStirrupDia);
            steps.Add("IntStirrupDiaArea= " + intStirrupDiaArea);
            steps.Add("dblXStirrupAs= IntXStirrupCount[" + this.sram.Cdm.IntXStirrupCount + "] * IntStirrupDiaArea["+ intStirrupDiaArea+"] ="+ dblXStirrupAs);
            steps.Add("dblYStirrupAs= IntYStirrupCount[" + this.sram.Cdm.IntYStirrupCount + "] * IntStirrupDiaArea[" + intStirrupDiaArea + "] ="+ dblYStirrupAs);
            steps.Add("dblStirrupAsmin= Math.Min(dblXStirrupAs[" + dblXStirrupAs+ "],dblYStirrupAs["+ dblYStirrupAs+"]) ="+ dblStirrupAsmin);

            steps.Add("if (dblStirrupAsmin[" + dblStirrupAsmin + "] < DblStirrupAsCal[" + this.sram.DblStirrupAsCal + "] * IntStirrupSpacing[" +
                this.sram.Cdm.IntStirrupSpacing + "] / IntStirrupSpacingCal[" + this.sram.IntStirrupSpacingCal+ "])");
            steps.Add("  {");
            steps.Add("      Err: 非加密区箍筋配筋不足");
            steps.Add("  }");
            steps.Add("else");
            steps.Add("  {");
            steps.Add("      Ok: 非加密区箍筋配筋满足计算要求");
            steps.Add("  }");

            steps.Add("if (dblStirrupAsmin[" + dblStirrupAsmin + "] < DblStirrupAsCal0[" + this.sram.DblStirrupAsCal0 + "] * IntStirrupSpacing0[" +
                this.sram.Cdm.IntStirrupSpacing0 + "] / IntStirrupSpacingCal[" + this.sram.IntStirrupSpacingCal + "])");
            steps.Add("  {");
            steps.Add("      Err: 加密区箍筋配筋不足");
            steps.Add("  }");
            steps.Add("else");
            steps.Add("  {");
            steps.Add("      Ok: 加密区箍筋配筋满足计算要求");
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
