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
                    Vector3d offset = displacement * i / parameter;
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

        public static void DivideCopyAlongPath(this ObjectIdCollection objs, Vector3d displacement, uint parameter)
        {
            for (uint i = 1; i < parameter; i++)
            {
                objs.CopyWithOffset(displacement * i / parameter);
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
                    Vector3d offset = displacement * (i+1);
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

        public static void TimesCopyAlongPath(this ObjectIdCollection objs, Vector3d displacement, uint parameter)
        {
            for (uint i = 1; i < parameter; i++)
            {
                objs.CopyWithOffset(displacement * (i + 1));
            }
        }

        public static void CopyWithOffset(this ObjectIdCollection objs, Vector3d offset)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
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
                            entity.TransformBy(Matrix3d.Displacement(offset));
                        }
                    }
                }
            }
        }
    }
}
