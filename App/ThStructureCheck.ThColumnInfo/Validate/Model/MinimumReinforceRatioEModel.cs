using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Validate.Model
{
    public class MinimumReinforceRatioEModel : ValidateModel
    {
        public double Jkb
        {
            get;
            set;
        }
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
