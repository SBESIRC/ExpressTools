using System;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using ThSitePlan.Configuration;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NFox.Cad.Collections;
using Autodesk.AutoCAD.EditorInput;
using AcHelper;
using DotNetARX;
using Linq2Acad;

namespace ThSitePlan.Engine
{
    public class ThSitePlanFrameCleanGenerator : ThSitePlanGenerator
    {
        public override ObjectId OriginFrame { get; set; }
        public override Tuple<ObjectId, Vector3d> Frame { get; set; }

        public override bool Generate(Database database, ThSitePlanConfigItem configItem)
        {
            ObjectId cleanframe = ThSitePlanDbEngine.Instance.FrameByName(configItem.Properties["Name"].ToString());
            using (var objs = FilterAll(database, cleanframe))
            {
                foreach (ObjectId oid in objs)
                {
                    if (oid.IsErased == true)
                    {
                        continue;
                    }
                    oid.Erase();
                }
            }
            return true;
        }

        public ObjectIdCollection FilterAll(Database database, ObjectId cleanframeid)
        {
            // 前面已经在选择时过滤掉了锁定图层上的图元，为什么这里还会有在锁定图层上的图元
            // 原因是将块引用炸开后，其子图元被释放出来，其子图元可能放置在锁定图层中
            // 这里暂时忽略掉在锁定图层中的图元
            void ed_SelectionAdded(object sender, SelectionAddedEventArgs e)
            {
                using (AcadDatabase acdb = AcadDatabase.Use(database))
                {
                    var lockedLayers = acdb.Layers.Where(o => o.IsLocked).Select(o => o.ObjectId);
                    ObjectId[] ids = e.AddedObjects.GetObjectIds();
                    for (int i = 0; i < ids.Length; i++)
                    {
                        var entity = acdb.Element<Entity>(ids[i]);
                        if (lockedLayers.Contains(entity.LayerId))
                        {
                            e.Remove(i);
                        }
                    }
                }
            }
            Active.Editor.SelectionAdded += ed_SelectionAdded;
            PromptSelectionResult psr = Active.Editor.SelectByPolyline(
                cleanframeid,
                PolygonSelectionMode.Crossing,
                null);
            Active.Editor.SelectionAdded -= ed_SelectionAdded;
            if (psr.Status == PromptStatus.OK)
            {
                return new ObjectIdCollection(psr.Value.GetObjectIds());
            }
            else
            {
                return new ObjectIdCollection();
            }
        }
    }
}
