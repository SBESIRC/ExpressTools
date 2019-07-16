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

        public ThAreaFrameDriver()
        {
            engines = new List<ThAreaFrameEngine>();
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
                }
                driver.engines.RemoveAll(e => e == null);

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
                return OrdinaryStoreys().Select(o => o.number).Union(OrdinaryAOccupancyStoreys().Select(o => o.number)).ToList();
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
    }
}
