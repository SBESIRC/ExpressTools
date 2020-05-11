using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Validate
{
    public class StirrupMaximumSpacingFModel : ValidateModel
    {
        public ColumnDataModel Cdm { get; set; }
        /// <summary>
        /// 箍筋间距限值
        /// </summary>
        public double IntStirrupSpacingLimited
        {
            get
            { return GetIntStirrupSpacingLimited();
            }
        }
        /// <summary>
        /// 保护层厚度
        /// </summary>
        public double ProtectThickness { get; set; }
        public string AntiSeismicGrade { get; set; }
        public bool IsFirstFloor { get; set; }

        public override bool ValidateProperty()
        {
            if (this.Code.Contains("KZ") || this.Code.Contains("ZHZ"))
            {
                return true;
            }
            return false;
        }
        private double GetIntStirrupSpacingLimited()
        {
            string antiSeismicGrade = this.AntiSeismicGrade;
            if(this.Code.ToUpper().Contains("ZHZ"))
            {
                antiSeismicGrade = "一级";
            }
            //纵向钢筋直径最小值
            double intBardiamin = Math.Min(this.Cdm.IntXBarDia, this.Cdm.IntYBarDia);
            intBardiamin = Math.Min(intBardiamin, this.Cdm.IntCBarDia);

            //箍筋间距限值
            double stirrupSpaceingLimited = ThValidate.GetStirrupMaximumDiameter(
                antiSeismicGrade,
                this.IsFirstFloor, intBardiamin);
            return stirrupSpaceingLimited;
        }
    }
}
