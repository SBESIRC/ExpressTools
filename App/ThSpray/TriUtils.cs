using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using TianHua.AutoCAD.Utility.ExtensionTools;
using DotNetARX;
using Linq2Acad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AcHelper;

namespace ThSpray
{
    /// <summary>
    /// 对polyline 进行区域分割
    /// </summary>
    public class SpaceSplit
    {
        private List<Curve> srcCurves = null;

        private List<Point3d> splitPoints = new List<Point3d>(); // 三角分割点
        public SpaceSplit(List<Curve> curves)
        {
            srcCurves = curves;
        }

        public static List<Point3d> MakeSplitPoints(List<Curve> curves)
        {
            var splitCal = new SpaceSplit(curves);
            splitCal.Do();
            return splitCal.splitPoints;
        }

        private void Do()
        {
            var profileNodes = new List<TriUtils.ProfileNode>();
            foreach (var curve in srcCurves)
            {
                var poly = curve as Polyline;
                var profileNode = TriUtils.ConvertPolyline2PathNodes(poly);

                if (profileNode != null)
                {
                    TriUtils.EraseSplitAndMergePoint(profileNode, profileNode.profile);
                    var polyLst = TriUtils.MakeMonotonePolygon(profileNode);
                    var triangles = new List<TriUtils.Triangle>();

                    if (polyLst != null && polyLst.Count != 0)
                    {
                        var trians = TriUtils.Convert2TriangleSelf(polyLst.First());
                        if (trians != null && trians.Count != 0)
                        {
                            double x = 0.0;
                            double y = 0.0;
                            var ptLst = trians.First().vertexs;
                            for (int i = 0; i < ptLst.Count; i++)
                            {
                                x += ptLst[i].X;
                                y += ptLst[i].Y;
                            }

                            x /= ptLst.Count;
                            y /= ptLst.Count;
                            splitPoints.Add(new Point3d(x, y, 0));
                        }
                    }
                }
            }
        }
    }
    public static class TriUtils
    {
        /// <summary>
        /// 节点类型
        /// </summary>
        public enum NodeType
        {
            Unknown = 0,
            RegularPoint = 1, // 非拐点
            StartPoint = 2, // 下面4类是拐点
            MergePoint = 3,
            SplitPoint = 4,
            EndPoint = 5,
        }

        public class TriangleSearch
        {
            private List<Triangle> srcTriangles;
            private Triangle startTrian = null;

            public List<List<TriangleEdge>> edgesLst = new List<List<TriangleEdge>>(); // 路径集
            public List<List<Triangle>> trianglesLst = new List<List<Triangle>>(); // 路径集
            public TriangleSearch(List<Triangle> triangles, Triangle srcStartTrian)
            {
                srcTriangles = triangles;
                startTrian = srcStartTrian;
            }

            public static List<Curve> MakeTriangleSearch(List<Triangle> triangles, Triangle srcStartTrian)
            {
                var search = new TriangleSearch(triangles, srcStartTrian);
                search.CalcuRelation();
                search.Do2();

                //int i = 0;
                //foreach (var drawTriangles in search.trianglesLst)
                //{
                //    i++;
                //    var layerName = "tri";
                //    layerName += i;
                //    TriUtils.DrawCurvesTriangle(drawTriangles, layerName);
                //}

                var curves = search.CalPath();

                TriUtils.DrawCurvesAdd(curves);
                return curves;
            }

            private void CalcuRelation()
            {
                for (int i = 0; i < srcTriangles.Count; i++)
                {
                    var curTriangle = srcTriangles[i];
                    if (curTriangle.relatedTriangles.Count > 2)
                        continue;

                    for (int j = i + 1; j < srcTriangles.Count; j++)
                    {
                        var nextTriangle = srcTriangles[j];
                        TriangleWithTriangle(curTriangle, nextTriangle);
                    }
                }
            }

            private void Do()
            {
                List<Triangle> resTotalEdges = new List<Triangle>();
                List<Point3d> ptLst = new List<Point3d>();
                resTotalEdges.Add(startTrian);
                ptLst.AddRange(startTrian.vertexs);
                SearchFromCurTriangle2(startTrian, resTotalEdges, ptLst);
            }

            private void Do2()
            {
                List<Triangle> resTotalEdges = new List<Triangle>();
                List<Point3d> ptLst = new List<Point3d>();
                SearchFromCurTriangle2(startTrian, resTotalEdges, ptLst);
            }

            private List<Curve> CalPath()
            {
                var curves = new List<Curve>();
                foreach (var triangles in trianglesLst)
                {
                    var poly = new Polyline();
                    for (int i = 0; i < triangles.Count - 1; i++)
                    {
                        var curTri = triangles[i];
                        var nextTri = triangles[i + 1];
                        var coEdge = CalCoEdge(curTri, nextTri);
                        if (coEdge == null)
                            continue;
                        var ptOne = coEdge.ptOne;
                        var ptTwo = coEdge.ptTwo;
                        var mid = new Point2d((ptOne.X + ptTwo.X) * 0.5, (ptOne.Y + ptTwo.Y) * 0.5);
                        poly.AddVertexAt(i, mid, 0, 0, 0);
                    }

                    curves.Add(poly);
                    //edgesLst.Add(edges);
                }

                return curves;
            }

            private TriangleEdge CalCoEdge(Triangle triFir, Triangle triSec)
            {
                for (int i = 0; i < triFir.CoEdges.Count; i++)
                {
                    var edgeFir = triFir.CoEdges[i];
                    for (int j = 0; j < triSec.CoEdges.Count; j++)
                    {
                        var edgeSec = triSec.CoEdges[j];
                        if ((Point3dIsEqualPoint3d(edgeFir.ptOne, edgeSec.ptOne) && Point3dIsEqualPoint3d(edgeFir.ptTwo, edgeSec.ptTwo))
                            || (Point3dIsEqualPoint3d(edgeFir.ptOne, edgeSec.ptTwo) && Point3dIsEqualPoint3d(edgeFir.ptTwo, edgeSec.ptOne)))
                            return edgeFir;
                    }
                }

                return null;
            }

            private void SearchFromCurTriangle2(Triangle startTrian, List<Triangle> resTotalEdges, List<Point3d> ptLst)
            {
                if (startTrian.relatedTriangles.Count == 0)
                {
                    trianglesLst.Add(resTotalEdges);
                    return;
                }

                resTotalEdges.Add(startTrian);
                foreach (var pt in startTrian.vertexs)
                {
                    if (!ptLst.Contains(pt))
                        ptLst.Add(pt);
                }

                if (startTrian.relatedTriangles.Count == 1)
                {
                    if (VertexContain(ptLst, startTrian.relatedTriangles.First().vertexs))
                    {
                        trianglesLst.Add(resTotalEdges);
                        return;
                    }
                }

                for (int i = 0; i < startTrian.relatedTriangles.Count; i++)
                {
                    var curTrian = startTrian.relatedTriangles[i];
                    if (VertexContain(ptLst, curTrian.vertexs))
                    {
                        continue;
                    }

                    var addEdges = new List<Triangle>();
                    addEdges.AddRange(resTotalEdges);
                    var addPtLst = new List<Point3d>();
                    addPtLst.AddRange(ptLst);
                    SearchFromCurTriangle2(curTrian, addEdges, addPtLst);
                }
            }

            private void SearchFromCurTriangle(Triangle startTrian, List<Triangle> resTotalEdges, List<Point3d> ptLst)
            {
                if (startTrian.relatedTriangles.Count == 0)
                {
                    trianglesLst.Add(resTotalEdges);
                    resTotalEdges.RemoveAt(resTotalEdges.Count - 1);
                    ptLst.RemoveAt(ptLst.Count - 1);
                    return;
                }

                if (startTrian.relatedTriangles.Count == 1)
                {
                    if (VertexContain(ptLst, startTrian.relatedTriangles.First().vertexs))
                    {
                        resTotalEdges.RemoveAt(resTotalEdges.Count - 1);
                        ptLst.RemoveAt(ptLst.Count - 1);
                        trianglesLst.Add(resTotalEdges);
                        return;
                    }
                }

                for (int i = 0; i < startTrian.relatedTriangles.Count; i++)
                {
                    var curTrian = startTrian.relatedTriangles[i];
                    if (VertexContain(ptLst, curTrian.vertexs))
                    {
                        continue;
                    }

                    resTotalEdges.Add(curTrian);
                    foreach (var pt in curTrian.vertexs)
                    {
                        if (!ptLst.Contains(pt))
                            ptLst.Add(pt);
                    }
                    var addEdges = new List<Triangle>();
                    addEdges.AddRange(resTotalEdges);
                    var addPtLst = new List<Point3d>();
                    addPtLst.AddRange(ptLst);
                    SearchFromCurTriangle(curTrian, addEdges, addPtLst);
                }

                resTotalEdges.RemoveAt(resTotalEdges.Count - 1);
                ptLst.RemoveAt(ptLst.Count - 1);
            }

