using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Validate
{
    public class ShearSpanRatioModel:ValidateModel
    {
        public double ShearSpanRatio { get; set; }
        public override bool ValidateProperty()
        {
            if (!base.ValidateProperty() ||
               !IsContainsCodeSign(new List<string> {"KZ", "ZHZ" }))
            {
                return false;
            }
            return false;
        }
    }
}
