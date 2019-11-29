using System;
using System.Collections.Generic;

namespace ThColumnInfo.Validate
{
    public class VerDirForceIronDiaRule : IRule
    {
        private VerDirForceIronModel verDirForceIronModel;
        public VerDirForceIronDiaRule(VerDirForceIronModel verDirForceIronModel)
        {
            this.verDirForceIronModel = verDirForceIronModel;
        }
        public List<ValidateResult> ValidateResults { get; set; } = new List<ValidateResult>();

        public void Validate()
        {
            if (verDirForceIronModel == null)
            {
                return;
            }
            double minValue = Math.Min(verDirForceIronModel.IntXBarDia, verDirForceIronModel.IntYBarDia);
            minValue = Math.Min(minValue, verDirForceIronModel.IntCBarDia);
            if(minValue<12)
            {
                ValidateResults.Add(ValidateResult.VerDirForceBarDiaLessThanTwelveMm);
            }
        }
    }
}
