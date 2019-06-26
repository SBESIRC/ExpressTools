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
            string[] dwgs = Directory.GetFiles(Path.Combine(Active.DocumentDirectory, @"建筑单体"), 
                                                "*.dwg", 
                                                SearchOption.TopDirectoryOnly);
            foreach(string dwg in dwgs)
            {
                driver.engines.Add(ThAreaFrameEngine.ResidentialEngine(dwg));
            }

            // 按建造编号排序
            driver.engines.Sort();

            return driver;
        }

        // Dispose()函数
        public void Dispose()
        {
            foreach(ThAreaFrameEngine engine in engines)
            {
                engine.Dispose();
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
                count = Math.Max(count, engine.Building.StandardStoreys().Count);
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
    }
}
