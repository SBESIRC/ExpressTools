using System;
using System.Collections.Generic;

namespace ThColumnInfo.Validate
{
    public class VerDirIronClearSpaceRule : IRule
    {
        private VerDirIronClearSpaceModel ruleModel;
        public VerDirIronClearSpaceRule(VerDirIronClearSpaceModel ruleModel)
        {
            this.ruleModel = ruleModel;
        }
        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();

        private double dblXBarspacing;
        private double dblYBarspacing;
        private string rule = "（《砼规》9.3.1-2）";
        public void Validate()
        {
            if(ruleModel==null || !ruleModel.ValidateProperty())
            {
                return;
            }
            this.dblXBarspacing = (ruleModel.Cdm.B -2*(ruleModel.ProtectLayerThickness+ ruleModel.Cdm.IntCBarDia + ruleModel.Cdm.IntStirrupDia)-
                ruleModel.Cdm.IntXBarCount*ruleModel.Cdm.IntXBarDia)/ (ruleModel.Cdm.IntXBarCount + 1);

            this.dblYBarspacing = (ruleModel.Cdm.H - 2 * (ruleModel.ProtectLayerThickness + ruleModel.Cdm.IntCBarDia + ruleModel.Cdm.IntStirrupDia) -
               ruleModel.Cdm.IntYBarCount * ruleModel.Cdm.IntYBarDia) / (ruleModel.Cdm.IntYBarCount + 1);

            double minValue = Math.Min(dblXBarspacing, dblYBarspacing);
            double maxValue = Math.Max(dblXBarspacing, dblYBarspacing);
            if(this.ruleModel.Code.ToUpper().Contains("ZHZ"))
            {
                this.rule = "高规 10.2.11 - 7";
                bool isValid = true;
                if(minValue < 80)
                {
                    isValid = false;
                    ValidateResults.Add("纵向钢筋净间距不足 [" + minValue + " < 80]，" + this.rule);
                }
                else
                {
                    if(this.ruleModel.AntiSeismicGrade.Contains("一级") ||
                        this.ruleModel.AntiSeismicGrade.Contains("二级") ||
                         this.ruleModel.AntiSeismicGrade.Contains("三级") ||
                         this.ruleModel.AntiSeismicGrade.Contains("四级") &&
                         !this.ruleModel.AntiSeismicGrade.Contains("特"))
                    {
                        if(maxValue>200)
                        {
                            isValid = false;
                            ValidateResults.Add("纵向钢筋净间距过大 [" + maxValue + " > 200]，" + this.rule);
                        }
                    }
                    else if(this.ruleModel.AntiSeismicGrade.Contains("非抗震"))
                    {
                        if (maxValue > 250)
                        {
                            isValid = false;
                            ValidateResults.Add("纵向钢筋净间距过大 [" + maxValue + " > 250]，" + this.rule);
                        }
                    }
                }
                if(isValid)
                {
                    CorrectResults.Add("纵向钢筋净间距Ok" + this.rule);
                }
            }
            else
            {
                if (minValue < 50)  //柱中纵向钢筋的净间距不应小于50mm
                {
                    ValidateResults.Add("纵向钢筋净间距不足 [" + minValue + " < 50]，" + this.rule);
                }
                else if (maxValue > 300) //且不宜大于300mm
                {
                    ValidateResults.Add("纵向钢筋净间距过大 [" + maxValue + " > 300]，" + this.rule);
                }
                else
                {
                    CorrectResults.Add("纵向钢筋净间距Ok" + this.rule);
                }
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            steps.Add("类别：纵筋净间距（侧面纵筋）");
            steps.Add("条目编号：43， 强制性：宜，适用构件：LZ、KZ、ZHZ");
            steps.Add("适用功能：智能识图，图纸校核，条文编号：砼规 9.3.1-2，条文页数：P123");
            steps.Add("条文：柱中纵向钢筋的净间距不应小于50mm ，且不宜大于300mm");
            steps.Add("柱号 = " + this.ruleModel.Text);
            steps.Add("dblXBarspacing=(B[" + ruleModel.Cdm.B + "]- 2 * (保护层厚度[" + ruleModel.ProtectLayerThickness + "] + IntCBarDia[" +
                ruleModel.Cdm.IntCBarDia + "] +IntStirrupDia[" + ruleModel.Cdm.IntStirrupDia + "]) - IntXBarCount[" +
                ruleModel.Cdm.IntXBarCount + "] * IntXBarDia[" + ruleModel.Cdm.IntXBarDia + "]) / (IntXBarCount[" + ruleModel.Cdm.IntXBarCount + "] + 1)");

            steps.Add("dblYBarspacing=(H["+ ruleModel.Cdm.H + "]- 2 * (保护层厚度[" + ruleModel.ProtectLayerThickness + "] + IntCBarDia[" +
                ruleModel.Cdm.IntCBarDia + "] +IntStirrupDia[" + ruleModel.Cdm.IntStirrupDia + "]) - IntYBarCount[" +
                ruleModel.Cdm.IntYBarCount + "] * IntYBarDia[" + ruleModel.Cdm.IntYBarDia + "]) / (InYBarCount[" + ruleModel.Cdm.IntYBarCount + "] + 1)");
            steps.Add("if ( "+ this.ruleModel.Code+".Contains(\"ZHZ\")");
            steps.Add("   {");
            steps.Add("     if (Math.Min(dblXBarspacing[" + this.dblXBarspacing + "],dblYBarspacing[" + this.dblYBarspacing + "]) < 80)");
            steps.Add("        {");
            steps.Add("           Err: 纵向钢筋净间距不足 "+this.rule); 
            steps.Add("        }");
            steps.Add("     else ");
            steps.Add("        {");
            steps.Add("         if( 抗震等级[)" + this.ruleModel.AntiSeismicGrade + "].Contains(\"一级\") || " +
                "抗震等级[)" + this.ruleModel.AntiSeismicGrade + "].Contains(\"二级\") || " +
                "抗震等级[)" + this.ruleModel.AntiSeismicGrade + "].Contains(\"三级\") || " +
                "抗震等级[)" + this.ruleModel.AntiSeismicGrade + "].Contains(\"四级\"))");
            steps.Add("          {");
            steps.Add("               if(Math.Max(dblXBarspacing[" + this.dblXBarspacing + "],dblYBarspacing[" + this.dblYBarspacing + "]) > 200)");
            steps.Add("                 {");
            steps.Add("                     Err: 纵向钢筋净间距过大 "+this.rule);
            steps.Add("                 }");
            steps.Add("           }");
            steps.Add("         else if( 抗震等级[)" + this.ruleModel.AntiSeismicGrade + "].Contains(\"非抗震\")");
            steps.Add("           {");
            steps.Add("               if(Math.Max(dblXBarspacing[" + this.dblXBarspacing + "],dblYBarspacing[" + this.dblYBarspacing + "]) > 300)");
            steps.Add("                 {");
            steps.Add("                     Err: 纵向钢筋净间距过大 " + this.rule);
            steps.Add("                 }");
            steps.Add("           }");
            steps.Add("        }");
            steps.Add("   }");
            steps.Add("else");
            steps.Add("   {");
            steps.Add("     if (Math.Min(dblXBarspacing[" + this.dblXBarspacing + "],dblYBarspacing[" + this.dblYBarspacing + "]) < 50)");
            steps.Add("       {");
            steps.Add("           Err: 纵向钢筋净间距不足 （《砼规》9.3.1-2）");
            steps.Add("       }");
            steps.Add("     else if(Math.Max(dblXBarspacing[" + this.dblXBarspacing + "],dblYBarspacing[" + this.dblYBarspacing + "]) > 300)");
            steps.Add("       {");
            steps.Add("           Err: 纵向钢筋净间距过大（《砼规》9.3.1-2）");
            steps.Add("       }");
            steps.Add("     else");
            steps.Add("       {");
            steps.Add("           Debugprint: 纵向钢筋净间距Ok （《砼规》9.3.1-2）");
            steps.Add("       }");
            steps.Add("   }");
            steps.Add("");
            return steps;
        }
    }
}
