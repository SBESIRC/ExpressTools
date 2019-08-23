using System;
using System.Collections.Generic;
using AcHelper;
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
            spaces = database.OutdoorParkingSpaces();
        }
    }
}
