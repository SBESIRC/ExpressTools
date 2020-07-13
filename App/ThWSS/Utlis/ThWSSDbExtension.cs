using Linq2Acad;
using DotNetARX;
using System.Linq;
using ThCADCore.NTS;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThWSS.Utlis
{
    public static class ThWSSDbExtension
    {
        /// <summary>
        /// 从一个多段线获取房间面积框线
        /// </summary>
        /// <param name="database"></param>
        /// <param name="plineId"></param>
        /// <returns></returns>
        public static Polyline AreaOutline(this Database database, ObjectId plineId)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var curve = acadDatabase.Element<Curve>(plineId);
                if (curve is Polyline pline)
                {
                    var objs = new DBObjectCollection();
                    pline.Explode(objs);
                    var outlines = objs.Polygons();
                    if (outlines.Count == 1)
                    {
                        return outlines[0] as Polyline;
                    }
                    else if (outlines.Count > 1)
                    {
                        // 自交的多段线会形成多个多边形
                        // 暂时不考虑这种复杂的多段线
                        return null;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// 喷淋面积框线图层
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        public static ObjectId CreateAreaOutlineLayer(this Database database)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var objId = LayerTools.AddLayer(database, ThWSSCommon.AreaOutlineLayer);
                LayerTools.SetLayerColor(database, ThWSSCommon.AreaOutlineLayer, 3);
                return objId;
            }
        }

        /// <summary>
        /// 喷淋保护盲区框线图层
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        public static ObjectId CreateSprayLayoutBlindRegionLayer(this Database database)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var objId = LayerTools.AddLayer(database, ThWSSCommon.SprayLayoutBlindRegionLayer);
                LayerTools.SetLayerColor(database, ThWSSCommon.SprayLayoutBlindRegionLayer, 11);
                return objId;
            }
        }

        /// <summary>
        /// 喷淋可布区域框线图层
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        public static ObjectId CreateSprayLayoutRegionLayer(this Database database)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var objId = LayerTools.AddLayer(database, ThWSSCommon.SprayLayoutRegionLayer);
                LayerTools.SetLayerColor(database, ThWSSCommon.SprayLayoutRegionLayer, 52);
                return objId;
            }
        }

        public static void EraseObjs(this Database database, ObjectIdCollection objs)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                foreach (ObjectId obj in objs)
                {
                    if (!obj.IsErased)
                    {
                        // 删除数据清理过程中临时“炸”到当前图纸中的对象
                        // 对于“炸”到锁定图层上的对象，我们仍然需要将他们删除
                        acadDatabase.Element<Entity>(obj, true, true).Erase();
                    }
                }
            }

            // A collection of object ids whose memory is to be reclaimed by deleting their objects.
            // All object ids in the collection must correspond to erased objects, which must be entirely closed
            var ids = objs.Cast<ObjectId>().Where(o => o.IsErased).ToArray();
            database.ReclaimMemoryFromErasedObjects(new ObjectIdCollection(ids));
        }
    }
}
