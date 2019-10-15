using System;
using System.Linq;
using System.Collections.Generic;

namespace ThAreaFrame
{
    class ThAreaFrameDriver : IDisposable
    {
        public List<ThAreaFrameEngine> engines;
        public List<ThAreaFrameParkingGarageEngine> parkingGarageEngines;

        public ThAreaFrameDriver()
        {
            engines = new List<ThAreaFrameEngine>();
            parkingGarageEngines = new List<ThAreaFrameParkingGarageEngine>();
        }

        // Dispose()函数
        public void Dispose()
        {
            foreach(ThAreaFrameEngine engine in engines)
            {
                engine.Dispose();
            }

            foreach(ThAreaFrameParkingGarageEngine engine in parkingGarageEngines)
            {
                engine.Dispose();
            }
        }

        // 
        public List<int> OrdinaryStoreyCollection
        {
            get
            {
                var storeys = new List<int>();
                foreach (ThAreaFrameEngine engine in engines)
                {
                    storeys = storeys.Union(engine.OrdinaryStoreyCollection).ToList();
                }

                // 对于地上楼层，按照楼层数升序排列
                return storeys.OrderBy(o => o).ToList();
            }
        }

        public List<int> UnderGroundStoreyCollection
        {
            get
            {
                var storeys = new List<int>();
                foreach (ThAreaFrameEngine engine in engines)
                {
                    storeys = storeys.Union(engine.UnderGroundStoreyCollection).ToList();
                }

                // 对于地下楼层，按照楼层数降序排列
                return storeys.OrderBy(o => Math.Abs(o)).ToList();
            }
        }

        // 标准楼层
        public int StandardStoreyCount()
        {
            return engines.Max(o => o.StandardStoreyCollections.Count);
        }

        // 地上计容建筑面积（住宅）
        //  公式：∑(∑(住宅x层计容面积)+∑(∑(住宅楼梯间PL线面积x计容系数))
        public double ResidentAreaOfAboveGround()
        {
            double area = 0.0;
            foreach (ThAreaFrameEngine engine in engines)
            {
                area += engine.ResidentAreaOfAboveGround(true) + engine.ResidentAreaOfRoof(true);
            }
            return area;
        }

        // 地上计容建筑面积（商业）
        //  公式：∑(∑(公建x层计容面积)+∑(∑(公建楼梯间PL线面积x计容系数))
        public double AOccupancyAreaOfAboveGround()
        {
            double area = 0.0;
            foreach (ThAreaFrameEngine engine in engines)
            {
                area += engine.AOccupancyAreaOfAboveGround(true) + engine.AOccupancyAreaOfRoof(true);
            }
            return area;
        }

        // "架空部分建筑面积（非计容）"
        public double AreaOfStilt()
        {
            return engines.Sum(o => o.AreaOfStilt());
        }

        // "地下室主楼建筑面积"
        //  公式：∑(∑(x层住宅建筑面积))+∑(∑(x层公建建筑面积))
        public double AreaOfUnderGround()
        {
            return engines.Sum(o => o.AreaOfUnderGround());
        }

        // "建筑基底面积"
        public double AreaOfFoundation()
        {
            return engines.Sum(o => o.AreaOfFoundation());
        }

        // 屋顶绿化
        public double AreaOfRoofGreenSpace()
        {
            return engines.Sum(o => o.AreaOfRoofGreenSpace());
        }

        // 室内停车场面积
        public double AreaOfParkingGarage()
        {
            return parkingGarageEngines.Sum(o => o.AreaOfParkingGarage());
        }

        // 地下停车位个数
        public int CountOfUnderGroundParkingSlot()
        {
            return parkingGarageEngines.Sum(o => o.CountOfUnderGroundParkingSlot());
        }
    }
}