            private bool VertexContain(List<Point3d> ptLst, List<Point3d> vectexs)
            {
                for (int i = 0; i < vectexs.Count; i++)
                {
                    if (!ptLst.Contains(vectexs[i]))
                        return false;
                }

                return true;
            }

            private void TriangleWithTriangle(Triangle triFir, Triangle triSec)
            {
                for (int i = 0; i < triFir.edges.Count; i++)
                {
                    var triFirEdge = triFir.edges[i];
                    for (int j = 0; j < triSec.edges.Count; j++)
                    {
                        var triSecEdge = triSec.edges[j];

                        if (IsEdgeEqual(triFirEdge, triSecEdge))
                        {
                            triFir.relatedTriangles.Add(triSec);
                            triSec.relatedTriangles.Add(triFir);
                            triFir.CoEdges.Add(triFirEdge);
                            triSec.CoEdges.Add(triFirEdge);
                            return;
                        }
                    }
                }
            }

            private bool IsEdgeEqual(TriangleEdge edgeFir, TriangleEdge edgeSec)
            {
                if ((Point3dIsEqualPoint3d(edgeFir.ptOne, edgeSec.ptOne) && Point3dIsEqualPoint3d(edgeFir.ptTwo, edgeSec.ptTwo))
                    || (Point3dIsEqualPoint3d(edgeFir.ptOne, edgeSec.ptTwo) && Point3dIsEqualPoint3d(edgeFir.ptTwo, edgeSec.ptOne)))
                    return true;
                return false;
            }
        }


        public class ValidMergeNode
        {
            public ValidMergeNode(PathNode node, List<PathNode> nodes, List<LineSegment2d> ptLines)
            {
                srcNode = node;
                nearNodes = nodes;
                profile = ptLines;
            }

            public void Do()
            {
                foreach (var vertex in nearNodes)
                {
                    var ptFir = srcNode.CurPoint;
                    var ptNext = vertex.CurPoint;
                    var line2d = new LineSegment2d(new Point2d(ptFir.X, ptFir.Y), new Point2d(ptNext.X, ptNext.Y));
                    var midPoint = new Point2d((ptFir.X + ptNext.X) * 0.5, (ptFir.Y + ptNext.Y) * 0.5);
                    if (LineInLoop(profile, line2d) && PtInLoop(profile, midPoint))
                    {
                        resNodes.Add(vertex);
                    }
                }
            }

            public static List<PathNode> MakeValidNodes(PathNode node, List<PathNode> nodes, List<LineSegment2d> ptLines)
            {
                var nodeCal = new ValidMergeNode(node, nodes, ptLines);
                nodeCal.Do();
                return nodeCal.resNodes;
            }

            public List<PathNode> resNodes = new List<PathNode>();
            private List<LineSegment2d> profile = null;
            private PathNode srcNode = null;
            private List<PathNode> nearNodes = null;
        }

        public enum LoopDir
        {
            ANTICLOCKWISE = 1,
            CLOCKWISE = 2,
        }

        public class TriangleEdge
        {
            public Point3d ptOne;
            public Point3d ptTwo;
            public TriangleEdge(Point3d ptFir, Point3d ptSec)
            {
                ptOne = ptFir;
                ptTwo = ptSec;
            }
        }

        /// <summary>
        /// 三角形
        /// </summary>
        public class Triangle
        {
            public List<TriangleEdge> edges;
            public Triangle(List<TriangleEdge> triEdges)
            {
                edges = triEdges;
            }

            public List<Point3d> vertexs = new List<Point3d>();
            public List<Triangle> relatedTriangles = new List<Triangle>(); // 和当前三角形相连的三角形
            public List<TriangleEdge> CoEdges = new List<TriangleEdge>(); // 和父节点的共边数据
        }

        /// <summary>
        /// 二维平面坐标及其计算处理
        /// </summary>
        public class XY
        {
            private double m_x;
            private double m_y;
            public XY(double x, double y)
            {
                m_x = x;
                m_y = y;
            }

            /// <summary>
            /// 计算有向角度
            /// </summary>
            /// <param name="dir"></param>
            /// <returns></returns>
            public double CalAngle(XY dir)
            {
                double val = m_x * dir.X + m_y * dir.Y;
                double tmp = Math.Sqrt(Math.Pow(m_x, 2) + Math.Pow(m_y, 2)) * Math.Sqrt(Math.Pow(dir.X, 2) + Math.Pow(dir.Y, 2));
                double angleRad = Math.Acos(CutRadRange(val / tmp));

                if (CrossProduct(dir) < 0)
                    return -angleRad;
                return angleRad;
            }

            public bool IsEqualTo(XY pt)
            {
                var pt1 = new Point2d(m_x, m_y);
                var pt2 = new Point2d(pt.X, pt.Y);
                if (pt1.IsEqualTo(pt2))
                    return true;
                return false;
            }
            private double CrossProduct(XY dir)
            {
                return (m_x * dir.Y - m_y * dir.X);
            }

            public double GetLength()
            {
                var length = Math.Sqrt(Math.Pow(m_x, 2) + Math.Pow(m_y, 2));
                return length;
            }

            public static XY operator -(XY left, XY right)
            {
                return new XY(left.X - right.X, left.Y - right.Y);
            }

            public static XY operator +(XY left, XY right)
            {
                return new XY(left.X + right.X, left.Y + right.Y);
            }

            public double X
            {
                get { return m_x; }
                set { m_x = value; }
            }

            public double Y
            {
                get { return m_y; }
                set { m_y = value; }
            }
        }

        public class ProfileNode
        {
            public ProfileNode(List<PathNode> pathNodes, LoopDir dir, List<LineSegment2d> lines)
            {
                ProfilePath = pathNodes;
                ProfileDir = dir;
                profile = lines;
            }

            public List<LineSegment2d> profile = null;
            public List<PathNode> ProfilePath
            {
                get;
                set;
            }

            public LoopDir ProfileDir
            {
                get;
                set;
            }
        }

        public class PathNode
        {
            public PathNode BeforeNode;
            public Point3d CurPoint;
            public PathNode NextNode;

            //在队列中的索引值
            public int Index
            {
                get;
                set;
            }

            // 新增加的与当前相连接的Node节点
            public List<PathNode> RelatedPoints
            {
                get;
                set;
            } = null;

            public NodeType Type
            {
                get;
                set;
            }

            public bool IsValid
            {
                get;
                set;
            } = true;

            /// <summary>
            /// 逆时针轮廓下一条边
            /// </summary>
            /// <returns></returns>
            public PathNode CalculateNextPathNodeNoFirstCCW()
            {
                if (RelatedPoints == null)
                {
                    return NextNode;
                }
                else
                {
                    var beforePt = BeforeNode.CurPoint;
                    var dirFir = new Vector2d(CurPoint.X - beforePt.X, CurPoint.Y - beforePt.Y);
                    double maxAngle = double.MinValue;
                    PathNode aimNode = NextNode;
                    for (int i = 0; i < RelatedPoints.Count; i++)
                    {
                        var nextPt = RelatedPoints[i].CurPoint;
                        var dirSec = new Vector2d(nextPt.X - CurPoint.X, nextPt.Y - CurPoint.Y);
                        var curAgnle = CalAngle(dirFir, dirSec);
                        if (curAgnle > maxAngle)
                        {
                            maxAngle = curAgnle;
                            aimNode = RelatedPoints[i];
                        }
                    }

                    return aimNode;
                }
            }

            /// <summary>
            /// 逆时针第一条边
            /// </summary>
            /// <returns></returns>
            public PathNode CalculateNextCCW()
            {
                if (RelatedPoints == null)
                {
                    return NextNode;
                }
                else
                {
                    var beforePt = BeforeNode.CurPoint;
                    var dirFir = new Vector2d(CurPoint.X - beforePt.X, CurPoint.Y - beforePt.Y);
                    double maxAngle = double.MinValue;
                    PathNode aimNode = NextNode;
                    for (int i = 0; i < RelatedPoints.Count; i++)
                    {
                        var nextPt = RelatedPoints[i].CurPoint;
                        var dirSec = new Vector2d(nextPt.X - CurPoint.X, nextPt.Y - CurPoint.Y);
                        var curAgnle = CalAngle(dirFir, dirSec);
                        if (curAgnle > maxAngle)
                        {
                            maxAngle = curAgnle;
                            aimNode = RelatedPoints[i];
                        }
                    }

                    return aimNode;
                }
            }

            public class NextPath
            {
                public PathNode ptHead;
                public PathNode ptTail;
                public double angle;

