using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Validate
{
    public class MinimumReinforcementRatioBRule : IRule
    {
        private MinimumReinforceRatioBModel minimumReinforceRatioBModel;
        private double dblsespmin = 0.0;
        public MinimumReinforcementRatioBRule(MinimumReinforceRatioBModel minimumReinforceRatioBModel)
        {
            this.minimumReinforceRatioBModel = minimumReinforceRatioBModel;
        }
        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();
        public void Validate()
        {
            if (this.minimumReinforceRatioBModel == null || 
                minimumReinforceRatioBModel.ValidateProperty()==false)
            {
                return;
            }
            this.dblsespmin = this.minimumReinforceRatioBModel.Dblsespmin;
            //混凝土强度大于等于C60,加上0.1
            List<double> concreteValues = ThColumnInfoUtils.GetDoubleValues(this.minimumReinforceRatioBModel.ConcreteStrength);
            if (concreteValues.Count > 0 && concreteValues[0] >= 60)
            {
                this.dblsespmin += 0.1;
            }
            if (minimumReinforceRatioBModel.IsFourClassHigherArchitecture)
            {
                this.dblsespmin += 0.1;
                minimumReinforceRatioBModel.Dblpsessmin += 0.1;
            }
            if (minimumReinforceRatioBModel.Cdm.DblP < (this.dblsespmin/100.0))
            {
                ValidateResults.Add("全截面抗震配筋率不足");
            }
            else
            {
                CorrectResults.Add("全截面配筋率满足抗震构造");
            }
            //Y侧
            if (minimumReinforceRatioBModel.Cdm.DblYP < (minimumReinforceRatioBModel.Dblpsessmin/100.0))
            {
                ValidateResults.Add("Y侧抗震配筋率不足");
            }
            else
            {
                CorrectResults.Add("Y侧配筋率满足抗震构造");
            }
            //X侧
            if (minimumReinforceRatioBModel.Cdm.DblXP < (minimumReinforceRatioBModel.Dblpsessmin/100.0))
            {
                ValidateResults.Add("X侧抗震配筋率不足");
            }
            else
            {
                CorrectResults.Add("X侧配筋率满足抗震构造");
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            steps.Add("类别：最小配筋率B（侧面纵筋）");
            steps.Add("条目编号：45， 强制性：应，适用构件：KZ、ZHZ");
            steps.Add("适用功能：智能识图，图纸校核，条文编号：砼规 11.4.12，条文页数：P174");
            steps.Add("条文：框架柱和框支柱中全部纵向受力钢筋的配筋百分率不应小于表11. 4. 12-1 规定的数值，同时，每一侧的配筋百分率不应小于0.2 ；对IV 类场地上较高的高层建筑，最小配筋百分率应增加0. 1");
            steps.Add("柱号 = " + this.minimumReinforceRatioBModel.Text);
            steps.Add("抗震等级：" +this.minimumReinforceRatioBModel.AntiSeismicGrade);
            steps.Add("结构类型：" + this.minimumReinforceRatioBModel.StructureType);
            steps.Add("是否角柱：" + this.minimumReinforceRatioBModel.IsCornerColumn);
            steps.Add("柱子类型：" + this.minimumReinforceRatioBModel.ColumnType);
            steps.Add("是否为IV类场地较高建筑：" + 
                this.minimumReinforceRatioBModel.IsFourClassHigherArchitecture);

            steps.Add(this.minimumReinforceRatioBModel.Cdm.GetDblAsCalculation());
            steps.Add(this.minimumReinforceRatioBModel.Cdm.GetDblpCalculation());
            steps.Add(this.minimumReinforceRatioBModel.Cdm.GetDblXAsCalculation());
            steps.Add(this.minimumReinforceRatioBModel.Cdm.GetDblxpCalculation());
            steps.Add(this.minimumReinforceRatioBModel.Cdm.GetDblYAsCalculation());
            steps.Add(this.minimumReinforceRatioBModel.Cdm.GetDblypCalculation());

            steps.Add("最小全截面配筋率限值 -> 查表 dblsespmin = " +
                minimumReinforceRatioBModel.Dblsespmin);
            steps.Add("if(混凝土强度[" + minimumReinforceRatioBModel.ConcreteStrength + "] >= 60)");
            steps.Add("  {");
            steps.Add("     dblsespmin=dblsespmin+0.1 ="+minimumReinforceRatioBModel.Dblsespmin+0.1);
            steps.Add("  }");

            steps.Add("if(是否为IV类场地较高建筑[" + minimumReinforceRatioBModel.IsFourClassHigherArchitecture + "])");
            steps.Add("  {");
            steps.Add("      Dblsespmin=Dblsespmin+0.1= "+ this.dblsespmin);
            steps.Add("      Dblpsessmin=Dblpsessmin+0.1= " + minimumReinforceRatioBModel.Dblpsessmin);
            steps.Add("  }");
            
            steps.Add("if (DblP[" + minimumReinforceRatioBModel.Cdm.DblP + "] < Dblsespmin[" + (minimumReinforceRatioBModel.Dblsespmin/100.0) + "])");
            steps.Add("  {");
            steps.Add("      Err：全截面抗震配筋率不足");
            steps.Add("  }");
            steps.Add("else");
            steps.Add("  {");
            steps.Add("      Debugprint：全截面配筋率满足抗震构造");
            steps.Add("  }");

            
            steps.Add("if (DblYP[" + minimumReinforceRatioBModel.Cdm.DblYP + "] < Dblpsessmin[" + (minimumReinforceRatioBModel.Dblpsessmin/100.0) + "])");
            steps.Add("  {");
            steps.Add("      Err：Y侧抗震配筋率不足");
            steps.Add("  }");
            steps.Add("else");
            steps.Add("  {");
            steps.Add("      Debugprint：Y侧配筋率满足抗震构造");
            steps.Add("  }");

            
            steps.Add("if (DblXP[" + minimumReinforceRatioBModel.Cdm.DblXP + "] < Dblpsessmin[" + (minimumReinforceRatioBModel.Dblpsessmin/100.0) + "])");
            steps.Add("  {");
            steps.Add("      Err：X侧抗震配筋率不足");
            steps.Add("  }");
            steps.Add("else");
            steps.Add("  {");
            steps.Add("      Debugprint：X侧配筋率满足抗震构造");
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
