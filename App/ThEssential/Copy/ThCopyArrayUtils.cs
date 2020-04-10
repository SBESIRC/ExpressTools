using Linq2Acad;
using System.Linq;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThEssential.Copy
{
    public static class ThCopyArrayUtils
    {
        public static DBObjectCollection DivideAlongPath(this ObjectIdCollection objs, Vector3d displacement, uint parameter)
        {
            var clones = new DBObjectCollection();
            for (uint i = 1; i < parameter; i++)
            {
                Vector3d offset = displacement * i / (parameter - 1);
                objs.TransformedCopy(offset).Cast<DBObject>().ForEachDbObject(o => clones.Add(o));
            }
            return clones;
        }

        public static DBObjectCollection TimesAlongPath(this ObjectIdCollection objs, Vector3d displacement, uint parameter)
        {
            var clones = new DBObjectCollection();
            for (uint i = 1; i < parameter; i++)
            {
                Vector3d offset = displacement * i;
                objs.TransformedCopy(offset).Cast<DBObject>().ForEachDbObject(o => clones.Add(o));
            }
            return clones;
        }

        private static DBObjectCollection TransformedCopy(this ObjectIdCollection objs, Vector3d offset)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var clones = new DBObjectCollection();
                foreach (ObjectId entity in objs)
                {
                    var obj = acadDatabase.Element<Entity>(entity);
                    clones.Add(obj.GetTransformedCopy(Matrix3d.Displacement(offset)));
                }
                return clones;
            }
        }

        public static void ClearWithDispose(this DBObjectCollection objs)
        {
            foreach (DBObject obj in objs)
            {
                obj.Dispose();
            }
            objs.Clear();
        }
    }
}
