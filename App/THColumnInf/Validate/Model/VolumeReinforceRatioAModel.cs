using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Validate
{
    public class VolumeReinforceRatioAModel : ValidateModel
    {
        /// <summary>
        /// 体积配筋率限值
        /// </summary>
        public double VolumnReinforceRatioLimited { get; set; }
        /// <summary>
        /// 柱表数据信息
        /// </summary>

        public ColumnDataModel Cdm { get; set; } 
        /// <summary>
        /// 保护层厚度
        /// </summary>
        public double ProtectLayerThickness { get; set; }
        public override bool ValidateProperty()
        {
            if(!(this.Code.ToUpper().Contains("KZ")|| this.Code.ToUpper().Contains("ZHZ")))
            {
                return true;
            }
            if(Cdm==null)
            {
                return false;
            }
            if(Cdm.B* Cdm.H* Cdm.IntStirrupSpacing==0.0)
            {
                return false;
            }
            return true;
        }
    }
}
