using System;
using System.Linq;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using Linq2Acad;

namespace ThSitePlan
{
    public static class ThSitePlanDbExtension
    {
        public static ObjectIdCollection SelectAll(this Database database, string layer)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var objs = new ObjectIdCollection();
                acadDatabase.ModelSpace
                    .OfType<Entity>()
                    .Where(o => o.Layer == layer)
                    .ForEachDbObject(o => objs.Add(o.ObjectId));
                return objs;
            }
        }

        public static void CopyWithMove(this Database database, 
            ObjectIdCollection objs, Matrix3d displacement)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                using (IdMapping idMap = new IdMapping())
                {
                    acadDatabase.Database.DeepCloneObjects(objs,
                            acadDatabase.Database.CurrentSpaceId, idMap, false);
                    foreach (IdPair pair in idMap)
                    {
                        if (pair.IsPrimary && pair.IsCloned)
                        {
                            var entity = acadDatabase.Element<Entity>(pair.Value, true);
                            entity.TransformBy(displacement);
                        }
                    }
                }
            }
        }

        public static ObjectId Frame(this Database database, string layer)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var frame = acadDatabase.ModelSpace
                    .OfType<Polyline>()
                    .Where(o => o.Layer == layer)
                    .FirstOrDefault();
                if (frame != null)
                {
                    return frame.ObjectId;
                }

                return ObjectId.Null;
            }
        }
    }
}
