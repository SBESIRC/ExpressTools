using Autodesk.AutoCAD.BoundaryRepresentation;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Linq2Acad;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ThCADCore.NTS;
using System.Text;
using System.Threading.Tasks;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThWSS.Utlis
{
    public static class GeUtils
    {
        /// <summary>
        /// 判断点是否再Polyline内
        /// 0.点在polyline上    1.点在polyline内    -1.点在polyline外
        /// </summary>
        /// <returns></returns>
        public static int CheckPointInPolyline(Polyline polyline, Point3d pt, double tol)
        {
            Debug.Assert(polyline != null);
            Point3d closestP = polyline.GetClosestPointTo(pt, false);
            if (Math.Abs(closestP.DistanceTo(pt)) < tol)
            {
                return 0;
            }

            Ray ray = new Ray();
            ray.BasePoint = pt;
            Vector3d vec = -(closestP - pt).GetNormal();
            ray.UnitDir = vec;
            Point3dCollection points = new Point3dCollection();
            polyline.IntersectWith(ray, Intersect.OnBothOperands, points, IntPtr.Zero, IntPtr.Zero);
            FilterEqualPoints(points, tol);
        RETRY:
            if (points.Count == 0)
            {
                ray.Dispose();
                return -1;
            }
            else
            {
                FilterEqualPoints(points, closestP, tol);
                for (int i = points.Count - 1; i >= 0; i--)
                {
                    if ((points[i].X - pt.X) * (closestP.X - pt.X) >= 0 &&
                        (points[i].Y - pt.Y) * (closestP.Y - pt.Y) >= 0)
                    {
                        points.RemoveAt(i);
                    }
                }

                for (int i = 0; i < points.Count; i++)
                {
                    if (PointIsPolyVert(polyline, points[i], new Tolerance(0.01, 0.01)))
                    {
                        if (PointIsPolyVert(polyline, pt, new Tolerance(0.01, 0.01)))
                        {
                            return 0;
                        }

                        //将射线方向旋转两度，避免撞到顶点
                        vec = vec.RotateBy(0.035, Vector3d.ZAxis);
                        ray.UnitDir = vec;
                        points.Clear();
                        polyline.IntersectWith(ray, Intersect.OnBothOperands, points, IntPtr.Zero, IntPtr.Zero);
                        goto RETRY;
                    }
                }
            }

            ray.Dispose();
            if (points.Count % 2 == 0)
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }

        /// <summary>
        /// 过滤掉容差范围内相同的点
        /// </summary>
        /// <param name="points"></param>
        /// <param name="tol"></param>
        public static void FilterEqualPoints(Point3dCollection points, double tol)
        {
            for (int i = 0; i < points.Count - 1; i++)
            {
                for (int j = i + 1; j < points.Count; j++)
                {
                    if (points[i].DistanceTo(points[j]) < tol)
                    {
                        points.Remove(points[j]);
                    }
                }
            }
        }

        /// <summary>
        /// 过滤掉容差范围内相同的点
        /// </summary>
        /// <param name="points"></param>
        /// <param name="tol"></param>
        public static void FilterEqualPoints(Point3dCollection points, Point3d pt, double tol)
        {
            Point3dCollection tempPoints = new Point3dCollection();
            for (int i = 0; i < points.Count - 1; i++)
            {
                if (points[i].DistanceTo(pt) < tol)
                {
                    tempPoints.Add(pt);
                }
            }
            points = tempPoints;
        }

        /// <summary>
        /// 判断点是否是短线段的顶点
        /// </summary>
        /// <param name="pPoly"></param>
        /// <param name="pt"></param>
        /// <param name="tol"></param>
        /// <returns></returns>
        public static bool PointIsPolyVert(Polyline pPoly, Point3d pt, Tolerance tol)
        {
            for (int i = 0; i < pPoly.NumberOfVertices; i++)
            {
                Point3d vert = pPoly.GetPoint3dAt(i);
                if (vert.IsEqualTo(pt, tol))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 判断当前点是凸点还是凹点(-1，凸点；1，凹点；0，点在线上，不是拐点)
        /// </summary>
        /// <param name="poly"></param>
        /// <param name="pt"></param>
        /// <param name="nextP"></param>
        /// <param name="preP"></param>
        /// <returns></returns>
        public static int IsConvexPoint(Polyline poly, Point3d pt, Point3d nextP, Point3d preP)
        {
            Vector3d nextV = (nextP - pt).GetNormal();
            Vector3d preV = (pt - preP).GetNormal();
            Point3d movePt = pt - nextV * 1 + preV * 1;
            return CheckPointInPolyline(poly, movePt, 0.0001);
            
            //Vector3d normal = poly.Normal;
            //Vector3d nextV = (nextP - pt).GetNormal();
            //Vector3d preV = (pt - preP).GetNormal();
            //Vector3d dir = preV.CrossProduct(nextV);

            //if (Math.Abs(dir.Z) < 0.001)
            //{
            //    return 0;
            //}
            //else if (!((dir.Z > 0 && normal.Z > 0) || (dir.Z < 0 && normal.Z < 0)))
            //{
            //    return 1;
            //}
            //else
            //{
            //    return -1;
            //}
        }

        /// <summary>
        /// 计算容差范围内的凸包
        /// </summary>
        /// <param name="poly"></param>
        /// <param name="tol"></param>
        /// <returns></returns>
        public static Polyline CreateConvexPolygon(Polyline poly, double tol)
        {
            //过滤掉重复点
            List<Point3d> allPts = new List<Point3d>();
            for (int i = 0; i < poly.NumberOfVertices; i++)
            {
                if (allPts.Where(x => x.IsEqualTo(poly.GetPoint3dAt(i), Tolerance.Global)).Count() <= 0)
                {
                    allPts.Add(poly.GetPoint3dAt(i));
                }
            }

            List<KeyValuePair<Point3d, bool>> pLst = new List<KeyValuePair<Point3d, bool>>();
            for (int i = 0; i < allPts.Count; i++)
            {
                var current = allPts[i];
                var next = allPts[(i + 1) % allPts.Count];
                int j = i - 1;
                if (j < 0)
                {
                    j = allPts.Count - 1;
                }
                var pre = allPts[j];

                bool isConvex = IsConvexPoint(poly, current, next, pre) == -1 ? true : false;
                pLst.Add(new KeyValuePair<Point3d, bool>(allPts[i], isConvex));
            }

            //控制第一个点是凸点（因为凹点可能被省略，凸点一定保留）
            while (!pLst.First().Value)
            {
                var temp = pLst.First();
                pLst.Remove(temp);
                pLst.Add(temp);
            }

            Polyline convexPoly = new Polyline() { Closed = true };
            int index = 0;
            for (int i = 0; i < pLst.Count; i++)
            {
                var current = pLst[i];
                convexPoly.AddVertexAt(index, new Point2d(current.Key.X, current.Key.Y), 0, 0, 0);

                if (current.Value)
                {
                    var next = pLst[(i + 1) % pLst.Count];
                    if (!next.Value)
                    {
                        int j = i;
                        while (j < pLst.Count)
                        {
                            var tempNext = pLst[(j + 1) % pLst.Count];
                            if (tempNext.Value)
                            {
                                if (tempNext.Key.DistanceTo(current.Key) <= tol)
                                {
                                    i = j;
                                    var tp = pLst[(j + 2) % pLst.Count];
                                    if (tp.Value)
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }
                            j++;
                        }
                    }
                }
                index++;
            }

            return convexPoly;
        }

        /// <summary>
        /// 去除在线上的点
        /// </summary>
        /// <param name="polygons"></param>
        /// <returns></returns>
        public static List<Polyline> ReovePointOnLine(List<Polyline> polygons, Tolerance tolerance)
        {
            //去除掉多余的线上的点
            List<Polyline> resPoly = new List<Polyline>();
            foreach (var polyline in polygons)
            {
                //去掉容差范围内相同的点
                List<Point3d> allPts = new List<Point3d>();
                for (int i = 0; i < polyline.NumberOfVertices; i++)
                {
                    if (allPts.Where(x => x.IsEqualTo(polyline.GetPoint3dAt(i), tolerance)).Count() <= 0)
                    {
                        allPts.Add(polyline.GetPoint3dAt(i));
                    }
                }

                //首尾点重叠要处理
                Polyline tempPoly = new Polyline() { Closed = true };
                int index = 0;
                for (int i = 0; i < allPts.Count; i++)
                {
                    var current = allPts[i];
                    var next = allPts[(i + 1) % allPts.Count];
                    int j = i - 1;
                    if (j < 0)
                    {
                        j = allPts.Count - 1;
                    }
                    var pre = allPts[j];

                    Vector3d preDir = (current - pre).GetNormal();
                    Vector3d nextDir = (next - current).GetNormal();
                    if (!preDir.IsParallelTo(nextDir, tolerance))
                    {
                        tempPoly.AddVertexAt(index, allPts[i].toPoint2d(), 0, 0, 0);
                        index++;
                    }
                }
                resPoly.Add(tempPoly);
            }

            return resPoly;
        }

        /// <summary>
        /// 获取射线交点
        /// </summary>
        /// <param name="roomLines"></param>
        /// <param name="sp"></param>
        /// <param name="transverseDir"></param>
        /// <returns></returns>
        public static List<Point3d> GetRayIntersectPoints(List<Line> roomLines, Point3d sp, Vector3d dir)
        {
            Ray ray = new Ray();
            ray.BasePoint = sp;
            ray.UnitDir = dir;

            List<Point3d> allPoints = new List<Point3d>();
            var intersectLines = roomLines.Where(x => !x.Delta.GetNormal().IsParallelTo(dir, new Tolerance(0.0001, 0.0001))).ToList();
            foreach (var line in intersectLines)
            {
                Point3dCollection points = new Point3dCollection();
                line.IntersectWith(ray, Intersect.OnBothOperands, points, IntPtr.Zero, IntPtr.Zero);
                if (points.Count > 0)
                {
                    allPoints.Add(points[0]);
                }
            }

            return allPoints;
        }

    }
}
