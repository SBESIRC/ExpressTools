using System;
using System.Collections.Generic;
using System.Linq;
using ThColumnInfo.Validate.Model;

namespace ThColumnInfo.Validate.Rules
{
    public class StirrupMaximumSpacingFRule : IRule
    {
        private StirrupMaximumSpacingFModel smsfm=null;
        private double stirrupSpaceingLimited = 0.0;
        private string rule = "（《砼规》11.4.12）";
        public StirrupMaximumSpacingFRule(StirrupMaximumSpacingFModel smsfModel)
        {
            this.smsfm = smsfModel;
        }       
        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();

        private double dblXSpace;
        private double dblYSpace;
        private double dblStirrupSpace;

        public void Validate()
        {
            if(this.smsfm == null || !smsfm.ValidateProperty())
            {
                return;
            }
            this.dblXSpace = (this.smsfm.Cdm.B - 2 * this.smsfm.ProtectThickness - 
                this.smsfm.Cdm.IntStirrupDia) / (this.smsfm.Cdm.IntYStirrupCount - 1);
            this.dblYSpace = (this.smsfm.Cdm.H - 2 * this.smsfm.ProtectThickness - 
                this.smsfm.Cdm.IntStirrupDia) / (this.smsfm.Cdm.IntXStirrupCount - 1);
            this.dblStirrupSpace = Math.Max(dblXSpace, dblYSpace);

            if (this.smsfm.Code.ToUpper().Contains("ZHZ") ||
               (this.smsfm.Jkb <= 2 && this.smsfm.Code.ToUpper().Contains("KZ")))
            {
                this.smsfm.AntiSeismicGrade = "一级";
            }
            //查表 箍筋间距限值
            this.stirrupSpaceingLimited = this.smsfm.IntStirrupSpacingLimited;
            if (this.smsfm.Cdm.IntStirrupSpacing > this.stirrupSpaceingLimited)
            {
                this.ValidateResults.Add("箍筋间距不满足抗震构造 ["+ this.smsfm.Cdm.IntStirrupSpacing +
                    " > " + this.stirrupSpaceingLimited+"]，"+this.rule);
            }
            else
            {
                this.CorrectResults.Add("箍筋间距满足抗震构造"+this.rule);
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            steps.Add("类别：箍筋最大间距F（箍筋）");
            steps.Add("条目编号：511， 强制性：应，适用构件：KZ、ZHZ");
            steps.Add("适用功能：智能识图，图纸校核，条文编号：砼规 11.4.12，条文页数：P176");
            steps.Add("条文：见图");
            steps.Add("柱号 = " + this.smsfm.Text);
            steps.Add("箍筋间距: intStirrupSpacing = " + smsfm.Cdm.IntStirrupSpacing);
            steps.Add("是否首层: " + smsfm.IsFirstFloor);

            double intBardiamin = Math.Min(this.smsfm.Cdm.IntXBarDia, this.smsfm.Cdm.IntYBarDia);
            intBardiamin = Math.Min(intBardiamin, this.smsfm.Cdm.IntCBarDia);
            //纵向钢筋直径最小值
            steps.Add("double intBardiamin =Math.Min(IntXBarDia["+
                this.smsfm.Cdm.IntXBarDia+ "],IntYBarDia["+ this.smsfm.Cdm.IntYBarDia+ "]," +
                "IntCBarDia["+ this.smsfm.Cdm.IntCBarDia+"]) = "+ intBardiamin);

            steps.Add("if (柱号[" + this.smsfm.Text + "].Contains(\"ZHZ\") || " +
                "(剪跨比[" + this.smsfm.Jkb + "] <= 2 && 柱号[" + this.smsfm.Text + "].Contains(\"KZ\"))");
            steps.Add("  {");
            steps.Add("     抗震等级： 一级");
            steps.Add("  }");

            steps.Add("箍筋间距限值(查表): IntStirrupSpacingLimited = " + smsfm.IntStirrupSpacingLimited);

            steps.Add("dblXSpace = (B[" + this.smsfm.Cdm.B + "] - 2 * 保护层厚度[" + this.smsfm.ProtectThickness +
               "]- IntStirrupDia[" + this.smsfm.Cdm.IntStirrupDia  + "]) / (IntYStirrupCount[" +
               this.smsfm.Cdm.IntYStirrupCount + "] - 1) = " + this.dblXSpace);
            steps.Add("dblYSpace = (H[" + this.smsfm.Cdm.H + "] - 2 * 保护层厚度[" + this.smsfm.ProtectThickness +
                "]- IntStirrupDia[" + this.smsfm.Cdm.IntStirrupDia + "]) / (IntXStirrupCount[" + 
                this.smsfm.Cdm.IntXStirrupCount + "] - 1) = " + this.dblYSpace);
            steps.Add("dblStirrupSpace=Math.Max(dblXSpace[" + this.dblXSpace + "] , dblYSpace[" + this.dblYSpace + "]) = " 
                + this.dblStirrupSpace);

            steps.Add("if (箍筋间距[" + smsfm.Cdm.IntStirrupSpacing + "] >" +
                " 箍筋间距限值[" + this.stirrupSpaceingLimited + "])");
            steps.Add("  {");
            steps.Add("      Err：箍筋间距不满足抗震构造" + this.rule);
            steps.Add("  }");
            steps.Add("else");
            steps.Add("  {");
            steps.Add("      Debugprint：箍筋间距满足抗震构造" + this.rule);
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
