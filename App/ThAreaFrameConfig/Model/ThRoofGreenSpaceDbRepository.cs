using System;
using System.Collections.Generic;
using AcHelper;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThAreaFrameConfig.Model
{
    internal class ThRoofGreenSpaceDbRepository : IThRoofGreenSpaceRepository
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

        private void ConstructRoofGreenSpaces()
        {
            spaces = database.RoofGreenSpaces();
        }
    }
}
