using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThWSS.Utlis
{
    public static class GetObjectUtils
    {
        /// <summary>
        /// 根据polygon区的范围内的元素（自定义polyline内的）
        /// </summary>
        /// <param name="ed"></param>
        /// <param name="pt1"></param>
        /// <param name="pt2"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static ObjectId[] GetObjectWithBounding(Editor ed, Polyline poly, SelectionFilter filter = null)
        {
            Point3dCollection polygon = new Point3dCollection();
            for (int i = 0; i < poly.NumberOfVertices; i++)
            {
                polygon.Add(poly.GetPoint3dAt(i));
            }
            PromptSelectionResult result;
            if (filter == null)
            {
                result = ed.SelectCrossingPolygon(polygon);
            }
            else
            {
                result = ed.SelectCrossingPolygon(polygon, filter);
            }

            ObjectId[] objIds = null;
            if (result.Status == PromptStatus.OK)
            {
                objIds = result.Value.GetObjectIds();
            }
            return objIds;
        }

        /// <summary>
        /// 获得附近一定范围内的polyline
        /// </summary>
        /// <param name="beamPolyline"></param>
        /// <param name="pt"></param>
        /// <returns></returns>
        public static Dictionary<Polyline, Point3d> GetNearPolyline(List<Polyline> beamPolyline, Point3d pt, double sideLength)
        {
            Dictionary<Polyline, Point3d> dic = new Dictionary<Polyline, Point3d>();
            foreach (var bPoly in beamPolyline)
            {
                var closeP = bPoly.GetClosestPointTo(pt, false);
                double dis = closeP.DistanceTo(pt);
                if (dis < sideLength)
                {
                    dic.Add(bPoly, closeP);
                }
            }

            return dic;
        }
    }
}
