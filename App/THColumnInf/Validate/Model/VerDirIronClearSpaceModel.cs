namespace ThColumnInfo.Validate
{
    public class VerDirIronClearSpaceModel: ValidateModel
    {
        /// <summary>
        /// 保护层厚度
        /// </summary>
        public double ProtectLayerThickness { get; set; }
        /// <summary>
        /// 箍筋直径
        /// </summary>
        public double IntStirrupDia { get; set; }
        /// <summary>
        /// 柱子长度
        /// </summary>
        public double B { get; set; }
        /// <summary>
        /// 柱子宽度
        /// </summary>
        public double H { get; set; }
        /// <summary>
        /// 角筋直径
        /// </summary>
        public double IntCBarDia { get; set; }
        public int IntXBarCount { get; set; }
        public double IntXBarDia { get; set; }
        public int IntYBarCount { get; set; }
        public double IntYBarDia { get; set; }
    }
}
