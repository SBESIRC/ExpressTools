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
            if(this.sfhem.IsNonAntiseismic)
            {
                return;
            }
            if(this.sfhem.AntiSeismicGrade.Contains("一") ||
                this.sfhem.AntiSeismicGrade.Contains("二") ||
                this.sfhem.AntiSeismicGrade.Contains("三") ||
                this.sfhem.AntiSeismicGrade.Contains("四"))
            {
                if (this.sfhem.Code.ToUpper().Contains("ZHZ") ||
                (this.sfhem.Jkb <= 2 && this.sfhem.Code.ToUpper().Contains("KZ")))
                {
                    if (this.sfhem.Cdm.IntStirrupSpacing!= this.sfhem.Cdm.IntStirrupSpacing0)
                    {
                        this.ValidateResults.Add("箍筋未全高加密 " + this.rule);
                    }
                    else
                    {
                        this.CorrectResults.Add("箍筋全高范围内已加密 " + this.rule);
                    }
                }
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            if (this.sfhem.IsNonAntiseismic)
            {
                return steps;
            }
            steps.Add("类别：箍筋全高加密（箍筋）");
            steps.Add("条目编号：510-1， 强制性：应，适用构件：KZ、ZHZ");
            steps.Add("适用功能：智能识图，图纸校核");
            steps.Add("条文编号：砼规 11.4.12-3，条文页数：P176");
            steps.Add("条文：见图");
            steps.Add("柱号 = " + this.sfhem.Text);
            steps.Add("剪跨比 = " + this.sfhem.Jkb);
            steps.Add("箍筋 = " + this.sfhem.Cdm.Ctri.HoopReinforcement);
            steps.Add("箍筋加密区间距 [IntStirrupSpacing]= " + this.sfhem.Cdm.IntStirrupSpacing);
            steps.Add("箍筋非加密区间距 [IntStirrupSpacing0]= " + this.sfhem.Cdm.IntStirrupSpacing0);
            steps.Add("if (抗震等级[" + this.sfhem.AntiSeismicGrade + "].Contains(\"特一级\") ||" +
               "抗震等级[" + this.sfhem.AntiSeismicGrade + "].Contains(\"一级\") || " +
               "抗震等级[" + this.sfhem.AntiSeismicGrade + "].Contains(\"二级\") || " +
               "抗震等级[" + this.sfhem.AntiSeismicGrade + "].Contains(\"三级\") || " +
               "抗震等级[" + this.sfhem.AntiSeismicGrade + "].Contains(\"四级\"))");
            steps.Add("  {");
            steps.Add("    if (柱号[" + this.sfhem.Text + "].Contains(\"ZHZ\") || " +
                "(剪跨比[" + this.sfhem.Jkb + "] <= 2 && 柱号[" + this.sfhem.Text + "].Contains(\"KZ\"))");
            steps.Add("      {");
            steps.Add("        if (IntStirrupSpacing[" + sfhem.Cdm.IntStirrupSpacing +
                                  "] != IntStirrupSpacing0[" + sfhem.Cdm.IntStirrupSpacing0 + "]");
            steps.Add("          {");
            steps.Add("             Err: 箍筋未全高加密 " + this.rule);
            steps.Add("           }");
            steps.Add("        else");
            steps.Add("           {");
            steps.Add("             Debugprint: 箍筋全高范围内已加密 " + this.rule);
            steps.Add("           }");
            steps.Add("      }");    
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
