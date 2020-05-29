using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using GeoAPI.Geometries;
using Linq2Acad;
using NetTopologySuite.Operation.Polygonize;
using NetTopologySuite.Operation.Union;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThCADCore.NTS;

namespace ThWSS.Utlis
{
    public class RegionDivisionUtils
    {
        readonly Tolerance tolerance = new Tolerance(0.0001, 0.0001);
        readonly double minDis = 700;

        public List<Polyline> DivisionRegion(Polyline room)
        {
            var roomBounding = GeUtils.CreateConvexPolygon(room, 1500);

            List<Line> pLines = new List<Line>();
            List<Point3d> points = new List<Point3d>();
            for (int i = 0; i < roomBounding.NumberOfVertices; i++)
            {
                var current = roomBounding.GetPoint3dAt(i);
                var next = roomBounding.GetPoint3dAt((i + 1) % roomBounding.NumberOfVertices);
                int j = i - 1;
                if (j < 0)
                {
                    j = roomBounding.NumberOfVertices - 1;
                }
                var pre = roomBounding.GetPoint3dAt(j);
                pLines.Add(new Line(current, next));

                int res = GeUtils.IsConvexPoint(room, current, next, pre);
                if (res == 1)   //凹点要分割
                {
                    points.Add(current);
                }
            }

            var bLines = GetDivisionines(pLines, points);
            var diviPoly = GetMinPolygon(bLines);
            diviPoly = CalMergeRule(diviPoly);
            return diviPoly;
        }

        /// <summary>
        /// 计算区域分割
        /// </summary>
        /// <param name="allLines"></param>
        /// <param name="allPoints"></param>
        /// <returns></returns>
        private List<Line> GetDivisionines(List<Line> allLines, List<Point3d> allPoints)
        {
            foreach (var pt in allPoints)
            {
                var tempLines = allLines.Where(x => x.StartPoint.IsEqualTo(pt, Tolerance.Global) || x.EndPoint.IsEqualTo(pt, Tolerance.Global)).ToList();
                var otherLines = allLines.Except(tempLines).ToList();
                Point3d bPoint = pt;
                Line bLine = null;
                foreach (var line in tempLines)
                {
                    Vector3d dir = line.Delta.GetNormal();
                    if (!line.EndPoint.IsEqualTo(pt, Tolerance.Global))
                    {
                        dir = -dir;
                    }

                    foreach (var oLine in otherLines)
                    {
                        Ray ray = new Ray();
                        ray.BasePoint = pt;
                        ray.UnitDir = dir;
                        Point3dCollection points = new Point3dCollection();
                        oLine.IntersectWith(ray, Intersect.OnBothOperands, points, IntPtr.Zero, IntPtr.Zero);
                        if (points.Count > 0)
                        {
                            var tempP = points[0];
                            if (bPoint == pt)
                            {
                                bPoint = tempP;
                                bLine = oLine;
                                continue;
                            }

                            if (tempP.DistanceTo(pt) < pt.DistanceTo(bPoint))
                            {
                                bPoint = tempP;
                                bLine = oLine;
                            }
                        }
                    }
                }
                if (bPoint != pt)
                {
                    allLines.Add(new Line(bPoint, pt));
                    allLines.Add(new Line(bLine.StartPoint, bPoint));
                    allLines.Add(new Line(bPoint, bLine.EndPoint));
                    allLines.Remove(bLine);
                }
            }

            return allLines;
        }

        /// <summary>
        /// 根据规则合并区域
        /// </summary>
        /// <param name="polygons"></param>
        /// <returns></returns>
        public List<Polyline> CalMergeRule(List<Polyline> polygons)
        {
            polygons = polygons.OrderBy(x => x.Area).ToList();
            //两个区域相交部分很多，且与多个区域相交的区域合并掉
            RETRY:
            foreach (var poly1 in polygons)
            {
                //面积太小无法排布的区域合并掉
                var obb1 = OrientedBoundingBox.Calculate(poly1);
                var needMerge = CalInvalidPolygon(obb1);

                int index = 0;
                double length = 0;
                Polyline interPoly = null;
                var otherPoly = polygons.Where(x => x != poly1).ToList();
                foreach (var poly2 in otherPoly)
                {
                    bool isInter = false;
                    double intersectLength;
                    Line line1;
                    Line line2;
                    if (needMerge)
                    {
                        isInter = CalPolyIntersect(poly1, poly2, tolerance, out intersectLength, out line1, out line2);
                    }
                    else
                    {
                        //var obb2 = OrientedBoundingBox.Calculate(poly2);
                        isInter = CalPolyIntersect(obb1, poly2, tolerance, out intersectLength, out line1, out line2);
                    }
                    if (isInter)
                    {
                        if (intersectLength > length && ((line1.Length * 2 / 3 < intersectLength && line2.Length * 2 / 3 < intersectLength) || needMerge))
                        {
                            interPoly = poly2;
                            length = intersectLength;
                        }
                        index++;
                    }
                }

                if ((index >= 2 || needMerge) && interPoly != null)
                {
                    //var resPoly = poly1.Union(interPoly);

                    var curves1 = new DBObjectCollection()
                    {
                        poly1
                    };
                    var region1 = Region.CreateFromCurves(curves1)[0] as Region;

                    var curves2 = new DBObjectCollection()
                    {
                        interPoly
                    };
                    var region2 = Region.CreateFromCurves(curves2)[0] as Region;
                    region1.BooleanOperation(BooleanOperationType.BoolUnite, region2);
                    var resPoly = region1.ToNTSPolygon().Shell.ToDbPolyline();
                    using (AcadDatabase ss = AcadDatabase.Active())
                    {
                        //ss.ModelSpace.Add(poly1);
                        //ss.ModelSpace.Add(interPoly);
                    }
                    polygons.Remove(poly1);
                    polygons.Remove(interPoly);
                    if (resPoly != null)
                    {
                        polygons.Add(resPoly);
                    }
                    goto RETRY;
                }
            }

            return polygons;
        }

