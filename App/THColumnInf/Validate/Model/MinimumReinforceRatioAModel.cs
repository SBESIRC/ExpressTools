using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Validate
{
    public class MinimumReinforceRatioAModel : ValidateModel
    {
        /// <summary>
        /// 纵筋的钢筋级别
        /// </summary>
        public double P1 { get; set; }
        /// <summary>
        /// 混凝土强度
        /// <=C60:0
        /// >C60:0.1
        /// </summary>
        public double P2 { get; set; }
        /// <summary>
        /// 最小单侧配筋率限值
        /// </summary>
        public double Dblpsmin { get; set; } = 0.2;

        public ColumnDataModel Cdm { get; set; }
        public override bool ValidateProperty()
        {
            if (!(this.Code.Contains("KZ") || this.Code.Contains("ZHZ") || this.Code.Contains("LZ")))
            {
                return false;
            }
            return true;
        }
    }
}
