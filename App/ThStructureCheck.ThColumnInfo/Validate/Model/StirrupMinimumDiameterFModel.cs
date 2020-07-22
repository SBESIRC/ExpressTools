using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Validate.Model
{
    public class StirrupMinimumDiameterFModel:ValidateModel
    {
        /// <summary>
        /// 箍筋直径
        /// </summary>
        public double IntStirrupDia { get; set; }
        public override bool ValidateProperty()
        {
            if (!base.ValidateProperty() ||
               !IsContainsCodeSign(new List<string> {"ZHZ" }))
            {
                return false;
            }
            return false;
        }
    }
}
