using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThSpray
{
    public class CommonUtils
    {
        public const int HashMapCount = 200;
        /// <summary>
        /// 返回值true 时，点在图形内
        /// </summary>
        /// <param name="polyline"></param>
        /// <param name="aimPoint"></param>
        /// <returns></returns>
        public static bool PointInnerEntity(List<LineSegment2d> profile, Point2d aimPoint)
        {
            LineSegment2d horizontalLine = new LineSegment2d(aimPoint, aimPoint + new Vector2d(1, 0) * 100000000);
            List<Point2d> ptLst = new List<Point2d>();

            foreach (var line in profile)
            {
                var intersectPts = line.IntersectWith(horizontalLine);
                if (intersectPts != null && intersectPts.Count() == 1)
                    ptLst.AddRange(intersectPts);
            }

            if (ptLst.Count % 2 == 1)
                return true;

            return false;
        }

        /// <summary>
        /// 计算点在环内
        /// </summary>
        /// <param name="loop"></param>
        /// <param name="pt"></param>
        /// <returns></returns>
        public static bool PtInLoop(List<TopoEdge> loop, Point2d pt)
        {
            Point2d end = new Point2d(pt.X + 100000000000, pt.Y);
            LineSegment2d intersectLine = new LineSegment2d(pt, end);
            var ptLst = new List<Point2d>();

            foreach (var edge in loop)
            {
                LineSegment2d line = new LineSegment2d(edge.Start, edge.End);
                var intersectPts = line.IntersectWith(intersectLine);
                if (intersectPts != null && intersectPts.Count() == 1)
                {
                    var nPt = intersectPts.First();
                    bool bInLst = false;
                    foreach (var curpt in ptLst)
                    {
                        if (CommonUtils.Point2dIsEqualPoint2d(nPt, curpt))
                        {
                            bInLst = true;
                            break;
                        }
                    }

                    if (!bInLst)
                        ptLst.Add(nPt);
                }

            }

            if (ptLst.Count % 2 == 1)
                return true;
            else
                return false;
        }

        public static bool PtInLoop(List<LineSegment2d> loop, Point2d pt)
        {
            Point2d end = new Point2d(pt.X + 100000000000, pt.Y);
            LineSegment2d intersectLine = new LineSegment2d(pt, end);
            var ptLst = new List<Point2d>();

            foreach (var edge in loop)
            {
                LineSegment2d line = new LineSegment2d(edge.StartPoint, edge.EndPoint);
                var intersectPts = line.IntersectWith(intersectLine);
                if (intersectPts != null && intersectPts.Count() == 1)
                {
                    var nPt = intersectPts.First();
                    bool bInLst = false;
                    foreach (var curpt in ptLst)
                    {
                        if (CommonUtils.Point2dIsEqualPoint2d(nPt, curpt))
                        {
                            bInLst = true;
                            break;
                        }
                    }

                    if (!bInLst)
                        ptLst.Add(nPt);
                }

            }

            if (ptLst.Count % 2 == 1)
                return true;
            else
                return false;
        }
        /// <summary>
        /// outerEdge是否包含innerEdge
        /// </summary>
        /// <param name="outerEdge"></param>
        /// <param name="innerEdge"></param>
        /// <returns></returns>
        public static bool OutLoopContainsInnerLoop(List<TopoEdge> outerEdge, List<TopoEdge> innerEdge)
        {
            bool bIn = true;
            foreach (var edge in innerEdge)
            {
                var pt = edge.Start;
                if (!PtInLoop(outerEdge, pt))
                {
                    bIn = false;
                    break;
                }
            }
            return bIn;
        }

        public static bool OutLoopContainsInnerLoop(List<LineSegment2d> outerprofile, List<TopoEdge> innerEdge)
        {
            bool bIn = true;
            foreach (var edge in innerEdge)
            {
                var pt = edge.Start;
                if (!PtInLoop(outerprofile, pt))
                {
                    bIn = false;
                    break;
                }
            }
            return bIn;
        }

        public static bool OutLoopContainsInnerLoop(List<LineSegment2d> outerprofile, List<LineSegment2d> innerEdge)
        {
            bool bIn = true;
            foreach (var edge in innerEdge)
            {
                var pt = edge.StartPoint;
                if (!PtInLoop(outerprofile, pt))
                {
                    bIn = false;
                    break;
                }
            }
            return bIn;
        }

        /// <summary>
        /// 面积计算
        /// </summary>
        /// <param name="loop"></param>
        /// <returns></returns>
        public static double CalcuLoopArea(List<TopoEdge> loop)
        {
            double area = 0.0;

            foreach (var edge in loop)
            {
                Point2d start = edge.Start;
                Point2d end = edge.End;
                area += 0.5 * (start.X * end.Y - start.Y * end.X);
            }

            return area;
        }

        /// <summary>
        /// 面积计算
        /// </summary>
        /// <param name="loop"></param>
        /// <returns></returns>
        public static double CalcuLoopArea(List<LineSegment2d> loop)
        {
            double area = 0.0;

            foreach (var edge in loop)
            {
                Point2d start = edge.StartPoint;
                Point2d end = edge.EndPoint;
                area += 0.5 * (start.X * end.Y - start.Y * end.X);
            }

            return area;
        }

        /// <summary>
        /// 计算形心
        /// </summary>
        /// <param name="edges"></param>
        /// <returns></returns>
        public static Point2d CalculateTopoEdgePos(List<TopoEdge> edges)
        {
            var pt = edges.First().Start;
            for (int j = 1; j < edges.Count; j++)
            {
                pt = CommonUtils.Point2dAddPoint2d(pt, edges[j].Start);
            }

            var ptCen = pt / edges.Count;
            return ptCen;
        }

        public static Point2d CalculateLineSegment2dPos(List<LineSegment2d> edges)
        {
            var pt = edges.First().StartPoint;
            for (int j = 1; j < edges.Count; j++)
            {
                pt = CommonUtils.Point2dAddPoint2d(pt, edges[j].StartPoint);
            }

            var ptCen = pt / edges.Count;
            return ptCen;
        }

        /// <summary>
        /// key值计算
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static int HashKey(Point2d point)
        {
            return ((int)(point.X * 50) % HashMapCount);
        }

        /// <summary>
        /// 调整因为精度问题导致的值范围
        /// </summary>
        /// <param name="rad"></param>
        /// <returns></returns>
        public static double CutRadRange(double rad)
        {
            if (IsAlmostNearZero(rad - 1))
                return 1;
            else if (IsAlmostNearZero(rad + 1))
                return -1;
            return rad;
        }

        /// 零值判断
        public static bool IsAlmostNearZero(double val, double tolerance = 1e-9)
        {
            if (val > -tolerance && val < tolerance)
                return true;
            return false;
        }

        /// <summary>
        /// 判断两个点是否相等
        /// </summary>
        /// <param name="ptFirst"></param>
        /// <param name="ptSecond"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static bool Point2dIsEqualPoint2d(Point2d ptFirst, Point2d ptSecond, double tolerance = 1e-6)
        {
            if (ptFirst.GetDistanceTo(ptSecond) < tolerance)
                return true;
            return false;
        }

        public static bool Point3dIsEqualPoint3d(Point3d ptFirst, Point3d ptSecond, double tolerance = 1e-6)
        {
            if (ptFirst.DistanceTo(ptSecond) < tolerance)
                return true;
            return false;
        }
        /// <summary>
        /// 两个顶点相加
        /// </summary>
        /// <param name="ptFirst"></param>
        /// <param name="ptSecond"></param>
        /// <returns></returns>
        public static Point2d Point2dAddPoint2d(Point2d ptFirst, Point2d ptSecond)
        {
            XY ptFir = new XY(ptFirst.X, ptFirst.Y);
            XY ptSec = new XY(ptSecond.X, ptSecond.Y);
            var res = ptFir + ptSec;
            return new Point2d(res.X, res.Y);
        }

        /// <summary>
        /// 点在线段上面
        /// </summary>
        /// <param name="point"></param>
        /// <param name="line"></param>
        /// <param name="tole"></param>
        /// <returns></returns>
        public static bool IsPointOnSegment(Point2d point, TopoEdge line, double tole = 1e-8)
        {
            var ptS = line.Start;
            var ptE = line.End;
            var lengthS = (ptS - point).Length;
            var lengthE = (ptE - point).Length;
            var lengthDiff = lengthS + lengthE - (ptS - ptE).Length;
            if (CommonUtils.IsAlmostNearZero(lengthDiff, tole))
                return true;

            return false;
        }

        /// <summary>
        /// 点在线段上面
        /// </summary>
        /// <param name="point"></param>
        /// <param name="line"></param>
        /// <param name="tole"></param>
        /// <returns></returns>
        public static bool IsPointOnSegment(Point2d point, LineSegment2d line, double tole = 1e-8)
        {
            var ptS = line.StartPoint;
            var ptE = line.EndPoint;
            var lengthS = (ptS - point).Length;
            var lengthE = (ptE - point).Length;
            var lengthDiff = lengthS + lengthE - (ptS - ptE).Length;
            if (CommonUtils.IsAlmostNearZero(lengthDiff, tole))
                return true;

            return false;
        }

        /// <summary>
        /// 空间夹角计算
        /// </summary>
        /// <param name="dir1"></param>
        /// <param name="dir2"></param>
        /// <returns></returns>
        public static double CalAngle(XY dir1, XY dir2)
        {
            double val = dir1.X * dir2.X + dir1.Y * dir2.Y;
            double tmp = Math.Sqrt(Math.Pow(dir1.X, 2) + Math.Pow(dir1.Y, 2)) * Math.Sqrt(Math.Pow(dir2.X, 2) + Math.Pow(dir2.Y, 2));
            double angleRad = Math.Acos(CommonUtils.CutRadRange(val / tmp));
            return angleRad;
        }

        public static Point3d Pt2Point3d(Point2d point2d)
        {
            var pt = new Point3d(point2d.X, point2d.Y, 0);
            return pt;
        }

        /// <summary>
        /// 平移
        /// </summary>
        /// <param name="line"></param>
        /// <param name="vec"></param>
        /// <returns></returns>
        public static LineSegment2d LineAddVector(LineSegment2d line, Vector2d vec)
        {
            var ptS = line.StartPoint;
            var ptE = line.EndPoint;
            var ptSadd = ptS + vec;
            var ptEadd = ptE + vec;
            return new LineSegment2d(ptSadd, ptEadd);
        }

        /// <summary>
        /// 平移
        /// </summary>
        /// <param name="line"></param>
        /// <param name="vec"></param>
        /// <returns></returns>
        public static LineSegment2d LineDecVector(LineSegment2d line, Vector2d vec)
        {
            var ptS = line.StartPoint;
            var ptE = line.EndPoint;
            var ptSadd = ptS - vec;
            var ptEadd = ptE - vec;
            return new LineSegment2d(ptSadd, ptEadd);
        }

        public static TopoEdge LineDecVector(TopoEdge edge, Vector2d vec)
        {
            var ptS = edge.Start;
            var ptE = edge.End;
            var ptSadd = ptS - vec;
            var ptEadd = ptE - vec;
            return new TopoEdge(ptSadd, ptEadd, CommonUtils.Vector2XY(ptEadd - ptSadd));
        }

        public static Line LineDecVector(Line line, Vector3d vec)
        {
            var ptS = line.StartPoint;
            var ptE = line.EndPoint;
            var ptSadd = ptS - vec;
            var ptEadd = ptE - vec;
            return new Line(ptSadd, ptEadd);
        }

        public static XY Vector2XY(Vector2d vec)
        {
            return new XY(vec.X, vec.Y);
        }

        public static List<Curve> line2d2Curves(List<LineSegment2d> lines)
        {
            var curves = new List<Curve>();
            foreach (var line in lines)
            {
                var ptS = line.StartPoint;
                var ptE = line.EndPoint;
                var ptS3d = new Point3d(ptS.X, ptS.Y, 0);
                var ptE3d = new Point3d(ptE.X, ptE.Y, 0);
                curves.Add(new Line(ptS3d, ptE3d));
            }

            return curves;
        }

        public static List<LineSegment2d> MoveLine2d(List<LineSegment2d> lines, Vector2d vec)
        {
            var resLines = new List<LineSegment2d>();
            foreach (var line in lines)
            {
                resLines.Add(CommonUtils.LineDecVector(line, vec));
            }

            return resLines;
        }
    }
}
