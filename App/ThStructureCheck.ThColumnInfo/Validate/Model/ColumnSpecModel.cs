namespace ThColumnInfo.Validate
{
    public class ColumnSpecModel: ValidateModel
    {
        public double B { get; set; }
        public double H { get; set; }
        public ColumnDataModel Cdm { get; set; }

        public override bool ValidateProperty()
        {
            return true;
        }
    }
}
