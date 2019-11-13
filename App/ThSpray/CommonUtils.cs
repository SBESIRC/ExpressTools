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

        public static Arc CreateArc(Point3d startPoint, Point3d centerPoint, Point3d endPoint, double radius)
        {
            Vector2d startVector = new Vector2d(startPoint.X - centerPoint.X, startPoint.Y - centerPoint.Y);
            Vector2d endVector = new Vector2d(endPoint.X - centerPoint.X, endPoint.Y - centerPoint.Y);
            var arc = new Arc(centerPoint, radius, startVector.Angle, endVector.Angle);
            return arc;
        }

        public static Arc CreateArcReverse(Point3d startPoint, Point3d centerPoint, Point3d endPoint, double radius, Vector3d normal)
        {
            Vector2d startVector = new Vector2d(startPoint.X - centerPoint.X, startPoint.Y - centerPoint.Y);
            Vector2d endVector = new Vector2d(endPoint.X - centerPoint.X, endPoint.Y - centerPoint.Y);
            var arc = new Arc(centerPoint, normal, radius, startVector.Angle, endVector.Angle);
            return arc;
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

        // assiatant function
        public static List<TopoEdge> ConvertEdges(List<TopoEdge> srcEdges)
        {
            if (srcEdges == null || srcEdges.Count == 0)
                return null;

            var resEdges = new List<TopoEdge>();
            foreach (var edge in srcEdges)
            {
                if (edge.IsLine)
                {
                    resEdges.Add(edge);
                }
                else
                {
                    var arc = edge.SrcCurve as Arc;
                    var startPoint = arc.StartPoint;
                    var endPoint = arc.EndPoint;
                    var center = arc.Center;
                    var radius = arc.Radius;
                    var midPoint = arc.GetPointAtParameter((arc.StartParam + arc.EndParam) * 0.5);
                    var topoEdge1 = new TopoEdge(edge.Start, midPoint, null);
                    var topoEdge2 = new TopoEdge(midPoint, edge.End, null);
                    resEdges.Add(topoEdge1);
                    resEdges.Add(topoEdge2);
                }
            }

            return resEdges;
        }

        public static void CalculateLineBoundary(Line line, ref double leftX, ref double leftY, ref double rightX, ref double rightY)
        {
            var startPt = line.StartPoint;
            var endPt = line.EndPoint;
            if (CommonUtils.IsAlmostNearZero(line.Angle - Math.PI * 0.5) || CommonUtils.IsAlmostNearZero(line.Angle - Math.PI * 1.5))
            {
                leftX = rightX = startPt.X;
                if (startPt.Y <= endPt.Y)
                {
                    leftY = startPt.Y;
                    rightY = endPt.Y;
                }
                else
                {
                    leftY = endPt.Y;
                    rightY = startPt.Y;
                }
            }
            else
            {
                // 非垂直
                if (startPt.X <= endPt.X)
                {
                    leftX = startPt.X;
                    rightX = endPt.X;
                }
                else
                {
                    rightX = startPt.X;
                    leftX = endPt.X;
                }

                if (startPt.Y <= endPt.Y)
                {
                    leftY = startPt.Y;
                    rightY = endPt.Y;
                }
                else
                {
                    rightY = startPt.Y;
                    leftY = endPt.Y;
                }
            }
        }

        public static void CalculateArcBoundary(Arc arc, ref double leftX, ref double leftY, ref double rightX, ref double rightY)
        {
            var ptCenter = arc.Center;
            var radius = arc.Radius;
            var leftPoint = arc.Center + new Vector3d(-1, 0, 0) * radius;
            var rightPoint = arc.Center + new Vector3d(1, 0, 0) * radius;
            leftX = leftPoint.X;
            leftY = leftPoint.Y - radius;
            rightX = rightPoint.X;
            rightY = rightPoint.Y + radius;
        }

        public static bool IntersectValid(Curve firstCurve, Curve secCurve)
        {
            // first
            double firLeftX = 0;
            double firLeftY = 0;
            double firRightX = 0;
            double firRightY = 0;

            // second
            double secLeftX = 0;
            double secLeftY = 0;
            double secRightX = 0;
            double secRightY = 0;
            if (firstCurve is Arc)
            {
                var firstArc = firstCurve as Arc;
                CalculateArcBoundary(firstArc, ref firLeftX, ref firLeftY, ref firRightX, ref firRightY);
            }
            else
            {
                var firLine = firstCurve as Line;
                CalculateLineBoundary(firLine, ref firLeftX, ref firLeftY, ref firRightX, ref firRightY);
            }

            if (secCurve is Arc)
            {
                var secArc = secCurve as Arc;
                CalculateArcBoundary(secArc, ref secLeftX, ref secLeftY, ref secRightX, ref secRightY);
            }
            else
            {
                var secLine = secCurve as Line;
                CalculateLineBoundary(secLine, ref secLeftX, ref secLeftY, ref secRightX, ref secRightY);
            }

            if (Math.Min(firRightX, secRightX) >= Math.Max(firLeftX, secLeftX)
                && Math.Min(firRightY, secRightY) >= Math.Max(firLeftY, secLeftY))
                return true;

            return false;
        }

        public static bool OutLoopContainsInnerLoop(List<LineSegment2d> outerprofile, List<LineSegment2d> innerEdge)
        {
            foreach (var edge in innerEdge)
            {
                var pt = edge.StartPoint;
                if (!PtInLoop(outerprofile, pt))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool OutLoopContainsInnerLoop(List<TopoEdge> outerprofile, List<TopoEdge> innerProfile)
        {
            foreach (var edge in innerProfile)
            {
                var pt = edge.Start;
                if (!PtInLoop(outerprofile, pt))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool OutLoopContainInnerLoop(List<TopoEdge> outerprofile, List<TopoEdge> innerProfile)
        {
            int pointInCount = 0;
            foreach (var edge in innerProfile)
            {
                var pt = edge.Start;
                bool IsEdgePoint = false;

                foreach (var outerEdge in outerprofile)
                {
                    if (CommonUtils.IsPointOnSegment(pt, outerEdge))
                    {
                        pointInCount++;
                        IsEdgePoint = true;
                        break;
                    }
                }

                if (IsEdgePoint == false)
                {
                    if (PtInLoop(outerprofile, pt))
                        return true;
                }
            }

            if (pointInCount == innerProfile.Count)
                return true;
            else
                return false;
        }

        public static bool PtInLoop(List<TopoEdge> loop, Point3d pt)
        {
            var ptLst = new List<Point2d>();
            // first
            double firLeftX = 0;
            double firLeftY = 0;
            double firRightX = 0;
            double firRightY = 0;
            foreach (var edge in loop)
            {
                Point2d end = new Point2d(pt.X + 100000000000, pt.Y);

                var curLine = new Line(pt, new Point3d(end.X, end.Y, 0));

                if (edge.IsLine)
                {
                    var firLine = edge.SrcCurve as Line;
                    CalculateLineBoundary(firLine, ref firLeftX, ref firLeftY, ref firRightX, ref firRightY);

                }
                else
                {
                    var firstArc = edge.SrcCurve as Arc;
                    CalculateArcBoundary(firstArc, ref firLeftX, ref firLeftY, ref firRightX, ref firRightY);
                }

                if (firRightX < pt.X || firRightY < pt.Y || firLeftY > pt.Y)
                    continue;
                //if (!CommonUtils.IntersectValid(curLine, edge.SrcCurve))
                //    continue;

                LineSegment2d intersectLine = new LineSegment2d(new Point2d(pt.X, pt.Y), end);
                LineSegment2d line = new LineSegment2d(new Point2d(edge.Start.X, edge.Start.Y), new Point2d(edge.End.X, edge.End.Y));
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
        /// 面积计算
        /// </summary>
        /// <param name="loop"></param>
        /// <returns></returns>
        public static double CalcuLoopArea(List<TopoEdge> loop)
        {
            double area = 0.0;

            foreach (var edge in loop)
            {
                area += 0.5 * (edge.Start.X * edge.End.Y - edge.Start.Y * edge.End.X);
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
            var pt = new Point2d(edges.First().Start.X, edges.First().Start.Y);
            for (int j = 1; j < edges.Count; j++)
            {
                pt = CommonUtils.Point2dAddPoint2d(pt, new Point2d(edges[j].Start.X, edges[j].Start.Y));
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
        public static int HashKey(Point3d point)
        {
            var posX = point.X;
            if (posX > 1E8)
                posX = posX * 1e-7;
            long index = ((long)(posX * 10) % HashMapCount);
            return (int)index;
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
            //if (ptFirst.GetDistanceTo(new Point2d(0, 0)) > 1e7)
            //{
            //    ptFirst = new Point2d(ptFirst.X * 1e-4, ptFirst.Y * 1e-4);
            //    ptSecond = new Point2d(ptSecond.X * 1e-4, ptSecond.Y * 1e-4);
            //}
            if (CommonUtils.IsAlmostNearZero(ptFirst.X - ptSecond.X, tolerance)
                && CommonUtils.IsAlmostNearZero(ptFirst.Y - ptSecond.Y, tolerance))
                return true;
            return false;
        }

        public static bool Point3dIsEqualPoint3d(Point3d ptFirst, Point3d ptSecond, double tolerance = 1e-6)
        {
            if (CommonUtils.IsAlmostNearZero(ptFirst.X - ptSecond.X, tolerance)
                && CommonUtils.IsAlmostNearZero(ptFirst.Y - ptSecond.Y, tolerance))
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

        public static Point3d Point3dAddPoint3d(Point3d ptFirst, Point3d ptSecond)
        {
            XY ptFir = new XY(ptFirst.X, ptFirst.Y);
            XY ptSec = new XY(ptSecond.X, ptSecond.Y);
            var res = ptFir + ptSec;
            return new Point3d(res.X, res.Y, 0);
        }

        ///// <summary>
        ///// 点在线段上面
        ///// </summary>
        ///// <param name="point"></param>
        ///// <param name="line"></param>
        ///// <param name="tole"></param>
        ///// <returns></returns>
        //public static bool IsPointOnSegment(Point2d point, TopoEdge line, double tole = 1e-8)
        //{
        //    var ptS = line.Start;
        //    var ptE = line.End;
        //    var lengthS = (ptS - point).Length;
        //    var lengthE = (ptE - point).Length;
        //    var lengthDiff = lengthS + lengthE - (ptS - ptE).Length;
        //    if (CommonUtils.IsAlmostNearZero(lengthDiff, tole))
        //        return true;

        //    return false;
        //}

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

        public static bool IsPointOnLine(Point3d pt, Line line, double tole = 1e-8)
        {
            var startPt = line.StartPoint;
            var endPt = line.EndPoint;

            if (CommonUtils.IsAlmostNearZero(line.Angle - Math.PI * 0.5) || CommonUtils.IsAlmostNearZero(line.Angle - Math.PI * 1.5))
            {
                var y1 = Math.Abs(pt.Y - startPt.Y);
                var y2 = Math.Abs(pt.Y - endPt.Y);
                if (CommonUtils.IsAlmostNearZero((y1 + y2 - line.Length), tole))
                    return true;
            }
            else
            {
                // 非垂直
                var y1 = Math.Abs(pt.Y - startPt.Y);
                var y2 = Math.Abs(pt.Y - endPt.Y);
                var vertical = Math.Abs(endPt.Y - startPt.Y) - (y1 + y2);
                var x1 = Math.Abs(pt.X - startPt.X);
                var x2 = Math.Abs(pt.X - endPt.X);
                var horizontal = Math.Abs(endPt.X - startPt.X) - (x1 + x2);
                if (CommonUtils.IsAlmostNearZero(vertical, tole) && CommonUtils.IsAlmostNearZero(horizontal, tole))
                    return true;
            }

            return false;
        }

        public static bool IsPointOnArc(Point3d point, Arc arc, double tole = 1e-8)
        {
            try
            {
                var param = arc.GetParameterAtPoint(point);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static bool IsPointOnSegment(Point3d point, TopoEdge edge, double tole = 1e-8)
        {
            if (edge.IsLine)
            {
                var line = edge.SrcCurve as Line;
                return CommonUtils.IsPointOnLine(point, line, tole);
            }
            else
            {
                var arc = edge.SrcCurve as Arc;
                return CommonUtils.IsPointOnArc(point, arc, tole);
            }
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

        public static Line LineAddVector(Line line, Vector2d vec)
        {
            var moveVec = new Vector3d(vec.X, vec.Y, 0);
            var ptS = line.StartPoint;
            var ptE = line.EndPoint;
            var ptSadd = ptS + moveVec;
            var ptEadd = ptE + moveVec;
            return new Line(ptSadd, ptEadd);
        }

        public static Point3d ptAddVector(Point3d pt, Vector2d vec)
        {
            var moveVec = new Vector3d(vec.X, vec.Y, 0);
            var ptRes = pt + moveVec;
            return ptRes;
        }

        public static Arc ArcAddVector(Arc arc, Vector2d vec)
        {
            var ptS = ptAddVector(arc.StartPoint, vec);
            var ptE = ptAddVector(arc.EndPoint, vec);
            var ptCenter = ptAddVector(arc.Center, vec);
            Vector2d startVector = new Vector2d(ptS.X - ptCenter.X, ptS.Y - ptCenter.Y);
            Vector2d endVector = new Vector2d(ptE.X - ptCenter.X, ptE.Y - ptCenter.Y);
            var resArc = new Arc(ptCenter, arc.Radius, startVector.Angle, endVector.Angle);
            return resArc;
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

        public static TopoEdge LineDecVector(TopoEdge edge, Vector2d srcVec)
        {
            var vec = new Vector3d(srcVec.X, srcVec.Y, 0);

            if (edge.IsLine)
            {
                var ptS = edge.Start;
                var ptE = edge.End;
                var ptSadd = ptS - vec;
                var ptEadd = ptE - vec;
                var line = new Line(ptSadd, ptEadd);
                var dir = line.GetFirstDerivative(ptSadd);
                // 直线是真实的数据
                return new TopoEdge(ptSadd, ptEadd, line, edge.StartDir, edge.EndDir);
            }
            else
            {
                var arc = edge.SrcCurve as Arc;
                var ptS = arc.StartPoint;
                var ptE = arc.EndPoint;
                var ptSadd = ptS - vec;
                var ptEadd = ptE - vec;
                var radius = arc.Radius;
                var ptCenter = arc.Center;
                var ptCenterAdd = ptCenter - vec;
                var tmpArc = CommonUtils.CreateArc(ptSadd, ptCenterAdd, ptEadd, radius);
                if (CommonUtils.Point3dIsEqualPoint3d(ptS, edge.Start))
                {
                    return new TopoEdge(ptSadd, ptEadd, tmpArc, edge.StartDir, edge.EndDir);
                }
                else if (CommonUtils.Point3dIsEqualPoint3d(ptS, edge.End))
                {
                    return new TopoEdge(ptEadd, ptSadd, tmpArc, edge.StartDir, edge.EndDir);
                }
            }

            return null;
        }

        public static Line LineDecVector(Line line, Vector3d vec)
        {
            var ptS = line.StartPoint;
            var ptE = line.EndPoint;
            var ptSadd = ptS - vec;
            var ptEadd = ptE - vec;
            return new Line(ptSadd, ptEadd);
        }

        public static XY Vector2XY(Vector3d vec)
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
