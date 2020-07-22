using System.Collections.Generic;
using System.Linq;
using ThColumnInfo.Validate.Model;

namespace ThColumnInfo.Validate.Rules
{
    public class StirrupMaximumSpacingKRule : IRule
    {
        private StirrupMaximumSpacingKModel smsk =null;
        private string rule = "（《砼规》11.4.14）";
        public StirrupMaximumSpacingKRule(StirrupMaximumSpacingKModel smsk)
        {
            this.smsk = smsk;
        }
        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();
        public void Validate()
        {
            if(this.smsk == null || smsk.ValidateProperty()==false)
            {
                return;
            }      
            if(this.smsk.AntiSeismicGrade.Contains("一级") ||
                this.smsk.AntiSeismicGrade.Contains("二级") &&
                !this.smsk.AntiSeismicGrade.Contains("特"))
            {
                if(this.smsk.IsCornerColumn && this.smsk.Cdm.IntStirrupSpacing>0)
                {
                    if(this.smsk.Cdm.IntStirrupSpacing != this.smsk.Cdm.IntStirrupSpacing0)
                    {
                        this.ValidateResults.Add("一、二级角柱应沿柱全高加密 " + this.rule);
                    }
                }
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            steps.Add("类别：箍筋最大间距（箍筋）");
            steps.Add("强制性：应，适用构件：KZ,LZ,ZHZ");
            steps.Add("条文：一、二级角柱应沿柱全高加密");
            steps.Add("柱号 = " + this.smsk.Text);
            double intStirrupSpacing = this.smsk.Cdm.IntStirrupSpacing;
            double intStirrupSpacing0 = this.smsk.Cdm.IntStirrupSpacing0;
            steps.Add("if (抗震等级["+this.smsk.AntiSeismicGrade+"].Contains(\"一级\") || " +
                "抗震等级[" + this.smsk.AntiSeismicGrade + "].Contains(\"二级\"))");
            steps.Add("  {");           
            steps.Add("      if(是否角柱[" + this.smsk.IsCornerColumn + "] && IntStirrupSpacing["+
                intStirrupSpacing+"] !=0.0)");
            steps.Add("        {");
            steps.Add("           if(intStirrupSpacing["+ intStirrupSpacing +
                "] == intStirrupSpacing0["+ intStirrupSpacing0+"])");
            steps.Add("              {");
            steps.Add("                Error: 一、二级角柱应沿柱全高加密 " + this.rule);
            steps.Add("              {");
            steps.Add("        }");
            steps.Add("  }");
            return steps;
        }
    }
}
