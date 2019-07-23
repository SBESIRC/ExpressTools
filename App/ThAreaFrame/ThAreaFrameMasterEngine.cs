using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Linq2Acad;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThAreaFrame
{
    class ThAreaFrameMasterEngine : IDisposable
    {
        private Database database;
        private List<string> names;
        public Database Database { get => database; set => database = value; }
        public List<string> Names { get => names; set => names = value; }

        // 构造函数 (current database)
        public static ThAreaFrameMasterEngine Engine()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                return EngineInternal(acadDatabase);
            }
        }

        // 构造函数 (side database)
        public static ThAreaFrameMasterEngine Engine(string fileName)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Open(fileName, DwgOpenMode.ReadOnly, true))
            {
                return EngineInternal(acadDatabase);
            }
        }

        // 构造函数 (AcadDatabase wrapper)
        private static ThAreaFrameMasterEngine EngineInternal(AcadDatabase acadDatabase)
        {
            var names = new List<string>();
            acadDatabase.Layers.ForEachDbObject(l => names.Add(l.Name));
            ThAreaFrameMasterEngine engine = new ThAreaFrameMasterEngine()
            {
                names = names,
                database = acadDatabase.Database,
            };
            return engine;
        }

        // Dispose()函数
        public void Dispose()
        {
            if (!database.IsDisposed)
            {
                database.Dispose();
            }
        }

        public double AreaOfPlanning()
        {
            double area = 0.0;
            foreach(string name in names.Where(n => n.StartsWith(@"用地_规划净用地")))
            {
                area += ThAreaFrameDbUtils.SumOfArea(database, name);
            }
            return area;
        }

        public double AreaOfGreenSpace()
        {
            double area = 0.0;
            foreach (string name in names.Where(n => n.StartsWith(@"用地_公共绿地")))
            {
                area += ThAreaFrameDbUtils.SumOfArea(database, name);
            }
            return area;
        }

        public int CountOfAboveGroundParkingLot()
        {
            int count = 0;
            foreach(string name in names.Where(n => n.StartsWith(@"车场车位_室外车位_露天车场_小型汽车")))
            {
                count += ThAreaFrameDbUtils.CountOfAreaFrames(database, name);
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
