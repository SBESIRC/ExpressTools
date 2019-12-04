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
    }
}
