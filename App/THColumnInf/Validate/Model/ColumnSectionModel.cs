namespace ThColumnInfo.Validate
{
    public class ColumnSectionModel: ValidateModel
    {
        /// <summary>
        /// 抗震等级
        /// </summary>
        public string AntiSeismicGrade { get; set; }
        /// <summary>
        /// 楼层总层数
        /// </summary>
        public int FloorTotalNums { get; set; }

        public ColumnDataModel Cdm { get; set; }

        public override bool ValidateProperty()
        {
            if (!(this.Code.Contains("KZ") || this.Code.Contains("ZHZ")))
            {
                return false;
            }
            if (FloorTotalNums<=0 || Cdm==null)
            {
                return false;
            }            
            return true;
        }
    }
}
