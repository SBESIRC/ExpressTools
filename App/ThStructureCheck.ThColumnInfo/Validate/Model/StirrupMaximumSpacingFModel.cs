using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Validate.Model
{
    public class StirrupMaximumSpacingFModel : ValidateModel
    {
        /// <summary>
        /// 箍筋间距限值
        /// </summary>
        public double IntStirrupSpacingLimited
        {
            get
            {
                return GetIntStirrupSpacingLimited();
            }
        }
        /// <summary>
        /// 保护层厚度
        /// </summary>
        public double ProtectThickness { get; set; }
        /// <summary>
        /// 是否是首层
        /// </summary>
        public bool IsFirstFloor { get; set; }
        /// <summary>
        /// 剪跨比
        /// </summary>
        public double Jkb { get; set; }

        public override bool ValidateProperty()
        {
            if (!base.ValidateProperty() ||
                !IsContainsCodeSign(new List<string> {"ZHZ","KZ","LZ" }))
            {
                return false;
            }
            return true;
        }
        private double GetIntStirrupSpacingLimited()
        {
            //纵向钢筋直径最小值
            double intBardiamin = Math.Min(this.Cdm.IntXBarDia, this.Cdm.IntYBarDia);
            intBardiamin = Math.Min(intBardiamin, this.Cdm.IntCBarDia);

            //箍筋间距限值
            double stirrupSpaceingLimited = ThValidate.GetStirrupMaximumDiameter(
                this.AntiSeismicGrade,
                this.IsFirstFloor, intBardiamin);
            return stirrupSpaceingLimited;
        }
    }
}
