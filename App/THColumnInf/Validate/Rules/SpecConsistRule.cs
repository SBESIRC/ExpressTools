using System;
using System.Collections.Generic;

namespace ThColumnInfo.Validate
{
    /// <summary>
    /// 截面
    /// </summary>
    public class SpecConsistRule : IRule
    {
        private ColumnSpecModel columnSpecModel;

        public List<string> ValidateResults { get; set; } = new List<string>();
        public List<string> CorrectResults { get; set; } = new List<string>();
        private string rule = "";
        public SpecConsistRule(ColumnSpecModel columnSpecModel)
        {
            this.columnSpecModel = columnSpecModel;
        }        
        public void Validate()
        {
            if (columnSpecModel == null || !columnSpecModel.ValidateProperty())
            {
                return;
            }
            if(columnSpecModel.Cdm.B== columnSpecModel.B && 
                columnSpecModel.Cdm.H== columnSpecModel.H)
            {
                this.CorrectResults.Add("截面尺寸与计算书一致" + this.rule);
            }
            else
            {
                this.ValidateResults.Add("截面尺寸与计算书不一致" + this.rule);
            }
        }
        public List<string> GetCalculationSteps()
        {
            List<string> steps = new List<string>();
            steps.Add("类别：规格一致（截面）");
            steps.Add("条目编号：， 强制性：，适用构件：");
            steps.Add("适用功能：");
            steps.Add("条文：");
            steps.Add("柱号 = " + this.columnSpecModel.Text);
            steps.Add("if (LenX[" + this.columnSpecModel.Cdm.B + "] == B[" + this.columnSpecModel.B + "] && " +
                "LenY[" + this.columnSpecModel.Cdm.H + "] == H[" + this.columnSpecModel.H + "]");
            steps.Add("   {");
            steps.Add("      Debugprint:截面尺寸与计算书一致 " + this.rule);
            steps.Add("   }");
            steps.Add("else");
            steps.Add("   {");
            steps.Add("      Err:截面尺寸与计算书不一致 " + this.rule);
            steps.Add("   }");
            return steps;
        }
    }
}
