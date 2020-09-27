using System.Collections.Generic;

namespace ThColumnInfo.Validate.Model
{
    public class AngularReinforcementDiaModel: ValidateModel
    {
        public double AngularReinforcementDia { get; set; }
        public double AngularReinforcementDiaLimited { get; set; }
        public bool IsCornerColumn { get; set; }

        public override bool ValidateProperty()
        {
            if (!base.ValidateProperty() ||
               !IsContainsCodeSign(new List<string> { "KZ", "LZ", "ZHZ" }))
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
