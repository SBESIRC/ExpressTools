using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Validate.Model
{
    public class StirrupFullHeightEncryptionModel : ValidateModel
    {
        /// <summary>
        /// 剪跨比
        /// </summary>
        public double Jkb { get; set; }
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
