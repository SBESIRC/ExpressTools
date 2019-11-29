using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo
{
    public class ParameterSetInfo: CNotifyPropertyChange
    {
        private int floorCount;
        private string antiSeismicGrade = "";
        private double protectLayerThickness;
        /// <summary>
        /// 楼层自然层数
        /// </summary>
        public int FloorCount
        {
            get
            {
                return floorCount;
            }
            set
            {
                floorCount = value;
                NotifyPropertyChange("FloorCount");
            }
        }
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
    }
}
