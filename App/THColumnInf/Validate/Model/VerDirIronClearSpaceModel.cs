namespace ThColumnInfo.Validate
{
    public class VerDirIronClearSpaceModel : ValidateModel
    {
        /// <summary>
        /// 保护层厚度
        /// </summary>
        public double ProtectLayerThickness { get; set; }
        public ColumnDataModel Cdm { get; set; }
        public override bool ValidateProperty()
        {
            if (!(this.Code.Contains("KZ") || this.Code.Contains("ZHZ") || this.Code.Contains("LZ")))
            {
                return false;
            }
            if(Cdm==null)
            {
                return false;
            }
            return true;
        }
    }
}
