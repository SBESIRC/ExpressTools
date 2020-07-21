using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Validate
{
    public class StirrupMinimumDiameterEModel:ValidateModel
    {
        /// <summary>
        /// 箍筋直径限值
        /// </summary>
        public double IntStirrupDiaLimited
        {
            get
            {
                return GetIntStirrupDiaLimited();
            }
        }
        /// <summary>
        /// 是否是首层(柱根)
        /// </summary>
        public bool IsFirstFloor { get; set; }
        public override bool ValidateProperty()
        {
            if (!base.ValidateProperty() ||
               !IsContainsCodeSign(new List<string> {"ZHZ" }))
            {
                return false;
            }
            return false;
        }
        private double GetIntStirrupDiaLimited()
        {
            return ThValidate.GetStirrupMinimumDiameter(
                this.AntiSeismicGrade,
                this.IsFirstFloor);
        }
    }
}
