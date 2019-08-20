using AcHelper;
using Linq2Acad;
using System;
using System.Linq;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThAreaFrameConfig.Model
{
    public static class ThAreaFrameDbExtension
    {
        public static ObjectIdCollection AreaFrameLines(this Database db, string layer)
        {
            var objectIdCollection = new ObjectIdCollection();
            using (AcadDatabase acadDatabase = AcadDatabase.Use(db))
            {
                acadDatabase.ModelSpace
                            .OfType<Polyline>()
                            .Where(e => e.Layer == layer)
                            .ForEachDbObject(e => objectIdCollection.Add(e.ObjectId));
            }
            return objectIdCollection;
        }

        public static List<IntPtr> AreaFrameLinesEx(this Database db, string layer)
        {
            var areaFrames = new List<IntPtr>();
            using (AcadDatabase acadDatabase = AcadDatabase.Use(db))
            {
                acadDatabase.ModelSpace
                            .OfType<Polyline>()
                            .Where(e => e.Layer == layer)
                            .ForEachDbObject(e => areaFrames.Add(e.ObjectId.OldIdPtr));
            }
            return areaFrames;
        }

        public static double Area(this IntPtr frame)
        {
            if (frame == (IntPtr)0)
            {
                return 0.0;
            }

            ObjectId objId = new ObjectId(frame);
            using (AcadDatabase acadDatabase = AcadDatabase.Use(objId.Database))
            {
                return acadDatabase.ElementOrDefault<Polyline>(objId).Area * (1.0 / 1000000.0);
            }
        }
    }
}
