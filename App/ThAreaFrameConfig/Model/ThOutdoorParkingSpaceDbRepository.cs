using AcHelper;
using Linq2Acad;
using System;
using System.Linq;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThAreaFrameConfig.Model
{
    public class ThOutdoorParkingSpaceDbRepository : IThOutdoorParkingSpaceRepository
    {
        private readonly Database database;
        private List<ThOutdoorParkingSpace> spaces;

        public List<ThOutdoorParkingSpace> Spaces
        {
            get
            {
                return spaces;
            }
        }

        public ThOutdoorParkingSpaceDbRepository()
        {
            database = Active.Database;
            ConstructOutdoorParkingSpaces();
        }

        public ThOutdoorParkingSpaceDbRepository(Database db)
        {
            database = db;
            ConstructOutdoorParkingSpaces();
        }

        public void AppendDefaultOutdoorParkingSpace()
        {
            spaces.Add(new ThOutdoorParkingSpace()
            {
                ID = Guid.NewGuid(),
                Number = spaces.Count + 1,
                Frame = ObjectId.Null.OldIdPtr,
            });
        }

        private void ConstructOutdoorParkingSpaces()
        {
            spaces = new List<ThOutdoorParkingSpace>();
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var names = new List<string>();
                acadDatabase.Layers.ForEachDbObject(l => names.Add(l.Name));
                foreach (string name in names.Where(n => n.StartsWith(@"车场车位_室外车位")))
                {
                    string[] tokens = name.Split('_');
                    if (tokens[2] != "露天车场" || tokens[3] != "小型汽车")
                        continue;

                    foreach (ObjectId objId in database.AreaFrameLines(name))
                    {
                        spaces.Add(new ThOutdoorParkingSpace()
                        {
                            ID = Guid.NewGuid(),
                            Number = spaces.Count + 1,
                            Frame = objId.OldIdPtr,
                        });
                    }
                }
            }
        }
    }
}
