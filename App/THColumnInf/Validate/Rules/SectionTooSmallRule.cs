using System;
using System.Collections.Generic;

namespace ThColumnInfo.Validate
{
    /// <summary>
    /// 截面
    /// </summary>
    public class SectionTooSmallRule : IRule
    {
        private ColumnSectionModel columnSectionModel;

        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();
        private string rule = "（《砼规》11.4.11-1）";
        public SectionTooSmallRule(ColumnSectionModel columnSectionModel)
        {
            this.columnSectionModel = columnSectionModel;
        }        
        public void Validate()
        {
            if (columnSectionModel == null || !columnSectionModel.ValidateProperty())
            {
                return;
            }
            double min = Math.Min(columnSectionModel.Cdm.B, columnSectionModel.Cdm.H);
            //矩形截面柱，抗震等级为四级或层数不超过2 层时，其最小截面尺寸不宜小于300mm 
            if (columnSectionModel.AntiSeismicGrade.Contains("四级") ||
                columnSectionModel.FloorTotalNums <= 2)
            {
                if (min < 300)
                {
                    this.ValidateResults.Add("最小截面不满足 [" + min + " < 300]" + this.rule);
                }
                else
                {
                    this.CorrectResults.Add("最小截面满足" + this.rule);
                }
            }
            //一、二、三级抗震等级且层数超过2层时不宜小于400mm
            else
            {
                if (Math.Min(columnSectionModel.Cdm.B, columnSectionModel.Cdm.H) < 400)
                {
                    this.ValidateResults.Add("最小截面不满足 [" + " < 400]" + this.rule);
                }
                else
                {
                    this.CorrectResults.Add("最小截面满足" + this.rule);
                }
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            steps.Add("类别：最小截面（截面）");
            steps.Add("条目编号：11， 强制性：宜，适用构件：KZ、ZHZ");
            steps.Add("适用功能：智能识图、图纸校核，条文编号：砼规 11.4.11-1，条文页数：175");
            steps.Add("条文：矩形截面柱，抗震等级为四级或层数不超过2 层时，其最小截面尺寸不宜小于300mm ，一、二、三级抗震等级且层数超过2层时不宜小于400mm");
            steps.Add("柱号 = " + this.columnSectionModel.Text);
            steps.Add("if (抗震等级[" + columnSectionModel.AntiSeismicGrade + "] == 四级 || 自然层数[" +
               columnSectionModel.FloorTotalNums + "] <= 2 )");
            steps.Add("  {");
            steps.Add("    if (Math.Min(B[" + columnSectionModel.Cdm.B +"],H["+ columnSectionModel.Cdm.H + "]) < 300 )");
            steps.Add("        {");
            steps.Add("            Err:最小截面不满足 （《砼规》11.4.11-1）");
            steps.Add("        }");
            steps.Add("    else");
            steps.Add("        {");
            steps.Add("            Debugprint:最小截面满足 （《砼规》11.4.11-1）");
            steps.Add("        }");
            steps.Add("  }");
            steps.Add("else");
            steps.Add("  {");
            steps.Add("    if (Math.Min(B[" + columnSectionModel.Cdm.B + "],H[" + columnSectionModel.Cdm.H + "]) < 400 )");
            steps.Add("       {");
            steps.Add("          Err:最小截面不满足 （《砼规》11.4.11-1）");
            steps.Add("       }");
            steps.Add("    else");
            steps.Add("       {");
            steps.Add("          Debugprint:最小截面满足（《砼规》11.4.11-1）");
            steps.Add("       }");
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
