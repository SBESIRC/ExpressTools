using AcHelper;
using Linq2Acad;
using System;
using System.Linq;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThAreaFrameConfig.Model
{
    internal class ThUnderGroundParkingDbRepository
    {
        private readonly Database database;
        private List<ThUnderGroundParking> parkings;

        public ThUnderGroundParkingDbRepository()
        {
            database = Active.Database;
            ConstructUnderGroundParkings();
        }

        public ThUnderGroundParkingDbRepository(Database db)
        {
            database = db;
            ConstructUnderGroundParkings();
        }

        public List<ThUnderGroundParking> Parkings
        {
            get
            {
                return parkings;
            }
        }

        public void AppendUnderGroundParking()
        {
            parkings.Add(new ThUnderGroundParking()
            {
                ID = Guid.NewGuid(),
                Number = parkings.Count + 1,
            });
        }

        private ObjectIdCollection AreaFrameLines(string layer)
        {
            var objectIdCollection = new ObjectIdCollection();
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                acadDatabase.ModelSpace
                            .OfType<Polyline>()
                            .Where(e => e.Layer == layer)
                            .ForEachDbObject(e => objectIdCollection.Add(e.ObjectId));
            }
            return objectIdCollection;
        }

        private void ConstructUnderGroundParkings()
        {
            parkings = new List<ThUnderGroundParking>();
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var names = new List<string>();
                acadDatabase.Layers.ForEachDbObject(l => names.Add(l.Name));
                foreach (string name in names.Where(n => n.StartsWith(@"单体车位_小型汽车")))
                {
                    string[] tokens = name.Split('_');
                    foreach (ObjectId objId in AreaFrameLines(name))
                    {
                        parkings.Add(new ThUnderGroundParking()
                        {
                            ID = Guid.NewGuid(),
                            Number = parkings.Count + 1,
                            Floors = UInt16.Parse(tokens[2]),
                            Storey = tokens[3]
                        });
                    }
                }
            }
        }
    }
}
