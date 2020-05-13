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
                this.ValidateResults.Add("非加密区箍筋配筋不足 [" + dblStirrupAsmin + " < " + compareValue1 + "]");
            }
            else
            {
                this.CorrectResults.Add("非加密区箍筋配筋满足计算要求");
            }
            double compareValue2 = this.sram.DblStirrupAsCal * this.sram.Cdm.IntStirrupSpacing / this.sram.IntStirrupSpacingCal;
            if (dblStirrupAsmin< compareValue2)
            {
                this.ValidateResults.Add("加密区箍筋配筋不足 ["+ dblStirrupAsmin + " < " + compareValue2 + "]");
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

            steps.Add("类别：配筋面积");
            steps.Add("条目编号：519， 强制性：应，适用构件：LZ、KZ、ZHZ");
            steps.Add("适用功能：图纸校核，条文编号：配筋规则，条文页数：-");
            steps.Add("条文：实配钢筋应满足计算值");

            steps.Add("柱号 = " + this.sram.Text);
            steps.Add("IntStirrupDia= " + (int)this.sram.Cdm.IntStirrupDia);
            steps.Add("IntStirrupDiaArea= " + intStirrupDiaArea);
            steps.Add("dblXStirrupAs= IntXStirrupCount[" + this.sram.Cdm.IntXStirrupCount + "] * IntStirrupDiaArea["+ intStirrupDiaArea+"] ="+ dblXStirrupAs);
            steps.Add("dblYStirrupAs= IntYStirrupCount[" + this.sram.Cdm.IntYStirrupCount + "] * IntStirrupDiaArea[" + intStirrupDiaArea + "] ="+ dblYStirrupAs);
            steps.Add("dblStirrupAsmin= Math.Min(dblXStirrupAs[" + dblXStirrupAs+ "],dblYStirrupAs["+ dblYStirrupAs+"]) ="+ dblStirrupAsmin);

            steps.Add("if (dblStirrupAsmin[" + dblStirrupAsmin + "] < DblStirrupAsCal0[" + this.sram.DblStirrupAsCal0 + "] * IntStirrupSpacing0[" +
                this.sram.Cdm.IntStirrupSpacing0 + "] / IntStirrupSpacingCal[" + this.sram.IntStirrupSpacingCal+ "])");
            steps.Add("  {");
            steps.Add("      Err: 非加密区箍筋配筋不足");
            steps.Add("  }");
            steps.Add("else");
            steps.Add("  {");
            steps.Add("      Debugprint: 非加密区箍筋配筋满足计算要求");
            steps.Add("  }");

            steps.Add("if (dblStirrupAsmin[" + dblStirrupAsmin + "] < DblStirrupAsCal[" + this.sram.DblStirrupAsCal + "] * IntStirrupSpacing[" +
                this.sram.Cdm.IntStirrupSpacing + "] / IntStirrupSpacingCal[" + this.sram.IntStirrupSpacingCal + "])");
            steps.Add("  {");
            steps.Add("      Err: 加密区箍筋配筋不足");
            steps.Add("  }");
            steps.Add("else");
            steps.Add("  {");
            steps.Add("      Debugprint: 加密区箍筋配筋满足计算要求");
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
