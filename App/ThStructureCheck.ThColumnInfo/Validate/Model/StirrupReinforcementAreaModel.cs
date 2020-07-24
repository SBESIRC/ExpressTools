using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Validate.Model
{
    public class StirrupReinforcementAreaModel : ValidateModel
    {
        /// <summary>
        /// 配筋面积限值
        /// </summary>
        public double DblStirrupAsCal { get; set; }
        /// <summary>
        /// 配筋面积限值
        /// </summary>
        public double DblStirrupAsCal0 { get; set; }
        /// <summary>
        /// 假定箍筋间距
        /// </summary>
        public double IntStirrupSpacingCal { get; set; }

        public override bool ValidateProperty()
        {
            if (!base.ValidateProperty() ||
               !IsContainsCodeSign(new List<string> { "LZ", "KZ", "ZHZ" })
               || Cdm == null)
            {
                return false;
            }
            if (this.IntStirrupSpacingCal == 0.0)
            {
                return false;
            }
            return true;
        }
    }
}
