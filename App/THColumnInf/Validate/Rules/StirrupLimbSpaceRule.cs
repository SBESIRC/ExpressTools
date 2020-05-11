using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Validate
{
    public class StirrupLimbSpaceRule:IRule
    {
        private StirrupLimbSpaceModel stirrupLimbSpaceModel = null;
        public StirrupLimbSpaceRule(StirrupLimbSpaceModel stirrupLimbSpaceModel)
        {
            this.stirrupLimbSpaceModel = stirrupLimbSpaceModel;
        }
        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();

        private double dblXSpace;
        private double dblYSpace;
        private double dblStirrupSpace;
        public void Validate()
        {
            if(stirrupLimbSpaceModel==null || stirrupLimbSpaceModel.ValidateProperty()==false)
            {
                return;
            }
            this.dblXSpace = (stirrupLimbSpaceModel.Cdm.B - 2 * stirrupLimbSpaceModel.ProtectLayerThickness) /
                (stirrupLimbSpaceModel.Cdm.IntYStirrupCount - 1);
            this.dblYSpace = (stirrupLimbSpaceModel.Cdm.H - 2 * stirrupLimbSpaceModel.ProtectLayerThickness) /
                (stirrupLimbSpaceModel.Cdm.IntXStirrupCount - 1);
            this.dblStirrupSpace = Math.Max(dblXSpace, dblYSpace);
            if (stirrupLimbSpaceModel.AntiSeismicGrade.Contains("一级") && 
                !stirrupLimbSpaceModel.AntiSeismicGrade.Contains("特"))
            {
                if(dblStirrupSpace > 200)
                {
                    this.ValidateResults.Add("箍筋肢距不满足抗震构造");
                }
                else
                {
                    this.CorrectResults.Add("箍筋肢距满足抗震构造");
                }
            }
            else if(stirrupLimbSpaceModel.AntiSeismicGrade.Contains("二级") ||
                stirrupLimbSpaceModel.AntiSeismicGrade.Contains("三级"))
            {
                if(dblStirrupSpace > Math.Max(250,20* stirrupLimbSpaceModel.Cdm.IntStirrupDia))
                {
                    this.ValidateResults.Add("箍筋肢距不满足抗震构造");
                }
                else
                {
                    this.CorrectResults.Add("箍筋肢距满足抗震构造");
                }
            }
            else if(stirrupLimbSpaceModel.AntiSeismicGrade.Contains("四级"))
            {
                if (dblStirrupSpace > 300)
                {
                    this.ValidateResults.Add("箍筋肢距不满足抗震构造");
                }
                else
                {
                    this.CorrectResults.Add("箍筋肢距满足抗震构造");
                }
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            steps.Add("类别：箍筋肢距（箍筋）");
            steps.Add("条目编号：51， 强制性：应，适用构件：KZ、ZHZ");
            steps.Add("适用功能：智能识图，图纸校核，条文编号：砼规 11.4.15，条文页数：P123");
            steps.Add("条文：柱箍筋加密区内的箍筋肢距：一级抗震等级不宜大于200mm ；二、兰级抗震等级不宜大于250mm 和20 倍箍筋直径中的较大值；四级抗震等级不宜大于300mm。");
            steps.Add("柱号 = " + this.stirrupLimbSpaceModel.Text);
            steps.Add("dblXSpace = (B["+ stirrupLimbSpaceModel.Cdm.B+"] - 2 * 保护层厚度["+ stirrupLimbSpaceModel.ProtectLayerThickness+ 
                "]) / (IntYStirrupCount["+ stirrupLimbSpaceModel.Cdm.IntYStirrupCount+"] - 1) = " + this.dblXSpace);
            steps.Add("dblYSpace = (H[" + stirrupLimbSpaceModel.Cdm.H + "] - 2 * 保护层厚度[" + stirrupLimbSpaceModel.ProtectLayerThickness +
                "]) / (IntXStirrupCount[" + stirrupLimbSpaceModel.Cdm.IntXStirrupCount + "] - 1) = " + this.dblYSpace);

            steps.Add("dblStirrupSpace=Math.Max(dblXSpace["+this.dblXSpace+ "] , dblYSpace["+ this.dblYSpace+"]) = "+this.dblStirrupSpace);
            steps.Add("if (抗震等级[" + stirrupLimbSpaceModel.AntiSeismicGrade + "] == 一级 ");
            steps.Add("  {");
            steps.Add("    if (dblStirrupSpace[" + this.dblStirrupSpace + "] > 200 )");
            steps.Add("       {");
            steps.Add("           Err：箍筋肢距不满足抗震构造");
            steps.Add("       }");
            steps.Add("    else");
            steps.Add("       {");
            steps.Add("           Debugprint: 箍筋肢距满足抗震构造");
            steps.Add("       }");
            steps.Add("  }");

            steps.Add("else if (抗震等级[" + stirrupLimbSpaceModel.AntiSeismicGrade + "] == 二级 || 抗震等级[" +
               stirrupLimbSpaceModel.AntiSeismicGrade + "] == 三级 )");
            steps.Add("  {");
            steps.Add("    if (dblStirrupSpace[" + this.dblStirrupSpace + "] > Math.Max(250, 20 * IntStirrupDia["+ stirrupLimbSpaceModel.Cdm.IntStirrupDia+"])");
            steps.Add("       {");
            steps.Add("           Err：箍筋肢距不满足抗震构造");
            steps.Add("       }");
            steps.Add("    else");
            steps.Add("       {");
            steps.Add("           Debugprint:箍筋肢距满足抗震构造");
            steps.Add("       }");
            steps.Add("  }");

            steps.Add("else if (抗震等级[" + stirrupLimbSpaceModel.AntiSeismicGrade + "] == 四级 )");
            steps.Add("  {");
            steps.Add("    if (dblStirrupSpace[" + this.dblStirrupSpace + "] > 300)");
            steps.Add("       {");
            steps.Add("           Err：箍筋肢距不满足抗震构造");
            steps.Add("       }");
            steps.Add("    else");
            steps.Add("       {");
            steps.Add("           Debugprint:箍筋肢距满足抗震构造");
            steps.Add("       }");
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
