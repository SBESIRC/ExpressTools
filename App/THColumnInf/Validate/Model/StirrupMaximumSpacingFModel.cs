using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Validate
{
    public class StirrupMaximumSpacingFModel : ValidateModel
    {
        /// <summary>
        /// 箍筋间距
        /// </summary>
        public double IntStirrupSpacing { get; set; }
        /// <summary>
        /// 箍筋间距限值
        /// </summary>
        public double IntStirrupSpacingLimited { get; set; }
        public override bool ValidateProperty()
        {
            if (this.Code.Contains("KZ") || this.Code.Contains("ZHZ"))
            {
                return true;
            }
            return false;
        }
    }
}
