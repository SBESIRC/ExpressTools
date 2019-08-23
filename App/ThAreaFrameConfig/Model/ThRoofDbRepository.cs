using System;
using System.Collections.Generic;
using AcHelper;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThAreaFrameConfig.Model
{
    internal class ThRoofDbRepository : IThRoofRepository
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

        public void AppendDefaultRoof()
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

        private void ConstructRoofs()
        {
            roofs = database.Roofs();
        }
    }
}
