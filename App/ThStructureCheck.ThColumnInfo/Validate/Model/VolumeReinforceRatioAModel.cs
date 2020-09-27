using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Validate.Model
{
    public class VolumeReinforceRatioAModel : ValidateModel
    {
        /// <summary>
        /// 体积配筋率限值
        /// </summary>
        public double VolumnReinforceRatioLimited
        {
            get
            {
                return GetVolumnReinforceRatioLimited();
            }
        }
        /// <summary>
        /// 保护层厚度
        /// </summary>
        public double ProtectLayerThickness { get; set; }
        public override bool ValidateProperty()
        {
            if (!base.ValidateProperty() ||
               !IsContainsCodeSign(new List<string> { "KZ", "ZHZ" })
               || Cdm == null)
            {
                return false;
            }
            if (Cdm.B * Cdm.H * Cdm.IntStirrupSpacing == 0.0)
            {
                return false;
            }
            return true;
        }
        private double GetVolumnReinforceRatioLimited()
        {
            double limitedValue = ThValidate.GetVolumeReinforcementRatioLimited(
                this.Code, this.AntiSeismicGrade);
            return limitedValue;
        }
    }
}
