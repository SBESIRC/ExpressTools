using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Validate
{
    public class StirrupMinimumDiameterDRule:IRule
    {
        private StirrupMinimumDiameterDModel smdd;
        private string rule = "（《砼规》11.4.12）";
        private double stirrupDiameterLimited;
        public StirrupMinimumDiameterDRule(StirrupMinimumDiameterDModel smdd)
        {
            this.smdd = smdd;
        }
        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();
        public void Validate()
        {
            if(this.smdd == null || smdd.ValidateProperty()==false)
            {
                return;
            }
            //修正抗震等级
            if (this.smdd.Code.ToUpper().Contains("ZHZ") ||
                (this.smdd.Jkb <= 2 && this.smdd.Code.ToUpper().Contains("KZ")))
            {
                this.smdd.AntiSeismicGrade = "一级";
            }
            //查表得到箍筋直径限值
            this.stirrupDiameterLimited = this.smdd.IntStirrupDiaLimited;
            //箍筋直径限值修正            
            if (this.smdd.Code.ToUpper().Contains("KZ") &&
                this.smdd.AntiSeismicGrade.Contains("四级") && this.smdd.Jkb <= 2)
            {
                this.stirrupDiameterLimited = 8.0;
            }
            if (smdd.IntStirrupDia < stirrupDiameterLimited)
            {
                this.ValidateResults.Add("箍筋直径不满足抗震构造 ["+ 
                    smdd.IntStirrupDia+ " < "+ stirrupDiameterLimited + "]，"+this.rule);
            }
            else
            {
                this.CorrectResults.Add("箍筋直径满足抗震构造"+this.rule);
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            steps.Add("类别：箍筋最小直径D（箍筋）");
            steps.Add("条目编号：510， 强制性：应，适用构件：KZ、ZHZ");
            steps.Add("适用功能：智能识图，图纸校核");
            steps.Add("条文编号：砼规 11.4.12，条文页数：P176");
            steps.Add("条文：框架柱和框支柱上、下两端箍筋应加密，加密区的箍筋最大间距和箍筋最小直径应符合表11.4.12的规定");
            steps.Add("柱号 = " + this.smdd.Text);

            steps.Add("箍筋直径：IntStirrupDia = " + smdd.IntStirrupDia);
            steps.Add("抗震等级 = " + this.smdd.AntiSeismicGrade);
            steps.Add("剪跨比 = " + this.smdd.Jkb);
            steps.Add("是否是首层 = " + this.smdd.IsFirstFloor);

            steps.Add("if (柱号[" + this.smdd.Text + "].Contains(\"ZHZ\") || " +
                "(剪跨比[" + this.smdd.Jkb + "] <= 2 && 柱号[" + this.smdd.Text + "].Contains(\"KZ\"))");
            steps.Add("  {");
            steps.Add("     抗震等级： 一级");
            steps.Add("  }");
            steps.Add("箍筋直径限值(查表) = " + this.smdd.IntStirrupDiaLimited);

            steps.Add("if (柱号["+this.smdd.Code+ "].Contains(\"KZ\") && 抗震等级[" + 
                smdd.AntiSeismicGrade + "] == 四级 && 剪跨比[" + smdd.Jkb + "] <= 2)");
            steps.Add("  {");
            steps.Add("     箍筋直径限值修正值 = 8");
            steps.Add("  }");

            steps.Add("if (IntStirrupDia[" + smdd.IntStirrupDia + "] < 箍筋直径限值[" + this.stirrupDiameterLimited + "])");
            steps.Add("  {");
            steps.Add("     Err: 箍筋直径不满足抗震构造" + this.rule);
            steps.Add("  }");
            steps.Add("else");
            steps.Add("  {");
            steps.Add("     Debugprint: 箍筋直径满足抗震构造" + this.rule);
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
