using System.Collections.Generic;

namespace ThColumnInfo.Validate.Model
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
            if (!base.ValidateProperty() ||
               !IsContainsCodeSign(new List<string> {"KZ", "ZHZ" }))
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
