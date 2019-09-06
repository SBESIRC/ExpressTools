using System;
using System.Collections.Generic;
using AcHelper;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThAreaFrameConfig.Model
{
    public class ThPlotSpaceDbRepository : IThPlotSpaceRepository
    {
        private List<ThPlotSpace> spaces;
        private readonly Database database;

        public List<ThPlotSpace> Spaces
        {
            get
            {
                return spaces;
            }
        }

        public ThPlotSpaceDbRepository()
        {
            database = Active.Database;
            ConstructPlotSpaces();
        }

        public ThPlotSpaceDbRepository(Database db)
        {
            database = db;
            ConstructPlotSpaces();
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

        public ThPlotSpace AreaFrame(DBObject dbobj)
        {
            foreach (var space in Spaces)
            {
                if (space.Frame == dbobj.ObjectId.OldIdPtr)
                {
                    return space;
                }
            }

            return null;
        }

        public void AppendDefaultPlotSpace()
        {
            spaces.Add(new ThPlotSpace()
            {
                ID = Guid.NewGuid(),
                Number = spaces.Count + 1,
                Frame = ObjectId.Null.OldIdPtr,
                HouseHold = 0
            });
        }

        private void ConstructPlotSpaces()
        {
            spaces = database.PlotSpaces();
        }
    }
}
