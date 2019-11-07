using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThSpray
{
    /// <summary>
    /// topo边的数据信息
    /// </summary>
    public class TopoEdge
    {
        private TopoEdge m_pair;
        private Point3d m_start = new Point3d();
        private Point3d m_end = new Point3d();
        private XY m_StartDir = null;
        private XY m_EndDir = null;
        private bool m_bUse = false;

        public Curve SrcCurve
        {
            get;
            set;
        }

        public bool IsLine
        {
            get;
            set;
        }

        public bool IsUse
        {
            get { return m_bUse; }
            set { m_bUse = true; }
        }

        public TopoEdge(Point3d start, Point3d end, Curve srcCurve, XY startDir = null, XY endDir = null)
        {
            m_start = start;
            m_end = end;
            m_StartDir = startDir;
            m_EndDir = endDir;
            SrcCurve = srcCurve;
            IsLine = (srcCurve is Line);
        }

        public TopoEdge Pair
        {
            get { return m_pair; }
            set { m_pair = value; }
        }

        public Point3d Start
        {
            get { return m_start; }
            set { m_start = value; }
        }

        public Point3d End
        {
            get { return m_end; }
            set { m_end = value; }
        }

        public XY StartDir
        {
            get { return m_StartDir; }
            set { m_StartDir = value; }
        }

        public XY EndDir
        {
            get { return m_EndDir; }
            set { m_EndDir = value; }
        }

        public static void MakeTopoEdge(Curve srcCurve, List<TopoEdge> topoEdges)
        {
            var ptS = srcCurve.StartPoint;
            var ptE = srcCurve.EndPoint;
            TopoEdge curEdge = null;
            TopoEdge pairEdge = null;
            Vector3d curveDir;
            Vector3d curveDirReverse;

            var drawCurves = new List<Curve>();
            if (srcCurve is Line)
            {
                var srcLine = srcCurve as Line;
                curveDir = srcLine.GetFirstDerivative(ptS).GetNormal();
                drawCurves.Add(new Line(ptS, ptS + curveDir * 10));

                curveDirReverse = curveDir.Negate();
                drawCurves.Add(new Line(ptE, ptE + curveDirReverse * 10));
                curEdge = new TopoEdge(ptS, ptE, srcCurve, CommonUtils.Vector2XY(curveDir), CommonUtils.Vector2XY(curveDir));
                var clone = srcCurve.Clone() as Line;
                clone.ReverseCurve();
                pairEdge = new TopoEdge(ptE, ptS, clone, CommonUtils.Vector2XY(curveDirReverse), CommonUtils.Vector2XY(curveDirReverse));
            }
            else
            {
                var srcArc = srcCurve as Arc;
                var arcDirS = srcArc.GetFirstDerivative(ptS).GetNormal();
                var arcDirE = srcArc.GetFirstDerivative(ptE).GetNormal();
                drawCurves.Add(new Line(ptS, ptS + arcDirS * 10));
                drawCurves.Add(new Line(ptE, ptE + arcDirE * 10));

                var arcDirRevS = arcDirE.Negate();
                var arcDirRevE = arcDirS.Negate();

                drawCurves.Add(new Line(ptE, ptE + arcDirRevS * 10));
                drawCurves.Add(new Line(ptS, ptS + arcDirRevE * 10));
                curEdge = new TopoEdge(ptS, ptE, srcCurve, CommonUtils.Vector2XY(arcDirS), CommonUtils.Vector2XY(arcDirE));
                //Utils.DrawProfile(new List<Curve>() { srcCurve }, "topoEdgeSrc");
                //var clone = CommonUtils.CreateArcReverse(srcArc.EndPoint, srcArc.Center, srcArc.StartPoint, srcArc.Radius, new Vector3d(0, 0, -1));
                //Utils.DrawProfile(new List<Curve>() { srcCurve }, "topoEdgeSrc");
                pairEdge = new TopoEdge(ptE, ptS, srcCurve.Clone() as Arc, CommonUtils.Vector2XY(arcDirRevS), CommonUtils.Vector2XY(arcDirRevE));
            }

            var polyline = new Polyline();
            //Utils.DrawProfile(drawCurves, "topoEdge");

            curEdge.Pair = pairEdge;
            pairEdge.Pair = curEdge;
            topoEdges.Add(curEdge);
            topoEdges.Add(pairEdge);
        }
    }

    class TopoSearch
    {
        private BoundBoxPlane m_planeBox = null;
        private List<List<TopoEdge>> m_srcLoops = null;

        public List<List<TopoEdge>> SrcLoops
        {
            get { return m_srcLoops; }
        }

        private List<List<TopoEdge>> m_partialLoops = new List<List<TopoEdge>>();

        internal class ProductLoop
        {
            public double LoopArea
            {
                get;
                set;
            }

            public List<TopoEdge> OneLoop
            {
                get;
                set;
            }

            public Point2d CenterPoint
            {
                get;
                set;
            }

            public ProductLoop(List<TopoEdge> edegs, double area, Point2d point)
            {
                this.OneLoop = edegs;
                this.LoopArea = area;
                CenterPoint = point;
            }
        }

        public class ProductLoopCom : IEqualityComparer<ProductLoop>
        {
            public bool Equals(ProductLoop x, ProductLoop y)
            {
                if (x == null)
                    return y == null;

                return CommonUtils.IsAlmostNearZero(Math.Abs(x.LoopArea) - Math.Abs(y.LoopArea), 0.1)
                && CommonUtils.Point2dIsEqualPoint2d(x.CenterPoint, y.CenterPoint);
            }

            public int GetHashCode(ProductLoop pro)
            {
                if (pro == null)
                    return 0;

                return ((int)(Math.Abs(pro.LoopArea * 1000)));
            }
        }

        /// <summary>
        /// 获取轮廓
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        public static List<Curve> MakeSrcProfileLoops(List<Curve> curves)
        {
            var search = new TopoSearch(curves);
            var loops = TopoSearch.RemoveDuplicate(search.m_srcLoops);
            var tmpEdgeLoops = search.TransFormProfileLoops(loops);
            return search.ConvertTopoEdges2Curve(tmpEdgeLoops);
        }

        public static Curve MoveTransform(Curve srcCurve, Vector2d vec)
        {
            if (srcCurve is Line)
            {
                return CommonUtils.LineAddVector(srcCurve as Line, vec);
            }
            else
            {
                return CommonUtils.ArcAddVector(srcCurve as Arc, vec);
            }
        }

        private TopoSearch(List<Curve> curves)
        {
            m_planeBox = new BoundBoxPlane(curves);

            if (m_planeBox.IsTranslation())
            {
                // 平移处理
                var curvesTrans = new List<Curve>();
                var trans = m_planeBox.TransValue;
                foreach (var curve in curves)
                {
                    var transCurve = MoveTransform(curve, trans);
                    curvesTrans.Add(transCurve);
                }
                //Utils.DrawProfile(curvesTrans, "test");
                m_srcLoops = TopoCalculate.MakeProfileLoop(curvesTrans);
            }
            else
            {
                // 不平移处理
                m_srcLoops = TopoCalculate.MakeProfileLoop(curves);
            }

        }

        /// <summary>
        /// 转化为CAD中的数据格式
        /// </summary>
        /// <param name="topoLoops"></param>
        /// <returns></returns>
        private List<Curve> ConvertTopoEdges2Curve(List<List<TopoEdge>> topoLoops)
        {
            if (topoLoops == null || topoLoops.Count == 0)
                return null;

            var polylines = new List<Curve>();
            foreach (var loop in topoLoops)
            {
                var curves = new List<Curve>();
                foreach (var edge in loop)
                {
                    curves.Add(edge.SrcCurve);
                }

                //Utils.DrawProfile(curves, "d");
                Utils.CreateGroup(curves, "profile", "profileShow");
                //var edges = AdjustDir(loop);
                //Polyline profile = Convert2Polyline(edges);
                //if (profile != null)
                //{
                //    polylines.Add(profile);
                //    var curves = new List<Curve>();
                //    foreach (var edge in loop)
                //    {
                //        curves.Add(edge.SrcCurve);
                //    }
                //    Utils.DrawProfile(curves, "7");
                //}
            }

            return polylines;
        }


        public static List<TopoEdge> AdjustDir(List<TopoEdge> edges)
        {
            var areaDir = CommonUtils.CalcuLoopArea(edges);
            if (areaDir > 0)
            {
                return edges;
            }
            else if (areaDir < 0)
            {
                edges.Reverse();
                for (int i = 0; i < edges.Count; i++)
                {
                    var curEdge = edges[i];
                    if (curEdge.IsLine)
                    {
                        var curCurve = curEdge.SrcCurve;
                        var ptS = curCurve.StartPoint;
                        var ptE = curCurve.EndPoint;
                        var resEdge = new TopoEdge(ptE, ptS, new Line(ptE, ptS));
                        edges[i] = resEdge;
                    }
                }
                return edges;
            }
            return null;
        }

        /// <summary>
        /// 转化为多段线
        /// </summary>
        /// <param name="topoEdges"></param>
        /// <returns></returns>
        public static Polyline Convert2Polyline(List<TopoEdge> topoEdges)
        {
            if (topoEdges.Count != 7)
                return null;
            var polyline = new Polyline();
            polyline.Closed = true;
            int arcIndex = -1;
            var firstArcEdge = GetFirstArcEdge(topoEdges, ref arcIndex);

            if (firstArcEdge == null)
            {
                for (int i = 0; i < topoEdges.Count; i++)
                {
                    var curEdge = topoEdges[i];
                    var point = (curEdge.SrcCurve as Line).StartPoint;
                    polyline.AddVertexAt(i, new Point2d(point.X, point.Y), 0, 0, 0);
                }
            }
            else
            {
                int curIndex = arcIndex + 1; // 当前下一个
                int vertexIndex = 0;
                Point2d polylineEndPoint = new Point2d();

                // 第一个节点
                var arcFirst = topoEdges[arcIndex].SrcCurve as Arc;
                var bulgeFirst = Math.Tan(arcFirst.TotalAngle / 4.0);
                var arcStartPtFirst = new Point2d(arcFirst.StartPoint.X, arcFirst.StartPoint.Y);
                polyline.AddVertexAt(0, arcStartPtFirst, bulgeFirst, 0, 0);
                polylineEndPoint = new Point2d(arcFirst.EndPoint.X, arcFirst.EndPoint.Y);

                // 后面的节点
                while (true)
                {
                    vertexIndex++;
                    curIndex = curIndex % topoEdges.Count;
                    if (curIndex == arcIndex)
                    {
                        break;
                    }

                    var curEdge = topoEdges[curIndex];

                    if (curEdge.IsLine)
                    {
                        try
                        {
                            // line
                            var line = curEdge.SrcCurve as Line;
                            var ptS = new Point2d(line.StartPoint.X, line.StartPoint.Y);
                            var ptE = new Point2d(line.EndPoint.X, line.EndPoint.Y);
                            if (CommonUtils.Point2dIsEqualPoint2d(polylineEndPoint, ptS, 1e-1))
                            {
                                polyline.AddVertexAt(vertexIndex, ptS, 0, 0, 0);
                                polylineEndPoint = ptE;
                            }
                            else if (CommonUtils.Point2dIsEqualPoint2d(polylineEndPoint, ptE, 1e-1))
                            {
                                polyline.AddVertexAt(vertexIndex, ptE, 0, 0, 0);
                                polylineEndPoint = ptS;
                            }
                            else
                            {
                                var p1 = polylineEndPoint;
                                var p2 = ptS;
                                var p3 = ptE;
                                int i = 0;
                            }
                        }
                        catch
                        {
                            var p1 = polylineEndPoint;
                            int i = 0;
                        }
                    }
                    else
                    {
                        // arc
                        var arc = curEdge.SrcCurve as Arc;
                        var bulge = Math.Tan(arc.TotalAngle / 4.0);
                        var arcStartPt = new Point2d(arc.StartPoint.X, arc.StartPoint.Y);
                        polyline.AddVertexAt(vertexIndex, arcStartPt, bulge, 0, 0);
                        polylineEndPoint = new Point2d(arc.EndPoint.X, arc.EndPoint.Y);
                    }

                    curIndex++;
                }
            }

            return polyline;
        }

        /// <summary>
        /// 为空表明都是直线
        /// </summary>
        /// <param name="topoEdges"></param>
        /// <returns></returns>
        public static TopoEdge GetFirstArcEdge(List<TopoEdge> topoEdges, ref int index)
        {
            for (int i = 0; i < topoEdges.Count; i++)
            {
                var edge = topoEdges[i];
                if (!edge.IsLine)
                {
                    index = i;
                    return edge;
                }
            }

            return null;
        }

        // 坐标转换处理
        private List<List<TopoEdge>> TransFormProfileLoops(List<List<TopoEdge>> loops)
        {
            if (loops == null || loops.Count == 0)
            {
                return null;
            }

            if (!m_planeBox.IsTranslation())
                return loops;

            var transValue = m_planeBox.TransValue;
            var topoLoops = new List<List<TopoEdge>>();

            foreach (var loop in loops)
            {
                var loopEdge = new List<TopoEdge>();
                foreach (var edge in loop)
                {
                    var transEdge = CommonUtils.LineDecVector(edge, transValue);
                    loopEdge.Add(transEdge);
                }

                topoLoops.Add(loopEdge);
            }

            return topoLoops;
        }

        /// <summary>
        /// 删除重复边
        /// </summary>
        /// <param name="srcEdgeLoops"></param>
        /// <returns></returns>
        public static List<List<TopoEdge>> RemoveDuplicate(List<List<TopoEdge>> srcEdgeLoops)
        {
            if (srcEdgeLoops == null)
                return null;
            if (srcEdgeLoops.Count == 1)
                return srcEdgeLoops;

            var edgeLoops = new List<List<TopoEdge>>();
            var proLoops = new List<ProductLoop>();
            for (int i = 0; i < srcEdgeLoops.Count; i++)
            {
                var curLoop = srcEdgeLoops[i];
                ConvertEdges(curLoop);
                var pt3d = curLoop.First().Start;
                var pt = new Point2d(pt3d.X, pt3d.Y);
                for (int j = 1; j < curLoop.Count; j++)
                {
                    var curPt = curLoop[j].Start;
                    pt = CommonUtils.Point2dAddPoint2d(new Point2d(pt.X, pt.Y), new Point2d(curPt.X, curPt.Y));
                }

                var ptCen = pt / curLoop.Count;
                if (ptCen.GetDistanceTo(new Point2d(0, 0)) > 1e7)
                    ptCen = new Point2d(ptCen.X * 1e-6, ptCen.Y * 1e-6);

                var curLoopArea = CommonUtils.CalcuLoopArea(curLoop);
                if (Math.Abs(curLoopArea) > 1e7)
                    curLoopArea *= 1e-6;
                proLoops.Add(new ProductLoop(srcEdgeLoops[i], curLoopArea, ptCen));
            }

            var tmpLoop = proLoops.Distinct(new ProductLoopCom()).ToList();
            foreach (var loop in tmpLoop)
            {
                edgeLoops.Add(loop.OneLoop);
            }

            return edgeLoops;
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
                    var arc1 = CommonUtils.CreateArc(startPoint, center, midPoint, radius);
                    var arc2 = CommonUtils.CreateArc(midPoint, center, endPoint, radius);
                    if (CommonUtils.Point3dIsEqualPoint3d(edge.Start, arc.StartPoint))
                    {
                        var topoEdge1 = new TopoEdge(edge.Start, midPoint, arc1);
                        var topoEdge2 = new TopoEdge(midPoint, edge.End, arc2);
                        resEdges.Add(topoEdge1);
                        resEdges.Add(topoEdge2);
                    }
                    else
                    {
                        var topoEdge3 = new TopoEdge(edge.Start, midPoint, arc2);
                        var topoEdge4 = new TopoEdge(midPoint, edge.End, arc1);
                        resEdges.Add(topoEdge3);
                        resEdges.Add(topoEdge4);
                    }
                    //var line1 = new Line(midPoint, midPoint + new Vector3d(1, 0, 0) * 100);
                    //var line2 = new Line(midPoint, midPoint + new Vector3d(0, 1, 0) * 100);
                    //var curves = new List<Curve>();
                    //curves.Add(line1);
                    //curves.Add(line2);
                    //Utils.DrawProfile(curves, "dd");
                }

            }

            return resEdges;
        }
    }

    /// <summary>
    /// topo imp
    /// </summary>
    class TopoCalculate
    {
        private List<Curve> m_curves = null;
        private List<TopoEdge> m_topoEdges = new List<TopoEdge>();
        private HashMap m_hashMap = new HashMap();
        private List<List<TopoEdge>> m_ProfileLoop = new List<List<TopoEdge>>();
        public List<List<TopoEdge>> ProfileLoops
        {
            get { return m_ProfileLoop; }
        }

        public static List<List<TopoEdge>> MakeProfileLoop(List<Curve> curves)
        {
            if (curves == null || curves.Count == 0)
                return null;

            var topoCal = new TopoCalculate(curves);
            return topoCal.ProfileLoops;
        }

        private TopoCalculate(List<Curve> SrcCurves)
        {
            m_curves = SrcCurves;
            var curves = ScatterCurves.MakeNewCurves(m_curves);
            //Utils.DrawProfile(curves, "scatter");
            //return;
            Calculate(curves);
        }

        private void Calculate(List<Curve> srcCurves)
        {
            foreach (var curve in srcCurves)
            {
                TopoEdge.MakeTopoEdge(curve, m_topoEdges);
            }

            foreach (var topoEdge in m_topoEdges)
            {
                m_hashMap.Add(topoEdge);
            }

            for (int i = 0; i < m_topoEdges.Count; i++)
            {
                if (m_topoEdges[i].IsUse)
                    continue;

                BuildOneLoop(m_topoEdges[i]);
            }
        }

        /// <summary>
        /// 获取下一条有效边
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
        private TopoEdge GetNextEdgeInMaps(TopoEdge edge)
        {
            var tailPoint = new Point2d(edge.End.X, edge.End.Y);
            int hashKey = CommonUtils.HashKey(tailPoint);
            var adjTopoEdges = m_hashMap[hashKey];
            if (adjTopoEdges.Count == 0)
            {
                return null;
            }

            var headPoint = new Point2d(edge.Start.X, edge.Start.Y);
            var curEndDir = new XY(edge.EndDir.X, edge.EndDir.Y);
            var clockWiseMatchs = new List<ClockWiseMatch>();
            for (int i = 0; i < adjTopoEdges.Count; i++)
            {
                var curEdge = adjTopoEdges[i];
                var curPtHead = new Point2d(curEdge.Start.X, curEdge.Start.Y);
                var curPtTail = new Point2d(curEdge.End.X, curEdge.End.Y);

                var edgeCurve = edge.SrcCurve;
                var edgeMidPoint = edgeCurve.GetPointAtParameter((edgeCurve.StartParam + edgeCurve.EndParam) * 0.5);
                var curEdgeCurve = curEdge.SrcCurve;
                var curEdgeMidPoint = curEdgeCurve.GetPointAtParameter((curEdgeCurve.StartParam + curEdgeCurve.EndParam) * 0.5);
                if (curEdge.IsUse || (CommonUtils.Point2dIsEqualPoint2d(headPoint, curPtTail, 1e-1) && CommonUtils.Point2dIsEqualPoint2d(tailPoint, curPtHead, 1e-1) 
                    && CommonUtils.Point2dIsEqualPoint2d(new Point2d(edgeMidPoint.X, edgeMidPoint.Y), new Point2d(curEdgeMidPoint.X, curEdgeMidPoint.Y), 1e-1)))
                    continue;

                if (CommonUtils.Point2dIsEqualPoint2d(tailPoint, curPtHead, 1e-1))
                {
                    var clockEdge = new ClockWiseMatch(curEdge);
                    clockEdge.Angle = curEndDir.CalAngle(clockEdge.StartDir);
                    clockWiseMatchs.Add(clockEdge);
                }
            }

            if (clockWiseMatchs.Count == 0)
                return null;

            clockWiseMatchs.Sort((s1, s2) => { return s1.Angle.CompareTo(s2.Angle); });
            clockWiseMatchs.First().TopoEdge.IsUse = true;
            return clockWiseMatchs.First().TopoEdge;
        }

        /// <summary>
        /// 删除末尾，且从另一条topoEdge寻找也是不可行的
        /// </summary>
        /// <param name="polys"></param>
        private void PopLastEdge(List<TopoEdge> polys)
        {
            if (polys.Count == 0)
                return;
            polys.Last().Pair.IsUse = true;
            polys.RemoveAt(polys.Count - 1);
        }

        /// <summary>
        /// 建立一条环
        /// </summary>
        /// <param name="edge"></param>
        private void BuildOneLoop(TopoEdge edge)
        {
            var polys = new List<TopoEdge>();
            edge.IsUse = true;
            polys.Add(edge);

            while (polys.Count != 0)
            {
                var curEdge = polys.Last();
                var nextEdge = GetNextEdgeInMaps(curEdge);
                if (nextEdge == null)
                {
                    PopLastEdge(polys);
                    continue;
                }

                polys.Add(nextEdge);
                var first = polys.First();
                var last = polys.Last();

                if (polys.Count > 1 && CommonUtils.Point2dIsEqualPoint2d(new Point2d(first.Start.X, first.Start.Y), new Point2d(last.End.X, last.End.Y), 1e-1))
                {
                    m_ProfileLoop.Add(polys);
                    break;
                }

                // 摘除环，继续寻找
                for (int i = 0; i < polys.Count - 1; i++)
                {
                    var Cedge = polys[i];
                    var point = new Point2d(Cedge.End.X, Cedge.End.Y);
                    if (CommonUtils.Point2dIsEqualPoint2d(point, new Point2d(last.End.X, last.End.Y), 1e-1))
                    {
                        var k = i + 1;
                        var nEraseindex = k;
                        var edgeLoop = new List<TopoEdge>();
                        for (; k < polys.Count; k++)
                        {
                            edgeLoop.Add(polys[k]);
                        }

                        if (edgeLoop.Count > 1)
                        {
                            m_ProfileLoop.Add(edgeLoop);
                        }
                        var nEraseCnt = polys.Count - nEraseindex;
                        polys.RemoveRange(nEraseindex, nEraseCnt);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 建立topo边和角度的映射关系
    /// </summary>
    class ClockWiseMatch
    {
        private XY m_dir;
        public double Angle
        {
            get;
            set;
        }
        public XY StartDir
        {
            get { return m_dir; }
        }

        private TopoEdge m_topoEdge;

        public TopoEdge TopoEdge
        {
            get { return m_topoEdge; }
        }

        public ClockWiseMatch(TopoEdge edge)
        {
            m_topoEdge = edge;
            m_dir = new XY(edge.StartDir.X, edge.StartDir.Y);
        }
    }

    /// <summary>
    /// Scatter srcLine for Build loop
    /// </summary>
    class ScatterCurves
    {
        class ScatterNode
        {
            public Curve srcCurve = null;
            public bool IsLine = true;
            public List<Point3d> ptLst = new List<Point3d>();

            public ScatterNode(Curve curve, bool isLine)
            {
                srcCurve = curve;
                IsLine = isLine;
                ptLst.Add(curve.StartPoint);
                ptLst.Add(curve.EndPoint);
            }
        }

        private List<Curve> m_curves;
        List<ScatterNode> m_ScatterNodes = new List<ScatterNode>();

        private List<Curve> m_geneCurves = new List<Curve>();

        public List<Curve> Curves
        {
            get { return m_geneCurves; }
        }

        public static List<Curve> MakeNewCurves(List<Curve> srcCurves)
        {
            var scatterCurves = new ScatterCurves(srcCurves);
            return scatterCurves.Curves;
        }


        private ScatterCurves(List<Curve> curves)
        {
            m_curves = curves;

            foreach (var curve in m_curves)
            {
                m_ScatterNodes.Add(new ScatterNode(curve, curve is Line));
            }

            IntersectCurves();
            SortXYZPoints();
            NewCurves();
        }

        /// <summary>
        /// 求交点
        /// </summary>
        private void IntersectCurves()
        {
            for (int i = 0; i < m_ScatterNodes.Count; i++)
            {
                var curCurve = m_ScatterNodes[i].srcCurve;
                for (int j = i + 1; j < m_ScatterNodes.Count; j++)
                {
                    var nextCurve = m_ScatterNodes[j].srcCurve;
                    var ptLst = new Point3dCollection();
                    curCurve.IntersectWith(nextCurve, Intersect.OnBothOperands, ptLst, (IntPtr)0, (IntPtr)0);
                    if (ptLst.Count != 0)
                    {
                        foreach (Point3d pt in ptLst)
                        {
                            if (!m_ScatterNodes[i].ptLst.Contains(pt))
                                m_ScatterNodes[i].ptLst.Add(pt);
                            if (!m_ScatterNodes[j].ptLst.Contains(pt))
                                m_ScatterNodes[j].ptLst.Add(pt);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 排序
        /// </summary>
        private void SortXYZPoints()
        {
            foreach (var scatterNode in m_ScatterNodes)
            {
                if (scatterNode.IsLine)
                {
                    var ptLst = scatterNode.ptLst;
                    var ptFirst = ptLst.First();
                    ptLst.Sort((s1, s2) => { return s1.DistanceTo(ptFirst).CompareTo(s2.DistanceTo(ptFirst)); });
                }
                else
                {
                    SortArcNode(scatterNode);
                }
            }
        }

        private static void SortArcNode(ScatterNode scatterNode)
        {
            var ptLst = scatterNode.ptLst;
            var arc = scatterNode.srcCurve as Arc;
            var ptArcS = ptLst.ElementAt(0);
            ptLst.RemoveAt(0);
            var ptArcE = ptLst.ElementAt(0);
            ptLst.RemoveAt(0);
            var arcs = new List<Arc>();

            if (ptLst.Count > 0)
            {
                var ptCenter = arc.Center;
                var radius = arc.Radius;
                foreach (var pt in ptLst)
                {
                    var arcNode = CommonUtils.CreateArc(ptArcS, ptCenter, pt, radius);
                    arcs.Add(arcNode);
                }

                arcs = arcs.OrderBy(s => s.TotalAngle).ToList();
            }

            var drawCurves = new List<Curve>();
            foreach (var drawarc in arcs)
            {
                drawCurves.Add(drawarc);
            }

            //Utils.DrawProfile(drawCurves, "drawCurves");
            var resPtLst = new List<Point3d>();

            if (arcs.Count == 0)
            {
                resPtLst.Add(ptArcS);
                resPtLst.Add(ptArcE);
            }
            else
            {
                resPtLst.Add(ptArcS);
                foreach (var resArc in arcs)
                {
                    resPtLst.Add(resArc.EndPoint);
                }
                resPtLst.Add(ptArcE);
            }

            scatterNode.ptLst = resPtLst;
        }

        /// <summary>
        /// 生成新的曲线
        /// </summary>
        private void NewCurves()
        {
            foreach (var scatterNode in m_ScatterNodes)
            {
                var bFlag = scatterNode.IsLine;
                var ptLst = scatterNode.ptLst;
                for (int i = 0; i < ptLst.Count; i++)
                {
                    try
                    {
                        var curPoint = ptLst[i];
                        if (i + 1 == ptLst.Count)
                            break;

                        var nextPoint = ptLst[i + 1];
                        if ((curPoint - nextPoint).Length > 1e-3)
                        {
                            if (bFlag)
                            {
                                var newLine = new Line(curPoint, nextPoint);
                                m_geneCurves.Add(newLine);
                            }
                            else
                            {
                                //if ((curPoint - nextPoint).Length < 100.0)
                                //{
                                //    var newLine = new Line(curPoint, nextPoint);
                                //}

                                var srcArc = scatterNode.srcCurve as Arc;
                                var radius = srcArc.Radius;
                                var ptCenter = srcArc.Center;
                                var arc = CommonUtils.CreateArc(curPoint, ptCenter, nextPoint, radius);
                                m_geneCurves.Add(arc);
                            }
                        }
                    }
                    catch
                    { }
                }
            }
        }
    }



    /// <summary>
    /// 加速数据的查找工作，提高效率
    /// </summary>
    class HashMap
    {
        private List<List<TopoEdge>> m_hashMapEdges;
        public List<TopoEdge> this[int index]
        {
            get { return m_hashMapEdges[index]; }
        }

        public HashMap()
        {
            m_hashMapEdges = new List<List<TopoEdge>>();
            for (int i = 0; i < CommonUtils.HashMapCount; i++)
            {
                m_hashMapEdges.Add(new List<TopoEdge>());
            }
        }

        public void Add(TopoEdge edgeItem)
        {
            var point = edgeItem.Start;
            int key = CommonUtils.HashKey(new Point2d(point.X, point.Y));
            m_hashMapEdges[key].Add(edgeItem);
        }
    }

    /// <summary>
    /// 二维平面包围盒处理
    /// </summary>
    class BoundBoxPlane
    {
        public BoundBoxPlane(List<Curve> curves)
        {
            CalcuBoundBox(curves);
        }

        public void CalcuBoundBox(List<Curve> curves)
        {
            if (curves.Count == 0)
                return;

            var ptLst = new List<XY>();
            foreach (var curve in curves)
            {
                if (curve is Arc)
                {
                    var bound = curve.Bounds;
                    if (bound.HasValue)
                    {
                        var value = bound.Value;
                        var minPoint = value.MinPoint;
                        var maxPoint = value.MaxPoint;
                        var minXY = new XY(minPoint.X, minPoint.Y);
                        var maxXY = new XY(maxPoint.X, maxPoint.Y);
                        ptLst.Add(minXY);
                        ptLst.Add(maxXY);
                    }
                }
                else
                {
                    var line = curve as Line;
                    var pxHead = line.StartPoint;
                    var pxEnd = line.EndPoint;
                    var pt1 = new XY(pxHead.X, pxHead.Y);
                    var pt2 = new XY(pxEnd.X, pxEnd.Y);
                    ptLst.Add(pt1);
                    ptLst.Add(pt2);
                }
            }

            m_leftBottom = new XY(ptLst[0].X, ptLst[0].Y);
            m_rightTop = new XY(ptLst[0].X, ptLst[0].Y);
            for (int i = 1; i < ptLst.Count; i++)
            {
                if (m_leftBottom.X > ptLst[i].X)
                    m_leftBottom.X = ptLst[i].X;
                if (m_leftBottom.Y > ptLst[i].Y)
                    m_leftBottom.Y = ptLst[i].Y;

                if (m_rightTop.X < ptLst[i].X)
                    m_rightTop.X = ptLst[i].X;
                if (m_rightTop.Y < ptLst[i].Y)
                    m_rightTop.Y = ptLst[i].Y;
            }
        }

        /// <summary>
        /// true 返回的是增加的偏移值
        /// </summary>
        /// <returns></returns>
        public bool IsTranslation()
        {
            // 平移处理
            if (m_leftBottom.X > 0 && m_leftBottom.Y > 0)
                return false;

            var offsetX = 0 - m_leftBottom.X;
            var offsetY = 0 - m_leftBottom.Y;
            TransValue = new Vector2d(offsetX, offsetY);
            return true;
        }

        public Vector2d TransValue
        {
            get;
            set;
        }

        public XY LeftBottom
        {
            get { return m_leftBottom; }
        }

        public XY RightTop
        {
            get { return m_rightTop; }
        }

        private XY m_leftBottom = null;
        private XY m_rightTop = null;
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
            double angleRad = Math.Acos(CommonUtils.CutRadRange(val / tmp));

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
}
