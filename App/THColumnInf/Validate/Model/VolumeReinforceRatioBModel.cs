using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Validate
{
    public class VolumeReinforceRatioBModel : ValidateModel
    {
        public double ShearSpanRatio { get; set; }
        /// <summary>
        /// 抗震等级
        /// </summary>
        public string Antiseismic { get; set; } = "";
        /// <summary>
        /// 柱表数据信息
        /// </summary>
        public ColumnDataModel Cdm { get; set; } 
        /// <summary>
        /// 设防烈度
        /// </summary>
        public double FortificationIntensity { get; set; }
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
