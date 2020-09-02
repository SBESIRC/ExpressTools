using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TianHua.FanSelection.Model
{
    /// <summary>
    /// 楼梯间（前室送风）模型
    /// </summary>
    public class StaircaseAirModel : ThFanVolumeModel
    {
        /// <summary>
        /// 这是AK的值
        /// </summary>
        public double OverAk = 0.0;
        public int StairN1 = 0;
        public int AAAA = 25300, BBBB = 27500;
        public int CCCC = 27800, DDDD = 28100;

        public enum LoadHeight
        {
            LoadHeightLow = 0,
            LoadHeightMiddle = 1,
            LoadHeightHigh = 2
        }
        public enum StairLocation
        {
            OnGround = 0,
            UnderGound = 1,
        }
        public enum SpaceState
        {
            Residence = 0,
            Business = 1,
        }
        public StaircaseAirModel()
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
                OverAk = Ak;
                double V = 0.7;
                return Math.Round(Ak * V * StairN1 * 3600);
            }
        }

        /// <summary>
        /// 送风阀漏风
        /// </summary>
        public double LeakVolume
        {
            get
            {
                double a = 0.0, b = 0.0;
                int p = 6;
                double tempN2 = 0.0;
                double N2 = 0.0;
                int length = FrontRoomDoors.Count();
                for (int i = 0; i < length; i++)
                {
                    if (FrontRoomDoors[i].Type.ToString().Equals("单扇"))
                    {
                        a += (FrontRoomDoors[i].Width_Door_Q +
                            FrontRoomDoors[i].Height_Door_Q) * 2
                            * FrontRoomDoors[i].Crack_Door_Q / 1000 * FrontRoomDoors[i].Count_Door_Q;
                    }
                    else
                    {
                        b += ((FrontRoomDoors[i].Width_Door_Q +
                                FrontRoomDoors[i].Height_Door_Q) * 2
                                + FrontRoomDoors[i].Height_Door_Q)
                                * FrontRoomDoors[i].Crack_Door_Q / 1000 * FrontRoomDoors[i].Count_Door_Q;
                    }
                }
                FrontRoomDoors.ForEach(o => tempN2 += o.Count_Door_Q);
                N2 = ((Count_Floor - StairN1) * tempN2 > 0) ? ((Count_Floor - StairN1) * tempN2) : 0;
                return Math.Round(0.827 * (a + b) * Math.Sqrt(p) * 1.25 * N2 * 3600);
            }
        }
        /// <summary>
        /// 合计
        /// </summary>
        public double TotalVolume
        {
            get
            {
                return DoorOpeningVolume + LeakVolume;
            }
        }

        /// <summary>
        /// 系统负担高度
        /// </summary>
        public LoadHeight Load { get; set; }

        /// <summary>
        /// 楼梯间位置
        /// </summary>
        public StairLocation Stair { get; set; }

        /// <summary>
        /// 空间业态
        /// </summary>
        public SpaceState Type_Area { get; set; }

        /// <summary>
        /// 前室门
        /// </summary>
        public List<ThEvacuationDoor> FrontRoomDoors { get; set; }

        /// <summary>
        /// 系统楼层数
        /// </summary>
        public int Count_Floor { get; set; }

        /// <summary>
        /// 应用场景
        /// </summary>
        public override string FireScenario
        {
            get
            {
                return "楼梯间（前室送风）";
            }
        }

    }
}
