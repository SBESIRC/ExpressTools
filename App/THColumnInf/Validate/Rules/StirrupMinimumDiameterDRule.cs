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
        public StirrupMinimumDiameterDRule(StirrupMinimumDiameterDModel stirMDD)
        {
            this.smdd = stirMDD;
        }
        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();
        public void Validate()
        {
            if(this.smdd == null || smdd.ValidateProperty()==false)
            {
                return;
            }
            if (smdd.IntStirrupDia < smdd.IntStirrupDiaLimited)
            {
                this.ValidateResults.Add("箍筋直径不满足抗震构造  (" + smdd.IntStirrupDia +
                "<"+ smdd.IntStirrupDiaLimited+") (砼规 11.4.12-2)");
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
            steps.Add("条目编号：59， 强制性：应，适用构件：LZ、KZ、ZHZ");
            steps.Add("适用功能：智能识图，图纸校核，条文编号：砼规 9.3.2-5，条文页数：P124");
            steps.Add("条文：柱中全部纵向受力钢筋的配筋率大于3% 时，箍筋直径不应小于8mm ，间距不应大于10d ，且不应大于200mm，d为纵向受力钢筋的最小直径。");

            steps.Add("if (IntStirrupDia[" + smdd.IntStirrupDia + "] < 箍筋直径限值[" + smdd.IntStirrupDiaLimited + "])");
            steps.Add("  {");
            steps.Add("     Err: 箍筋直径不满足抗震构造");
            steps.Add("  }");
            steps.Add("else");
            steps.Add("  {");
            steps.Add("     Ok: 箍筋直径满足抗震构造");
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
