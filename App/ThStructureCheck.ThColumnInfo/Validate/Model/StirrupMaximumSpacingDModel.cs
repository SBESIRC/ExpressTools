using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Validate.Model
{
    public class StirrupMaximumSpacingDModel:ValidateModel
    {
        public override bool ValidateProperty()
        {
            if (!base.ValidateProperty() ||
               !IsContainsCodeSign(new List<string> { "LZ", "KZ", "ZHZ" })
               || Cdm == null)
            {
                return false;
            }
            return true;
        }
    }
}
