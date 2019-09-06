using System;
using System.Collections.Generic;
using AcHelper;
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

        public ThPublicGreenSpace AreaFrame(DBObject dbobj)
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
            spaces = database.PublicGreenSpaces();
        }
    }
}
