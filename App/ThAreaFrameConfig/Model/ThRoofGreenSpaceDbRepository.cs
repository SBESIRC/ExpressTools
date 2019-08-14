using AcHelper;
using Linq2Acad;
using System;
using System.Linq;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThAreaFrameConfig.Model
{
    internal class ThRoofGreenSpaceDbRepository
    {
        private readonly Database database;
        private List<ThRoofGreenSpace> spaces;

        public ThRoofGreenSpaceDbRepository()
        {
            database = Active.Database;
            ConstructRoofGreenSpaces();
        }

        public ThRoofGreenSpaceDbRepository(Database db)
        {
            database = db;
            ConstructRoofGreenSpaces();
        }

        public List<ThRoofGreenSpace> Spaces
        {
            get
            {
                return spaces;
            }
        }

        public void AppendRoofGreenSpace()
        {
            spaces.Add(new ThRoofGreenSpace()
            {
                ID = Guid.NewGuid(),
                Number = spaces.Count + 1,
                Frame = ObjectId.Null.OldIdPtr,
                Coefficient = 1.0
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

        private void ConstructRoofGreenSpaces()
        {
            spaces = new List<ThRoofGreenSpace>();
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var names = new List<string>();
                acadDatabase.Layers.ForEachDbObject(l => names.Add(l.Name));
                foreach (string name in names.Where(n => n.StartsWith(@"屋顶构件_屋顶绿地")))
                {
                    string[] tokens = name.Split('_');
                    foreach (ObjectId objId in AreaFrameLines(name))
                    {
                        spaces.Add(new ThRoofGreenSpace()
                        {
                            ID = Guid.NewGuid(),
                            Number = spaces.Count + 1,
                            Frame = objId.OldIdPtr,
                            Coefficient = double.Parse(tokens[2])
                        });
                    }
                }
            }
        }
    }
}
