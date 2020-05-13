using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Validate
{
    public class StirrupMaximumSpacingFRule : IRule
    {
        private StirrupMaximumSpacingFModel smsfm=null;
        private double stirrupSpaceingLimited = 0.0;
        public StirrupMaximumSpacingFRule(StirrupMaximumSpacingFModel smsfModel)
        {
            this.smsfm = smsfModel;
        }       
        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();

        public void Validate()
        {
            if(this.smsfm == null || smsfm.ValidateProperty()==false)
            {
                return;
            }
            //查表 箍筋间距限值
            this.stirrupSpaceingLimited = this.smsfm.IntStirrupSpacingLimited;
            //箍筋间距限值修正
            double dblXSpace = (this.smsfm.Cdm.B - 2 * this.smsfm.ProtectThickness)
                / (this.smsfm.Cdm.IntYStirrupCount - 1);
            double dblYSpace = (this.smsfm.Cdm.H - 2 * this.smsfm.ProtectThickness)
                / (this.smsfm.Cdm.IntXStirrupCount - 1);
            double dblStirrupSpace = Math.Max(dblXSpace, dblYSpace);

            if (this.smsfm.AntiSeismicGrade.Contains("一级") &&
                !this.smsfm.AntiSeismicGrade.Contains("特"))
            {
                if (this.smsfm.Cdm.IntStirrupDia > 12 && dblStirrupSpace <= 150)
                {
                    stirrupSpaceingLimited = 150;
                }
            }
            else if (this.smsfm.AntiSeismicGrade.Contains("二级"))
            {
                if (this.smsfm.Cdm.IntStirrupDia >= 10 && dblStirrupSpace <= 150)
                {
                    stirrupSpaceingLimited = 150;
                }
            }
            if (this.smsfm.Cdm.IntStirrupSpacing > stirrupSpaceingLimited)
            {
                this.ValidateResults.Add("箍筋间距不满足抗震构造");
            }
            else
            {
                this.CorrectResults.Add("箍筋间距满足抗震构造");
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            steps.Add("类别：箍筋最大间距F（箍筋）");
            steps.Add("条目编号：511， 强制性：应，适用构件：KZ、ZHZ");
            steps.Add("适用功能：智能识图，图纸校核，条文编号：砼规 11.4.12-2，条文页数：P176");
            steps.Add("条文：见图");
            steps.Add("柱号 = " + this.smsfm.Text);
            steps.Add("箍筋间距: intStirrupSpacing = " + smsfm.Cdm.IntStirrupSpacing);
            double intBardiamin = Math.Min(this.smsfm.Cdm.IntXBarDia, this.smsfm.Cdm.IntYBarDia);
            intBardiamin = Math.Min(intBardiamin, this.smsfm.Cdm.IntCBarDia);
            //纵向钢筋直径最小值
            steps.Add("double intBardiamin =Math.Min(IntXBarDia["+
                this.smsfm.Cdm.IntXBarDia+ "],IntYBarDia["+ this.smsfm.Cdm.IntYBarDia+ "]," +
                "IntCBarDia["+ this.smsfm.Cdm.IntCBarDia+"]) = "+ intBardiamin);
            steps.Add("是否底层: " + smsfm.IsFirstFloor);

            double IntStirrupSpacingLimited = smsfm.IntStirrupSpacingLimited;
            steps.Add("箍筋间距限值: IntStirrupSpacingLimited = " + IntStirrupSpacingLimited);

            //箍筋间距限值修正
            double dblXSpace = (this.smsfm.Cdm.B - 2 * this.smsfm.ProtectThickness)
                / (this.smsfm.Cdm.IntYStirrupCount - 1);
            double dblYSpace = (this.smsfm.Cdm.H - 2 * this.smsfm.ProtectThickness)
                / (this.smsfm.Cdm.IntXStirrupCount - 1);
            double dblStirrupSpace = Math.Max(dblXSpace, dblYSpace);

            steps.Add("double dblXSpace = (B[" + this.smsfm.Cdm.B + "] - 2 * 保护层厚度[" +
                this.smsfm.ProtectThickness + "]) / (IntYStirrupCount[" +
                this.smsfm.Cdm.IntYStirrupCount + "] - 1) = " + dblXSpace) ;
            steps.Add("double dblYSpace = (H[" + this.smsfm.Cdm.H + "] - 2 * 保护层厚度[" +
                this.smsfm.ProtectThickness + "]) / (IntYStirrupCount[" +
                this.smsfm.Cdm.IntYStirrupCount + "] - 1) = " + dblYSpace);
            steps.Add("double dblStirrupSpace = Math.Max(dblXSpace[" + dblXSpace + "],dblYSpace[" +
                dblYSpace + "]) = " + dblStirrupSpace);

            steps.Add("if (抗震等级[" + this.smsfm.AntiSeismicGrade + "].Contains(\"一级\") &&" +
                " !抗震等级[" + this.smsfm.AntiSeismicGrade + "].Contains(\"一级\"))");
            steps.Add("  {");
            steps.Add("        if (IntStirrupDia[" + this.smsfm.Cdm.IntStirrupDia + "] > 12 &&" +
                " dblStirrupSpace[" + dblStirrupSpace + "] <= 150)");
            steps.Add("        {");
            steps.Add("              IntStirrupSpacingLimited = 150");
            steps.Add("         }");
            steps.Add("  }");
            steps.Add("else if (抗震等级[" + this.smsfm.AntiSeismicGrade + "].Contains(\"二级\"))");
            steps.Add("  {");
            steps.Add("        if (IntStirrupDia[" + this.smsfm.Cdm.IntStirrupDia + "] >= 10 &&" +
                " dblStirrupSpace[" + dblStirrupSpace + "] <= 150)");
            steps.Add("        {");
            steps.Add("              IntStirrupSpacingLimited = 150");
            steps.Add("        }");
            steps.Add("  }");

            steps.Add("if (箍筋间距[" + smsfm.Cdm.IntStirrupSpacing + "] >" +
                " 箍筋间距限值[" + this.stirrupSpaceingLimited + "])");
            steps.Add("  {");
            steps.Add("      Err：箍筋间距不满足抗震构造");
            steps.Add("  }");
            steps.Add("else");
            steps.Add("  {");
            steps.Add("      Debugprint：箍筋间距满足抗震构造");
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
