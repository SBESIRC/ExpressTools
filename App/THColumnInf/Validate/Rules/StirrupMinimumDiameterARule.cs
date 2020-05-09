using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Validate
{
    public class StirrupMinimumDiameterARule:IRule
    {
        private StirrupMinimumDiameterAModel smda;
        public StirrupMinimumDiameterARule(StirrupMinimumDiameterAModel smda)
        {
            this.smda = smda;
        }

        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();

        public void Validate()
        {
            if (this.smda == null || this.smda.ValidateProperty() == false)
            {
                return;
            }
            double intBarDiamax = Math.Max(this.smda.Cdm.IntCBarDia,
                this.smda.Cdm.IntXBarDia);
            intBarDiamax = Math.Max(intBarDiamax, this.smda.Cdm.IntYBarDia);
            if(this.smda.Cdm.IntStirrupDia<(0.25* intBarDiamax))
            {
                this.ValidateResults.Add("箍筋直径小于1/4纵筋最大直径 ("+ this.smda.Cdm.IntStirrupDia+
                    "<"+ (0.25 * intBarDiamax)+ ") (砼规 9.3.2-1)");
            }
            else
            {
                this.CorrectResults.Add("箍筋直径大于1/4纵筋最大直径");
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            double intBardiamax = Math.Max(this.smda.Cdm.IntXBarDia,
                this.smda.Cdm.IntYBarDia);
            steps.Add("类别：箍筋最小直径A（箍筋）");
            steps.Add("条目编号：52， 强制性：应，适用构件：LZ、KZ、ZHZ");
            steps.Add("适用功能：智能识图，图纸校核，条文编号：砼规 9.3.2-1，条文页数：P123");
            steps.Add("条文：柱箍筋加密区内的箍筋肢距：一级抗震等级不宜大于200mm ；二、兰级抗震等级不宜大于250mm 和20 倍箍筋直径中的较大值；四级抗震等级不宜大于300mm。");

            intBardiamax = Math.Max(this.smda.Cdm.IntCBarDia, intBardiamax);
            steps.Add("intBardiamax=Math.Max(IntCBarDia[" + this.smda.Cdm.IntCBarDia + "],IntXBarDia[" + this.smda.Cdm.IntXBarDia +
                "],IntYBarDia[" + this.smda.Cdm.IntYBarDia + "]) =" + intBardiamax);         
            steps.Add("if (IntStirrupDia[" + this.smda.Cdm.IntStirrupDia + "] < (0.25 * intBardiamax["+ intBardiamax + "]))");
            steps.Add("  {");
            steps.Add("     箍筋直径小于1/4纵筋最大直径");
            steps.Add("  }");
            steps.Add("else");
            steps.Add("  {");
            steps.Add("     箍筋直径大于1/4纵筋最大直径");
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
