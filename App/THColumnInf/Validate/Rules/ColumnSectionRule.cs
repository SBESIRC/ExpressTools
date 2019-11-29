using System;
using System.Collections.Generic;

namespace ThColumnInfo.Validate
{
    /// <summary>
    /// 截面
    /// </summary>
    public class ColumnSectionRule:IRule
    {
        private ColumnSectionModel columnSectionModel;

        public List<ValidateResult> ValidateResults { get; set; } = new List<ValidateResult>();

        public ColumnSectionRule(ColumnSectionModel columnSectionModel)
        {
            this.columnSectionModel = columnSectionModel;
        }        
        public void Validate()
        {
            if (columnSectionModel == null)
            {
                return;
            }
            SectionTooSmall();
            LongLessThanShortTriple();
        }
        private void SectionTooSmall()
        {
            if (columnSectionModel.AntiSeismicGrade >= 4)
            {
                if (columnSectionModel.FloorTotalNums <= 2)
                {
                    if (Math.Min(columnSectionModel.B, columnSectionModel.H) < 300)
                    {
                        this.ValidateResults.Add( ValidateResult.SectionTooSmall);
                    }
                }
                else
                {
                    if (Math.Min(columnSectionModel.B, columnSectionModel.H) < 400)
                    {
                        this.ValidateResults.Add(ValidateResult.SectionTooSmall);
                    }
                }
            }
        }
        private void LongLessThanShortTriple()
        {
            if(columnSectionModel.B <= 0 || columnSectionModel.H <= 0)
            {
                return;
            }
            if(columnSectionModel.AntiSeismicGrade> 4)
            {
                if(Math.Max(columnSectionModel.B, columnSectionModel.H) /
                    Math.Min(columnSectionModel.B, columnSectionModel.H) >3.0)
                {
                    this.ValidateResults.Add(ValidateResult.LongLessThanShortTriple);
                }
            }
        }
    }
}