                public Vector2d vec;
                public NextPath(PathNode head, PathNode tail)
                {
                    ptHead = head;
                    ptTail = tail;
                    vec = new Vector2d(tail.CurPoint.X - head.CurPoint.X, tail.CurPoint.Y - head.CurPoint.Y);
                }
            }

            /// <summary>
            /// 逆时针轮廓下一条边
            /// </summary>
            /// <returns></returns>
            public PathNode CalculateNextPathNodeCCW(Point3d ptHead, Point3d ptTail)
            {
                if (RelatedPoints == null)
                {
                    return NextNode;
                }
                else
                {
                    var nextPathLst = new List<NextPath>();
                    if (!Point3dIsEqualPoint3d(BeforeNode.CurPoint, ptHead))
                        nextPathLst.Add(new NextPath(this, BeforeNode));
                    if (!Point3dIsEqualPoint3d(NextNode.CurPoint, ptHead))
                        nextPathLst.Add(new NextPath(this, NextNode));

                    foreach (var node in RelatedPoints)
                    {
                        if (!Point3dIsEqualPoint3d(node.CurPoint, ptHead))
                            nextPathLst.Add(new NextPath(this, node));
                    }

                    var dirFir = new Vector2d(ptTail.X - ptHead.X, ptTail.Y - ptHead.Y);
                    foreach (var tmpPath in nextPathLst)
                    {
                        tmpPath.angle = CalAngle(dirFir, tmpPath.vec);
                    }

                    nextPathLst = nextPathLst.OrderBy(s => s.angle).ToList();
                    return nextPathLst.Last().ptTail;
                }
            }

            ///// <summary>
            ///// 顺时针轮廓下一条边
            ///// </summary>
            ///// <returns></returns>
            //public PathNode CalculateNextPathNodeCW()
            //{
            //    if (RelatedPoints == null)
            //    {
            //        return BeforeNode;
            //    }
            //    else
            //    {

            //    }
            //}

            public PathNode(Point3d curPt, NodeType nodeType = NodeType.RegularPoint)
            {
                CurPoint = curPt;
                Type = nodeType;
            }

            public void SetBeforeNode(PathNode beforeNode)
            {
                BeforeNode = beforeNode;
            }

            public void SetNextNode(PathNode nextNode)
            {
                NextNode = nextNode;
            }

            public PathNode(PathNode beforeNode, Point3d curPt, PathNode nextNode, NodeType nodeType = NodeType.RegularPoint)
            {
                BeforeNode = beforeNode;
                CurPoint = curPt;
                NextNode = nextNode;
                Type = nodeType;
            }
        }

        public static double CalAngle(Vector2d dirFir, Vector2d dirSec)
        {
            double val = dirFir.X * dirSec.X + dirFir.Y * dirSec.Y;
            double tmp = Math.Sqrt(Math.Pow(dirFir.X, 2) + Math.Pow(dirFir.Y, 2)) * Math.Sqrt(Math.Pow(dirSec.X, 2) + Math.Pow(dirSec.Y, 2));
            double angleRad = Math.Acos(CutRadRange(val / tmp));

            if (CrossProduct(dirFir, dirSec) < 0)
                return -angleRad;

            return angleRad;
        }


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

        public static double CrossProduct(Vector2d dirFirst, Vector2d dirSec)
        {
            return (dirFirst.X * dirSec.Y - dirFirst.Y * dirSec.X);
        }

        public static double CrossProduct(Vector3d dirFirst, Vector3d dirSec)
        {
            return (dirFirst.X * dirSec.Y - dirFirst.Y * dirSec.X);
        }

        class NodeComparer : IComparer<PathNode>
        {
            public int Compare(PathNode cur, PathNode next)
            {
                if (cur.CurPoint.Y > next.CurPoint.Y)
                    return -1;
                else if (cur.CurPoint.Y < next.CurPoint.Y)
                {
                    return 1;
                }

                if (cur.CurPoint.X > next.CurPoint.X)
                    return 1;
                else if (cur.CurPoint.X < next.CurPoint.X)
                    return -1;

                return 0;
            }
        }

        public static List<Curve> GetAllCurves()
        {
            List<Curve> curves = null;
            using (var db = AcadDatabase.Active())
            {
                // curve 读取
                curves = db.ModelSpace.OfType<Curve>().ToList<Curve>();
            }

            return curves;
        }

        public static double CalculateArea(Point3dCollection ptLst)
        {
            double area = 0.0;
            for (int i = 0; i < ptLst.Count; i++)
            {
                var start = ptLst[i];
                var end = ptLst[(i + 1) % ptLst.Count];
                area += 0.5 * (start.X * end.Y - start.Y * end.X);
            }

            return area;
        }

        public static double CalculateArea(List<Point3d> ptLst)
        {
            double area = 0.0;
            for (int i = 0; i < ptLst.Count; i++)
            {
                var start = ptLst[i];
                var end = ptLst[(i + 1) % ptLst.Count];
                area += 0.5 * (start.X * end.Y - start.Y * end.X);
            }

            return area;
        }

        public static List<Point3d> DealHeightPts(List<Point3d> srcPtLst)
        {
            var resPtLst = new List<Point3d>();
            var delta = 0.05;
            int cEqual = 0;
            for (int i = 0; i < srcPtLst.Count; i++)
            {
                var curPoint = srcPtLst[i];
                var nextPoint = srcPtLst[((i + 1) % srcPtLst.Count)];
                if (curPoint.Y == nextPoint.Y)
                {
                    cEqual++;
                    curPoint += new Vector3d(0, 1, 0) * (cEqual * delta);
                }

                resPtLst.Add(curPoint);
            }

            return resPtLst;
        }

        public static List<Point3d> CombineCollinearPoints(List<Point3d> srcPtLst)
        {
            var resPtLst = new List<Point3d>();
            for (int i = 0; i < srcPtLst.Count; i++)
            {
                var beforePt = srcPtLst[((i - 1 + srcPtLst.Count) % srcPtLst.Count)];
                var curPoint = srcPtLst[i];
                var nextPoint = srcPtLst[((i + 1) % srcPtLst.Count)];
                if (!IsPointOnSegment(beforePt, nextPoint, curPoint))
                    resPtLst.Add(curPoint);
            }

            return resPtLst;
        }

        public static bool IsPointOnSegment(Point3d pointFir, Point3d pointSec, Point3d aimPoint, double tole = 1e-8)
        {
            var lengthS = (pointFir - aimPoint).Length;
            var lengthE = (pointSec - aimPoint).Length;
            var lengthDiff = lengthS + lengthE - (pointFir - pointSec).Length;
            if (IsAlmostNearZero(lengthDiff, tole))
                return true;

            return false;
        }

        public static List<Point3d> EraseSmallLengthPoint(List<Point3d> ptLst)
        {
            if (ptLst == null || ptLst.Count == 0)
                return null;

            var resPtLst = new List<Point3d>();
            resPtLst.Add(ptLst.First());
            for (int i = 1; i < ptLst.Count; i++)
            {
                var curPt = ptLst[i];
                var lastPt = resPtLst.Last();
                if (Math.Abs(curPt.X - lastPt.X) > 1 || Math.Abs(curPt.Y - lastPt.Y) > 1)
                {
                    resPtLst.Add(curPt);
                }
            }

            if (resPtLst.Count > 0)
            {
                var resPtLast = resPtLst.Last();
                if (Math.Abs(resPtLast.X - resPtLst.First().X) < 1 && Math.Abs(resPtLast.Y - resPtLst.First().Y) < 1)
                {
                    resPtLst.RemoveAt(resPtLst.Count - 1);
                }
            }
            return resPtLst;
        }

