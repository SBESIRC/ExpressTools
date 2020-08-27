using System;
using System.Linq;
using System.Collections.Generic;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using Linq2Acad;

using Dreambuild.AutoCAD;

namespace TianHua.FanSelection.UI
{
    public static class ThFanSelectionTypeSelect
    {
        //一种型号风机对应一条性能曲线，传入一个性能点和一个型号-性能曲线字典，返回与该点对应的型号名(即字典中的一个Key值)
        public static List<string> GetTypePolyline(Dictionary<string, Polyline> orgpolyline, Point3d typepoint)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                //由选型点沿Y方向做射线
                Ray xRay = new Ray();
                xRay.BasePoint = typepoint;
                xRay.UnitDir = Vector3d.YAxis;

                //筛选出位于选型点上方的性能曲线并获取曲线到指定点的距离
                IntPtr ptr = new IntPtr();
                Dictionary<string, double> polynamedistance = new Dictionary<string, double>();
                foreach (var item in orgpolyline)
                {
                    Point3dCollection intersectpointcoll = new Point3dCollection();
                    item.Value.IntersectWith(xRay, Intersect.OnBothOperands, intersectpointcoll, ptr, ptr);
                    //对于沿Y轴呈S型的Polyline,此时射线与polyline有多个交点，暂时不支持
                    if (intersectpointcoll.Count > 1)
                    {
                        throw new NotSupportedException();
                    }
                    else if (intersectpointcoll.Count == 1)
                    {
                        double dist = intersectpointcoll[0].DistanceTo(typepoint);
                        polynamedistance.Add(item.Key,dist);
                    }
                }
                if (polynamedistance.Count == 0)
                {
                    return new List<string>();
                }

                //按距离值从小到大排序
                var orderpolydiet = polynamedistance.OrderBy(q=>q.Value);
                double minvalue = orderpolydiet.First().Value;
                List<string> keyofpol = new List<string>();
                //获取距离最小的性能曲线对应的型号名
                var neworderpolydiet = orderpolydiet.Where(q=>q.Value.Equals(minvalue)).Select(p=>p.Key).ToList();
                return neworderpolydiet;
            }

        }

        //获取指定polyline上所有顶点中与指定点最近的一个顶点
        public static Point3d CloseVertice(Polyline poly, Point3d typepoint)
        {
            var verticescoll = poly.GetPolyPoints().ToList();
            if (verticescoll.All(p => p.X < typepoint.X))
            {
                return new Point3d();
            }
            return verticescoll.Where(p => p.X >= typepoint.X).OrderBy(p => p.X).First();
        }

        public static Point3d MinVertextInPoly(this Polyline poly)
        {
            var verticescoll = poly.GetPolyPoints().ToList();
            return verticescoll.OrderBy(q=>q.Y).First();
        }

        public static bool IfTypepoSmaller(Polyline poly, Point3d typepoint)
        {
            if (typepoint.Y <= poly.MinVertextInPoly().Y)
            {
                return false;
            }
            return true;
        }

    }
}
