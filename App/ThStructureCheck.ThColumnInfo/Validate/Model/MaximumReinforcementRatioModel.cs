using System.Collections.Generic;

namespace ThColumnInfo.Validate.Model
{
    public class MaximumReinforcementRatioModel : ValidateModel
    {
        public override bool ValidateProperty()
        {
            if(!base.ValidateProperty() || 
                !IsContainsCodeSign(new List<string> { "KZ" , "ZHZ" , "LZ" , "ZHZ" }))
            {
                return false;
            }
            if(Cdm==null)
            {
                return false;
            }
            return true;
        }
    }
}
