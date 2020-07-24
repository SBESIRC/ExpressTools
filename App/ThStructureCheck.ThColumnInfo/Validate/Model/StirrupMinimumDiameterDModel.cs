using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Validate.Model
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
        /// <summary>
        /// 剪跨比
        /// </summary>
        public double Jkb { get; set; }
        public override bool ValidateProperty()
        {
            if (!base.ValidateProperty() ||
               !IsContainsCodeSign(new List<string> { "KZ", "ZHZ" }))
            {
                return false;
            }
            return true;
        }
        private double GetIntStirrupDiaLimited()
        {
            return ThValidate.GetStirrupMinimumDiameter(
                this.AntiSeismicGrade,
                this.IsFirstFloor);
        }
    }
}
