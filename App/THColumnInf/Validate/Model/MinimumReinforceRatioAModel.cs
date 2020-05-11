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
        public double P1
        {
            get
            {
                return GetP1();
            }
        }
        /// <summary>
        /// 混凝土强度
        /// <=C60:0
        /// >C60:0.1
        /// </summary>
        public double P2
        {
            get
            {
                return GetP2();
            }
        }
        /// <summary>
        /// 最小单侧配筋率限值
        /// </summary>
        public double Dblpsmin { get; set; } = 0.2;
        /// <summary>
        /// 混凝土强度
        /// </summary>
        public string ConcreteStrength { get; set; }
        public ColumnDataModel Cdm { get; set; }
        public override bool ValidateProperty()
        {
            if (!(this.Code.Contains("KZ") || this.Code.Contains("ZHZ") || this.Code.Contains("LZ")))
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// 获取P1值
        /// </summary>
        /// <returns></returns>
        private double GetP1()
        {
            return ThSpecificationValidate.paraSetInfo.
                GetLongitudinalReinforcementGrade(Cdm.Ctri.BEdgeSideMiddleReinforcement);
        }
        /// <summary>
        /// 获取P2值
        /// </summary>
        /// <returns></returns>
        private double GetP2()
        {
            double value = 0.0;
            if (!string.IsNullOrEmpty(this.ConcreteStrength))
            {
                List<double> values = ThColumnInfoUtils.GetDoubleValues(this.ConcreteStrength);
                if (values.Count > 0)
                {
                    if (values[0] >= 60)
                    {
                        value = 0.1;
                    }
                }
            }
            return value;
        }
    }
}
