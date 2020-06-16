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
using GeometryExtensions;
using DotNetARX;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThWSS.Utlis
{
    public class RegionDivisionUtils
    {
        readonly Tolerance tolerance = new Tolerance(0.001, 0.001);
        readonly double minDis = 700;

        public List<Polyline> DivisionRegion(Polyline room)
        {
            List<Line> pLines = new List<Line>();
            List<Point3d> points = new List<Point3d>();
            for (int i = 0; i < room.NumberOfVertices; i++)
            {
                var current = room.GetPoint3dAt(i);
                var next = room.GetPoint3dAt((i + 1) % room.NumberOfVertices);
                int j = i - 1;
                if (j < 0)
                {
                    j = room.NumberOfVertices - 1;
                }
                var pre = room.GetPoint3dAt(j);
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
                if (poly1.Area <= 0)
                {
                    continue;
                }

                //面积太小无法排布的区域合并掉
                var needMerge = CalInvalidPolygon(poly1);

                int index = 0;
                double length = 0;
                Polyline interPoly = null;
                var otherPoly = polygons.Where(x => x != poly1).ToList();
                foreach (var poly2 in otherPoly)
                {
                    bool isInter = CalPolyIntersect(poly1, poly2, tolerance, out LinearEntity2d intersectPart, out Line line1, out Line line2, out bool needMergeRule2);
                    if (isInter)
                    {
                        double intersectLength = (intersectPart as LineSegment2d).Length;
                        if ((intersectLength > length && ((line1.Length * 2 / 3 < intersectLength && line2.Length * 2 / 3 < intersectLength) || needMerge)) || needMergeRule2)
                        {
                            interPoly = poly2;
                            length = intersectLength;

                            if (needMergeRule2)
                            {
                                needMerge = true;
                            }
                        }
                        index++;
                    }
                }

                if ((index >= 2 || needMerge) && interPoly != null)
                {
                    var resPoly = MergePolyline(poly1, interPoly);
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
        public bool CalPolyIntersect(Polyline poly1, Polyline poly2, Tolerance tol, out LinearEntity2d intersectPart, out Line line1, out Line line2, out bool needMerge)
        {
            line1 = null;
            line2 = null;
            intersectPart = null;
            needMerge = false;
            bool res = false;
            int index = 0;

            var segments = new PolylineSegmentCollection(poly1);
            var segments2 = new PolylineSegmentCollection(poly2);
            foreach (var segment1 in segments)
            {
                foreach (var segment2 in segments2)
                {
                    var Line3d1 = segment1.ToCurve2d() as LinearEntity2d;
                    var Line3d2 = segment2.ToCurve2d() as LinearEntity2d;

                    LinearEntity2d overlopCuv = Line3d1.Overlap(Line3d2, tol);
                    if (overlopCuv == null)
                    {
                        overlopCuv = Line3d2.Overlap(Line3d1, tol);
                    }
                    if (overlopCuv != null && (overlopCuv as LineSegment2d).Length > 10)
                    {
                        line1 = new Line(Line3d1.StartPoint.toPoint3d(), Line3d1.EndPoint.toPoint3d());
                        line2 = new Line(Line3d2.StartPoint.toPoint3d(), Line3d2.EndPoint.toPoint3d());
                        intersectPart = overlopCuv;
                        double minLength = line1.Length < line2.Length ? line1.Length : line2.Length;
                        res = true;
                        index++;
                        if (index == 2)
                        {
                            needMerge = true;
                            return res;
                        }
                    }
                }
            }

            return res;
        }

        /// <summary>
        /// 算出无效的需要被合并的polygon（面积过小或者过窄）
        /// </summary>
        /// <param name="poly"></param>
        /// <returns></returns>
        public bool CalInvalidPolygon(Polyline poly)
        {
            var obb1 = OrientedBoundingBox.Calculate(poly);

            List<Line> polyAllLines = new List<Line>();
            for (int i = 0; i < obb1.NumberOfVertices; i++)
            {
                var next = obb1.GetPoint3dAt((i + 1) % obb1.NumberOfVertices);
                polyAllLines.Add(new Line(obb1.GetPoint3dAt(i), next));
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
        /// 合并两个有边相交的polyline
        /// </summary>
        /// <param name="pl1"></param>
        /// <param name="pl2"></param>
        /// <param name="interLine1"></param>
        /// <param name="iterLine2"></param>
        /// <returns></returns>
        public Polyline MergePolyline(Polyline pl1, Polyline pl2)
        {
            List<Line> polyAllLines = new List<Line>();
            for (int i = 0; i < pl1.NumberOfVertices; i++)
            {
                var next = pl1.GetPoint3dAt((i + 1) % pl1.NumberOfVertices);
                var current = pl1.GetPoint3dAt(i);
                polyAllLines.Add(new Line(current, next));
            }

            List<Line> polyAllLines2 = new List<Line>();
            for (int i = 0; i < pl2.NumberOfVertices; i++)
            {
                var next = pl2.GetPoint3dAt((i + 1) % pl2.NumberOfVertices);
                var current = pl2.GetPoint3dAt(i);
                polyAllLines2.Add(new Line(current, next));
            }

            polyAllLines = RemoveLineInterPart(polyAllLines, polyAllLines2);
            var resPolygons = CreatePolyline(polyAllLines);
            return GeUtils.ReovePointOnLine(new List<Polyline>() { resPolygons }, tolerance).First();
        }

        /// <summary>
        /// 去掉重合部分的线
        /// </summary>
        /// <param name="lines1"></param>
        /// <param name="lines2"></param>
        /// <returns></returns>
        public List<Line> RemoveLineInterPart(List<Line> lines1, List<Line> lines2)
        {
            List<Line> resLines = new List<Line>();
        RETRY:
            foreach (var line in lines1)
            {
                var s = line;
                foreach (var oLine in lines2)
                {
                    var segLine1 = new LineSegment3d(line.StartPoint, line.EndPoint);
                    var segLine2 = new LineSegment3d(oLine.StartPoint, oLine.EndPoint);

                    LinearEntity3d overlopCuv = segLine2.Overlap(segLine1, tolerance);
                    if (overlopCuv == null)
                    {
                        overlopCuv = segLine1.Overlap(segLine2, tolerance);
                    }
                    if (overlopCuv != null && overlopCuv.HasStartPoint && overlopCuv.HasEndPoint)
                    {
                        resLines.AddRange(RemoveLineInterPart(line, overlopCuv));
                        resLines.AddRange(RemoveLineInterPart(oLine, overlopCuv));
                        lines1.Remove(line);
                        lines2.Remove(oLine);
                        goto RETRY;
                    }
                }
            }
            resLines.AddRange(lines1);
            resLines.AddRange(lines2);
            return resLines;
        }

        /// <summary>
        /// 去除这条线共线部分
        /// </summary>
        /// <param name="line"></param>
        /// <param name="linearEntity"></param>
        /// <returns></returns>
        public List<Line> RemoveLineInterPart(Line line, LinearEntity3d linearEntity)
        {
            double sDis = line.StartPoint.DistanceTo(linearEntity.StartPoint);
            double eDis = line.StartPoint.DistanceTo(linearEntity.EndPoint);
            List<Line> lineLst = new List<Line>();
            if (sDis < eDis)
            {
                if (!line.StartPoint.IsEqualTo(linearEntity.StartPoint))
                {
                    lineLst.Add(new Line(line.StartPoint, linearEntity.StartPoint));
                }
                if (!line.EndPoint.IsEqualTo(linearEntity.EndPoint))
                {
                    lineLst.Add(new Line(line.EndPoint, linearEntity.EndPoint));
                }
            }
            else
            {
                if (!line.StartPoint.IsEqualTo(linearEntity.EndPoint))
                {
                    lineLst.Add(new Line(line.StartPoint, linearEntity.EndPoint));
                }
                if (!line.EndPoint.IsEqualTo(linearEntity.StartPoint))
                {
                    lineLst.Add(new Line(line.EndPoint, linearEntity.StartPoint));
                }
            }

            return lineLst;
        }

        /// <summary>
        /// 创建polyline
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        public Polyline CreatePolyline(List<Line> lines)
        {
            Line firLine = lines.First();
            lines.Remove(firLine);
            List<Point3d> points = new List<Point3d>() { firLine.StartPoint, firLine.EndPoint };
            while (lines.Count > 0)
            {
                var matchLine = lines.Where(x => x.StartPoint.IsEqualTo(points.Last()) || x.EndPoint.IsEqualTo(points.Last())).ToList();
                if (matchLine.Count <= 0)
                {
                    break;
                }

                firLine = matchLine.First();
                if (firLine.StartPoint.IsEqualTo(points.Last()))
                {
                    if (firLine.EndPoint.IsEqualTo(points.First()))
                    {
                        break;
                    }
                    points.Add(firLine.EndPoint);
                }
                else
                {
                    if (firLine.StartPoint.IsEqualTo(points.First()))
                    {
                        break;
                    }
                    points.Add(firLine.StartPoint);
                }
                lines.Remove(firLine);
            }

            Polyline polyline = new Polyline() { Closed = true };
            for (int i = 0; i < points.Count; i++)
            {
                polyline.AddVertexAt(i, points[i].ToPoint2D(), 0, 0, 0);
            }
            return polyline;
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

            return GeUtils.ReovePointOnLine(polygons, tolerance);
        }
    }
}
