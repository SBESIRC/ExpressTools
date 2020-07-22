using System.Collections.Generic;

namespace ThColumnInfo.Validate.Model
{
    public class VerDirForceIronModel: ValidateModel
    {
        public override bool ValidateProperty()
        {
            if (!base.ValidateProperty() ||
               !IsContainsCodeSign(new List<string> { "LZ", "KZ", "ZHZ" }))
            {
                return false;
            }
            return true;
        }
    }
}
