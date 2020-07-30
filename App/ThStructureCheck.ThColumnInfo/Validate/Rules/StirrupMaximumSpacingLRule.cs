using System;
using System.Collections.Generic;
using System.Linq;
using ThColumnInfo.Validate.Model;

namespace ThColumnInfo.Validate.Rules
{
    public class StirrupMaximumSpacingLRule : IRule
    {
        private StirrupMaximumSpacingFModel smsfm=null;
        private double stirrupSpaceingLimited = 0.0;
        private string rule = "（《砼规》11.4.12-2）";
        public StirrupMaximumSpacingLRule(StirrupMaximumSpacingFModel smsfModel)
        {
            this.smsfm = smsfModel;
        }       
        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();

        public void Validate()
        {
            if(this.smsfm == null || !smsfm.ValidateProperty())
            {
                return;
            }
            if(this.smsfm.IsNonAntiseismic)
            {
                return;
            }
            if(this.smsfm.AntiSeismicGrade.Contains("一")||
                this.smsfm.AntiSeismicGrade.Contains("二") ||
                this.smsfm.AntiSeismicGrade.Contains("三") ||
                this.smsfm.AntiSeismicGrade.Contains("四"))
            {
                string oldAntiSeismicGrade = this.smsfm.AntiSeismicGrade;
                if (this.smsfm.AntiSeismicGrade.Contains("特")) 
                {
                    this.smsfm.AntiSeismicGrade = "一级";
                }
                //查表 箍筋间距限值
                this.stirrupSpaceingLimited = this.smsfm.IntStirrupSpacingLimited;
                this.smsfm.AntiSeismicGrade = oldAntiSeismicGrade;
                if (this.smsfm.Cdm.IntStirrupSpacing > this.stirrupSpaceingLimited)
                {
                    this.ValidateResults.Add("加密区箍筋间距大于" + this.stirrupSpaceingLimited + "[" + this.smsfm.Cdm.IntStirrupSpacing +
                        " > " + this.stirrupSpaceingLimited + "]，" + this.rule);
                }
                else
                {
                    this.CorrectResults.Add("加密区箍筋间距满足抗震构造" + this.rule);
                }
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            steps.Add("类别：箍筋最大间距L（箍筋）");
            steps.Add("强制性：应，适用构件：KZ、ZHZ");
            steps.Add("条文编号：砼规 11.4.12-2");
            steps.Add("条文：加密区箍筋间距满足抗震构造");
            steps.Add("柱号 = " + this.smsfm.Text);
            steps.Add("箍筋间距: intStirrupSpacing = " + smsfm.Cdm.IntStirrupSpacing);
            steps.Add("是否首层: " + smsfm.IsFirstFloor);

            steps.Add("if (抗震等级[" + this.smsfm.AntiSeismicGrade + "].Contains(\"特一级\") ||" +
               "抗震等级[" + this.smsfm.AntiSeismicGrade + "].Contains(\"一级\") || " +
               "抗震等级[" + this.smsfm.AntiSeismicGrade + "].Contains(\"二级\") || " +
               "抗震等级[" + this.smsfm.AntiSeismicGrade + "].Contains(\"三级\") || " +
               "抗震等级[" + this.smsfm.AntiSeismicGrade + "].Contains(\"四级\"))");
            steps.Add("  {");
            steps.Add("        抗震等级 = 一级");
            steps.Add(        "箍筋间距限值(查表): IntStirrupSpacingLimited = " + smsfm.IntStirrupSpacingLimited);
            steps.Add("       if (箍筋间距[" + smsfm.Cdm.IntStirrupSpacing + "] >" +
                      " 箍筋间距限值[" + this.stirrupSpaceingLimited + "])");
            steps.Add("         {");
            steps.Add("             Err：加密区箍筋间距大于" + this.stirrupSpaceingLimited + " " + this.rule);
            steps.Add("         }");
            steps.Add("       else");
            steps.Add("         {");
            steps.Add("             Debugprint：加密区箍筋间距满足抗震构造" + this.rule);
            steps.Add("         }");
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
