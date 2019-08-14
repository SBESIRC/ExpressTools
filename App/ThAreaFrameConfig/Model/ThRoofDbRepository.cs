using AcHelper;
using Linq2Acad;
using System;
using System.Linq;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThAreaFrameConfig.Model
{
    internal class ThRoofDbRepository
    {
        private readonly Database database;
        private List<ThRoof> roofs;

        public ThRoofDbRepository()
        {
            database = Active.Database;
            ConstructRoofs();
        }

        public ThRoofDbRepository(Database db)
        {
            database = db;
            ConstructRoofs();
        }

        public List<ThRoof> Roofs
        {
            get
            {
                return roofs;
            }
        }

        public void AppendRoof()
        {
            roofs.Add(new ThRoof()
            {
                ID = Guid.NewGuid(),
                Number = roofs.Count + 1,
                Frame = ObjectId.Null.OldIdPtr,
                Coefficient = 1.0,
                FARCoefficient = 1.0
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

        private void ConstructRoofs()
        {
            roofs = new List<ThRoof>();
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var names = new List<string>();
                acadDatabase.Layers.ForEachDbObject(l => names.Add(l.Name));
                foreach (string name in names.Where(n => n.StartsWith(@"屋顶构件_屋顶绿地")))
                {
                    string[] tokens = name.Split('_');
                    foreach (ObjectId objId in AreaFrameLines(name))
                    {
                        roofs.Add(new ThRoof()
                        {
                            ID = Guid.NewGuid(),
                            Number = roofs.Count + 1,
                            Frame = objId.OldIdPtr,
                            Coefficient = double.Parse(tokens[2]),
                            FARCoefficient = double.Parse(tokens[3])
                        });
                    }
                }
            }
        }
    }
}
