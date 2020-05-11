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
        public double IntStirrupDiaLimited
        {
            get
            {
                return GetIntStirrupDiaLimited();
            }
        }
        /// <summary>
        /// 抗震等级
        /// </summary>
        public string AntiSeismicGrade { get; set; }
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
            if (this.Code.Contains("KZ") || this.Code.Contains("ZHZ"))
            {
                return true;
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
