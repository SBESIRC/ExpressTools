namespace ThColumnInfo.Validate
{
    public class AxialCompressionRatioModel: ValidateModel
    {
        /// <summary>
        /// 轴压比
        /// </summary>
        public double AxialCompressionRatio { get; set; }
        /// <summary>
        /// 轴压比限值
        /// </summary>
        public double AxialCompressionRatioLimited { get; set; }

        public override bool ValidateProperty()
        {
            if (!(this.Code.Contains("KZ") || this.Code.Contains("ZHZ")))
            {
                return false;
            }
            if (AxialCompressionRatio <= 0 || AxialCompressionRatioLimited <= 0)
            {
                return false;
            }
            return true;
        }
    }
}
