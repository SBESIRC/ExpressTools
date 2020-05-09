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
        public void Validate()
        {
            if(ruleModel==null)
            {
                return;
            }
            this.dblXBarspacing = (ruleModel.Cdm.B -2*(ruleModel.ProtectLayerThickness+ ruleModel.Cdm.IntCBarDia + ruleModel.Cdm.IntStirrupDia)-
                ruleModel.Cdm.IntXBarCount*ruleModel.Cdm.IntXBarDia)/ (ruleModel.Cdm.IntXBarCount + 1);

            this.dblYBarspacing = (ruleModel.Cdm.H - 2 * (ruleModel.ProtectLayerThickness + ruleModel.Cdm.IntCBarDia + ruleModel.Cdm.IntStirrupDia) -
               ruleModel.Cdm.IntYBarCount * ruleModel.Cdm.IntYBarDia) / (ruleModel.Cdm.IntYBarCount + 1);

            double minValue = Math.Min(dblXBarspacing, dblYBarspacing);
            double maxValue = Math.Max(dblXBarspacing, dblYBarspacing);
            if (minValue<50)  //柱中纵向钢筋的净间距不应小于50mm
            {
                ValidateResults.Add("纵向钢筋净间距不足");
            }
            else if (maxValue > 300) //且不宜大于300mm
            {
                ValidateResults.Add("纵向钢筋净间距过大");
            }
            else
            {
                CorrectResults.Add("纵向钢筋净间距Ok");
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            steps.Add("类别：纵筋净间距（侧面纵筋）");
            steps.Add("条目编号：43， 强制性：宜，适用构件：LZ、KZ、ZHZ");
            steps.Add("适用功能：智能识图，图纸校核，条文编号：砼规 9.3.1-2，条文页数：P123");
            steps.Add("条文：柱中纵向钢筋的净间距不应小于50mm ，且不宜大于300mm");
            steps.Add("dblXBarspacing=(B[" + ruleModel.Cdm.B + "]- 2 * (保护层厚度[" + ruleModel.ProtectLayerThickness + "] + IntCBarDia[" +
                ruleModel.Cdm.IntCBarDia + "] +IntStirrupDia[" + ruleModel.Cdm.IntStirrupDia + "]) - IntXBarCount[" +
                ruleModel.Cdm.IntXBarCount + "] * IntXBarDia[" + ruleModel.Cdm.IntXBarDia + "]) / (IntXBarCount[" + ruleModel.Cdm.IntXBarCount + "1)");

            steps.Add("dblYBarspacing=(H["+ ruleModel.Cdm.H + "]- 2 * (保护层厚度[" + ruleModel.ProtectLayerThickness + "] + IntCBarDia[" +
                ruleModel.Cdm.IntCBarDia + "] +IntStirrupDia[" + ruleModel.Cdm.IntStirrupDia + "]) - IntYBarCount[" +
                ruleModel.Cdm.IntYBarCount + "] * IntYBarDia[" + ruleModel.Cdm.IntYBarDia + "]) / (InYBarCount[" + ruleModel.Cdm.IntYBarCount + "1)");

            steps.Add("if (Math.Min(dblXBarspacing[" + this.dblXBarspacing + "],dblYBarspacing[" + this.dblYBarspacing + "]) < 50)");
            steps.Add("  {");
            steps.Add("      Err: 纵向钢筋净间距不足");
            steps.Add("  }");
            steps.Add("else if(Math.Max(dblXBarspacing[" + this.dblXBarspacing + "],dblYBarspacing[" + this.dblYBarspacing + "]) >= 300)");
            steps.Add("  {");
            steps.Add("      Err: 纵向钢筋净间距过大");
            steps.Add("  }");
            steps.Add("else");
            steps.Add("  {");
            steps.Add("      Debugprint: 纵向钢筋净间距ok");
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
