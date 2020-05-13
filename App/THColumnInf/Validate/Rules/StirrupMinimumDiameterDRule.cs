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
            //查表得到箍筋直径限值
            double stirrupDiameterLimited = this.smdd.IntStirrupDiaLimited;
            //箍筋直径限值修正
            if (this.smdd.Jkb < 2 &&
                this.smdd.AntiSeismicGrade.Contains("四级"))
            {
                stirrupDiameterLimited = 8.0;
            }
            if (smdd.IntStirrupDia < smdd.IntStirrupDiaLimited)
            {
                this.ValidateResults.Add("箍筋直径不满足抗震构造");
            }
            else
            {
                this.CorrectResults.Add("箍筋直径满足抗震构造");
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            steps.Add("类别：箍筋最小直径D（箍筋）");
            steps.Add("条目编号：510， 强制性：应，适用构件：LZ、KZ、ZHZ");
            steps.Add("适用功能：智能识图，图纸校核，条文编号：砼规 9.3.2-5，条文页数：P124");
            steps.Add("条文：柱中全部纵向受力钢筋的配筋率大于3% 时，箍筋直径不应小于8mm ，间距不应大于10d ，且不应大于200mm，d为纵向受力钢筋的最小直径。");
            steps.Add("柱号 = " + this.smdd.Text);
            double stirrupDiameterLimited = this.smdd.IntStirrupDiaLimited;
            //箍筋直径限值修正
            if (this.smdd.Jkb < 2 &&
                this.smdd.AntiSeismicGrade.Contains("四级"))
            {
                stirrupDiameterLimited = 8.0;
            }

            steps.Add("箍筋直径：IntStirrupDia = "+ smdd.IntStirrupDia);
            steps.Add("是否底层：" + smdd.IsFirstFloor);
            steps.Add("箍筋直径限值：IntStirrupDiaLimited = " + smdd.IntStirrupDiaLimited);
            steps.Add("剪跨比 ：Jkb = " + smdd.Jkb);

            steps.Add("if (剪跨比[" + smdd.Jkb + "] < 2 && 抗震等级[" + smdd.AntiSeismicGrade + "] == 四级)");
            steps.Add("  {");
            steps.Add("     箍筋直径限值： IntStirrupDiaLimited = 8");
            steps.Add("  }");

            steps.Add("if (IntStirrupDia[" + smdd.IntStirrupDia + "] < 箍筋直径限值[" + stirrupDiameterLimited + "])");
            steps.Add("  {");
            steps.Add("     Err: 箍筋直径不满足抗震构造");
            steps.Add("  }");
            steps.Add("else");
            steps.Add("  {");
            steps.Add("     Debugprint: 箍筋直径满足抗震构造");
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
