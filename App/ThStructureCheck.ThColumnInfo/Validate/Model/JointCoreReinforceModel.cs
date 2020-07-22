using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Validate.Model
{
    public class JointCoreReinforceModel : ValidateModel
    {
        public double CoreJointReinforceArea { get; set; }
        public override bool ValidateProperty()
        {
            if (!base.ValidateProperty() ||
               !IsContainsCodeSign(new List<string> { "KZ", "LZ", "ZHZ" }))
            {
                return false;
            }
            return true;
        }
    }
}
