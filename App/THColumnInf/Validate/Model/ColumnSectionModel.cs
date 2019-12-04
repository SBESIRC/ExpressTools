namespace ThColumnInfo.Validate
{
    public class ColumnSectionModel: ValidateModel
    {
        /// <summary>
        /// 抗震等级
        /// </summary>
        public int AntiSeismicGrade { get; set; }
        /// <summary>
        /// 楼层总层数
        /// </summary>
        public int FloorTotalNums { get; set; }

        /// <summary>
        /// 截面长度
        /// </summary>
        public double B;
        /// <summary>
        /// 截面宽度
        /// </summary>
        public double H;

        public override bool ValidateProperty()
        {
            if (AntiSeismicGrade<=0 || FloorTotalNums<=0 || B<=0 || H<=0)
            {
                return false;
            }
            return true;
        }
    }
}
