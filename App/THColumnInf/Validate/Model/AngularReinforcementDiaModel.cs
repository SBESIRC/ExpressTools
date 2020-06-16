namespace ThColumnInfo.Validate
{
    public class AngularReinforcementDiaModel: ValidateModel
    {
        public double AngularReinforcementDia { get; set; }
        public double AngularReinforcementDiaLimited { get; set; }
        public bool IsCornerColumn { get; set; }

        public override bool ValidateProperty()
        {
            if (!(this.Code.Contains("KZ") || this.Code.Contains("ZHZ") || this.Code.Contains("LZ")))
            {
                return false;
            }
            if (!this.IsCornerColumn)
            {
                //不是角柱，不需要验证
                return false;
            }
            if (this.AngularReinforcementDia <= 0.0 ||
                this.AngularReinforcementDiaLimited <= 0.0)
            {
                return false;
            }
            return true;
        }
    }
}
