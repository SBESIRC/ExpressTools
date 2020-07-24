using System.Collections.Generic;
using ThColumnInfo.Validate.Model;

namespace ThColumnInfo.Validate.Rules
{
    public class StirrupFullHeightEncryptionRule : IRule
    {
        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();
        private StirrupFullHeightEncryptionModel sfhem;
        private string rule = "（《砼规》11.4.12-3）";
        public StirrupFullHeightEncryptionRule(StirrupFullHeightEncryptionModel sfhem)
        {
            this.sfhem = sfhem;
        }        

        public void Validate()
        {
            if (this.sfhem == null || !sfhem.ValidateProperty())
            {
                return;
            }
            if (this.sfhem.Code.ToUpper().Contains("ZHZ") ||
                (this.sfhem.Jkb <= 2 && this.sfhem.Code.ToUpper().Contains("KZ")))
            {
                if(this.sfhem.Cdm.Ctri.HoopReinforcement.Contains("/"))
                {
                    this.ValidateResults.Add("箍筋应全高范围内加密"+this.rule);
                }
                else
                {
                    this.CorrectResults.Add("箍筋全高范围内已加密"+ this.rule);
                }
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            steps.Add("类别：箍筋全高加密（箍筋）");
            steps.Add("条目编号：510-1， 强制性：应，适用构件：KZ、ZHZ");
            steps.Add("适用功能：智能识图，图纸校核");
            steps.Add("条文编号：砼规 11.4.12-3，条文页数：P176");
            steps.Add("条文：见图");
            steps.Add("柱号 = " + this.sfhem.Text);
            steps.Add("剪跨比 = " + this.sfhem.Jkb);
            steps.Add("箍筋 = " + this.sfhem.Cdm.Ctri.HoopReinforcement);

            steps.Add("if (柱号[" + this.sfhem.Text + "].Contains(\"ZHZ\") || " +
                "(剪跨比[" + this.sfhem.Jkb + "] <= 2 && 柱号[" + this.sfhem.Text + "].Contains(\"KZ\"))");
            steps.Add("  {");
            steps.Add("    if (箍筋[" + sfhem.Cdm.Ctri.HoopReinforcement + "].Contains(\"/\")");
            steps.Add("      {");
            steps.Add("         Err: 箍筋应全高范围内加密" + this.rule);
            steps.Add("       }");
            steps.Add("    else");
            steps.Add("       {");
            steps.Add("         Debugprint: 箍筋全高范围内已加密" + this.rule);
            steps.Add("       }");
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
