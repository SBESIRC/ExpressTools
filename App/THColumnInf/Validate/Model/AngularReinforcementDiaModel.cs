namespace ThColumnInfo.Validate
{
    public class AngularReinforcementDiaModel: ValidateModel
    {
        public double AngularReinforcementDia { get; set; }
        public double AngularReinforcementDiaLimited { get; set; }

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
