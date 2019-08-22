using AcHelper;
using Linq2Acad;
using System;
using System.Linq;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThAreaFrameConfig.Model
{
    public class ThPublicGreenSpaceDbRepository : IThPublicGreenSpaceRepository
    {
        private List<ThPublicGreenSpace> spaces;
        private readonly Database database;

        public List<ThPublicGreenSpace> Spaces
        {
            get
            {
                return spaces;
            }
        }

        public ThPublicGreenSpaceDbRepository()
        {
            database = Active.Database;
            ConstructPublicGreenSpaces();
        }

        public ThPublicGreenSpaceDbRepository(Database db)
        {
            database = db;
            ConstructPublicGreenSpaces();
        }

        public void AppendDefaultPublicGreenSpace()
        {
            spaces.Add(new ThPublicGreenSpace()
            {
                ID = Guid.NewGuid(),
                Number = spaces.Count + 1,
                Frame = (IntPtr)0,
            });
        }

        private void ConstructPublicGreenSpaces()
        {
            spaces = new List<ThPublicGreenSpace>();
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var names = new List<string>();
                acadDatabase.Layers.ForEachDbObject(l => names.Add(l.Name));
                foreach (string name in names.Where(n => n.StartsWith(@"用地_公共绿地")))
                {
                    string[] tokens = name.Split('_');
                    foreach (ObjectId objId in database.AreaFrameLines(name))
                    {
                        spaces.Add(new ThPublicGreenSpace()
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
