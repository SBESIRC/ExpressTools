using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AcHelper;
using AcHelper.Wrappers;

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
                return OrdinaryAOccupancyStoreys().Select(o => o.number).Union(OrdinaryStoreys().Select(o => o.number)).OrderBy(i => i).ToList();
            }
        }

        public List<int> UnderGroundStoreyCollection
        {
            get
            {
                return UnderGroundStoreys().Select(o => o.number).Union(UnderGroundAOccupancyStoreys().Select(o => o.number)).OrderBy(i => i).ToList();
            }
        }

        // 普通楼层
        public List<ResidentialStorey> OrdinaryStoreys()
        {
            List<ResidentialStorey> storeys = new List<ResidentialStorey>();
            foreach(ThAreaFrameEngine engine in engines)
            {
                storeys = storeys.Union(engine.Building.OrdinaryStoreys()).ToList();
            }
            return storeys;
        }

        // 标准楼层
        public int StandardStoreyCount()
        {
            int count = 0;
            foreach (ThAreaFrameEngine engine in engines)
            {
                count = Math.Max(count, engine.StandardStoreyCount);
            }
            return count;
        }

        // 地下楼层
        public List<ResidentialStorey> UnderGroundStoreys()
        {
            List<ResidentialStorey> storeys = new List<ResidentialStorey>();
            foreach (ThAreaFrameEngine engine in engines)
            {
                storeys = storeys.Union(engine.Building.UnderGroundStoreys()).ToList();
            }
            return storeys;
        }

        // 公建普通楼层
        public List<AOccupancyStorey> OrdinaryAOccupancyStoreys()
        {
            List<AOccupancyStorey> storeys = new List<AOccupancyStorey>();
            foreach (ThAreaFrameEngine engine in engines)
            {
                storeys = storeys.Union(engine.AOccupancyBuilding.OrdinaryStoreys()).ToList();
            }
            return storeys;
        }

        // 公建标准楼层
        public int StandardAOccupancyStoreyCount()
        {
            int count = 0;
            foreach (ThAreaFrameEngine engine in engines)
            {
                count = Math.Max(count, engine.AOccupancyBuilding.StandardStoreys().Count);
            }
            return count;
        }

        // 公建地下楼层
        public List<AOccupancyStorey> UnderGroundAOccupancyStoreys()
        {
            List<AOccupancyStorey> storeys = new List<AOccupancyStorey>();
            foreach (ThAreaFrameEngine engine in engines)
            {
                storeys = storeys.Union(engine.AOccupancyBuilding.UnderGroundStoreys()).ToList();
            }
            return storeys;
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
            double area = 0.0;
            foreach (ThAreaFrameEngine engine in engines)
            {
                area += engine.AreaOfStilt();
            }
            return area;
        }

        // "地下室主楼建筑面积"
        //  公式：∑(∑(x层住宅建筑面积))+∑(∑(x层公建建筑面积))
        public double AreaOfUnderGround()
        {
            double area = 0.0;
            foreach (ThAreaFrameEngine engine in engines)
            {
                area += engine.AreaOfUnderGround();
            }
            return area;
        }

        // "建筑基底面积"
        public double AreaOfFoundation()
        {
            double area = 0.0;
            foreach (ThAreaFrameEngine engine in engines)
            {
                area += engine.AreaOfFoundation();
            }
            return area;
        }

        // 屋顶绿化
        public double AreaOfRoofGreenSpace()
        {
            double area = 0.0;
            foreach (ThAreaFrameEngine engine in engines)
            {
                area += engine.AreaOfRoofGreenSpace();
            }
            return area;
        }

        // 室内停车场面积
        public double AreaOfParkingGarage()
        {
            double area = 0.0;
            foreach (ThAreaFrameParkingGarageEngine engine in parkingGarageEngines)
            {
                area += engine.AreaOfParkingGarage();
            }
            return area;
        }

        // 地下停车位个数
        public int CountOfUnderGroundParkingSlot()
        {
            int count = 0;
            foreach (ThAreaFrameParkingGarageEngine engine in parkingGarageEngines)
            {
                count += engine.CountOfUnderGroundParkingSlot();
            }
            return count;
        }
    }
}
