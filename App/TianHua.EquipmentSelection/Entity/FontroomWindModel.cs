using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TianHua.FanSelection.Model
{
    /// <summary>
    /// 独立或合用前室（楼梯间送风）模型
    /// </summary>
    public class FontroomWindModel : ThFanVolumeModel
    {
        /// <summary>
        /// 这是AK的值
        /// </summary>
        public double OverAk = 0.0;
        public int AAAA = 24800, BBBB = 25800;
        public int CCCC = 26000, DDDD = 28100;
        public enum LoadHeight
        {
            LoadHeightLow = 0,
            LoadHeightMiddle = 1,
            LoadHeightHigh = 2
        }
        public FontroomWindModel()
        {
            FrontRoomDoors = new List<ThEvacuationDoor>();
        }

        /// <summary>
        /// 门开启风量
        /// </summary>
        public double DoorOpeningVolume
        {
            get
            {
                double Ak = 0.0;
                FrontRoomDoors.ForEach(o => Ak += o.Width_Door_Q * o.Height_Door_Q * o.Count_Door_Q);
                double V = 0.7;
                OverAk = Ak;
                return Math.Round(Ak * V * Math.Min(Count_Floor, 3) * 3600);
            }
        }

        /// <summary>
        /// 送风阀漏风
        /// </summary>
        public double LeakVolume
        {
            get
            {
                double Af = (double)Length_Valve * Width_Valve/1000000;
                int N3 = (Count_Floor - 3 > 0) ? (Count_Floor - 3) : 0;
                return Math.Round(0.083 * Af * N3 * 3600);
            }
        }
        /// <summary>
        /// 合计
        /// </summary>
        public double TotalVolume
        {
            get
            {
                return Math.Round(DoorOpeningVolume + LeakVolume);
            }
        }

        /// <summary>
        /// 系统负担高度
        /// </summary>
        public LoadHeight Load { get; set; }

        /// <summary>
        /// 系统楼层数
        /// </summary>
        public int Count_Floor { get; set; }

        /// <summary>
        /// 前室门
        /// </summary>
        public List<ThEvacuationDoor> FrontRoomDoors { get; set; }

        /// <summary>
        /// 送风阀截面长
        /// </summary>
        public int Length_Valve { get; set; }

        // <summary>
        /// 送风阀截面宽
        /// </summary>
        public int Width_Valve { get; set; }

        public override string FireScenario
        {
            get
            {
                return "独立或合用前室（楼梯间送风）";
            }
        }

    }
}
