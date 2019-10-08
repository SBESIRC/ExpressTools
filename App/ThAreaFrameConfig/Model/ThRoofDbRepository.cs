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

        public ThRoof AreaFrame(DBObject dbobj)
        {
            foreach(var roof in Roofs)
            {
                if (roof.Frame == dbobj.ObjectId.OldIdPtr)
                {
                    return roof;
                }
            }

            return null;
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
                Category = "住宅",
                Coefficient = 1.0,
                FARCoefficient = 1.0,
                Frame = ObjectId.Null.OldIdPtr
            });
        }

        private void ConstructRoofs()
        {
            roofs = database.Roofs();
        }
    }
}
