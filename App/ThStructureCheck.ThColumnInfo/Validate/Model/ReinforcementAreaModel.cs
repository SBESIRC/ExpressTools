using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Validate.Model
{
    public class ReinforcementAreaModel:ValidateModel
    {
        /// <summary>
        /// X向限值
        /// </summary>
        public double DblXAsCal { get; set; }
        /// <summary>
        /// Y向限值
        /// </summary>
        public double DblYAsCal { get; set; }
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
