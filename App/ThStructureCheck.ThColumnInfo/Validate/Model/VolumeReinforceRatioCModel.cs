using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Validate
{
    public class VolumeReinforceRatioCModel : ValidateModel
    {
        /// <summary>
        /// 体积配筋率限值
        /// </summary>
        public double VolumnReinforceRatioLimited { get; set; }
        /// <summary>
        /// 保护层厚度
        /// </summary>
        public double ProtectLayerThickness { get; set; }
        public override bool ValidateProperty()
        {
            if (!base.ValidateProperty() ||
                !IsContainsCodeSign(new List<string> { "LZ", "KZ", "ZHZ" })
                || Cdm == null)
            {
                return false;
            }
            return true;
        }
    }
}
