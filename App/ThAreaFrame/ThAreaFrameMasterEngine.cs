using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Linq2Acad;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThAreaFrame
{
    public class ThAreaFrameMasterEngine : IDisposable
    {
        private List<string> names;
        private IThAreaFrameDataSource dataSource;

        public static ThAreaFrameMasterEngine Engine(IThAreaFrameDataSource ds)
        {
            ThAreaFrameMasterEngine engine = new ThAreaFrameMasterEngine()
            {
                dataSource = ds,
                names = ds.Layers()
            };
            return engine;
        }

        // Dispose()函数
        public void Dispose()
        {
            //
        }

        public double AreaOfPlanning()
        {
            double area = 0.0;
            foreach(string name in names.Where(n => n.StartsWith(@"用地_规划净用地")))
            {
                area += dataSource.SumOfArea(name);
            }
            return area;
        }

        public double AreaOfGreenSpace()
        {
            double area = 0.0;
            foreach (string name in names.Where(n => n.StartsWith(@"用地_公共绿地")))
            {
                area += dataSource.SumOfArea(name);
            }
            return area;
        }

        public int CountOfAboveGroundParkingLot()
        {
            int count = 0;
            foreach(string name in names.Where(n => n.StartsWith(@"车场车位_室外车场_露天车场_小型汽车")))
            {
                // 车场层数
                var space = ThAreaFrameOutdoorParkingSpace.Space(name);
                count += dataSource.CountOfAreaFrames(name)* int.Parse(space.multiple);
            }
            return count;
        }

        public int CountOfHousehold()
        {
            foreach (string name in names.Where(n => n.StartsWith(@"用地_规划净用地")))
            {
                string[] tokens = name.Split('_');
                return int.Parse(tokens[2]);
            }
            return 0;
        }

        public int CountOfHouseholdPopulation()
        {
            return (int)(CountOfHousehold() * 3.2);
        }
    }
}
