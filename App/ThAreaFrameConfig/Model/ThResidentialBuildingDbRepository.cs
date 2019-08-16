using AcHelper;
using Linq2Acad;
using System;
using System.Linq;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThAreaFrameConfig.Model
{
    internal class ThResidentialBuildingDbRepository
    {
        private readonly Database database;
        private ThResidentialBuilding building;
        public ThResidentialBuilding Building {
            get
            {
                return building;
            }
        }

        // 构造函数
        public ThResidentialBuildingDbRepository()
        {
            database = Active.Database;
            ConstructRepository();
        }

        public ThResidentialBuildingDbRepository(Database db)
        {
            database = db;
            ConstructRepository();
        }

        private void ConstructRepository()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var names = new List<string>();
                acadDatabase.Layers.ForEachDbObject(l => names.Add(l.Name));
                foreach(var name in names.Where(n => n.StartsWith(@"单体基底")))
                {
                    string[] tokens = name.Split('_');
                    building = new ThResidentialBuilding()
                    {
                        Number = tokens[1],
                        Name = tokens[2],
                        Category = tokens[3],
                        AboveGroundFloorNumber = tokens[4],
                        UnderGroundFloorNumber = tokens[5],
                        Layer = name,
                    };
                }
                if (building == null)
                {
                    building = new ThResidentialBuilding()
                    {
                        Number = "",
                        Name = "",
                        Category = "住宅",
                        AboveGroundFloorNumber = "",
                        UnderGroundFloorNumber = "",
                        Layer = "",
                    };
                }
            }
        }
    }
}
