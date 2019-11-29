using System;
using System.Collections.Generic;

namespace ThColumnInfo.Validate
{
    public class VerDirIronClearSpaceRule : IRule
    {
        private VerDirIronClearSpaceModel ruleModel;
        public VerDirIronClearSpaceRule(VerDirIronClearSpaceModel ruleModel)
        {
            this.ruleModel = ruleModel;
        }
        public List<ValidateResult> ValidateResults { get; set; } = new List<ValidateResult>();

        public void Validate()
        {
            if(ruleModel==null)
            {
                return;
            }
            double bDirIronClearSpace=(ruleModel.B-2*(ruleModel.ProtectLayerThickness+ ruleModel.IntCBarDia+ ruleModel.IntStirrupDia)-
                ruleModel.IntXBarCount*ruleModel.IntXBarDia)/ (ruleModel.IntXBarCount + 1);

            double hDirIronClearSpace = (ruleModel.H - 2 * (ruleModel.ProtectLayerThickness + ruleModel.IntCBarDia + ruleModel.IntStirrupDia) -
               ruleModel.IntYBarCount * ruleModel.IntYBarDia) / (ruleModel.IntYBarCount + 1);

            double minValue = Math.Min(bDirIronClearSpace, hDirIronClearSpace);
            if(minValue<=50)    
            {
                ValidateResults.Add(ValidateResult.VerDirIronClearSpaceNotEnough);
            }
            double maxValue= Math.Max(bDirIronClearSpace, hDirIronClearSpace);
            if (maxValue>300)
            {
                ValidateResults.Add(ValidateResult.VerDirIronClearSpaceNotEnoughTooLarge);
            }
        }
    }
}
