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
        /// 
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
    }
}
