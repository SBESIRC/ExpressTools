using System.Collections.Generic;

namespace ThColumnInfo.Validate
{
    public class AngularReinforcementDiaRule : IRule
    {
        private AngularReinforcementDiaModel angularReinforcementDiaModel;
        public AngularReinforcementDiaRule(AngularReinforcementDiaModel angularReinforcementDiaModel)
        {
            this.angularReinforcementDiaModel = angularReinforcementDiaModel;
        }
        public List<ValidateResult> ValidateResults { get; set; } = new List<ValidateResult>();

        public void Validate()
        {
            if (angularReinforcementDiaModel == null)
            {
                return;
            }
            if (angularReinforcementDiaModel .AngularReinforcementDia<= 0.0 || 
                angularReinforcementDiaModel.AngularReinforcementDiaLimited <= 0.0)
            {
                return;
            }
            if(angularReinforcementDiaModel.AngularReinforcementDia <
                angularReinforcementDiaModel.AngularReinforcementDiaLimited)
            {
                ValidateResults.Add(ValidateResult.AngularReinforcementDiaIsNotEnough);
            }
        }
    }
}
