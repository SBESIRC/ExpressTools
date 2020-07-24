﻿using System.Collections.Generic;
using System.Linq;
using ThColumnInfo.Validate.Model;

namespace ThColumnInfo.Validate.Rules
{
    public class MinimumReinforcementRatioARule : IRule
    {
        private MinimumReinforceRatioAModel minimumReinforceRatioAModel;
        public MinimumReinforcementRatioARule(MinimumReinforceRatioAModel minimumReinforceRatioAModel)
        {
            this.minimumReinforceRatioAModel = minimumReinforceRatioAModel;
        }
        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();
        private string rule = "（《砼规》8.5.1）";
        public void Validate()
        {
            if (this.minimumReinforceRatioAModel == null || !this.minimumReinforceRatioAModel.ValidateProperty())
            {
                return;
            }
            //最小全截面配筋率限值
            double dblpmin = (minimumReinforceRatioAModel.P1 + minimumReinforceRatioAModel.P2);

            if(this.minimumReinforceRatioAModel.AntiSeismicGrade.Contains("特")&&
               this.minimumReinforceRatioAModel.AntiSeismicGrade.Contains("一级"))
            {
                if(this.minimumReinforceRatioAModel.Code.ToUpper().Contains("ZHZ"))
                {
                    this.rule = "高规 3.10.4-3";
                    //全截面
                    if (minimumReinforceRatioAModel.Cdm.DblP < 0.016)
                    {
                        ValidateResults.Add("全截面配筋率不足 [" + minimumReinforceRatioAModel.Cdm.DblP
                            + " < 0.016]，" + this.rule);
                    }
                    else
                    {
                        CorrectResults.Add("全截面配筋率满足基本构造"+this.rule);
                    }
                }
            }
            else
            {
                //全截面
                if (minimumReinforceRatioAModel.Cdm.DblP < (dblpmin / 100.0))
                {
                    ValidateResults.Add("全截面配筋率不足 [" + minimumReinforceRatioAModel.Cdm.DblP
                        + " < " + (dblpmin / 100.0) + "]，" + this.rule);
                }
                else
                {
                    CorrectResults.Add("全截面配筋率满足基本构造（《砼规》8.5.1）");
                }
            }
            //Y侧
            if (minimumReinforceRatioAModel.Cdm.DblYP < (minimumReinforceRatioAModel.Dblpsmin / 100.0))
            {
                ValidateResults.Add("Y侧配筋率不足 [" + minimumReinforceRatioAModel.Cdm.DblYP
                    + " < " + (minimumReinforceRatioAModel.Dblpsmin / 100.0) + "]，" + this.rule);
            }
            else
            {
                CorrectResults.Add("Y侧配筋率满足基本构造" + this.rule);
            }
            //X侧
            if (minimumReinforceRatioAModel.Cdm.DblXP < (minimumReinforceRatioAModel.Dblpsmin / 100.0))
            {
                ValidateResults.Add("X侧配筋率不足 [" + minimumReinforceRatioAModel.Cdm.DblXP
                    + " < " + (minimumReinforceRatioAModel.Dblpsmin / 100.0) + "]，" + this.rule);
            }
            else
            {
                CorrectResults.Add("X侧配筋率满足基本构造" + this.rule);
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            steps.Add("类别：最小配筋率A（侧面纵筋）");
            steps.Add("条目编号：44， 强制性：应，适用构件：LZ、KZ、ZHZ");
            steps.Add("适用功能：智能识图，图纸校核，条文编号：砼规 8.5.1，条文页数：P109");
            steps.Add("条文：钢筋混凝土结构构件中纵向受力钢筋的配筋百分率不应小于表8. 5.1 规定的数值。");
            steps.Add("柱号 = " + this.minimumReinforceRatioAModel.Text);
            double dblpmin = minimumReinforceRatioAModel.P1 + minimumReinforceRatioAModel.P2;
            steps.Add("dblpmin = (P1["+ minimumReinforceRatioAModel.P1+ "] + P2[" + minimumReinforceRatioAModel.P2+"])/100.0 = "+ (dblpmin / 100.0));
            steps.Add(this.minimumReinforceRatioAModel.Cdm.GetDblAsCalculation());
            steps.Add(this.minimumReinforceRatioAModel.Cdm.GetDblpCalculation());

            steps.Add("if(抗震等级 ["+ this.minimumReinforceRatioAModel.AntiSeismicGrade+"].Contains(\"特一级\"))");
            steps.Add("  {");
            steps.Add("    if (DblP[" + minimumReinforceRatioAModel.Cdm.DblP + "] < 0.016)");
            steps.Add("      {");
            steps.Add("          Err：全截面配筋率不足  " + this.rule);
            steps.Add("      }");
            steps.Add("    else");
            steps.Add("      {");
            steps.Add("          Debugprint：全截面配筋率满足基本构造 " + this.rule);
            steps.Add("      }");
            steps.Add("  }");
            steps.Add("else");
            steps.Add("  {");
            steps.Add("    if (DblP[" + minimumReinforceRatioAModel.Cdm.DblP + "] < dblpmin[" + (dblpmin / 100.0) + "] )");
            steps.Add("      {");
            steps.Add("          Err：全截面配筋率不足（《砼规》8.5.1）");
            steps.Add("      }");
            steps.Add("    else");
            steps.Add("      {");
            steps.Add("          Debugprint：全截面配筋率满足基本构造（《砼规》8.5.1）");
            steps.Add("      }");
            steps.Add("  }");

            steps.Add(this.minimumReinforceRatioAModel.Cdm.GetDblYAsCalculation());
            steps.Add(this.minimumReinforceRatioAModel.Cdm.GetDblypCalculation());
            steps.Add("if (DblYP[" + minimumReinforceRatioAModel.Cdm.DblYP + "] < Dblpsmin[" + (minimumReinforceRatioAModel.Dblpsmin/100.0) + "])");
            steps.Add("  {");
            steps.Add("      Err：Y侧配筋率不足（《砼规》8.5.1）");
            steps.Add("  }");
            steps.Add("else");
            steps.Add("  {");
            steps.Add("      Debugprint：Y侧配筋率满足基本构造（《砼规》8.5.1）");
            steps.Add("  }");

            steps.Add(this.minimumReinforceRatioAModel.Cdm.GetDblXAsCalculation());
            steps.Add(this.minimumReinforceRatioAModel.Cdm.GetDblxpCalculation());
            steps.Add("if (DblXP[" + minimumReinforceRatioAModel.Cdm.DblXP + "] < Dblpsmin[" + (minimumReinforceRatioAModel.Dblpsmin/100.0) + "])");
            steps.Add("  {");
            steps.Add("      Err：X侧配筋率不足（《砼规》8.5.1）");
            steps.Add("  }");
            steps.Add("else");
            steps.Add("  {");
            steps.Add("      Debugprint：X侧配筋率满足基本构造（《砼规》8.5.1）");
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
