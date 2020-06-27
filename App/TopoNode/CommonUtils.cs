using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using DotNetARX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace TopoNode
{
    public class CurveNode
    {
        public LineSegment2d Line2d = null;
        public string LayerName = null;

        public CurveNode(LineSegment2d line2d, string layerName)
        {
            Line2d = line2d;
            LayerName = layerName;
        }
    }

    public class CoEdge
    {
        private bool m_IsErase = false; // 是否已经删除
        public bool IsErase
        {
            get { return m_IsErase; }
            set { m_IsErase = value; }
        }

        private int m_nChange = 0; // 修改的次数
        public int ChangeCount
        {
            get { return m_nChange; }
            set { m_nChange = value; }
        }

        private CurveNode m_coNode = null;
        private List<CoEdge> m_relevantLines = null; // 相关联的线段
        public List<CoEdge> RelevantLines
        {
            get { return m_relevantLines; }
        }

        public CurveNode CoNode
        {
            get { return m_coNode; }
            set { m_coNode = value; }
        }

        public CoEdge(CurveNode curveNode)
        {
            m_coNode = curveNode;
            m_relevantLines = new List<CoEdge>();
        }
    }

    /// <summary>
    /// 共边处理
    /// </summary>
    public class CoEdgeErase
    {
        private List<CoEdge> m_coEdges = null;
        private CoEdgeErase(List<CurveNode> lineNodes)
        {
            m_coEdges = new List<CoEdge>();
            for (int i = 0; i < lineNodes.Count; i++)
            {
                m_coEdges.Add(new CoEdge(lineNodes[i]));
            }
        }

        // 接口调用
        public static List<Line> MakeCoEdgeErase(List<CurveNode> lineNodes)
        {
            var edgeErase = new CoEdgeErase(lineNodes);
            edgeErase.CoRelationPre();
            edgeErase.CoEdgeEraseDo();
            var eraselines = edgeErase.Traverse2Lines();
            return eraselines;
        }

        /// <summary>
        /// 边的关系预处理-类似无向图
        /// </summary>
        private void CoRelationPre()
        {
            for (int i = 0; i < m_coEdges.Count; i++)
            {
                var curEdge = m_coEdges[i];
                if (curEdge.IsErase)
                    continue;

                var curLine = curEdge.CoNode.Line2d;
                var curLinePtS = curLine.StartPoint;
                var curLinePtE = curLine.EndPoint;
                for (int j = i + 1; j < m_coEdges.Count; j++)
                {
                    var nextEdge = m_coEdges[j];
                    if (curEdge.IsErase || nextEdge.IsErase)
                        continue;

                    var nextLine = nextEdge.CoNode.Line2d;
                    var nextLinePtS = nextLine.StartPoint;
                    var nextLinePtE = nextLine.EndPoint;
                    if (CommonUtils.IsAlmostNearZero(CommonUtils.CalAngle(CommonUtils.Vector2XY(curLine.Direction), CommonUtils.Vector2XY(nextLine.Direction)), 1e-6)
                       || CommonUtils.IsAlmostNearZero(CommonUtils.CalAngle(CommonUtils.Vector2XY(curLine.Direction), CommonUtils.Vector2XY(nextLine.Direction.Negate())), 1e-6))
                    {
                        // 重合线
                        if ((CommonUtils.Point2dIsEqualPoint2d(curLinePtS, nextLinePtS, 9e-1) && CommonUtils.Point2dIsEqualPoint2d(curLinePtE, nextLinePtE, 9e-1))
                        || (CommonUtils.Point2dIsEqualPoint2d(curLinePtS, nextLinePtE, 9e-1) && CommonUtils.Point2dIsEqualPoint2d(curLinePtE, nextLinePtS, 9e-1)))
                        {
                            curEdge.IsErase = true;
                        }
                        else if (CommonUtils.IsPointOnSegment(nextLinePtS, curLine, 9e-1) && CommonUtils.IsPointOnSegment(nextLinePtE, curLine, 9e-1)) // 完全包含线nextLine
                        {
                            nextEdge.IsErase = true;
                        }
                        else if (CommonUtils.IsPointOnSegment(curLinePtS, nextLine, 9e-1) && CommonUtils.IsPointOnSegment(curLinePtE, nextLine, 9e-1)) // 完全包含线curLine
                        {
                            curEdge.IsErase = true;
                        }
                        else if (CommonUtils.IsPointOnSegment(nextLinePtS, curLine, 9e-1) || (CommonUtils.IsPointOnSegment(nextLinePtE, curLine, 9e-1))) // 部分包含线
                        {
                            curEdge.RelevantLines.Add(nextEdge);
                            nextEdge.RelevantLines.Add(curEdge);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 部分包含的细分处理
        /// </summary>
        private void CoEdgeEraseDo()
        {
            for (int i = 0; i < m_coEdges.Count; i++)
            {
                var curEdge = m_coEdges[i];
                var curReleEdges = curEdge.RelevantLines;
                if (curEdge.IsErase || curReleEdges.Count == 0)
                    continue;

                // 每次都需要判断，因为相关性会改变
                for (int j = 0; j < curReleEdges.Count; j++)
                {
                    if (curEdge.IsErase)
                        break;

                    var curLine = curEdge.CoNode.Line2d; // 可能会不断修改curEdge中line的值
                    var releEdge = curReleEdges[j];
                    if (releEdge.IsErase)
                        continue;

                    var curLinePtS = curLine.StartPoint;
                    var curLinePtE = curLine.EndPoint;
                    var releLine = releEdge.CoNode.Line2d;
                    var releLinePtS = releLine.StartPoint;
                    var releLinePtE = releLine.EndPoint;

                    // 当前段被相关重合段裁剪
                    if (CommonUtils.IsPointOnSegment(releLinePtS, curLine) && !CommonUtils.IsPointOnSegment(releLinePtE, curLine)) // 部分包含当前线条
                    {
                        if (CommonUtils.IsPointOnSegment(curLinePtS, releLine))
                        {
                            curEdge.CoNode.Line2d = new LineSegment2d(releLinePtS, curLinePtE); // 裁剪当前线段
                        }
                        else if (CommonUtils.IsPointOnSegment(curLinePtE, releLine))
                        {
                            curEdge.CoNode.Line2d = new LineSegment2d(releLinePtS, curLinePtS);
                        }

                        curEdge.ChangeCount++;
                        continue;
                    }
                    else if (CommonUtils.IsPointOnSegment(releLinePtE, curLine) && !CommonUtils.IsPointOnSegment(releLinePtS, curLine)) // 部分包含当前线条
                    {
                        if (CommonUtils.IsPointOnSegment(curLinePtS, releLine))
                        {
                            curEdge.CoNode.Line2d = new LineSegment2d(releLinePtE, curLinePtE);
                        }
                        else if (CommonUtils.IsPointOnSegment(curLinePtE, releLine))
                        {
                            curEdge.CoNode.Line2d = new LineSegment2d(releLinePtE, curLinePtS);
                        }

                        curEdge.ChangeCount++;
                        continue;
                    }

                    // 部分裁剪完出现完全包含或者相同的情况
                    if (curEdge.ChangeCount != 0)
                    {
                        if ((CommonUtils.Point2dIsEqualPoint2d(curLinePtS, releLinePtS) && CommonUtils.Point2dIsEqualPoint2d(curLinePtE, releLinePtE))
                           || (CommonUtils.Point2dIsEqualPoint2d(curLinePtS, releLinePtE) && CommonUtils.Point2dIsEqualPoint2d(curLinePtE, releLinePtS)))
                        {
                            curEdge.IsErase = true;
                        }
                        else if (CommonUtils.IsPointOnSegment(curLinePtS, releLine) && CommonUtils.IsPointOnSegment(curLinePtE, releLine))
                        {
                            curEdge.IsErase = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// data convert
        /// </summary>
        /// <returns></returns>
        private List<Line> Traverse2Lines()
        {
            var lines = new List<Line>();
            for (int i = 0; i < m_coEdges.Count; i++)
            {
                if (m_coEdges[i].IsErase)
                    continue;

                var curveNode = m_coEdges[i].CoNode;
                var line2d = curveNode.Line2d;

                var start = line2d.StartPoint;
                var end = line2d.EndPoint;
                var line = new Line(new Point3d(start.X, start.Y, 0), new Point3d(end.X, end.Y, 0));
                line.Layer = curveNode.LayerName;
                lines.Add(line);
            }

            return lines;
        }
    }

    // 喷淋放置的一些参数
    public enum SprayType
    {
        SPRAYUP = 0,
        SPRAYDOWN = 1,
    }

    // 点选，选线，布线
    public enum PutType
    {
        PICKPOINT = 0,
        CHOOSECURVE = 1,
        DRAWCURVE = 2,
    }

    public class PlaceData
    {
        public double minSprayGap = 1800;
        public double maxSprayGap = 3400;
        public double minWallGap = 100;
        public double maxWallGap = 1700;
        public double minBeamGap = 150;
        public double maxBeamGap = 1800;
        public SprayType type = SprayType.SPRAYUP;
        public PutType putType = PutType.PICKPOINT;
    }

    class CommonUtils
    {
        public const int HashMapCount = 545;
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

        // 翻转方向
        public static List<TopoEdge> ReverseTopoEdges(List<TopoEdge> loop)
        {
            var topoEdges = new List<TopoEdge>();
            for (int i = loop.Count - 1; i >= 0; i--)
            {
                var curEdge = loop[i];
                var curEdgeS = curEdge.Start;
                var curEdgeE = curEdge.End;
                var startDir = curEdge.StartDir;
                var endDir = curEdge.EndDir;
                var curCurve = curEdge.SrcCurve;
                var newEdge = new TopoEdge(curEdge.End, curEdge.Start, curCurve, startDir.Negate(), endDir.Negate());
                topoEdges.Add(newEdge);
            }

            return topoEdges;
        }

        /// <summary>
        /// 数据打撒成直线段
        /// </summary>
        /// <param name="curves"></param>
        /// <returns></returns>
        public static List<LineSegment2d> TesslateCurve2Lines(Curve curve)
        {
            var lines = new List<LineSegment2d>();
            if (curve is Line)
            {
                var line = curve as Line;
                var ptS = line.StartPoint;
                var ptE = line.EndPoint;
                var lineSegment2d = new LineSegment2d(new Point2d(ptS.X, ptS.Y), new Point2d(ptE.X, ptE.Y));
                lines.Add(lineSegment2d);
            }
            else if (curve is Arc)
            {
                var arc = curve as Arc;
                var polyline = arc.Spline.ToPolyline();
                var lineNodes = Polyline2dLines(polyline as Polyline);
                if (lineNodes != null)
                    lines.AddRange(lineNodes);
            }
            else if (curve is Circle)
            {
                var circle = curve as Circle;
                var spline = circle.Spline;
                var polyline = spline.ToPolyline();
                var lineNodes = Polyline2dLines(polyline as Polyline);
                if (lineNodes != null)
                    lines.AddRange(lineNodes);
            }
            else if (curve is Ellipse)
            {
                var ellipse = curve as Ellipse;
                var polyline = ellipse.Spline.ToPolyline();
                var lineNodes = Polyline2dLines(polyline as Polyline);
                if (lineNodes != null)
                    lines.AddRange(lineNodes);
            }
            else if (curve is Polyline)
            {
                var lineNodes = Polyline2dLines(curve as Polyline);
                if (lineNodes != null)
                    lines.AddRange(lineNodes);
            }
            else if (curve is Spline)
            {
                var polyline = (curve as Spline).ToPolyline();
                if (polyline is Polyline)
                {
                    var lineNodes = Polyline2dLines(polyline as Polyline);
                    if (lineNodes != null)
                        lines.AddRange(lineNodes);
                }
            }

            return lines;
        }

        /// <summary>
        /// 删除在curves上的点
        /// </summary>
        /// <param name="pts"></param>
        /// <param name="curves"></param>
        /// <returns></returns>
        public static List<Point3d> ErasePointsOnCurves(List<Point3d> pts, List<Curve> curves)
        {
            if (pts == null || pts.Count == 0)
                return null;

            var validPts = new List<Point3d>();
            foreach (var pt in pts)
            {
                if (!PtOnCurves(pt, curves))
                    validPts.Add(pt);
            }

            return validPts;
        }

        /// <summary>
        /// 点是否在线段上面
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="curves"></param>
        /// <returns></returns>
        public static bool PtOnCurves(Point3d pt, List<Curve> curves, double tole = 1e-2)
        {
            foreach (var curve in curves)
            {
                if (curve is Line)
                {
                    var line = curve as Line;
                    if (IsPointOnLine(pt, line, tole))
                        return true;
                }
                else if (curve is Arc)
                {
                    var arc = curve as Arc;
                    if (IsPointOnArc(pt, arc))
                        return true;
                }
            }

            return false;
        }

        public static List<LineSegment2d> Polyline2dLines(Polyline polyline)
        {
            if (polyline == null)
                return null;

            var lines = new List<LineSegment2d>();
            if (polyline.Closed)
            {
                for (int i = 0; i < polyline.NumberOfVertices; i++)
                {
                    var bulge = polyline.GetBulgeAt(i);
                    if (CommonUtils.IsAlmostNearZero(bulge))
                    {
                        var line2d = polyline.GetLineSegment2dAt(i);
                        lines.Add(line2d);
                    }
                    else
                    {
                        var type = polyline.GetSegmentType(i);
                        if (type == SegmentType.Arc)
                        {
                            var arc3d = polyline.GetArcSegmentAt(i);
                            var normal = arc3d.Normal;
                            var axisZ = Vector3d.ZAxis;
                            var arc = new Arc();
                            if (normal.IsEqualTo(Vector3d.ZAxis.Negate()))
                                arc.CreateArcSCE(arc3d.EndPoint, arc3d.Center, arc3d.StartPoint);
                            else
                                arc.CreateArcSCE(arc3d.StartPoint, arc3d.Center, arc3d.EndPoint);
                            var pline = arc.Spline.ToPolyline();
                            var lineNodes = Polyline2dLines(pline as Polyline);
                            if (lineNodes != null)
                                lines.AddRange(lineNodes);
                        }
                    }
                }
            }
            else
            {
                for (int j = 0; j < polyline.NumberOfVertices - 1; j++)
                {
                    var bulge = polyline.GetBulgeAt(j);
                    if (CommonUtils.IsAlmostNearZero(bulge))
                    {
                        var line2d = polyline.GetLineSegment2dAt(j);
                        lines.Add(line2d);
                    }
                    else
                    {
                        var type = polyline.GetSegmentType(j);
                        if (type == SegmentType.Arc)
                        {
                            var arc3d = polyline.GetArcSegmentAt(j);
                            var normal = arc3d.Normal;
                            var axisZ = Vector3d.ZAxis;
                            var arc = new Arc();
                            if (normal.IsEqualTo(Vector3d.ZAxis.Negate()))
                                arc.CreateArcSCE(arc3d.EndPoint, arc3d.Center, arc3d.StartPoint);
                            else
                                arc.CreateArcSCE(arc3d.StartPoint, arc3d.Center, arc3d.EndPoint);
                            var pline = arc.Spline.ToPolyline();
                            var lineNodes = Polyline2dLines(pline as Polyline);
                            if (lineNodes != null)
                                lines.AddRange(lineNodes);
                        }
                    }
                }
            }

            return lines;
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

        public static bool PtInPolylines(List<Polyline> polylines, Point3d pt)
        {
            if (polylines == null || polylines.Count == 0)
                return false;
            var point2d = new Point2d(pt.X, pt.Y);
            foreach (var poly in polylines)
            {
                if (PtInLoop(poly, point2d))
                    return true;
            }

            return false;
        }

        public static bool HasPolylines(List<Tuple<Point3d, double>> polys, Polyline aimPoly)
        {
            var center = CalCenterPoint(aimPoly);
            var area = Math.Abs(aimPoly.Area);

            foreach (var tuple in polys)
            {
                var inCenter = tuple.Item1;
                var inArea = tuple.Item2;

                if (IsAlmostNearZero(Math.Abs(inArea - area), 1)
                    && Point3dIsEqualPoint3d(center, inCenter, 1e-1))
                    return true;
            }

            polys.Add(new Tuple<Point3d, double>(center, area));
            return false;
        }

        public static Point3d CalCenterPoint(Polyline poly)
        {
            var ptCol = poly.Vertices();
            double xSum = 0;
            double ySum = 0;

            var count = ptCol.Count;
            for (int i = 0; i < count; i++)
            {
                var curPt = ptCol[i];
                xSum += curPt.X;
                ySum += curPt.Y;
            }


            var pt = new Point3d(xSum / count, ySum / count, 0);
            return pt;
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

        public static bool PtInLoop(Polyline polyline, Point2d pt)
        {
            if (polyline.Closed == false)
                return false;

            Point2d end = new Point2d(pt.X + 100000000000, pt.Y);
            LineSegment2d intersectLine = new LineSegment2d(pt, end);
            var ptLst = new List<Point2d>();

            var curves = new List<Curve>();
            for (int i = 0; i < polyline.NumberOfVertices; i++)
            {
                var bulge = polyline.GetBulgeAt(i);
                if (CommonUtils.IsAlmostNearZero(bulge))
                {
                    LineSegment3d line3d = polyline.GetLineSegmentAt(i);
                    curves.Add(new Line(line3d.StartPoint, line3d.EndPoint));
                }
                else
                {
                    var type = polyline.GetSegmentType(i);
                    if (type == SegmentType.Arc)
                    {
                        var arc3d = polyline.GetArcSegmentAt(i);
                        var normal = arc3d.Normal;
                        var axisZ = Vector3d.ZAxis;
                        var arc = new Arc();
                        if (normal.IsEqualTo(Vector3d.ZAxis.Negate()))
                            arc.CreateArcSCE(arc3d.EndPoint, arc3d.Center, arc3d.StartPoint);
                        else
                            arc.CreateArcSCE(arc3d.StartPoint, arc3d.Center, arc3d.EndPoint);
                        curves.Add(arc);
                    }
                }
            }

            Point2d[] intersectPts;
            foreach (var curve in curves)
            {
                if (curve is Line)
                {
                    var line = curve as Line;
                    var lineS = line.StartPoint;
                    var lineE = line.EndPoint;
                    var s2d = new Point2d(lineS.X, lineS.Y);
                    var e2d = new Point2d(lineE.X, lineE.Y);
                    var line2d = new LineSegment2d(s2d, e2d);
                    intersectPts = line2d.IntersectWith(intersectLine);
                }
                else
                {
                    var arc = curve as Arc;
                    var arcS = arc.StartPoint;
                    var arcE = arc.EndPoint;
                    var arcMid = arc.GetPointAtParameter(0.5 * (arc.StartParam + arc.EndParam));
                    var arc2s = new Point2d(arcS.X, arcS.Y);
                    var arc2mid = new Point2d(arcMid.X, arcMid.Y);
                    var arc2e = new Point2d(arcE.X, arcE.Y);
                    CircularArc2d ar = new CircularArc2d(arc2s, arc2mid, arc2e);
                    intersectPts = ar.IntersectWith(intersectLine);
                }

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

        public static bool PtInLoop(List<Curve> loop, Point3d pt)
        {
            Point2d end = new Point2d(pt.X + 100000000000, pt.Y);
            LineSegment2d intersectLine = new LineSegment2d(new Point2d(pt.X, pt.Y), end);
            var ptLst = new List<Point2d>();

            foreach (var curve in loop)
            {
                var ptS = curve.StartPoint;
                var ptE = curve.EndPoint;
                LineSegment2d line = new LineSegment2d(new Point2d(ptS.X, ptS.Y), new Point2d(ptE.X, ptE.Y));
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

            firLeftX -= 0.1;
            firLeftY -= 0.1;
            firRightX += 0.1;
            firRightY += 0.1;

            secLeftX -= 0.1;
            secLeftY -= 0.1;
            secRightX += 0.1;
            secRightY += 0.1;
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

        ///// <summary>
        ///// 删除共边
        ///// </summary>
        ///// <param name="lines"></param>
        ///// <param name="outLines"></param>
        //public static List<Curve> RemoveCollinearLines(List<Curve> srcCurves)
        //{
        //    var outCurves = new List<Curve>();
        //    var lines3d = new List<Line>();
        //    foreach (var curve in srcCurves)
        //    {
        //        if (curve is Line)
        //        {
        //            lines3d.Add(curve as Line);
        //        }
        //        else if (curve is Polyline)
        //        {
        //            var lines = Utils.GetLineFromPolyline(curve as Polyline);
        //            if (lines != null && lines.Count != 0)
        //            {
        //                foreach (var line2d in lines)
        //                {
        //                    lines3d.
        //                }
        //            }

        //        }
        //        else
        //        {
        //            outCurves.Add(curve);
        //        }
        //    }

        //    var lines2d = new List<LineSegment2d>();
        //    foreach (var line in lines3d)
        //    {
        //        var start = line.StartPoint;
        //        var end = line.EndPoint;
        //        lines2d.Add(new LineSegment2d(new Point2d(start.X, start.Y), new Point2d(end.X, end.Y)));
        //    }

        //    var eraseLines = CoEdgeErase.MakeCoEdgeErase(lines2d);

        //    foreach (var line2d in eraseLines)
        //    {
        //        var start = line2d.StartPoint;
        //        var end = line2d.EndPoint;
        //        outCurves.Add(new Line(new Point3d(start.X, start.Y, 0), new Point3d(end.X, end.Y, 0)));
        //    }

        //    return outCurves;
        //}

        /// <summary>
        /// 外轮廓包含内轮廓
        /// </summary>
        /// <param name="outerprofile"></param>
        /// <param name="innerProfile"></param>
        /// <returns></returns>
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

        public static bool OutLoopContainInnerLoop2(List<TopoEdge> outerprofile, List<TopoEdge> innerProfile)
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
            {
                var innerPt = CalInnerPoint.MakeInnerPoint(innerProfile);
                if (PtInLoop(outerprofile, innerPt))
                    return true;
            }

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

                LineSegment2d intersectLine = new LineSegment2d(new Point2d(pt.X, pt.Y), end);
                Point2d[] intersectPts;
                if (edge.IsLine)
                {
                    LineSegment2d line = new LineSegment2d(new Point2d(edge.Start.X, edge.Start.Y), new Point2d(edge.End.X, edge.End.Y));
                    intersectPts = line.IntersectWith(intersectLine);
                }
                else
                {
                    var arc3d = edge.SrcCurve as Arc;
                    var startPt = arc3d.StartPoint;
                    var endPt = arc3d.EndPoint;
                    var midPoint = arc3d.GetPointAtParameter((arc3d.StartParam + arc3d.EndParam) * 0.5);
                    var arc = new CircularArc2d(new Point2d(startPt.X, startPt.Y), new Point2d(midPoint.X, midPoint.Y), new Point2d(endPt.X, endPt.Y));

                    intersectPts = arc.IntersectWith(intersectLine);
                }

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
            long index = ((long)(posX * 23)) % HashMapCount;
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
                if (CommonUtils.IsAlmostNearZero(pt.X - startPt.X, tole))
                {
                    var y1 = Math.Abs(pt.Y - startPt.Y);
                    var y2 = Math.Abs(pt.Y - endPt.Y);
                    if (CommonUtils.IsAlmostNearZero((y1 + y2 - line.Length), tole))
                        return true;
                }
            }
            else if (CommonUtils.IsAlmostNearZero(line.Angle) || CommonUtils.IsAlmostNearZero(line.Angle - Math.PI))
            {
                if (CommonUtils.IsAlmostNearZero(pt.Y - startPt.Y, tole))
                {
                    var X1 = Math.Abs(pt.X - startPt.X);
                    var X2 = Math.Abs(pt.X - endPt.X);
                    if (CommonUtils.IsAlmostNearZero((X1 + X2 - line.Length), tole))
                        return true;
                }
            }
            else
            {
                // 非垂直
                var maxx = Math.Max(startPt.X, endPt.X);
                var minX = Math.Min(startPt.X, endPt.X);

                var maxY = Math.Max(startPt.Y, endPt.Y);
                var minY = Math.Min(startPt.Y, endPt.Y);

                var equal = Math.Abs((pt.X - startPt.X) * (endPt.Y - startPt.Y) - (endPt.X - startPt.X) * (pt.Y - startPt.Y));

                if ((IsAlmostNearZero(equal, tole)) && (pt.X >= minX && pt.X <= maxx) && (pt.Y >= minY && pt.Y <= maxY))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsPointOnArc(Point3d point, Arc arc)
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
                return CommonUtils.IsPointOnLine(point, line, 1e-3);
            }
            else
            {
                var arc = edge.SrcCurve as Arc;
                return CommonUtils.IsPointOnArc(point, arc);
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
            var resLine = new Line(ptSadd, ptEadd);
            resLine.Layer = line.Layer;
            return resLine;
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
            resArc.Layer = arc.Layer;
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
                line.Layer = edge.SrcCurve.Layer;
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
                tmpArc.Layer = arc.Layer;
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
        public static List<Curve> RemoveCollinearLines(List<Curve> srcCurves)
        {
            var outCurves = new List<Curve>();
            var lines3d = new List<Line>();
            foreach (var curve in srcCurves)
            {
                if (curve is Line line)
                {
                    if (!IsAlmostNearZero(line.StartPoint.Z, 1E-10))
                    {
                        var ptS = line.StartPoint;
                        var ptE = line.EndPoint;
                        var lineZ0 = new Line(new Point3d(ptS.X, ptS.Y, 0), new Point3d(ptE.X, ptE.Y, 0));
                        lineZ0.Layer = line.Layer;
                        lines3d.Add(lineZ0);
                    }
                    else
                        lines3d.Add(line);
                }
                else if (curve is Arc arc)
                {
                    if (!IsAlmostNearZero(arc.Center.Z, 1E-10))
                    {
                        var ptS = arc.StartPoint;
                        var ptSZ0 = new Point3d(ptS.X, ptS.Y, 0);
                        var ptMid = arc.GetPointAtParameter(0.5 * (arc.StartParam + arc.EndParam));
                        var ptMidZ0 = new Point3d(ptMid.X, ptMid.Y, 0);
                        var ptE = arc.EndPoint;
                        var ptEZ0 = new Point3d(ptE.X, ptE.Y, 0);

                        var arcZ0 = new Arc();
                        arcZ0.CreateArc(ptSZ0, ptMidZ0, ptEZ0);
                        arcZ0.Layer = arc.Layer;
                        outCurves.Add(arcZ0);
                    }
                    else
                        outCurves.Add(curve);
                }
            }

            var curveNodes = new List<CurveNode>();
            foreach (var line in lines3d)
            {
                var start = line.StartPoint;
                var end = line.EndPoint;
                var line2d = new LineSegment2d(new Point2d(start.X, start.Y), new Point2d(end.X, end.Y));
                var curveNode = new CurveNode(line2d, line.Layer);
                curveNodes.Add(curveNode);
            }

            var eraseLines = CoEdgeErase.MakeCoEdgeErase(curveNodes);

            if (eraseLines != null && eraseLines.Count != 0)
                outCurves.AddRange(eraseLines);

            return outCurves;
        }
    }

    class EdgeLoop
    {
        private int m_nDeep = 0;
        public int Deep
        {
            get { return m_nDeep; }
            set { m_nDeep = value; }
        }

        private List<TopoEdge> m_loop;
        public List<TopoEdge> CurLoop
        {
            get { return m_loop; }
        }

        private List<EdgeLoop> m_childLoops = new List<EdgeLoop>();
        public List<EdgeLoop> ChildLoops
        {
            get { return m_childLoops; }
        }

        public EdgeLoop(List<TopoEdge> edges, int nDeep)
        {
            m_loop = edges;
            m_nDeep = nDeep;
        }
    }

    /// <summary>
    /// loop inner
    /// </summary>
    class LoopEntity
    {
        private List<EdgeLoop> m_edgeLoops;
        private int m_nMaxDeep = 0;
        private List<List<TopoEdge>> m_rootInnerLoop = new List<List<TopoEdge>>(); // root inner loop

        public List<List<List<TopoEdge>>> m_entitys = new List<List<List<TopoEdge>>>();
        public List<List<TopoEdge>> RootInnerLoop
        {
            get { return m_rootInnerLoop; }
        }

        public LoopEntity(List<List<TopoEdge>> loops)
        {
            m_edgeLoops = new List<EdgeLoop>();
            for (int i = 0; i < loops.Count; i++)
            {
                var curLoop = loops[i];
                var nCount = 0;
                for (int j = 0; j < loops.Count; j++)
                {
                    if (i == j)
                        continue;

                    // 被包含的次数
                    if (CommonUtils.OutLoopContainsInnerLoop(loops[j], curLoop))
                        nCount++;
                }

                if (m_nMaxDeep < nCount)
                    m_nMaxDeep = nCount;

                m_edgeLoops.Add(new EdgeLoop(curLoop, nCount));
            }
        }

        // calculate every entity (outLoop innerLoop)
        public void CalcuChild()
        {
            for (int i = 0; i < m_edgeLoops.Count; i++)
            {
                var edgeLoop = m_edgeLoops[i];
                var nDeep = edgeLoop.Deep;
                if (nDeep == m_nMaxDeep)
                    continue;

                for (int j = 0; j < m_edgeLoops.Count; j++)
                {
                    if (i == j)
                        continue;

                    var nextLoop = m_edgeLoops[j];
                    var nextDeep = nextLoop.Deep;
                    if ((nDeep == nextDeep - 1) && (CommonUtils.OutLoopContainsInnerLoop(edgeLoop.CurLoop, nextLoop.CurLoop)))
                    {
                        edgeLoop.ChildLoops.Add(nextLoop);
                    }
                }

                if (nDeep == 0)
                {
                    foreach (var child in edgeLoop.ChildLoops)
                    {
                        m_rootInnerLoop.Add(child.CurLoop);
                    }

                    return;
                }
            }
        }
    }

    /// <summary>
    /// 内轮廓合并处理
    /// </summary>
    class InnerLoopProfile
    {
        class Profile
        {
            public Profile(List<TopoEdge> loop)
            {
                m_loop = loop;
            }

            public bool m_bUse = false;
            public List<TopoEdge> m_loop = null;
            public List<Profile> m_relatedLoops = new List<Profile>();
        }

        private List<List<TopoEdge>> m_srcLoops;
        private List<List<TopoEdge>> m_outLoops = new List<List<TopoEdge>>();
        public List<List<TopoEdge>> OutLoops
        {
            get { return m_outLoops; }
        }

        private List<Profile> m_profiles = new List<Profile>();

        public InnerLoopProfile(List<List<TopoEdge>> edgeLoops)
        {
            m_srcLoops = edgeLoops;
        }

        public void Do()
        {
            for (int i = 0; i < m_srcLoops.Count; i++)
            {
                m_profiles.Add(new Profile(m_srcLoops[i]));
            }

            for (int i = 0; i < m_profiles.Count; i++)
            {
                var curProfile = m_profiles[i];
                for (int j = i + 1; j < m_profiles.Count; j++)
                {
                    var nextProfile = m_profiles[j];
                    if (LoopIntersectLoop(curProfile.m_loop, nextProfile.m_loop))
                    {
                        curProfile.m_relatedLoops.Add(nextProfile);
                        nextProfile.m_relatedLoops.Add(curProfile);
                    }
                }
            }

            var edgeLoops = new List<List<TopoEdge>>();
            // 相邻区域收集
            for (int j = 0; j < m_profiles.Count; j++)
            {
                if (m_profiles[j].m_bUse)
                    continue;

                if (m_profiles[j].m_relatedLoops.Count == 0)
                {
                    m_profiles[j].m_bUse = true;
                    m_outLoops.Add(m_profiles[j].m_loop);
                }
                else
                {
                    var loops = SearchFromOneProfile(m_profiles[j]);
                    edgeLoops.Add(loops);
                }
            }

            foreach (var edges in edgeLoops)
            {
                var curves = TopoUtils.MakeNoScatterProfile(edges);
                curves.Sort((s1, s2) => { return Math.Abs(CommonUtils.CalcuLoopArea(s1)).CompareTo(Math.Abs(CommonUtils.CalcuLoopArea(s2))); });
                List<TopoEdge> edgesN = null;
                // 方向调整
                if (CommonUtils.CalcuLoopArea(curves.Last()) < 0)
                    edgesN = CommonUtils.ReverseTopoEdges(curves.Last());
                else
                    edgesN = curves.Last();

                m_outLoops.Add(edgesN);
            }
        }

        private List<TopoEdge> SearchFromOneProfile(Profile Searchprofile)
        {
            var topoEdges = new List<TopoEdge>();
            var profiles = new List<Profile>();
            profiles.Add(Searchprofile);
            while (profiles.Count != 0)
            {
                var curProfile = profiles.First();
                topoEdges.AddRange(curProfile.m_loop);
                curProfile.m_bUse = true;
                profiles.RemoveAt(0);
                var childProfiles = curProfile.m_relatedLoops;
                foreach (var profile in childProfiles)
                {
                    if (!profile.m_bUse)
                        profiles.Add(profile);
                }
            }

            return topoEdges;
        }

        /// <summary>
        /// 环与环有公共区域
        /// </summary>
        /// <param name="loopFir"></param>
        /// <param name="loopSec"></param>
        /// <returns></returns>
        private bool LoopIntersectLoop(List<TopoEdge> loopFir, List<TopoEdge> loopSec)
        {
            if (loopFir == null || loopFir.Count == 0)
                return false;

            if (loopSec == null || loopSec.Count == 0)
                return false;

            var loopFirCurves = new List<Curve>();
            var loopSecCurves = new List<Curve>();
            foreach (var loopFirCurve in loopFir)
            {
                loopFirCurves.Add(loopFirCurve.SrcCurve);
            }

            foreach (var loopSecCurve in loopSec)
            {
                loopSecCurves.Add(loopSecCurve.SrcCurve);
            }

            foreach (var curve in loopFirCurves)
            {
                if (Utils.CurveIntersectWithLoop(curve, loopSecCurves))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
