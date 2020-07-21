
using System.Collections.Generic;

namespace ThColumnInfo.Validate
{
    public class AngularReinforcementNumModel : ValidateModel
    {
        public int AngularReinforcementNum { get; set; }
        public override bool ValidateProperty()
        {
            if (!base.ValidateProperty() ||
               !IsContainsCodeSign(new List<string> { "LZ", "KZ", "ZHZ" }))
            {
                return false;
            }
            if (AngularReinforcementNum<=0)
            {
                return false;
            }
            return true;
        }
    }
}
