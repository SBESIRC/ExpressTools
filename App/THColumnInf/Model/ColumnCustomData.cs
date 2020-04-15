using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo
{
    public class ColumnCustomData :CNotifyPropertyChange
    {
        private string antiSeismicGrade = "一级";    //抗震等级
        private double protectLayerThickness=25; //保护层厚度
        private string concreteStrength = "C30"; //混凝土强度
        private int hoopReinforceEnlargeTimes=1; //箍筋放大倍数
        private int longitudinalReinforceEnlargeTimes = 1; //纵筋放大倍数

        private string hoopReinforceFullHeightEncryption= "否"; //箍筋全高加密 -> "是"、“否”
        private string cornerColumn = "否"; //角柱 -> "是"、“空”

        /// <summary>
        /// 抗震等级
        /// </summary>
        public string AntiSeismicGrade
        {
            get
            {
               return antiSeismicGrade;
            }
            set
            {
                antiSeismicGrade = value;
                NotifyPropertyChange("AntiSeismicGrade");
            }
        }
        /// <summary>
        /// 保护层厚度
        /// </summary>
        public double ProtectLayerThickness
        {
            get
            {
                return protectLayerThickness;
            }
            set
            {
                protectLayerThickness = value;
                NotifyPropertyChange("ProtectLayerThickness");
            }
        }
        /// <summary>
        /// 混凝土强度
        /// </summary>
        public string ConcreteStrength
        {
            get
            {
                return concreteStrength;
            }
            set
            {
                concreteStrength = value;
                NotifyPropertyChange("ConcreteStrength");
            }
        }
        /// <summary>
        /// 箍筋放大倍数
        /// </summary>
        public int HoopReinforcementEnlargeTimes
        {
            get
            {
                return hoopReinforceEnlargeTimes;
            }
            set
            {
                hoopReinforceEnlargeTimes = value;
                NotifyPropertyChange("HoopReinforcementEnlargeTimes");
            }
        }

        /// <summary>
        /// 纵筋放大倍数
        /// </summary>
        public int LongitudinalReinforceEnlargeTimes
        {
            get
            {
                return longitudinalReinforceEnlargeTimes;
            }
            set
            {
                longitudinalReinforceEnlargeTimes = value;
                NotifyPropertyChange("LongitudinalReinforceEnlargeTimes");
            }
        }
        /// <summary>
        /// 箍筋全高度加密
        /// </summary>
        public string HoopReinforceFullHeightEncryption
        {
            get
            {
                return hoopReinforceFullHeightEncryption;
            }
            set
            {
                hoopReinforceFullHeightEncryption = value;
                NotifyPropertyChange("HoopReinforceFullHeightEncryption");
            }
        }
        /// <summary>
        /// 是否角柱
        /// </summary>
        public string CornerColumn
        {
            get
            {
                return cornerColumn;
            }
            set
            {
                cornerColumn = value;
                NotifyPropertyChange("CornerColumn");
            }
        }

        /// <summary>
        /// 抗震等级颜色值
        /// </summary>
        public short AntiSeismicGradeColorIndex { get; set; } = 0;
        /// <summary>
        /// 混凝土强度颜色值
        /// </summary>
        public short ConcreteStrengthColorIndex { get; set; } = 0;
        /// <summary>
        /// 保护层厚度颜色值
        /// </summary>
        public short ProtectThickColorIndex { get; set; } = 0;
        /// <summary>
        /// 箍筋放大倍数颜色值
        /// </summary>
        public short HoopEnlargeTimesColorIndex { get; set; } = 0;
        /// <summary>
        /// 纵筋放大倍数颜色值
        /// </summary>
        public short LongitudinalEnlargeTimesColorIndex { get; set; } = 0;
        /// <summary>
        /// 箍筋全高度加密颜色值
        /// </summary>
        public short HoopFullHeightEncryptionColorIndex { get; set; } = 0;
        /// <summary>
        /// 是否角柱颜色值
        /// </summary>
        public short CornerColumnColorIndex { get; set; } = 0;
    }
}