        /// <summary>
        /// 转化为节点
        /// </summary>
        /// <param name="poly"></param>
        /// <returns></returns>
        public static ProfileNode ConvertPolyline2PathNodes(Polyline poly)
        {
            if (poly == null || poly.Vertices().Count < 3)
                return null;

            var ptLst = new List<Point3d>();
            var splitCurves = Utils.Polyline2Lines(poly);
            for (int i = 0; i < splitCurves.Count; i++)
            {
                var curLine = splitCurves[i];
                ptLst.Add(new Point3d(curLine.StartPoint.X, curLine.StartPoint.Y, 0));

                if (i == splitCurves.Count - 1)
                    ptLst.Add(new Point3d(curLine.EndPoint.X, curLine.EndPoint.Y, 0));
            }

            ptLst = TriUtils.EraseSmallLengthPoint(ptLst);
            ptLst = CombineCollinearPoints(ptLst);
            ptLst = DealHeightPts(ptLst);
            
            if (CalculateArea(ptLst) < 0)
            {
                ptLst.Reverse();
            }

            var pathNodes = new List<PathNode>();

            for (int i = 0; i < ptLst.Count; i++)
            {
                var beforePt = ptLst[((i - 1 + ptLst.Count) % ptLst.Count)];
                var curPoint = ptLst[i];
                var nextPoint = ptLst[((i + 1) % ptLst.Count)];
                pathNodes.Add(new PathNode(curPoint));
            }

            for (int j = 0; j < pathNodes.Count; j++)
            {
                var beforeNode = pathNodes[((j - 1 + pathNodes.Count) % pathNodes.Count)];
                var curNode = pathNodes[j];
                var nextNode = pathNodes[((j + 1) % pathNodes.Count)];
                curNode.SetBeforeNode(beforeNode);
                curNode.SetNextNode(nextNode);
            }

            var dir = CalcuLoopDir(pathNodes);
            DefinePathNodes(pathNodes, dir);

            pathNodes.Sort(new NodeComparer());
            for (int k = 0; k < pathNodes.Count; k++)
            {
                pathNodes[k].Index = k;
            }

            var polyline = CalLines(ptLst);
            return new ProfileNode(pathNodes, dir, polyline);
        }

        /// <summary>
        /// 删除汇合和分裂顶点 逆时针寻找最大角（有正负角之分）
        /// </summary>
        /// <param name="profileNode"></param>
        public static void EraseSplitAndMergePoint(ProfileNode profileNode, List<LineSegment2d> profile)
        {
            var pathNodes = profileNode.ProfilePath;
            for (int i = 0; i < pathNodes.Count; i++)
            {
                var pathNode = pathNodes[i];
                if (pathNode.Type == NodeType.MergePoint)
                {
                    DealWithMergePoint2(pathNode, i, pathNodes, profile);
                }
                else if (pathNode.Type == NodeType.SplitPoint)
                {
                    DealWithSplitPoint(pathNode, i, pathNodes, profile);
                }
            }
        }

        /// <summary>
        /// 处理汇合顶点
        /// </summary>
        /// <param name="pathNode"></param>
        /// <param name="index"></param>
        /// <param name="pathNodes"></param>
        public static void DealWithMergePoint(PathNode pathNode, int index, List<PathNode> pathNodes)
        {
            var nextPathNode = pathNodes[index + 1];
            if (pathNode.RelatedPoints == null)
            {
                pathNode.RelatedPoints = new List<PathNode>();
            }
            pathNode.RelatedPoints.Add(nextPathNode);
            pathNode.Type = NodeType.RegularPoint;

            if (nextPathNode.RelatedPoints == null)
            {
                nextPathNode.RelatedPoints = new List<PathNode>();
            }

            nextPathNode.RelatedPoints.Add(pathNode);
            if (nextPathNode.Type == NodeType.SplitPoint)
                nextPathNode.Type = NodeType.RegularPoint;
        }

        public static void DealWithMergePoint2(PathNode pathNode, int index, List<PathNode> pathNodes, List<LineSegment2d> profile)
        {
            List<PathNode> leftEdge = new List<PathNode>();
            List<PathNode> rightEdge = new List<PathNode>();

            foreach (var node in pathNodes)
            {
                var beforeNode = node.BeforeNode;
                var nextNode = node.NextNode;
                if (node.Index >= index)
                    break;

                if (beforeNode.Index > index || nextNode.Index > index)
                    FindNearIntersectEdge(pathNode, node, leftEdge, rightEdge);
            }

            PathNode nodeRight = null;
            if (rightEdge.First().Index > index)
            {
                nodeRight = rightEdge.First();
            }
            else if (rightEdge.Last().Index > index)
            {
                nodeRight = rightEdge.Last();
            }

            PathNode nodeLeft = null;
            if (leftEdge.First().Index > index)
            {
                nodeLeft = leftEdge.First();
            }
            else if (leftEdge.Last().Index > index)
            {
                nodeLeft = leftEdge.Last();
            }

            // 中间没有其他点，则任意选
            if (nodeLeft.NextNode.Index == nodeRight.Index)
            {
                if (pathNode.RelatedPoints == null)
                {
                    pathNode.RelatedPoints = new List<PathNode>();
                }

                pathNode.RelatedPoints.Add(nodeLeft);
                pathNode.Type = NodeType.RegularPoint;

                if (nodeLeft.RelatedPoints == null)
                {
                    nodeLeft.RelatedPoints = new List<PathNode>();
                }

                nodeLeft.RelatedPoints.Add(pathNode);
            }
            else
            {
                var pathLst = CClockWiseFromPointToEnd(nodeLeft, nodeRight);

                pathLst = ValidMergeNode.MakeValidNodes(pathNode, pathLst, profile); 
                pathLst.Sort(new NodeComparer());
                var maxNode = pathLst.First();

                if (pathNode.RelatedPoints == null)
                {
                    pathNode.RelatedPoints = new List<PathNode>();
                }

                pathNode.RelatedPoints.Add(maxNode);
                pathNode.Type = NodeType.RegularPoint;

                if (maxNode.RelatedPoints == null)
                {
                    maxNode.RelatedPoints = new List<PathNode>();
                }

                if (maxNode.Type == NodeType.SplitPoint)
                    maxNode.Type = NodeType.RegularPoint;
                maxNode.RelatedPoints.Add(pathNode);
            }
        }

        /// <summary>
        /// 处理分裂顶点
        /// </summary>
        /// <param name="pathNode"></param>
        /// <param name="index"></param>
        public static void DealWithSplitPoint(PathNode pathNode, int index, List<PathNode> pathNodes, List<LineSegment2d> profile)
        {
            List<PathNode> leftEdge = new List<PathNode>();
            List<PathNode> rightEdge = new List<PathNode>();

            foreach (var node in pathNodes)
            {
                var beforeNode = node.BeforeNode;
                var nextNode = node.NextNode;
                if (node.Index >= index)
                    break;

                if (beforeNode.Index > index || nextNode.Index > index)
                    FindNearIntersectEdge(pathNode, node, leftEdge, rightEdge);
            }

            PathNode nodeRight = null;
            if (rightEdge.Count == 0 || leftEdge.Count == 0)
                return;
            if (rightEdge.First().Index < index)
            {
                nodeRight = rightEdge.First();
            }
            else if (rightEdge.Last().Index < index)
            {
                nodeRight = rightEdge.Last();
            }

            PathNode nodeLeft = null;
            if (leftEdge.First().Index < index)
            {
                nodeLeft = leftEdge.First();
            }
            else if (leftEdge.Last().Index < index)
            {
                nodeLeft = leftEdge.Last();
            }

            // 中间没有其他点，则任意选
            if (nodeRight.NextNode.Index == nodeLeft.Index)
            {
                if (pathNode.RelatedPoints == null)
                {
                    pathNode.RelatedPoints = new List<PathNode>();
                }

                pathNode.RelatedPoints.Add(nodeLeft);
                pathNode.Type = NodeType.RegularPoint;

                if (nodeLeft.RelatedPoints == null)
                {
                    nodeLeft.RelatedPoints = new List<PathNode>();
                }

                nodeLeft.RelatedPoints.Add(pathNode);
            }
            else
            {
                var pathLst = CClockWiseFromPointToEnd(nodeRight, nodeLeft);
                pathLst = ValidMergeNode.MakeValidNodes(pathNode, pathLst, profile);
                pathLst.RemoveAt(0);
                pathLst.RemoveAt(pathLst.Count - 1);
                pathLst.Sort(new NodeComparer());
                var minNode = pathLst.Last();

                if (pathNode.RelatedPoints == null)
                {
                    pathNode.RelatedPoints = new List<PathNode>();
                }

                pathNode.RelatedPoints.Add(minNode);
                pathNode.Type = NodeType.RegularPoint;

                if (minNode.RelatedPoints == null)
                {
                    minNode.RelatedPoints = new List<PathNode>();
                }
                minNode.RelatedPoints.Add(pathNode);
            }
        }

        public static List<LineSegment2d> CalLines(List<Point3d> ptLst)
        {
            if (ptLst == null || ptLst.Count == 0)
                return null;

            var ptLst2d = new List<Point2d>();
            foreach (var pt in ptLst)
            {
                ptLst2d.Add(new Point2d(pt.X, pt.Y));
            }
            var lines = new List<LineSegment2d>();
            for (int i = 0; i < ptLst2d.Count; i++)
            {
                var curPoint = ptLst2d[i];
                var nextPoint = ptLst2d[((i + 1) % ptLst2d.Count)];
                lines.Add(new LineSegment2d(curPoint, nextPoint));
            }
            return lines;
        }
        public static bool Point3dIsEqualPoint3d(Point3d ptFirst, Point3d ptSecond, double tolerance = 1e-6)
        {
            if (IsAlmostNearZero(ptFirst.X - ptSecond.X, tolerance)
                && IsAlmostNearZero(ptFirst.Y - ptSecond.Y, tolerance))
                return true;
            return false;
        }

