﻿using Autodesk.AutoCAD.Colors;
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

        public bool ValidEdge
        {
            get;
            set;
        }

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
            ValidEdge = true;
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

            if (srcCurve is Line)
            {
                var srcLine = srcCurve as Line;
                curveDir = srcLine.GetFirstDerivative(ptS).GetNormal();

                curveDirReverse = curveDir.Negate();
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

                var arcDirRevS = arcDirE.Negate();
                var arcDirRevE = arcDirS.Negate();
                curEdge = new TopoEdge(ptS, ptE, srcCurve, CommonUtils.Vector2XY(arcDirS), CommonUtils.Vector2XY(arcDirE));
                pairEdge = new TopoEdge(ptE, ptS, srcCurve.Clone() as Arc, CommonUtils.Vector2XY(arcDirRevS), CommonUtils.Vector2XY(arcDirRevE));
            }

            curEdge.Pair = pairEdge;
            pairEdge.Pair = curEdge;
            topoEdges.Add(curEdge);
            topoEdges.Add(pairEdge);
        }
    }

    class TopoSearch
    {
        private BoundBoxPlane m_planeBox = null;
        private List<Profile> m_srcLoops = null;

        public List<Profile> SrcLoops
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

            public Point3d CenterPoint
            {
                get;
                set;
            }

            public ProductLoop(List<TopoEdge> edegs, double area, Point3d point)
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
                && CommonUtils.Point3dIsEqualPoint3d(x.CenterPoint, y.CenterPoint);
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
            //var loops = TopoSearch.RemoveDuplicate(search.m_srcLoops);
            var tmpEdgeLoops = search.TransFormProfileLoops(search.m_srcLoops);
            return search.ConvertTopoEdges2Curve(tmpEdgeLoops);
        }

        public static List<Curve> MakeSrcProfileLoopsFromPoint(List<Curve> curves, Point3d pt)
        {
            var search = new TopoSearch(curves, pt);
            var tmpEdgeLoops = search.TransFormProfileLoops(search.m_srcLoops);
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
                m_srcLoops = TopoCalculate.MakeProfileLoop(curvesTrans);
            }
            else
            {
                // 不平移处理
                m_srcLoops = TopoCalculate.MakeProfileLoop(curves);
            }

        }

        private TopoSearch(List<Curve> curves, Point3d pt)
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

                pt = pt + new Vector3d(trans.X, trans.Y, 0);
                m_srcLoops = TopoCalculate.MakeProfileLoopFromPoint(curvesTrans, pt);
            }
            else
            {
                // 不平移处理
                m_srcLoops = TopoCalculate.MakeProfileLoopFromPoint(curves, pt);
            }

        }

        /// <summary>
        /// 转化为CAD中的数据格式, 输出为多段线的集合
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
                Polyline profile = ConvertLoop2Polyline(loop);
                if (profile != null)
                    polylines.Add(profile);
            }
            return polylines;
        }

        /// <summary>
        /// 转化为CAD中的数据格式，点选方式，输入只有一个环
        /// </summary>
        /// <param name="topoLoops"></param>
        /// <returns></returns>
        private List<Curve> ConvertTopoEdges2Curves(List<List<TopoEdge>> topoLoops)
        {
            if (topoLoops == null || topoLoops.Count == 0 || topoLoops.Count != 1)
                return null;

            var profile = new List<Curve>();
            foreach (var loop in topoLoops)
            {
                foreach (var edge in loop)
                {
                    profile.Add(edge.SrcCurve);
                }
            }

            return profile;
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

        public static Polyline ConvertLoop2Polyline(List<TopoEdge> topoEdges)
        {
            if (topoEdges == null || topoEdges.Count == 0)
                return null;

            var polyline = new Polyline();
            polyline.Closed = true;
            for (int i = 0; i < topoEdges.Count; i++)
            {
                var edge = topoEdges[i];
                if (edge.IsLine)
                {
                    polyline.AddVertexAt(i, new Point2d(edge.Start.X, edge.Start.Y), 0, 0, 0);
                }
                else
                {
                    Arc arc = edge.SrcCurve as Arc;
                    double bulge = 0;
                    if (CommonUtils.Point3dIsEqualPoint3d(arc.StartPoint, edge.Start))
                    {
                        bulge = Math.Tan(arc.TotalAngle / 4.0);
                    }
                    else
                    {
                        bulge = -Math.Tan(arc.TotalAngle / 4.0);
                    }

                    polyline.AddVertexAt(i, new Point2d(edge.Start.X, edge.Start.Y), bulge, 0, 0);
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
        private List<List<TopoEdge>> TransFormProfileLoops(List<Profile> loops)
        {
            if (loops == null || loops.Count == 0)
            {
                return null;
            }

            var resEdgesLoop = new List<List<TopoEdge>>();
            if (!m_planeBox.IsTranslation())
            {
                foreach (var profile in loops)
                {
                    if (profile.IsValid)
                    {
                        resEdgesLoop.Add(profile.TopoEdges);
                    }
                }
                return resEdgesLoop;
            }

            var transValue = m_planeBox.TransValue;

            foreach (var loop in loops)
            {
                var loopProfile = new List<TopoEdge>();
                if (loop.IsValid)
                {
                    foreach (var edge in loop.TopoEdges)
                    {
                        var transEdge = CommonUtils.LineDecVector(edge, transValue);
                        loopProfile.Add(transEdge);
                    }
                }
                resEdgesLoop.Add(loopProfile);
            }

            return resEdgesLoop;
        }

        public static List<List<TopoEdge>> PostProcessProfiles(List<List<TopoEdge>> srcEdgeLoops)
        {
            if (srcEdgeLoops == null)
                return null;
            if (srcEdgeLoops.Count == 1)
                return srcEdgeLoops;

            var edgeLoops = new List<List<TopoEdge>>();

            return edgeLoops;
        }
        /// <summary>
        /// 删除重复边
        /// </summary>
        /// <param name="srcEdgeLoops"></param>
        /// <returns></returns>
        public static List<Profile> RemoveDuplicate(List<Profile> srcEdgeLoops)
        {
            if (srcEdgeLoops == null)
                return null;
            if (srcEdgeLoops.Count == 1)
                return srcEdgeLoops;

            var proLoops = new List<ProductLoop>();
            for (int i = 0; i < srcEdgeLoops.Count; i++)
            {
                var curLoop = srcEdgeLoops[i].TopoEdges;
                var resEdges = CommonUtils.ConvertEdges(curLoop);
                var pt3d = curLoop.First().Start;
                for (int j = 1; j < resEdges.Count; j++)
                {
                    var curPt = resEdges[j].Start;
                    pt3d = CommonUtils.Point3dAddPoint3d(pt3d, curPt);
                }

                var ptCen = pt3d / resEdges.Count;
                //if (ptCen.GetDistanceTo(new Point3d(0, 0)) > 1e7)
                //    ptCen = new Point2d(ptCen.X * 1e-6, ptCen.Y * 1e-6);

                var curLoopArea = CommonUtils.CalcuLoopArea(resEdges);
                //if (Math.Abs(curLoopArea) > 1e7)
                //    curLoopArea *= 1e-6;
                proLoops.Add(new ProductLoop(srcEdgeLoops[i].TopoEdges, curLoopArea, ptCen));
            }

            var tmpLoop = proLoops.Distinct(new ProductLoopCom()).ToList();
            var resProfileLoops = new List<Profile>();
            foreach (var loop in tmpLoop)
            {
                resProfileLoops.Add(new Profile(loop.OneLoop, true));
            }

            return resProfileLoops;
        }
    }

    public class Profile
    {
        public Profile(List<TopoEdge> loop, bool bValid)
        {
            TopoEdges = loop;
            IsValid = bValid;
        }

        public Profile()
        {
            TopoEdges = new List<TopoEdge>();
            IsValid = true;
        }

        public XY leftBottomPoint; // 左边界
        public XY rightTopPoint;   // 右边界

        // 原始edges
        public List<TopoEdge> TopoEdges
        {
            get;
            set;
        } = null;

        public bool IsValid
        {
            get;
            set;
        } = true;

        // 离散后的edges
        public List<TopoEdge> TesslateEdges
        {
            get;
            set;
        } = null;

        public Point3d ProfileInnerPoint
        {
            get;
            set;
        } = Point3d.Origin;

        // 当前轮廓真实面积
        public double ProfileArea
        {
            get;
            set;
        } = 0;
    }

    public class CalcuContainPointProfile
    {
        class CurveNode
        {
            public Point3d point;
            public Curve curve;
            public CurveNode(Curve srcCurve, Point3d pt)
            {
                curve = srcCurve;
                point = pt;
            }
        }

        class CurveBound
        {
            public Curve curve;
            public bool IsValid = true;
            public XY leftBottomPoint; // 左边界
            public XY rightTopPoint;   // 右边界

            public CurveBound(Curve srcCurve, bool bValid)
            {
                curve = srcCurve;
                IsValid = bValid;
            }

            // 可相交curves集
            public List<CurveBound> relatedCurveBounds = new List<CurveBound>();
        }

        private List<CurveBound> curveBoundLst = new List<CurveBound>();

        public List<TopoEdge> Profile
        {
            get
            {
                if (m_ProfileLoop.Count > 0)
                    return m_ProfileLoop.First().TopoEdges;
                else
                    return null;
            }
        }

        private List<Profile> m_ProfileLoop = new List<Profile>();
        private HashMap m_hashMap = new HashMap();
        private List<TopoEdge> m_topoEdges = new List<TopoEdge>();
        private List<Curve> srcCurves = null;
        private Point3d aimPoint;

        public CalcuContainPointProfile(List<Curve> curves, Point3d pt)
        {
            srcCurves = curves;
            aimPoint = pt;
        }

        private CurveBound CalculateCurveBound(List<Curve> curves, Curve rightCurve)
        {
            double leftX = 0;
            double leftY = 0;
            double rightX = 0;
            double rightY = 0;
            CurveBound rightCurveBound = null;
            foreach (var curve in curves)
            {
                if (curve is Line)
                {
                    var line = curve as Line;
                    CommonUtils.CalculateLineBoundary(line, ref leftX, ref leftY, ref rightX, ref rightY);

                }
                else if (curve is Arc)
                {
                    var arc = curve as Arc;
                    CommonUtils.CalculateArcBoundary(arc, ref leftX, ref leftY, ref rightX, ref rightY);
                }
                else
                {
                    var bounds = curve.Bounds;
                    if (bounds.HasValue)
                    {
                        var bound = bounds.Value;
                        leftX = bound.MinPoint.X;
                        leftY = bound.MinPoint.Y;
                        rightX = bound.MaxPoint.X;
                        rightY = bound.MaxPoint.Y;
                    }
                }

                var curveBound = new CurveBound(curve, true);
                curveBound.leftBottomPoint = new XY(leftX - 0.01, leftY - 0.01);
                curveBound.rightTopPoint = new XY(rightX + 0.01, rightY + 0.01);
                curveBoundLst.Add(curveBound);
                if (curve.Equals(rightCurve))
                    rightCurveBound = curveBound;
            }

            double firLeftX = 0;
            double firLeftY = 0;
            double firRightX = 0;
            double firRightY = 0;

            double secLeftX = 0;
            double secLeftY = 0;
            double secRightX = 0;
            double secRightY = 0;
            for (int i = 0; i < curveBoundLst.Count; i++)
            {
                var curBound = curveBoundLst[i];
                firLeftX = curBound.leftBottomPoint.X;
                firLeftY = curBound.leftBottomPoint.Y;
                firRightX = curBound.rightTopPoint.X;
                firRightY = curBound.rightTopPoint.Y;

                for (int j = i + 1; j < curveBoundLst.Count; j++)
                {
                    var nextBound = curveBoundLst[j];
                    secLeftX = nextBound.leftBottomPoint.X;
                    secLeftY = nextBound.leftBottomPoint.Y;
                    secRightX = nextBound.rightTopPoint.X;
                    secRightY = nextBound.rightTopPoint.Y;
                    if (Math.Min(firRightX, secRightX) >= Math.Max(firLeftX, secLeftX)
                        && Math.Min(firRightY, secRightY) >= Math.Max(firLeftY, secLeftY))
                    {
                        curBound.relatedCurveBounds.Add(nextBound);
                        nextBound.relatedCurveBounds.Add(curBound);
                    }
                }
            }

            return rightCurveBound;
        }

        private List<Curve> CalcuRelatedCurvesFromCurve(CurveBound srcBound)
        {
            if (srcBound == null || srcBound.curve == null)
                return null;

            // 网状拓扑结构延伸
            var curves = new List<Curve>();
            var curveBounds = new List<CurveBound>();
            curveBounds.Add(srcBound);

            while (curveBounds.Count != 0)
            {
                var curBound = curveBounds.First();
                curveBounds.RemoveAt(0);
                curves.Add(curBound.curve);
                curBound.IsValid = false;

                if (curBound.relatedCurveBounds.Count != 0)
                {
                    foreach (var relatedBound in curBound.relatedCurveBounds)
                    {
                        if (relatedBound.IsValid && !curveBounds.Contains(relatedBound))
                            curveBounds.Add(relatedBound);
                    }
                }
            }

            return curves;
        }

        public List<Curve> DoCalRelatedCurves()
        {
            var rightCurve = CalcuRightCurve(srcCurves);
            if (rightCurve == null)
                return null;

            var rightCurveBound = CalculateCurveBound(srcCurves, rightCurve);
            var nearCurves = CalcuRelatedCurvesFromCurve(rightCurveBound);
            return nearCurves;
        }

        public void DoCal()
        {
            var rightCurve = CalcuRightCurve(srcCurves);
            if (rightCurve == null)
                return;

            var rightCurveBound = CalculateCurveBound(srcCurves, rightCurve);
            var nearCurves = CalcuRelatedCurvesFromCurve(rightCurveBound);
            var scatterCurves = ScatterCurves.MakeNewCurves(nearCurves);
            rightCurve = CalcuRightCurve(scatterCurves);
            if (rightCurve == null)
                return;

            TopoEdge startEdge = null;
            foreach (var curve in scatterCurves)
            {
                TopoEdge.MakeTopoEdge(curve, m_topoEdges);
                if (curve.Equals(rightCurve))
                {
                    var lastEdge = m_topoEdges.Last();
                    var startPt = lastEdge.Start;
                    var endPt = lastEdge.End;

                    if (endPt.Y > startPt.Y)
                    {
                        startEdge = lastEdge;
                    }
                    else
                    {
                        startEdge = m_topoEdges[m_topoEdges.Count - 2];
                    }
                }
            }

            if (startEdge == null)
                return;

            foreach (var topoEdge in m_topoEdges)
            {
                m_hashMap.Add(topoEdge);
            }

            BuildOneLoop(startEdge);
        }

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

                if (polys.Count > 1 && CommonUtils.Point3dIsEqualPoint3d(first.Start, last.End, 1e-1))
                {
                    if (CommonUtils.CalcuLoopArea(polys) > 10000)
                    {
                        m_ProfileLoop.Add(new Profile(polys, true));
                        break;
                    }
                }

                // 摘除环，继续寻找
                for (int i = 0; i < polys.Count - 1; i++)
                {
                    var Cedge = polys[i];
                    if (CommonUtils.Point3dIsEqualPoint3d(Cedge.End, last.End, 1e-1))
                    {
                        var k = i + 1;
                        var nEraseindex = k;
                        var edgeLoop = new List<TopoEdge>();
                        for (; k < polys.Count; k++)
                        {
                            edgeLoop.Add(polys[k]);
                        }

                        if (edgeLoop.Count > 1 && CommonUtils.CalcuLoopArea(polys) > 10000)
                        {
                            m_ProfileLoop.Add(new Profile(edgeLoop, true));
                        }
                        var nEraseCnt = polys.Count - nEraseindex;
                        polys.RemoveRange(nEraseindex, nEraseCnt);
                    }
                }
            }
        }

        /// <summary>
        /// 获取下一条有效边
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
        private TopoEdge GetNextEdgeInMaps(TopoEdge edge)
        {
            var tailPoint = edge.End;
            int hashKey = CommonUtils.HashKey(tailPoint);
            var adjTopoEdges = m_hashMap[hashKey];
            adjTopoEdges.AddRange(m_hashMap[(hashKey - 1 + CommonUtils.HashMapCount) % CommonUtils.HashMapCount]);
            adjTopoEdges.AddRange(m_hashMap[(hashKey + 1) % CommonUtils.HashMapCount]);
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
                var curPtHead = curEdge.Start;
                if (curEdge.IsUse)
                {
                    continue;
                }
                if (curEdge.Pair == edge)
                {
                    continue;
                }

                if (CommonUtils.Point3dIsEqualPoint3d(tailPoint, curPtHead, 1e-1))
                {
                    var clockEdge = new ClockWiseMatch(curEdge);
                    clockEdge.Angle = curEndDir.CalAngle(clockEdge.StartDir);
                    clockWiseMatchs.Add(clockEdge);
                }
            }

            if (clockWiseMatchs.Count == 0)
                return null;

            clockWiseMatchs.Sort((s1, s2) => { return s1.Angle.CompareTo(s2.Angle); });
            clockWiseMatchs.Last().TopoEdge.IsUse = true;
            return clockWiseMatchs.Last().TopoEdge;
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
            polys.Last().ValidEdge = false;
            polys.Last().Pair.ValidEdge = false;
            polys.RemoveAt(polys.Count - 1);
        }

        private Curve CalcuRightCurve(List<Curve> curves)
        {
            double firLeftX = 0;
            double firLeftY = 0;
            double firRightX = 0;
            double firRightY = 0;
            var curveNodes = new List<CurveNode>();

            Point2d end = new Point2d(aimPoint.X + 100000000000, aimPoint.Y);
            LineSegment2d intersectLine = new LineSegment2d(new Point2d(aimPoint.X, aimPoint.Y), end);
            Line lineHori = new Line(aimPoint, new Point3d(end.X, end.Y, 0));
            var ptLst = new Point3dCollection();
            foreach (var curve in curves)
            {
                if (curve is Line)
                {
                    var line = curve as Line;
                    CommonUtils.CalculateLineBoundary(line, ref firLeftX, ref firLeftY, ref firRightX, ref firRightY);
                }
                else if (curve is Arc)
                {
                    var arc = curve as Arc;
                    CommonUtils.CalculateArcBoundary(arc, ref firLeftX, ref firLeftY, ref firRightX, ref firRightY);
                }
                else
                {
                    var bounds = curve.Bounds;
                    if (bounds.HasValue)
                    {
                        var bound = bounds.Value;
                        firLeftY = bound.MinPoint.Y;
                        firRightY = bound.MaxPoint.Y;
                    }
                }

                if (aimPoint.Y > firRightY || aimPoint.Y < firLeftY)
                    continue;

                Point2d[] intersectPts;
                if (curve is Line)
                {
                    var line3d = curve as Line;
                    LineSegment2d line = new LineSegment2d(new Point2d(line3d.StartPoint.X, line3d.StartPoint.Y), new Point2d(line3d.EndPoint.X, line3d.EndPoint.Y));
                    intersectPts = line.IntersectWith(intersectLine);
                    if (intersectPts != null && intersectPts.Count() == 1)
                    {
                        var interPt = intersectPts.First();
                        curveNodes.Add(new CurveNode(curve, new Point3d(interPt.X, interPt.Y, 0)));
                    }
                }
                else if (curve is Arc)
                {
                    var arc3d = curve as Arc;
                    var startPt = arc3d.StartPoint;
                    var endPt = arc3d.EndPoint;
                    var midPoint = arc3d.GetPointAtParameter((arc3d.StartParam + arc3d.EndParam) * 0.5);
                    var arc = new CircularArc2d(new Point2d(startPt.X, startPt.Y), new Point2d(midPoint.X, midPoint.Y), new Point2d(endPt.X, endPt.Y));

                    intersectPts = arc.IntersectWith(intersectLine);
                    if (intersectPts != null && intersectPts.Count() == 1)
                    {
                        var interPt = intersectPts.First();
                        curveNodes.Add(new CurveNode(curve, new Point3d(interPt.X, interPt.Y, 0)));
                    }
                }
                else
                {
                    lineHori.IntersectWith(curve, Intersect.OnBothOperands, ptLst, new System.IntPtr(0), new System.IntPtr(0));
                    if (ptLst.Count != 0)
                    {
                        if (ptLst.Count == 1)
                            curveNodes.Add(new CurveNode(curve, ptLst[0]));
                        else
                        {
                            var tmpPtLst = new List<Point3d>();
                            for (int i = 0; i < ptLst.Count; i++)
                                tmpPtLst.Add(ptLst[i]);
                            tmpPtLst.Sort((s1, s2) => { return s1.X.CompareTo(s2.X); });
                            curveNodes.Add(new CurveNode(curve, tmpPtLst[0]));
                        }
                    }
                }
            }

            if (curveNodes.Count != 0)
            {
                curveNodes.Sort((s1, s2) => { return (s1.point - aimPoint).Length.CompareTo((s2.point - aimPoint).Length); });
                return curveNodes.First().curve;
            }

            return null;
        }
    }

    /// <summary>
    /// 计算内部点
    /// </summary>
    class CalInnerPoint
    {
        public Point3d InnerPt;
        private List<TopoEdge> loop;
        public CalInnerPoint(List<TopoEdge> topoEdges)
        {
            loop = topoEdges;
        }

        private void Do()
        {
            if (CommonUtils.CalcuLoopArea(loop) > 0)
            {
                for (int i = 0; i < loop.Count; i++)
                {

                    var curEdge = loop[i];
                    var curDir = curEdge.End - curEdge.Start;
                    var nextEdge = loop[(i + 1) / loop.Count];
                    var nextDir = nextEdge.End - nextEdge.Start;
                    var cross = curDir.X * nextDir.Y - curDir.Y * nextDir.X;
                    if (cross > 0)
                    {
                        InnerPt = new Point3d((curEdge.Start.X + curEdge.End.X + nextEdge.End.X) / 3, (curEdge.Start.Y + curEdge.End.Y + nextEdge.End.Y) / 3, 0);
                        return;
                    }
                }
            }
            else
            {
                for (int i = 0; i < loop.Count; i++)
                {
                    var curEdge = loop[i];
                    var curDir = curEdge.End - curEdge.Start;
                    var nextEdge = loop[(i + 1) % loop.Count];
                    var nextDir = nextEdge.End - nextEdge.Start;
                    var cross = curDir.X * nextDir.Y - curDir.Y * nextDir.X;
                    if (cross < 0)
                    {
                        InnerPt = new Point3d((curEdge.Start.X + curEdge.End.X + nextEdge.End.X) / 3, (curEdge.Start.Y + curEdge.End.Y + nextEdge.End.Y) / 3, 0);
                        return;
                    }
                }
            }
        }

        public static Point3d MakeInnerPoint(List<TopoEdge> loop)
        {
            var cal = new CalInnerPoint(loop);
            cal.Do();
           return cal.InnerPt;
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
        private List<Profile> m_ProfileLoop = new List<Profile>();
        public List<Profile> ProfileLoops
        {
            get { return m_ProfileLoop; }
        }

        public static List<Profile> MakeProfileLoop(List<Curve> curves)
        {
            if (curves == null || curves.Count == 0)
                return null;

            var topoCal = new TopoCalculate(curves);
            return topoCal.ProfileLoops;
        }

        public static List<Profile> MakeProfileLoopFromPoint(List<Curve> curves, Point3d pt)
        {
            if (curves == null || curves.Count == 0)
                return null;

            var topoCal = new TopoCalculate(curves, pt);
            return topoCal.ProfileLoops;
        }

        private TopoCalculate(List<Curve> SrcCurves)
        {
            m_curves = SrcCurves;
            //Utils.DrawProfile(m_curves, "src");
            var curves = ScatterCurves.MakeNewCurves(m_curves);

            //Utils.DrawProfile(curves, "scatter");
            //return;
            Calculate(curves);
        }

        private TopoCalculate(List<Curve> SrcCurves, Point3d pt)
        {
            m_curves = SrcCurves;

            var profileCalcu = new CalcuContainPointProfile(m_curves, pt);
            profileCalcu.DoCal();
            var outEdges = profileCalcu.Profile;

            if (outEdges == null)
                return;
            m_ProfileLoop.Add(new Profile(outEdges, true));
        }


        private void Calculate(List<Curve> srcCurves, Point3d pt)
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

            m_ProfileLoop = TopoSearch.RemoveDuplicate(m_ProfileLoop);
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

            m_ProfileLoop = TopoSearch.RemoveDuplicate(m_ProfileLoop);
            CalculateBound();
            PostProcessLoop();
        }

        private void CalculateBound()
        {
            var ptLst = new List<Point3d>();
            foreach (var loopProfile in m_ProfileLoop)
            {
                ptLst.Clear();
                var curLoop = loopProfile.TopoEdges;
                for (int i = 0; i < curLoop.Count; i++)
                {
                    var curEdge = curLoop[i];
                    if (curEdge.IsLine)
                    {
                        ptLst.Add(curEdge.Start);
                        ptLst.Add(curEdge.End);
                    }
                    else
                    {
                        var arc = curEdge.SrcCurve as Arc;
                        var ptCenter = arc.Center;
                        var radius = arc.Radius;
                        var leftPoint = arc.Center + new Vector3d(-1, 0, 0) * radius;
                        var rightPoint = arc.Center + new Vector3d(1, 0, 0) * radius;
                        var leftX = leftPoint.X;
                        var leftY = leftPoint.Y - radius;
                        var rightX = rightPoint.X;
                        var rightY = rightPoint.Y + radius;
                        ptLst.Add(new Point3d(leftX, leftY, 0));
                        ptLst.Add(new Point3d(rightX, rightY, 0));
                    }
                }

                var leftBottom = new XY(ptLst[0].X, ptLst[0].Y);
                var rightTop = new XY(ptLst[0].X, ptLst[0].Y);
                for (int i = 1; i < ptLst.Count; i++)
                {
                    if (leftBottom.X > ptLst[i].X)
                        leftBottom.X = ptLst[i].X;
                    if (leftBottom.Y > ptLst[i].Y)
                        leftBottom.Y = ptLst[i].Y;

                    if (rightTop.X < ptLst[i].X)
                        rightTop.X = ptLst[i].X;
                    if (rightTop.Y < ptLst[i].Y)
                        rightTop.Y = ptLst[i].Y;
                }

                loopProfile.leftBottomPoint = leftBottom;
                loopProfile.rightTopPoint = rightTop;
            }
        }

        private void PostProcessLoop()
        {
            for (int i = 0; i < m_ProfileLoop.Count; i++)
            {
                var curProfile = m_ProfileLoop[i];
                for (int j = 0; j < m_ProfileLoop.Count; j++)
                {
                    if (i == j)
                        continue;

                    var nextProfile = m_ProfileLoop[j];
                    if (!IntersectValid(curProfile, nextProfile))
                        continue;

                    if (nextProfile.TesslateEdges == null)
                        nextProfile.TesslateEdges = CommonUtils.ConvertEdges(nextProfile.TopoEdges);
                    if (nextProfile.ProfileInnerPoint == Point3d.Origin)
                        nextProfile.ProfileInnerPoint = CalInnerPoint.MakeInnerPoint(nextProfile.TesslateEdges);
                    if (OutProfileContainInnerProfile(curProfile, nextProfile))
                    {
                        curProfile.IsValid = false;
                        break;
                    }
                }
            }
        }

        private bool OutProfileContainInnerProfile(Profile outterProfile, Profile innerProfile)
        {
            if (CommonUtils.PtInLoop(outterProfile.TopoEdges, innerProfile.ProfileInnerPoint))
            {
                // 计算内外面积
                if (outterProfile.ProfileArea < 10)
                    outterProfile.ProfileArea = Math.Abs(CommonUtils.CalcuLoopArea(outterProfile.TopoEdges));

                if (innerProfile.ProfileArea < 10)
                    innerProfile.ProfileArea = Math.Abs(CommonUtils.CalcuLoopArea(innerProfile.TopoEdges));

                if (outterProfile.ProfileArea > innerProfile.ProfileArea)
                    return true;
            }
            
            return false;
        }

        private bool IntersectValid(Profile profileFir, Profile profileSec)
        {
            var firLeftX = profileFir.leftBottomPoint.X;
            var firLeftY = profileFir.leftBottomPoint.Y;
            var firRightX = profileFir.rightTopPoint.X;
            var firRightY = profileFir.rightTopPoint.Y;

            var secLeftX = profileSec.leftBottomPoint.X;
            var secLeftY = profileSec.leftBottomPoint.Y;
            var secRightX = profileSec.rightTopPoint.X;
            var secRightY = profileSec.rightTopPoint.Y;
            if (Math.Min(firRightX, secRightX) >= Math.Max(firLeftX, secLeftX)
                && Math.Min(firRightY, secRightY) >= Math.Max(firLeftY, secLeftY))
                return true;

            return false;
        }
        /// <summary>
        /// 获取下一条有效边
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
        private TopoEdge GetNextEdgeInMaps(TopoEdge edge)
        {
            var tailPoint = edge.End;
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
                var curPtHead = curEdge.Start;
                if (curEdge.IsUse)
                {
                    continue;
                }
                if (curEdge.Pair == edge)
                {
                    continue;
                }

                if (CommonUtils.Point3dIsEqualPoint3d(tailPoint, curPtHead, 1e-1))
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
            polys.Last().ValidEdge = false;
            polys.Last().Pair.ValidEdge = false;
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

                if (polys.Count > 1 && CommonUtils.Point3dIsEqualPoint3d(first.Start, last.End, 1e-1))
                {
                    m_ProfileLoop.Add(new Profile(polys, true));
                    break;
                }

                // 摘除环，继续寻找
                for (int i = 0; i < polys.Count - 1; i++)
                {
                    var Cedge = polys[i];
                    if (CommonUtils.Point3dIsEqualPoint3d(Cedge.End, last.End, 1e-1))
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
                            m_ProfileLoop.Add(new Profile(edgeLoop, true));
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

                    if (!CommonUtils.IntersectValid(curCurve, nextCurve))
                        continue;

                    var ptLst = new Point3dCollection();
                    curCurve.IntersectWith(nextCurve, Intersect.OnBothOperands, ptLst, (IntPtr)0, (IntPtr)0);
                    if (ptLst.Count != 0)
                    {
                        foreach (Point3d pt in ptLst)
                        {
                            m_ScatterNodes[i].ptLst.Add(pt);
                            m_ScatterNodes[j].ptLst.Add(pt);
                        }
                    }
                }
            }
        }

        private void SortLineNode(ScatterNode lineCode)
        {
            var line = lineCode.srcCurve as Line;
            var ptLst = lineCode.ptLst;
            var ptFirst = ptLst.First();
            if (CommonUtils.IsAlmostNearZero(line.Angle - Math.PI * 0.5) || CommonUtils.IsAlmostNearZero(line.Angle - Math.PI * 1.5))
            {
                ptLst.Sort((s1, s2) => { return (Math.Abs(s1.Y - ptFirst.Y)).CompareTo(Math.Abs(s2.Y - ptFirst.Y)); });
            }
            else
            {
                ptLst.Sort((s1, s2) => { return Math.Abs((s1.X - ptFirst.X)).CompareTo(Math.Abs(s2.X - ptFirst.X)); });
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
                    SortLineNode(scatterNode);
                }
                else
                {
                    SortArcNode(scatterNode);
                }
            }
        }

        private void SortArcNode(ScatterNode scatterNode)
        {
            var ptLst = scatterNode.ptLst;
            var arc = scatterNode.srcCurve as Arc;
            scatterNode.ptLst = ptLst.OrderBy(s => arc.GetParameterAtPoint(s)).ToList();
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
                        if ((curPoint - nextPoint).Length > 1e-4)
                        {
                            if (bFlag)
                            {
                                var newLine = new Line(curPoint, nextPoint);
                                m_geneCurves.Add(newLine);
                            }
                            else
                            {
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
            int key = CommonUtils.HashKey(point);
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
        /// 计算BoundCurves的边界
        /// </summary>
        /// <returns></returns>
        public List<Line> CalcuBoundCurves()
        {
            var lines = new List<Line>();
            lines.Add(CalculateLeftEdge());
            lines.Add(CalculateBottomEdge());
            lines.Add(CalculateRightEdge());
            lines.Add(CalculateTopEdge());
            return lines;
        }

        public Line CalculateLeftEdge()
        {
            var line = new Line(new Point3d(m_leftBottom.X, m_leftBottom.Y, 0), new Point3d(m_leftBottom.X, m_rightTop.Y, 0));
            return line;
        }

        public Line CalculateBottomEdge()
        {
            var line = new Line(new Point3d(m_leftBottom.X, m_leftBottom.Y, 0), new Point3d(m_rightTop.X, m_leftBottom.Y, 0));
            return line;
        }

        public Line CalculateRightEdge()
        {
            var line = new Line(new Point3d(m_rightTop.X, m_leftBottom.Y, 0), new Point3d(m_rightTop.X, m_rightTop.Y, 0));
            return line;
        }

        public Line CalculateTopEdge()
        {
            var line = new Line(new Point3d(m_rightTop.X, m_rightTop.Y, 0), new Point3d(m_leftBottom.X, m_rightTop.Y, 0));
            return line;
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