        /// <summary>
        /// 计算两个polygon是否相交
        /// </summary>
        /// <param name="poly1"></param>
        /// <param name="poly2"></param>
        /// <param name="tol"></param>
        /// <param name="intersectLength"></param>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <returns></returns>
        public bool CalPolyIntersect(Polyline poly1, Polyline poly2, Tolerance tol, out double intersectLength, out Line line1, out Line line2)
        {
            line1 = null;
            line2 = null;
            for (int i = 0; i < poly1.NumberOfVertices; i++)
            {
                var s = poly1.GetLineSegmentAt(i);
                for (int j = 0; j < poly2.NumberOfVertices; j++)
                {
                    var Line3d1 = poly1.GetLineSegmentAt(i);
                    var Line3d2 = poly2.GetLineSegmentAt(j);

                    LinearEntity3d overlopCuv = Line3d1.Overlap(Line3d2, tol);
                    if (overlopCuv == null)
                    {
                        overlopCuv = Line3d2.Overlap(Line3d1, tol);
                    }
                    if (overlopCuv != null)
                    {
                        line1 = new Line(Line3d1.StartPoint, Line3d1.EndPoint);
                        line2 = new Line(Line3d2.StartPoint, Line3d2.EndPoint);
                        intersectLength = (overlopCuv as LineSegment3d).Length;
                        return true;
                    }
                }
            }

            intersectLength = 0;
            return false;
        }

        /// <summary>
        /// 算出无效的需要被合并的polygon（面积过小或者过窄）
        /// </summary>
        /// <param name="poly"></param>
        /// <returns></returns>
        public bool CalInvalidPolygon(Polyline poly)
        {
            List<Line> polyAllLines = new List<Line>();
            for (int i = 0; i < poly.NumberOfVertices; i++)
            {
                var next = poly.GetPoint3dAt((i + 1) % poly.NumberOfVertices);
                polyAllLines.Add(new Line(poly.GetPoint3dAt(i), next));
            }

            Line maxLine = polyAllLines.OrderByDescending(x => x.Length).First();
            Vector3d dir = maxLine.Delta.GetNormal();
            List<Line> otherLines = polyAllLines.Where(x => x.Delta.GetNormal().IsParallelTo(dir, tolerance)).ToList();
            double maxLength = otherLines.Max(x =>
            {
                double sDis = x.GetClosestPointTo(maxLine.StartPoint, true).DistanceTo(maxLine.StartPoint);
                double eDis = x.GetClosestPointTo(maxLine.EndPoint, true).DistanceTo(maxLine.EndPoint);
                return sDis < eDis ? sDis : eDis;
            });

            return maxLength <= minDis;
        }

        /// <summary>
        /// 获得所有最小轮廓线
        /// </summary>
        /// <param name="bLines"></param>
        /// <returns></returns>
        public List<Polyline> GetMinPolygon(List<Line> bLines)
        {
            DBObjectCollection dBObject = new DBObjectCollection();
            foreach (var bLine in bLines)
            {
                dBObject.Add(bLine);
            }
            var objCollection = dBObject.Polygons();
            List<Polyline> polygons = objCollection.Cast<Polyline>().ToList();

            //去除掉多余的线上的点
            List<Polyline> resPoly = new List<Polyline>();
            foreach (var polyline in polygons)
            {
                Polyline tempPoly = new Polyline() { Closed = true };
                int index = 0;
                for (int i = 0; i < polyline.NumberOfVertices - 1; i++)
                {
                    var current = polyline.GetPoint3dAt(i);
                    var next = polyline.GetPoint3dAt((i + 1) % (polyline.NumberOfVertices - 1));
                    int j = i - 1;
                    if (j < 0)
                    {
                        j = polyline.NumberOfVertices - 2;
                    }
                    var pre = polyline.GetPoint3dAt(j);

                    Vector3d preDir = (current - pre).GetNormal();
                    Vector3d nextDir = (next - current).GetNormal();
                    if (!preDir.IsParallelTo(nextDir, tolerance))
                    {
                        tempPoly.AddVertexAt(index, polyline.GetPoint2dAt(i), 0, 0, 0);
                        index++;
                    }
                }
                resPoly.Add(tempPoly);
            }

            return resPoly;
        }
    }
}