        public static bool Point2dIsEqualPoint2d(Point2d ptFirst, Point2d ptSecond, double tolerance = 1e-6)
        {
            if (IsAlmostNearZero(ptFirst.X - ptSecond.X, tolerance)
                && IsAlmostNearZero(ptFirst.Y - ptSecond.Y, tolerance))
                return true;
            return false;
        }

        /// <summary>
        /// 寻找相邻的左右边
        /// </summary>
        /// <param name="curNode"></param>
        /// <param name="aimNode"></param>
        /// <param name="leftEdges"></param>
        /// <param name="rightEdges"></param>
        public static void FindNearIntersectEdge(PathNode curNode, PathNode aimNode, List<PathNode> leftEdge, List<PathNode> rightEdge)
        {
            var curX = curNode.CurPoint.X;
            var edge = new List<PathNode>();
            double leftMidX = 0;
            if (leftEdge.Count > 1)
                leftMidX = (leftEdge.First().CurPoint.X + leftEdge.Last().CurPoint.X) * 0.5;

            double rightMidX = 0;
            if (rightEdge.Count > 1)
                rightMidX = (rightEdge.First().CurPoint.X + rightEdge.Last().CurPoint.X) * 0.5;

            Point3d maxPoint;
            double intersectX;
            if (aimNode.BeforeNode.Index > curNode.Index)
            {
                maxPoint = aimNode.BeforeNode.CurPoint;
            }
            else
            {
                maxPoint = aimNode.NextNode.CurPoint;
            }

            var ratio = (curNode.CurPoint.Y - aimNode.CurPoint.Y) / (maxPoint.Y - aimNode.CurPoint.Y);
            var vec = (maxPoint - curNode.CurPoint).GetNormal();
            intersectX = curX + vec.X * (ratio * (Math.Abs(maxPoint.X - curX)));
            // left
            if (intersectX < curX)
            {
                edge.Add(aimNode);
                edge.Add(aimNode.NextNode);

                if (leftEdge.Count == 0)
                {
                    leftEdge.AddRange(edge);
                }
                else
                {
                    var edgeMidX = (edge.First().CurPoint.X + leftEdge.Last().CurPoint.X) * 0.5;
                    if (edgeMidX > leftMidX)
                    {
                        leftEdge.Clear();
                        leftEdge.AddRange(edge);
                    }
                }
            }
            else if (intersectX > curX)
            {
                // right
                edge.Add(aimNode);
                edge.Add(aimNode.BeforeNode);

                if (rightEdge.Count == 0)
                {
                    rightEdge.AddRange(edge);
                }
                else
                {
                    var edgeMidX = (edge.First().CurPoint.X + edge.Last().CurPoint.X) * 0.5;
                    if (edgeMidX < rightMidX)
                    {
                        rightEdge.Clear();
                        rightEdge.AddRange(edge);
                    }
                }
            }
        }

        /// <summary>
        /// 轮廓方向
        /// </summary>
        /// <param name="loop"></param>
        /// <returns></returns>
        public static LoopDir CalcuLoopDir(List<PathNode> loop)
        {
            double area = 0.0;

            foreach (var edge in loop)
            {
                Point3d start = edge.CurPoint;
                Point3d end = edge.NextNode.CurPoint;
                area += 0.5 * (start.X * end.Y - start.Y * end.X);
            }

            if (area > 0)
            {
                return LoopDir.ANTICLOCKWISE;
            }
            else
            {
                return LoopDir.CLOCKWISE;
            }
        }

        /// <summary>
        /// 定义每个node类型
        /// </summary>
        /// <param name="pathNodes"></param>
        /// <returns></returns>
        public static void DefinePathNodes(List<PathNode> pathNodes, LoopDir dir)
        {
            if (dir == LoopDir.ANTICLOCKWISE)
            {
                foreach (var node in pathNodes)
                {
                    DefineAntiDirNode(node);
                }
            }
            else
            {
                foreach (var node in pathNodes)
                {
                    DefineClockWiseNode(node);
                }
            }

            //PostProcessPathNodes(pathNodes);
        }

        public static void PostProcessPathNodes(List<PathNode> pathNodes)
        {
            foreach (var pathNode in pathNodes)
            {

            }
        }

        /// <summary>
        /// 定义逆时针pathNode点
        /// </summary>
        /// <param name="pathNode"></param>
        public static void DefineAntiDirNode(PathNode pathNode)
        {
            var beforePt = pathNode.BeforeNode.CurPoint;
            var curPoint = pathNode.CurPoint;
            var nextPoint = pathNode.NextNode.CurPoint;

            // 多凹和多凸将分的更细-不影响正确性
            if (beforePt.Y >= curPoint.Y && nextPoint.Y >= curPoint.Y)
            {
                // 向下凹
                var beforeDir = new Vector2d(curPoint.X - beforePt.X, curPoint.Y - beforePt.Y);
                var nextDir = new Vector2d(nextPoint.X - curPoint.X, nextPoint.Y - curPoint.Y);
                var angle = CalAngle(beforeDir, nextDir);
                if (angle < 0)
                {
                    if (beforePt.Y != curPoint.Y)
                        pathNode.Type = NodeType.MergePoint;
                }
                else if (angle > 0)
                {
                    if (nextPoint.Y != curPoint.Y)
                        pathNode.Type = NodeType.EndPoint;
                }
            }
            else if (beforePt.Y <= curPoint.Y && nextPoint.Y <= curPoint.Y)
            {
                // 向上凸
                var beforeDir = new Vector2d(curPoint.X - beforePt.X, curPoint.Y - beforePt.Y);
                var nextDir = new Vector2d(nextPoint.X - curPoint.X, nextPoint.Y - curPoint.Y);
                var angle = CalAngle(beforeDir, nextDir);
                if (angle < 0)
                {
                    if (beforePt.Y != curPoint.Y)
                        pathNode.Type = NodeType.SplitPoint;
                }
                else if (angle > 0)
                {
                    if (nextPoint.Y != curPoint.Y)
                        pathNode.Type = NodeType.StartPoint;
                }
            }
        }

        /// <summary>
        /// 定义顺时针pathNode点
        /// </summary>
        /// <param name="pathNode"></param>
        public static void DefineClockWiseNode(PathNode pathNode)
        {
            var beforePt = pathNode.BeforeNode.CurPoint;
            var curPoint = pathNode.CurPoint;
            var nextPoint = pathNode.NextNode.CurPoint;

            if (beforePt.Y >= curPoint.Y && nextPoint.Y > curPoint.Y)
            {
                pathNode.Type = NodeType.MergePoint;
            }
            else if (beforePt.Y < curPoint.Y && nextPoint.Y <= curPoint.Y)
            {
                pathNode.Type = NodeType.StartPoint;
            }
            else if (beforePt.Y > curPoint.Y && nextPoint.Y >= curPoint.Y)
            {
                pathNode.Type = NodeType.EndPoint;
            }
            else if (beforePt.Y <= curPoint.Y && nextPoint.Y < curPoint.Y)
            {
                pathNode.Type = NodeType.SplitPoint;
            }
        }

        public static List<Curve> Polyline2dLines(Polyline polyline)
        {
            if (polyline == null)
                return null;

            var lines = new List<Curve>();
            if (polyline.Closed)
            {
                for (int i = 0; i < polyline.NumberOfVertices; i++)
                {
                    var bulge = polyline.GetBulgeAt(i);
                    if (IsAlmostNearZero(bulge))
                    {
                        LineSegment3d line3d = polyline.GetLineSegmentAt(i);
                        lines.Add(new Line(line3d.StartPoint, line3d.EndPoint));
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
                            lines.Add(arc);
                        }
                    }
                }
            }
            else
            {
                for (int j = 0; j < polyline.NumberOfVertices - 1; j++)
                {
                    try
                    {
                        var bulge = polyline.GetBulgeAt(j);
                        if (IsAlmostNearZero(bulge))
                        {
                            LineSegment3d line3d = polyline.GetLineSegmentAt(j);
                            lines.Add(new Line(line3d.StartPoint, line3d.EndPoint));
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
                                lines.Add(arc);
                            }
                        }
                    }
                    catch
                    { }
                }
            }

