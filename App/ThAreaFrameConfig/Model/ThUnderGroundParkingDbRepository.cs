using System;
using System.Collections.Generic;
using AcHelper;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThAreaFrameConfig.Model
{
    internal class ThUnderGroundParkingDbRepository : IThUnderGroundParkingRepository
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

        public void AppendDefaultUnderGroundParking()
        {
            parkings.Add(new ThUnderGroundParking()
            {
                ID = Guid.NewGuid(),
                Number = parkings.Count + 1,
                Floors = 1,
                Storey = "",
                Frames = new List<IntPtr>(),
            });
        }

        private void ConstructUnderGroundParkings()
        {
            parkings = database.UnderGroundParkings();
        }
    }
}
