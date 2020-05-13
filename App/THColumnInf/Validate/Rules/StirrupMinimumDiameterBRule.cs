using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Validate
{
    public class StirrupMinimumDiameterBRule:IRule
    {
        private StirrupMinimumDiameterBModel smdb;
        public StirrupMinimumDiameterBRule(StirrupMinimumDiameterBModel smdb)
        {
            this.smdb = smdb;
        }
        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();

        public void Validate()
        {
            if(this.smdb == null || this.smdb.ValidateProperty()==false)
            {
                return;
            }            
            if(this.smdb.Cdm.IntStirrupDia<6)
            {
                this.ValidateResults.Add("箍筋直径小于6");
            }
            else
            {
                this.CorrectResults.Add("箍筋直径不小于6");
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            steps.Add("类别：箍筋最小直径B（箍筋）");
            steps.Add("条目编号：53， 强制性：应，适用构件：LZ、KZ、ZHZ");
            steps.Add("适用功能：智能识图，图纸校核，条文编号：砼规 9.3.2-1，条文页数：P123");
            steps.Add("条文：箍筋直径不应小于d/4 ，且不应小于6mm, d 为纵向钢筋的最大直径；");
            steps.Add("柱号 = " + this.smdb.Text);
            steps.Add("if (IntStirrupDia[" + this.smdb.Cdm.IntStirrupDia + "] < 6)");
            steps.Add("  {");
            steps.Add("     Err：箍筋直径小于6");
            steps.Add("  }");
            steps.Add("else");
            steps.Add("  {");
            steps.Add("     Debugprint：箍筋直径不小于6");
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
