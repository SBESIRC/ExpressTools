namespace ThColumnInfo.Validate
{
    public class MaximumReinforcementRatioModel : ValidateModel
    {
       public ColumnDataModel Cdm { get; set; }
        public override bool ValidateProperty()
        {
            if (!(this.Code.Contains("KZ") || this.Code.Contains("ZHZ") || this.Code.Contains("LZ")))
            {
                return false;
            }   
            return true;
        }
    }
}
