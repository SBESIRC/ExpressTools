using System;
using System.Collections.Generic;
using System.Linq;
using ThColumnInfo.Validate.Model;

namespace ThColumnInfo.Validate.Rules
{
    public class StirrupMaximumSpacingGRule : IRule
    {
        private StirrupMaximumSpacingGModel smsg =null;
        private string rule = "（《高规》10.2.10-2）";
        public StirrupMaximumSpacingGRule(StirrupMaximumSpacingGModel smsg)
        {
            this.smsg = smsg;
        }
        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();
        public void Validate()
        {
            if(this.smsg == null || !smsg.ValidateProperty())
            {
                return;
            }
            if (this.smsg.IsNonAntiseismic)
            {
                return ;
            }
            if (this.smsg.AntiSeismicGrade.Contains("一级") || 
                this.smsg.AntiSeismicGrade.Contains("二级") ||
                this.smsg.AntiSeismicGrade.Contains("三级") ||
                this.smsg.AntiSeismicGrade.Contains("四级"))
            {
                double intBardiamin = Math.Min(this.smsg.Cdm.IntXBarDia,this.smsg.Cdm.IntYBarDia);
                double intStirrupSpacing = this.smsg.Cdm.IntStirrupSpacing;
                double intStirrupSpacing0 = this.smsg.Cdm.IntStirrupSpacing0;
                if (this.smsg.Code.ToUpper().Contains("ZHZ") &&
                    intStirrupSpacing != 0.0)
                {
                    if(intStirrupSpacing == intStirrupSpacing0)
                    {
                        if (intStirrupSpacing0 > Math.Min(100.0,6* intBardiamin))
                        {
                            this.ValidateResults.Add("箍筋间距不应大于100mm与6倍纵筋直径的较小值 "  + this.rule);
                        }
                        else
                        {
                            this.CorrectResults.Add("箍筋间距不大于100mm与6倍纵筋直径的较小值 " + this.rule);
                        }
                    }
                    else
                    {
                        this.ValidateResults.Add("箍筋未全高加密 " + this.rule);
                    }
                }
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            if(this.smsg.IsNonAntiseismic)
            {
                return steps;
            }
            steps.Add("类别：箍筋最大间距G（箍筋）");
            steps.Add("强制性：强条，适用构件：ZHZ");
            steps.Add("条文：转换柱、抗震时(一二三四级)、箍筋应全高加密(非加密区间距等于加密区间距)，箍筋间距不应大于100mm与6倍纵筋直径的较小值");
            steps.Add("柱号 = " + this.smsg.Text);

            double intStirrupSpacing = this.smsg.Cdm.IntStirrupSpacing;
            double intStirrupSpacing0 = this.smsg.Cdm.IntStirrupSpacing0;
            double intBardiamin = Math.Min(this.smsg.Cdm.IntXBarDia, this.smsg.Cdm.IntYBarDia);

            steps.Add("if (抗震等级["+this.smsg.AntiSeismicGrade+"].Contains(\"特一级\") ||" +
                "抗震等级[" + this.smsg.AntiSeismicGrade + "].Contains(\"一级\") || " +
                "抗震等级[" + this.smsg.AntiSeismicGrade + "].Contains(\"二级\") || " +
                "抗震等级[" + this.smsg.AntiSeismicGrade + "].Contains(\"三级\") || " +
                "抗震等级[" + this.smsg.AntiSeismicGrade + "].Contains(\"四级\"))");
            steps.Add("  {");
            steps.Add("    double intBardiamin = Math.Min("+ this.smsg.Cdm.IntXBarDia+","+ this.smsg.Cdm.IntYBarDia+") = "+
                Math.Min(this.smsg.Cdm.IntXBarDia, this.smsg.Cdm.IntYBarDia));
            steps.Add("    double intStirrupSpacing = " + intStirrupSpacing);
            steps.Add("    double intStirrupSpacing0 = " + intStirrupSpacing0);
            steps.Add("     if(柱号["+ this.smsg.Code+ "].Contains(\"ZHZ\") && intStirrupSpacing0["+
                intStirrupSpacing0 + "] != 0)");
            steps.Add("        {");
            steps.Add("            if(intStirrupSpacing["+ intStirrupSpacing+ "] == intStirrupSpacing0[" + intStirrupSpacing0+"])");
            steps.Add("               {");
            steps.Add("                   if(intStirrupSpacing0["+ intStirrupSpacing0+"] > Math.Min(100,6 *"+ intBardiamin+"))");
            steps.Add("                      {");
            steps.Add("                          Error: 箍筋间距不应大于100mm与6倍纵筋直径的较小值 " + this.rule);
            steps.Add("                      }");
            steps.Add("                   else");
            steps.Add("                      {");
            steps.Add("                          DebugPrint: 箍筋间距不大于100mm与6倍纵筋直径的较小值 " + this.rule);
            steps.Add("                      }");
            steps.Add("               }");
            steps.Add("            else");
            steps.Add("               {");
            steps.Add("                          Error: 箍筋未全高加密 " + this.rule);
            steps.Add("               }");
            steps.Add("        }");
            steps.Add("  }");
            return steps;
        }
    }
}
