using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Linq2Acad;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThAreaFrame
{
    class ThAreaFrameParkingGarageEngine
    {
        private Database database;
        private List<string> names;
        public Database Database { get => database; set => database = value; }
        public List<string> Names { get => names; set => names = value; }

        // 构造函数 (current database)
        public static ThAreaFrameParkingGarageEngine Engine()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                return EngineInternal(acadDatabase);
            }
        }

        // 构造函数 (side database)
        public static ThAreaFrameParkingGarageEngine Engine(string fileName)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Open(fileName, DwgOpenMode.ReadOnly, true))
            {
                return EngineInternal(acadDatabase);
            }
        }

        // 构造函数 (side database)
        public static ThAreaFrameParkingGarageEngine Engine(Database database)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                return EngineInternal(acadDatabase);
            }
        }

        // 构造函数 (AcadDatabase wrapper)
        private static ThAreaFrameParkingGarageEngine EngineInternal(AcadDatabase acadDatabase)
        {
            var names = new List<string>();
            acadDatabase.Layers.ForEachDbObject(l => names.Add(l.Name));
            ThAreaFrameParkingGarageEngine engine = new ThAreaFrameParkingGarageEngine()
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

        // 室内停车场面积
        public double AreaOfParkingGarage()
        {
            double area = 0.0;
            foreach (string name in names.Where(n => n.StartsWith(@"附属公建_主体_室内停车库")))
            {
                area += ThAreaFrameDbUtils.SumOfArea(database, name);
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
                count += ThAreaFrameDbUtils.CountOfAreaFrames(database, name) * int.Parse(space.multiple);
            }
            return count;
        }
    }
}