            return lines;
        }

        public static bool LineInLoop(List<LineSegment2d> loop, LineSegment2d line)
        {
            var ptLst = new List<Point2d>();
            ptLst.Add(line.StartPoint);
            ptLst.Add(line.EndPoint);

            foreach (var edge in loop)
            {
                var intersectPts = line.IntersectWith(edge);
                if (intersectPts != null && intersectPts.Count() == 1)
                {
                    var nPt = intersectPts.First();
                    if (!ptLst.Contains(nPt))
                        ptLst.Add(nPt);
                }

            }

            if (ptLst.Count == 2)
                return true;

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
                        if (Point2dIsEqualPoint2d(nPt, curpt))
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
        /// 单调多边形
        /// </summary>
        /// <param name="profileNode"></param>
        /// <returns></returns>
        public static List<List<PathNode>> MakeMonotonePolygon(ProfileNode profileNode)
        {
            var profileLst = new List<List<PathNode>>();
            var pathNodes = profileNode.ProfilePath;

            foreach (var pathNode in pathNodes)
            {
                if (pathNode.IsValid)
                {
                    pathNode.IsValid = false;
                    var paths = CClockWiseFromPoint(pathNode);
                    profileLst.Add(paths);
                }
            }
            return profileLst;
        }

        public static void DrawCurvesTriangleEdge(List<List<TriangleEdge>> edgesLst)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                for (int i = 0; i < edgesLst.Count; i++)
                {
                    var edges = edgesLst[i];
                    var curves = new List<Curve>();
                    for (int j = 0; j < edges.Count; j++)
                    {
                        var edge = edges[j];
                        var line = new Line(edge.ptOne, edge.ptTwo);
                        curves.Add(line);
                        acadDatabase.ModelSpace.Add(line);
                    }
                    
                }
            }
        }

        public static void DrawCurvesTriangle(List<Triangle> triangles, string layerName)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                CreateLayer(layerName, Color.FromRgb(255, 0, 0));
                for (int i = 0; i < triangles.Count; i++)
                {
                    var angle = triangles[i];
                    for (int j = 0; j < angle.edges.Count; j++)
                    {
                        var edge = angle.edges[j];
                        var line = new Line(edge.ptOne, edge.ptTwo);
                        var objId = acadDatabase.ModelSpace.Add(line);
                        acadDatabase.ModelSpace.Element(objId, true).Layer = layerName;
                    }

                }
            }
        }

        public static void CreateLayer(string aimLayer, Color color)
        {
            LayerTableRecord layerRecord = null;
            using (var db = AcadDatabase.Active())
            {
                foreach (var layer in db.Layers)
                {
                    if (layer.Name.Equals(aimLayer))
                    {
                        layerRecord = db.Layers.Element(aimLayer);
                        break;
                    }
                }

                // 创建新的图层
                if (layerRecord == null)
                {
                    layerRecord = db.Layers.Create(aimLayer);
                    layerRecord.Color = color;
                    layerRecord.IsPlottable = false;
                }
            }
        }

        /// <summary>
        /// 绘制
        /// </summary>
        /// <param name="curves"></param>
        public static void DrawCurvesAdd(List<Curve> curves, Color color = null)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                foreach (var curve in curves)
                {
                    // curve.UpgradeOpen();
                    if (color == null)
                        curve.Color = Color.FromRgb(255, 0, 0);
                    else
                        curve.Color = color;
                    // 添加到modelSpace中

                    acadDatabase.ModelSpace.Add(curve);
                }
            }
        }


        public static void DrawPointsAdd(List<Point3d> ptLst, Color color = null)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                foreach (var pt in ptLst)
                {
                    var line1 = new Line(pt, pt + new Vector3d(1, 0, 0) * 10);
                    var line2 = new Line(pt, pt + new Vector3d(0, 1, 0) * 10);
                    // curve.UpgradeOpen();
                    if (color == null)
                    {
                        line1.Color = Color.FromRgb(255, 0, 0);
                        line2.Color = Color.FromRgb(255, 0, 0);
                    }
                    else
                    {
                        line1.Color = color;
                        line2.Color = color;
                    }
                    // 添加到modelSpace中

                    acadDatabase.ModelSpace.Add(line1);
                    acadDatabase.ModelSpace.Add(line2);
                }
            }
        }

        public static void DrawCurvesAdd(List<PathNode> pathNodes)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                foreach (var pathNode in pathNodes)
                {
                    var pt = pathNode.CurPoint;
                    var line1 = new Line(pt, pt + new Vector3d(1, 0, 0) * 10);
                    var line2 = new Line(pt, pt + new Vector3d(0, 1, 0) * 10);
                    // curve.UpgradeOpen();
                    line1.Color = Color.FromRgb(0, 255, 255);
                    // 添加到modelSpace中
                    acadDatabase.ModelSpace.Add(line1);
                    line2.Color = Color.FromRgb(0, 255, 255);
                    acadDatabase.ModelSpace.Add(line2);
                }
            }
        }

        public static void DrawCurvesAdd(List<Point3d> pathNodes)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                foreach (var pathNode in pathNodes)
                {
                    var line1 = new Line(pathNode, pathNode + new Vector3d(1, 0, 0) * 10);
                    var line2 = new Line(pathNode, pathNode + new Vector3d(0, 1, 0) * 10);
                    // curve.UpgradeOpen();
                    line1.Color = Color.FromRgb(0, 255, 255);
                    // 添加到modelSpace中
                    acadDatabase.ModelSpace.Add(line1);
                    line2.Color = Color.FromRgb(0, 255, 255);
                    acadDatabase.ModelSpace.Add(line2);
                }
            }
        }

        public static Polyline ConverToPolylon(List<PathNode> polygon)
        {
            var poly = new Polyline();
            poly.Closed = true;
            for (int i = 0; i < polygon.Count; i++)
            {
                var point = polygon[i].CurPoint;
                poly.AddVertexAt(i, new Point2d(point.X, point.Y), 0, 0, 0);
            }

            return poly;
        }

        public static Polyline ConvertPoints2Polyline(List<Point3d> ptLst)
        {
            var poly = new Polyline();
            poly.Closed = true;
            for (int i = 0; i < ptLst.Count; i++)
            {
                var point = ptLst[i];
                poly.AddVertexAt(i, new Point2d(point.X, point.Y), 0, 0, 0);
            }

            return poly;
        }

        public static Polyline ConvertPoints2Polyline(List<Curve> curves)
        {
            var poly = new Polyline();
            poly.Closed = true;
            for (int i = 0; i < curves.Count; i++)
            {
                var point = curves[i].StartPoint;
                poly.AddVertexAt(i, new Point2d(point.X, point.Y), 0, 0, 0);
            }

            return poly;
        }

        public static List<Point3d> CalTriangleLine(List<PathNode> pathNodes)
        {
            if (pathNodes == null || pathNodes.Count == 0)
                return null;

            var resPathNodes = RegePathNodes(pathNodes);
            var pointLst = new List<Point3d>();
            resPathNodes.Sort(new NodeComparer());
            var stack = new Stack<PathNode>();
            stack.Push(resPathNodes[0]);
            stack.Push(resPathNodes[1]);

            for (int i = 2; i < resPathNodes.Count; i++)
            {
                var curPathNode = resPathNodes[i];
                var curPoint = curPathNode.CurPoint;
                var vertex = stack.Last();
                var oneEdgeNode = stack.First();

                // 另一边
                if (Point3dIsEqualPoint3d(curPoint, vertex.BeforeNode.CurPoint)
                    || Point3dIsEqualPoint3d(curPoint, vertex.NextNode.CurPoint))
                {
                    while (stack.Count != 0)
                    {
                        var ptLst = new List<Point3d>();
                        var popNode = stack.Pop(); // 弹出的
                        if (stack.Count == 0)
                            break;

                        var curTopNode = stack.First(); // 当前顶端

                        var center = new Point3d((curPoint.X + popNode.CurPoint.X) / 2.0, (curPoint.Y + popNode.CurPoint.Y) / 2.0, 0);
                        pointLst.Add(center);
                    }

                    stack.Push(resPathNodes[i - 1]);
                    stack.Push(resPathNodes[i]);
                }
                else
                {
                    if (Point3dIsEqualPoint3d(curPoint, oneEdgeNode.BeforeNode.CurPoint)) // 同一边 右侧
                    {
                        while (stack.Count != 0)
                        {
                            var popNode = stack.Pop(); // 弹出的
                            if (stack.Count == 0)
                            {
                                stack.Push(popNode);
                                stack.Push(curPathNode);
                                break;
                            }
                            var curTopNode = stack.First(); // 当前顶端
                            var vecFrom = popNode.CurPoint - curTopNode.CurPoint;
                            var vecTo = curPoint - popNode.CurPoint;
                            // 右侧向上拐
                            if (CrossProduct(vecFrom, vecTo) >= 0)
                            {
                                stack.Push(popNode);
                                stack.Push(curPathNode);
                                break;
                            }
                            else if (CrossProduct(vecFrom, vecTo) < 0) // 右侧向下拐，增加引线
                            {
                                var center = new Point3d((curPoint.X + popNode.CurPoint.X) / 2.0, (curPoint.Y + popNode.CurPoint.Y) / 2.0, 0);
                                pointLst.Add(center);
                            }
                        }
                    }
                    else if (Point3dIsEqualPoint3d(curPoint, oneEdgeNode.NextNode.CurPoint)) // 同一边 左侧
                    {
                        while (stack.Count != 0)
                        {
                            var popNode = stack.Pop(); // 弹出的
                            if (stack.Count == 0)
                            {
                                stack.Push(popNode);
                                stack.Push(curPathNode);
                                break;
                            }
                            var curTopNode = stack.First(); // 当前顶端
                            var vecFrom = popNode.CurPoint - curTopNode.CurPoint;
                            var vecTo = curPoint - popNode.CurPoint;
                            // 左侧向上拐
                            if (CrossProduct(vecFrom, vecTo) <= 0)
                            {
                                stack.Push(popNode);
                                stack.Push(curPathNode);
                                break;
                            }
                            else if (CrossProduct(vecFrom, vecTo) > 0) // 左侧向下拐，增加引线
                            {
                                var center = new Point3d((curPoint.X + popNode.CurPoint.X) / 2.0, (curPoint.Y + popNode.CurPoint.Y) / 2.0, 0);
                                pointLst.Add(center);
                            }
                        }
                    }
                }
            }

            return pointLst;
        }

        public static List<Point3d> CalTriangleCenter(List<PathNode> pathNodes)
        {
            if (pathNodes == null || pathNodes.Count == 0)
                return null;

            var resPathNodes = RegePathNodes(pathNodes);
            var pointLst = new List<Point3d>();
            resPathNodes.Sort(new NodeComparer());
            var stack = new Stack<PathNode>();
            stack.Push(resPathNodes[0]);
            stack.Push(resPathNodes[1]);

            for (int i = 2; i < resPathNodes.Count; i++)
            {
                var curPathNode = resPathNodes[i];
                var curPoint = curPathNode.CurPoint;
                var vertex = stack.Last();
                var oneEdgeNode = stack.First();

                // 另一边
                if (Point3dIsEqualPoint3d(curPoint, vertex.BeforeNode.CurPoint)
                    || Point3dIsEqualPoint3d(curPoint, vertex.NextNode.CurPoint))
                {
                    while (stack.Count != 0)
                    {
                        var ptLst = new List<Point3d>();
                        var popNode = stack.Pop(); // 弹出的
                        if (stack.Count == 0)
                            break;

                        var curTopNode = stack.First(); // 当前顶端

                        var center = new Point3d((curPoint.X + curTopNode.CurPoint.X + popNode.CurPoint.X) / 3.0, (curPoint.Y + curTopNode.CurPoint.Y + popNode.CurPoint.Y) / 3.0, 0);
                        pointLst.Add(center);
                    }

                    stack.Push(resPathNodes[i - 1]);
                    stack.Push(resPathNodes[i]);
                }
                else
                {
                    if (Point3dIsEqualPoint3d(curPoint, oneEdgeNode.BeforeNode.CurPoint)) // 同一边 右侧
                    {
                        while (stack.Count != 0)
                        {
                            var popNode = stack.Pop(); // 弹出的
                            if (stack.Count == 0)
                            {
                                stack.Push(popNode);
                                stack.Push(curPathNode);
                                break;
                            }
                            var curTopNode = stack.First(); // 当前顶端
                            var vecFrom = popNode.CurPoint - curTopNode.CurPoint;
                            var vecTo = curPoint - popNode.CurPoint;
                            // 右侧向上拐
                            if (CrossProduct(vecFrom, vecTo) >= 0)
                            {
                                stack.Push(popNode);
                                stack.Push(curPathNode);
                                break;
                            }
                            else if (CrossProduct(vecFrom, vecTo) < 0) // 右侧向下拐，增加引线
                            {
                                var center = new Point3d((curPoint.X + curTopNode.CurPoint.X + popNode.CurPoint.X) / 3.0, (curPoint.Y + curTopNode.CurPoint.Y + popNode.CurPoint.Y) / 3.0, 0);
                                pointLst.Add(center);
                            }
                        }
                    }
                    else if (Point3dIsEqualPoint3d(curPoint, oneEdgeNode.NextNode.CurPoint)) // 同一边 左侧
                    {
                        while (stack.Count != 0)
                        {
                            var popNode = stack.Pop(); // 弹出的
                            if (stack.Count == 0)
                            {
                                stack.Push(popNode);
                                stack.Push(curPathNode);
                                break;
                            }
                            var curTopNode = stack.First(); // 当前顶端
                            var vecFrom = popNode.CurPoint - curTopNode.CurPoint;
                            var vecTo = curPoint - popNode.CurPoint;
                            // 左侧向上拐
                            if (CrossProduct(vecFrom, vecTo) <= 0)
                            {
                                stack.Push(popNode);
                                stack.Push(curPathNode);
                                break;
                            }
                            else if (CrossProduct(vecFrom, vecTo) > 0) // 左侧向下拐，增加引线
                            {
                                var center = new Point3d((curPoint.X + curTopNode.CurPoint.X + popNode.CurPoint.X) / 3.0, (curPoint.Y + curTopNode.CurPoint.Y + popNode.CurPoint.Y) / 3.0, 0);
                                pointLst.Add(center);
                            }
                        }
                    }
                }
            }

            return pointLst;
        }

        public static List<Curve> Convert2Triangle(List<PathNode> pathNodes)
        {
            if (pathNodes == null || pathNodes.Count == 0)
                return null;

            var resPathNodes = RegePathNodes(pathNodes);
            var profileLst = new List<Curve>();
            resPathNodes.Sort(new NodeComparer());
            var stack = new Stack<PathNode>();
            stack.Push(resPathNodes[0]);
            stack.Push(resPathNodes[1]);

            for (int i = 2; i < resPathNodes.Count; i++)
            {
                var curPathNode = resPathNodes[i];
                var curPoint = curPathNode.CurPoint;
                var vertex = stack.Last();
                var oneEdgeNode = stack.First();

                // 另一边
                if (Point3dIsEqualPoint3d(curPoint, vertex.BeforeNode.CurPoint)
                    || Point3dIsEqualPoint3d(curPoint, vertex.NextNode.CurPoint))
                {
                    while (stack.Count != 0)
                    {
                        var ptLst = new List<Point3d>();
                        var popNode = stack.Pop(); // 弹出的
                        if (stack.Count == 0)
                            break;

                        var curTopNode = stack.First(); // 当前顶端
                        ptLst.Add(curPoint);
                        ptLst.Add(curTopNode.CurPoint);
                        ptLst.Add(popNode.CurPoint);
                        profileLst.Add(ConvertPoints2Polyline(ptLst));
                    }

                    stack.Push(resPathNodes[i - 1]);
                    stack.Push(resPathNodes[i]);
                }
                else
                {
                    if (Point3dIsEqualPoint3d(curPoint, oneEdgeNode.BeforeNode.CurPoint)) // 同一边 右侧
                    {
                        while (stack.Count != 0)
                        {
                            var popNode = stack.Pop(); // 弹出的
                            if (stack.Count == 0)
                            {
                                stack.Push(popNode);
                                stack.Push(curPathNode);
                                break;
                            }
                            var curTopNode = stack.First(); // 当前顶端
                            var vecFrom = popNode.CurPoint - curTopNode.CurPoint;
                            var vecTo = curPoint - popNode.CurPoint;
                            // 右侧向上拐
                            if (CrossProduct(vecFrom, vecTo) >= 0)
                            {
                                stack.Push(popNode);
                                stack.Push(curPathNode);
                                break;
                            }
                            else if (CrossProduct(vecFrom, vecTo) < 0) // 右侧向下拐，增加引线
                            {
                                var ptLst = new List<Point3d>();
                                ptLst.Add(curPoint);
                                ptLst.Add(popNode.CurPoint);
                                ptLst.Add(curTopNode.CurPoint);
                                profileLst.Add(ConvertPoints2Polyline(ptLst));
                            }
                        }
                    }
                    else if (Point3dIsEqualPoint3d(curPoint, oneEdgeNode.NextNode.CurPoint)) // 同一边 左侧
                    {
                        while (stack.Count != 0)
                        {
                            var popNode = stack.Pop(); // 弹出的
                            if (stack.Count == 0)
                            {
                                stack.Push(popNode);
                                stack.Push(curPathNode);
                                break;
                            }
                            var curTopNode = stack.First(); // 当前顶端
                            var vecFrom = popNode.CurPoint - curTopNode.CurPoint;
                            var vecTo = curPoint - popNode.CurPoint;
                            // 左侧向上拐
                            if (CrossProduct(vecFrom, vecTo) <= 0)
                            {
                                stack.Push(popNode);
                                stack.Push(curPathNode);
                                break;
                            }
                            else if (CrossProduct(vecFrom, vecTo) > 0) // 左侧向下拐，增加引线
                            {
                                var ptLst = new List<Point3d>();
                                ptLst.Add(curPoint);
                                ptLst.Add(popNode.CurPoint);
                                ptLst.Add(curTopNode.CurPoint);
                                profileLst.Add(ConvertPoints2Polyline(ptLst));
                            }
                        }
                    }
                }
            }

            return profileLst;
        }

        public static List<Triangle> Convert2TriangleSelf(List<PathNode> pathNodes)
        {
            if (pathNodes == null || pathNodes.Count == 0)
                return null;

            var resPathNodes = RegePathNodes(pathNodes);
            resPathNodes.Sort(new NodeComparer());
            var stack = new Stack<PathNode>();
            stack.Push(resPathNodes[0]);
            stack.Push(resPathNodes[1]);

            var triangles = new List<Triangle>();
            for (int i = 2; i < resPathNodes.Count; i++)
            {
                var curPathNode = resPathNodes[i];
                var curPoint = curPathNode.CurPoint;
                var vertex = stack.Last();
                var oneEdgeNode = stack.First();

                // 另一边
                if (Point3dIsEqualPoint3d(curPoint, vertex.BeforeNode.CurPoint)
                    || Point3dIsEqualPoint3d(curPoint, vertex.NextNode.CurPoint))
                {
                    while (stack.Count != 0)
                    {
                        var popNode = stack.Pop(); // 弹出的
                        if (stack.Count == 0)
                            break;

                        var curTopNode = stack.First(); // 当前顶端

                        var edge1 = new TriangleEdge(curPoint, curTopNode.CurPoint);
                        var edge2 = new TriangleEdge(curTopNode.CurPoint, popNode.CurPoint);
                        var edge3 = new TriangleEdge(curPoint, popNode.CurPoint);
                        var edges = new List<TriangleEdge>();
                        edges.Add(edge1);
                        edges.Add(edge2);
                        edges.Add(edge3);
                        var trian = new Triangle(edges);
                        trian.vertexs.Add(curPoint);
                        trian.vertexs.Add(curTopNode.CurPoint);
                        trian.vertexs.Add(popNode.CurPoint);
                        triangles.Add(trian);
                    }

                    stack.Push(resPathNodes[i - 1]);
                    stack.Push(resPathNodes[i]);
                }
                else
                {
                    if (Point3dIsEqualPoint3d(curPoint, oneEdgeNode.BeforeNode.CurPoint)) // 同一边 右侧
                    {
                        while (stack.Count != 0)
                        {
                            var popNode = stack.Pop(); // 弹出的
                            if (stack.Count == 0)
                            {
                                stack.Push(popNode);
                                stack.Push(curPathNode);
                                break;
                            }
                            var curTopNode = stack.First(); // 当前顶端
                            var vecFrom = popNode.CurPoint - curTopNode.CurPoint;
                            var vecTo = curPoint - popNode.CurPoint;
                            // 右侧向上拐
                            if (CrossProduct(vecFrom, vecTo) >= 0)
                            {
                                stack.Push(popNode);
                                stack.Push(curPathNode);
                                break;
                            }
                            else if (CrossProduct(vecFrom, vecTo) < 0) // 右侧向下拐，增加引线
                            {
                                var edge1 = new TriangleEdge(curPoint, curTopNode.CurPoint);
                                var edge2 = new TriangleEdge(curTopNode.CurPoint, popNode.CurPoint);
                                var edge3 = new TriangleEdge(curPoint, popNode.CurPoint);
                                var edges = new List<TriangleEdge>();
                                edges.Add(edge1);
                                edges.Add(edge2);
                                edges.Add(edge3);
                                var trian = new Triangle(edges);
                                trian.vertexs.Add(curPoint);
                                trian.vertexs.Add(curTopNode.CurPoint);
                                trian.vertexs.Add(popNode.CurPoint);
                                triangles.Add(trian);
                            }
                        }
                    }
                    else if (Point3dIsEqualPoint3d(curPoint, oneEdgeNode.NextNode.CurPoint)) // 同一边 左侧
                    {
                        while (stack.Count != 0)
                        {
                            var popNode = stack.Pop(); // 弹出的
                            if (stack.Count == 0)
                            {
                                stack.Push(popNode);
                                stack.Push(curPathNode);
                                break;
                            }
                            var curTopNode = stack.First(); // 当前顶端
                            var vecFrom = popNode.CurPoint - curTopNode.CurPoint;
                            var vecTo = curPoint - popNode.CurPoint;
                            // 左侧向上拐
                            if (CrossProduct(vecFrom, vecTo) <= 0)
                            {
                                stack.Push(popNode);
                                stack.Push(curPathNode);
                                break;
                            }
                            else if (CrossProduct(vecFrom, vecTo) > 0) // 左侧向下拐，增加引线
                            {
                                var edge1 = new TriangleEdge(curPoint, curTopNode.CurPoint);
                                var edge2 = new TriangleEdge(curTopNode.CurPoint, popNode.CurPoint);
                                var edge3 = new TriangleEdge(curPoint, popNode.CurPoint);
                                var edges = new List<TriangleEdge>();
                                edges.Add(edge1);
                                edges.Add(edge2);
                                edges.Add(edge3);
                                var trian = new Triangle(edges);
                                trian.vertexs.Add(curPoint);
                                trian.vertexs.Add(curTopNode.CurPoint);
                                trian.vertexs.Add(popNode.CurPoint);
                                triangles.Add(trian);
                            }
                        }
                    }
                }
            }

            return triangles;
        }

        /// <summary>
        /// 重新设置前后节点
        /// </summary>
        /// <param name="pathNodes"></param>
        /// <returns></returns>
        public static List<PathNode> RegePathNodes(List<PathNode> pathNodes)
        {
            if (pathNodes.First().CurPoint.Equals(pathNodes.Last().CurPoint))
                pathNodes.Remove(pathNodes.Last());

            var resPathNodes = new List<PathNode>();
            pathNodes.ForEach(s => resPathNodes.Add(new PathNode(s.CurPoint)));

            for (int j = 0; j < resPathNodes.Count; j++)
            {
                var beforeNode = resPathNodes[((j - 1 + resPathNodes.Count) % resPathNodes.Count)];
                var curNode = resPathNodes[j];
                var nextNode = resPathNodes[((j + 1) % resPathNodes.Count)];
                curNode.SetBeforeNode(beforeNode);
                curNode.SetNextNode(nextNode);
            }

            return resPathNodes;
        }

        public static List<PathNode> CClockWiseFromPoint(PathNode pathNode)
        {
            var pathNodes = new List<PathNode>();
            var curNode = pathNode;
            pathNodes.Add(pathNode);
            var secNode = curNode.CalculateNextCCW();
            pathNodes.Add(secNode);
            secNode.IsValid = false;

            curNode = pathNodes.Last();
            Point3d ptHead;
            Point3d ptTail;
            do
            {
                ptHead = pathNodes[pathNodes.Count - 2].CurPoint;
                ptTail = pathNodes[pathNodes.Count - 1].CurPoint;

                var next = curNode.CalculateNextPathNodeCCW(ptHead, ptTail);
                next.IsValid = false;
                pathNodes.Add(next);
                curNode = next;
            } while (curNode != pathNode);

            return pathNodes;
        }

        public static List<PathNode> CClockWiseFromPointToEnd(PathNode pathNode, PathNode endNode)
        {
            var pathNodes = new List<PathNode>();
            var curNode = pathNode;
            pathNodes.Add(pathNode);
            var secNode = curNode.CalculateNextCCW();
            pathNodes.Add(secNode);

            curNode = pathNodes.Last();
            Point3d ptHead;
            Point3d ptTail;
            do
            {
                ptHead = pathNodes[pathNodes.Count - 2].CurPoint;
                ptTail = pathNodes[pathNodes.Count - 1].CurPoint;

                var next = curNode.CalculateNextPathNodeCCW(ptHead, ptTail);
                pathNodes.Add(next);
                curNode = next;
            } while (curNode != endNode);

            return pathNodes;
        }

        public static List<PathNode> CClockWiseFromEndPoint(PathNode pathNode)
        {
            var pathNodes = new List<PathNode>();
            return pathNodes;
        }

        public static List<PathNode> ClockWiseFromStartPoint(PathNode pathNode)
        {
            var pathNodes = new List<PathNode>();
            return pathNodes;
        }

        public static List<PathNode> ClockWiseFromEndPoint(PathNode pathNode)
        {
            var pathNodes = new List<PathNode>();
            return pathNodes;
        }
    }
}
