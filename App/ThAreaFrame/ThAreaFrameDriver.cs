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

        public static ThAreaFrameDriver ResidentialDriver()
        {
            ThAreaFrameDriver driver = new ThAreaFrameDriver();
            try
            {
                string[] dwgs = Directory.GetFiles(Path.Combine(Active.DocumentDirectory, @"建筑单体"),
                                    "*.dwg",
                                    SearchOption.TopDirectoryOnly);
                foreach (string dwg in dwgs)
                {
                    driver.engines.Add(ThAreaFrameEngine.Engine(dwg));
                    driver.parkingGarageEngines.Add(ThAreaFrameParkingGarageEngine.Engine(dwg));
                }
                driver.engines.RemoveAll(e => e == null);
                driver.parkingGarageEngines.RemoveAll(e => e == null);

                // 按建造编号排序
                driver.engines.Sort();

                return driver;
            }
            catch
            {
                return driver;
            }
        }

        // Dispose()函数
        public void Dispose()
        {
            foreach(ThAreaFrameEngine engine in engines)
            {
                engine.Dispose();
            }
        }

        // 
        public List<int> OrdinaryStoreyCollection
        {
            get
            {
                return OrdinaryAOccupancyStoreys().Select(o => o.number).Union(OrdinaryStoreys().Select(o => o.number)).ToList();
            }
        }

        public List<int> UnderGroundStoreyCollection
        {
            get
            {
                return UnderGroundStoreys().Select(o => o.number).Union(UnderGroundAOccupancyStoreys().Select(o => o.number)).ToList();
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
        public double ResidentAreaOfAboveGround()
        {
            double area = 0.0;
            foreach (ThAreaFrameEngine engine in engines)
            {
                area += engine.ResidentAreaOfAboveGround(engine.AreaOfRoof(false));
            }
            return area;
        }

        // 地上计容建筑面积（商业）
        public double AOccupancyAreaOfAboveGround()
        {
            double area = 0.0;
            foreach (ThAreaFrameEngine engine in engines)
            {
                area += engine.AOccupancyAreaOfAboveGround(engine.AreaOfRoof(false));
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
