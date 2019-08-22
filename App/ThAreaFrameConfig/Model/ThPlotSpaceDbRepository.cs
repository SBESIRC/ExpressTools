using AcHelper;
using Linq2Acad;
using System;
using System.Linq;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;


namespace ThAreaFrameConfig.Model
{
    public class ThPlotSpaceDbRepository : IThPlotSpaceRepository
    {
        private List<ThPlotSpace> spaces;
        private readonly Database database;

        public List<ThPlotSpace> Spaces
        {
            get
            {
                return spaces;
            }
        }

        public ThPlotSpaceDbRepository()
        {
            database = Active.Database;
            ConstructPlotSpaces();
        }

        public ThPlotSpaceDbRepository(Database db)
        {
            database = db;
            ConstructPlotSpaces();
        }

        public void AppendDefaultPlotSpace()
        {
            spaces.Add(new ThPlotSpace()
            {
                ID = Guid.NewGuid(),
                Number = spaces.Count + 1,
                Frame = ObjectId.Null.OldIdPtr,
                HouseHold = 0
            });
        }

        private void ConstructPlotSpaces()
        {
            spaces = new List<ThPlotSpace>();
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var names = new List<string>();
                acadDatabase.Layers.ForEachDbObject(l => names.Add(l.Name));
                foreach (string name in names.Where(n => n.StartsWith(@"用地_规划净用地")))
                {
                    string[] tokens = name.Split('_');
                    foreach (ObjectId objId in database.AreaFrameLines(name))
                    {
                        spaces.Add(new ThPlotSpace()
                        {
                            ID = Guid.NewGuid(),
                            Number = spaces.Count + 1,
                            Frame = objId.OldIdPtr,
                            HouseHold = UInt16.Parse(tokens[2])
                        });
                    }
                }
            }
        }
    }
}
