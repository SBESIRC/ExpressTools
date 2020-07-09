using Linq2Acad;
using DotNetARX;
using GeometryExtensions;
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
                    var segments = new PolylineSegmentCollection(pline);
                    return segments.ToPolyline();
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
    }
}
