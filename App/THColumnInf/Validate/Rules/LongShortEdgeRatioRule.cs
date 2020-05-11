using System;
using System.Collections.Generic;

namespace ThColumnInfo.Validate
{
    /// <summary>
    /// 长短边比值（截面）
    /// </summary>
    public class LongShortEdgeRatioRule : IRule
    {
        private ColumnSectionModel columnSectionModel;

        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();
        public LongShortEdgeRatioRule(ColumnSectionModel columnSectionModel)
        {
            this.columnSectionModel = columnSectionModel;
        }        
        public void Validate()
        {
            if (columnSectionModel == null)
            {
                return;
            }
            if (columnSectionModel.Cdm.B <= 0 || columnSectionModel.Cdm.H <= 0)
            {
                return;
            }
            //长短边比值
            if (columnSectionModel.AntiSeismicGrade.Contains("一级") ||
                columnSectionModel.AntiSeismicGrade.Contains("二级") ||
                columnSectionModel.AntiSeismicGrade.Contains("三级") ||
                columnSectionModel.AntiSeismicGrade.Contains("四级"))
            {
                if (Math.Max(columnSectionModel.Cdm.B, columnSectionModel.Cdm.H) /
                    Math.Min(columnSectionModel.Cdm.B, columnSectionModel.Cdm.H) > 3.0)
                {
                    this.ValidateResults.Add("长边大于短边的3倍");
                }
                else
                {
                    this.CorrectResults.Add("长短边比值Ok");
                }
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            steps.Add("类别：长短边比值（截面）");
            steps.Add("条目编号：12， 强制性：宜，适用构件：KZ、ZHZ");
            steps.Add("适用功能：智能识图、图纸校核，条文编号：砼规 11.4.11-3，条文页数：175");
            steps.Add("条文：长边小于等于短边的3倍");
            steps.Add("柱号 = " + this.columnSectionModel.Text);
            steps.Add("if (抗震等级[" + columnSectionModel.AntiSeismicGrade + "] >= 四级 )");
            steps.Add(" {");
            steps.Add("   if (Math.Max(B[" + columnSectionModel.Cdm.B + "],H["+columnSectionModel.Cdm.H + "]) / Math.Min(B["+
                columnSectionModel.Cdm.B +"],H["+columnSectionModel.Cdm.H + "]) > 3 )");
            steps.Add("     {");
            steps.Add("         Err:长边小于等于短边的3倍");
            steps.Add("     }");
            steps.Add("  else");
            steps.Add("    {");
            steps.Add("        Debugprint:长短边比值小于3.");
            steps.Add("    }");
            steps.Add(" }");
            steps.Add("");
            return steps;
        }
    }
}
