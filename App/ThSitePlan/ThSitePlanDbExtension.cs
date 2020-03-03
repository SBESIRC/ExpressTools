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
            ObjectIdCollection collection, 
            Matrix3d displacement)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                foreach (ObjectId objId in collection)
                {
                    var entity = acadDatabase.Element<Entity>(objId);
                    var clone = entity.GetTransformedCopy(displacement);
                    acadDatabase.ModelSpace.Add(clone);
                }
            }
        }
    }
}
