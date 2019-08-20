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
    }
}
