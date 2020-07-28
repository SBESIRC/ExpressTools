using System;
using System.Collections.Generic;
using ThColumnInfo.Validate.Model;

namespace ThColumnInfo.Validate.Rules
{
    public class VerDirIronClearSpaceBRule : IRule
    {
        private VerDirIronClearSpaceModel ruleModel;
        public VerDirIronClearSpaceBRule(VerDirIronClearSpaceModel ruleModel)
        {
            this.ruleModel = ruleModel;
        }
        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();

        private double dblXBarspacing;
        private double dblYBarspacing;
        private string rule = "（《高规》10.2.11-7）";
        private bool isValid = true;
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
                this.rule = "";
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
                         this.ruleModel.AntiSeismicGrade.Contains("四级"))
                    {
                        if(maxValue>200)
                        {
                            isValid = false;
                            ValidateResults.Add("纵向钢筋净间距大于200mm [" + maxValue + " > 200]，" + this.rule);
                        }
                    }
                    else if(this.ruleModel.AntiSeismicGrade.Contains("非"))
                    {
                        if (maxValue > 250)
                        {
                            isValid = false;
                            ValidateResults.Add("纵向钢筋净间距大于250mm [" + maxValue + " > 250]，" + this.rule);
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
                this.isValid = false; //不是ZHZ,不要检测
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            steps.Add("类别：纵筋净间距（侧面纵筋）");
            steps.Add("强制性：应、宜，适用构件：ZHZ");
            steps.Add("条文编号：《高规》10.2.11-7");
            steps.Add("条文：转换柱、纵筋净间距不应小于80mm，抗震时不宜大于200mm，非抗震时不宜大于250mm");
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
            steps.Add("         if( 抗震等级[" + this.ruleModel.AntiSeismicGrade + "].Contains(\"一级\") || " +
                "抗震等级[" + this.ruleModel.AntiSeismicGrade + "].Contains(\"二级\") || " +
                "抗震等级[" + this.ruleModel.AntiSeismicGrade + "].Contains(\"三级\") || " +
                "抗震等级[" + this.ruleModel.AntiSeismicGrade + "].Contains(\"四级\"))");
            steps.Add("          {");
            steps.Add("               if(Math.Max(dblXBarspacing[" + this.dblXBarspacing + "],dblYBarspacing[" + this.dblYBarspacing + "]) > 200)");
            steps.Add("                 {");
            steps.Add("                     Err: 纵向钢筋净间距大于200mm " + this.rule);
            steps.Add("                 }");
            steps.Add("               else");
            steps.Add("                 {");
            steps.Add("                     Debugprint: 纵向钢筋净间距Ok " + this.rule);
            steps.Add("                 }");
            steps.Add("           }");
            steps.Add("         else if( 抗震等级[" + this.ruleModel.AntiSeismicGrade + "].Contains(\"非抗震\")");
            steps.Add("           {");
            steps.Add("               if(Math.Max(dblXBarspacing[" + this.dblXBarspacing + "],dblYBarspacing[" + this.dblYBarspacing + "]) > 250)");
            steps.Add("                 {");
            steps.Add("                     Err: 纵向钢筋净间距大于250mm " + this.rule);
            steps.Add("                 }");
            steps.Add("               else");
            steps.Add("                 {");
            steps.Add("                     Debugprint: 纵向钢筋净间距Ok " + this.rule);
            steps.Add("                 }");
            steps.Add("           }");
            steps.Add("        }");
            steps.Add("   }");
            steps.Add("");
            return steps;
        }
    }
}
