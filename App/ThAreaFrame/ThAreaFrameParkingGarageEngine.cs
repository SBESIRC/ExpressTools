using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Linq2Acad;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThAreaFrame
{
    public class ThAreaFrameParkingGarageEngine : IDisposable
    {
        private List<string> names;
        private IThAreaFrameDataSource dataSource;

        public static ThAreaFrameParkingGarageEngine Engine(IThAreaFrameDataSource ds)
        {
            ThAreaFrameParkingGarageEngine engine = new ThAreaFrameParkingGarageEngine()
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

        // 室内停车场面积
        public double AreaOfParkingGarage()
        {
            double area = 0.0;
            foreach (string name in names.Where(n => n.StartsWith(@"附属公建_主体_室内停车库")))
            {
                area += dataSource.SumOfArea(name);
            }
            return area;
        }

        // 地下停车位个数
        public int CountOfUnderGroundParkingSlot()
        {
            int count = 0;
            foreach (string name in names.Where(n => n.StartsWith(@"单体车位_小型汽车")))
            {
                // 车场层数
                var space = ThAreaFrameIndoorParkingSpace.Space(name);
                count += dataSource.CountOfAreaFrames(name) * int.Parse(space.multiple);
            }
            return count;
        }
    }
}
