using Linq2Acad;
using System.Linq;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThEssential.Copy
{
    public static class ThCopyArrayUtils
    {
        public static List<Entity> DivideAlongPath(this ObjectIdCollection objs, Vector3d displacement, uint parameter)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var entities = new List<List<Entity>>();
                for (uint i = 1; i < parameter; i++)
                {
                    List<Entity> clones = new List<Entity>();
                    Vector3d offset = displacement * i / (parameter - 1);
                    foreach (ObjectId entity in objs)
                    {
                        Entity clone = acadDatabase.Element<Entity>(entity).Clone() as Entity;
                        clone.TransformBy(Matrix3d.Displacement(offset));
                        clones.Add(clone);
                    }
                    entities.Add(clones);
                }
                return entities.SelectMany(e => e).ToList();
            }
        }

        public static List<Entity> TimesAlongPath(this ObjectIdCollection objs, Vector3d displacement, uint parameter)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var entities = new List<List<Entity>>();
                for (uint i = 1; i < parameter; i++)
                {
                    List<Entity> clones = new List<Entity>();
                    Vector3d offset = displacement * i;
                    foreach (ObjectId entity in objs)
                    {
                        Entity clone = acadDatabase.Element<Entity>(entity).Clone() as Entity;
                        clone.TransformBy(Matrix3d.Displacement(offset));
                        clones.Add(clone);
                    }
                    entities.Add(clones);
                }
                return entities.SelectMany(e => e).ToList();
            }
        }
    }
}
