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

        public void RegisterAreaFrameModifiedEvent(ObjectEventHandler handler)
        {
            database.ObjectModified += handler;
        }

        public void UnRegisterAreaFrameModifiedEvent(ObjectEventHandler handler)
        {
            database.ObjectModified -= handler;
        }

        public void RegisterAreaFrameErasedEvent(ObjectErasedEventHandler handler)
        {
            database.ObjectErased += handler;
        }

        public void UnRegisterAreaFrameErasedEvent(ObjectErasedEventHandler handler)
        {
            database.ObjectErased -= handler;
        }

        public ThOutdoorParkingSpace AreaFrame(DBObject dbobj)
        {
            foreach (var space in Spaces)
            {
                if (space.Frames.Contains(dbobj.ObjectId.OldIdPtr))
                {
                    return space;
                }
            }

            return null;
        }

        public void AppendDefaultOutdoorParkingSpace()
        {
            spaces.Add(new ThOutdoorParkingSpace()
            {
                ID = Guid.NewGuid(),
                Number = spaces.Count + 1,
                Frames = new List<IntPtr>(),
                Storey = 1,
            });
        }

        private void ConstructOutdoorParkingSpaces()
        {
            spaces = database.OutdoorParkingSpaces();
        }
    }
}
