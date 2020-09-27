using System;
using System.Collections.Generic;
using ThColumnInfo.Validate.Model;

namespace ThColumnInfo.Validate.Rules
{
    /// <summary>
    /// 长短边比值（截面）
    /// </summary>
    public class LongShortEdgeRatioRule : IRule
    {
        private ColumnSectionModel columnSectionModel;
        private string rule = "（《砼规》11.4.11-3）";

        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();
        public LongShortEdgeRatioRule(ColumnSectionModel columnSectionModel)
        {
            this.columnSectionModel = columnSectionModel;
        }        
        public void Validate()
        {
           if(columnSectionModel==null|| !columnSectionModel.ValidateProperty())
            {
                return;
            }
           if(columnSectionModel.IsNonAntiseismic)
            {
                return;
            }
            double max = Math.Max(columnSectionModel.Cdm.B, columnSectionModel.Cdm.H);
            double min = Math.Min(columnSectionModel.Cdm.B, columnSectionModel.Cdm.H);
            if ((max / min) > 3.0)
            {
                this.ValidateResults.Add("长边大于短边的3倍 [("+ max+"/"+min+") > 3.0 ]，"+this.rule);
            }
            else
            {
                this.CorrectResults.Add("长边小于等于短边的3倍" + this.rule);
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            if (columnSectionModel.IsNonAntiseismic)
            {
                return steps;
            }
            steps.Add("类别：长短边比值（截面）");
            steps.Add("条目编号：12， 强制性：宜，适用构件：KZ、ZHZ");
            steps.Add("适用功能：智能识图、图纸校核，条文编号：砼规 11.4.11-3，条文页数：175");
            steps.Add("条文：长边小于等于短边的3倍");
            steps.Add("柱号 = " + this.columnSectionModel.Text);
            steps.Add("if (Math.Max(B[" + columnSectionModel.Cdm.B + "],H[" + columnSectionModel.Cdm.H + "]) / Math.Min(B[" +
                columnSectionModel.Cdm.B + "],H[" + columnSectionModel.Cdm.H + "]) > 3 )");
            steps.Add("  {");
            steps.Add("     Err:长边大于短边的3倍 （《砼规》11.4.11-3）");
            steps.Add("  }");
            steps.Add("else");
            steps.Add("  {");
            steps.Add("     Debugprint:长边小于等于短边的3倍 （《砼规》11.4.11-3）");
            steps.Add("  }");
            steps.Add("");
            return steps;
        }
    }
}
