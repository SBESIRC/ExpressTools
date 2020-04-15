using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Validate
{
    public class StirrupMinimumDiameterDModel:ValidateModel
    {
        /// <summary>
        /// 箍筋直径
        /// </summary>
        public double IntStirrupDia { get; set; }

        /// <summary>
        /// 箍筋直径限值
        /// </summary>
        public double IntStirrupDiaLimited { get; set; }
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
