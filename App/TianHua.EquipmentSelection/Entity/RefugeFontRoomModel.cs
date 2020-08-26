using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TianHua.FanSelection.Model
{
    /// <summary>
    /// 避难走道前室模型
    /// </summary>
    public class RefugeFontRoomModel :ThFanVolumeModel
    {
        public RefugeFontRoomModel()
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
                int V = 1;
                return Math.Round(Ak * V * 3600);
            }
        }
        /// <summary>
        /// 前室门
        /// </summary>
        public List<ThEvacuationDoor> FrontRoomDoors { get; set; }

        /// <summary>
        /// 前室门宽
        /// </summary>
        public double Width_Door_Q { get; set; }

        /// <summary>
        /// 前室门高
        /// </summary>
        public double Height_Door_Q { get; set; }

        /// <summary>
        /// 前室门数量
        /// </summary>
        public int Count_Door_Q { get; set; }

        /// <summary>
        /// 应用场景
        /// </summary>
        public override string FireScenario
        {
            get
            {
                return "避难走道前室";
            }
        }

    }
}
