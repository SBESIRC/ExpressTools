﻿using AcHelper;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TopoNode
{
    public class PolylineLayer
    {
        public Polyline profile = null;           // outProfile
        public List<string> profileLayers = null; // outProfile
        public List<Curve> profileCurves = null;  // outProfile
        public List<PolylineLayer> InnerPolylineLayers = new List<PolylineLayer>(); // innerProfiles
        public PolylineLayer(List<string> layers, List<Curve> srcCurves, Polyline poly)
        {
            profile = poly;
            profileLayers = layers;
            profileCurves = srcCurves;
        }
    }

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

        public Point3d intersectPt;

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

        private Tuple<List<Profile>, List<Profile>> m_loopMap = null;

        public Tuple<List<Profile>, List<Profile>> LoopMap
        {
            get { return m_loopMap; }
        }

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

        public static PolylineLayer MakeSrcProfileLayerLoops(List<Curve> curves)
        {
            var search = new TopoSearch(curves);
            //var loops = TopoSearch.RemoveDuplicate(search.m_srcLoops);
            var tmpEdgeLoops = search.TransFormProfileLoops(search.m_srcLoops);
            return search.ConvertTopoEdges2PolylineLayer(tmpEdgeLoops);
        }

        public static List<Curve> MakeSrcProfileLoopsFromPoint(List<Curve> curves, Point3d pt)
        {
            var search = new TopoSearch(curves, pt);
            var tmpEdgeLoops = search.TransFormProfileLoops(search.m_srcLoops);
            return search.ConvertTopoEdges2Curve(tmpEdgeLoops);
        }

        public static PolylineLayer MakeSrcProfileLoopsLayerFromPoint(List<Curve> totalCurves, List<Curve> relatedCurves, Point3d pt)
        {
            var search = new TopoSearch(totalCurves, relatedCurves, pt);
            var tmpEdgeLoops = search.TransFormProfileLoops();
            return search.ConvertTopoEdges2PolylineLayer(tmpEdgeLoops);
        }

        public static PolylineLayer MakeSrcProfileLoopsLayerFromPoints(List<Curve> totalCurves, List<Curve> relatedCurves, List<Point3d> pts)
        {
            var search = new TopoSearch(totalCurves, relatedCurves, pts);
            var tmpEdgeLoops = search.TransFormProfileLoops(search.m_srcLoops);
            return search.ConvertTopoEdges2PolylineLayer(tmpEdgeLoops);
        }

        public static PolylineLayer MakeSrcProfileLoopsLayerFromPoint(List<Curve> relatedCurves, Point3d pt)
        {
            var search = new TopoSearch(relatedCurves, pt);
            var tmpEdgeLoops = search.TransFormProfileLoops(search.m_srcLoops);
            return search.ConvertTopoEdges2PolylineLayer(tmpEdgeLoops);
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

        private TopoSearch(List<Curve> totalCurves, List<Curve> relatedCurves, Point3d pt)
        {
            m_planeBox = new BoundBoxPlane(totalCurves);

            if (m_planeBox.IsTranslation())
            {
                // 平移处理
                var relatedCurvesTrans = new List<Curve>();
                var trans = m_planeBox.TransValue;
                foreach (var curve in relatedCurves)
                {
                    var transCurve = MoveTransform(curve, trans);
                    relatedCurvesTrans.Add(transCurve);
                }

                var totalCurvesTrans = new List<Curve>();
                foreach (var curve in totalCurves)
                {
                    var transTotalCurve = MoveTransform(curve, trans);
                    totalCurvesTrans.Add(transTotalCurve);
                }

                pt = pt + new Vector3d(trans.X, trans.Y, 0);
                m_loopMap = TopoCalculate.MakeProfileLoopFromPoint(totalCurvesTrans, relatedCurvesTrans, pt);
            }
            else
            {
                // 不平移处理
                m_loopMap = TopoCalculate.MakeProfileLoopFromPoint(totalCurves, relatedCurves, pt);
            }
        }

        private TopoSearch(List<Curve> totalCurves, List<Curve> relatedCurves, List<Point3d> pts)
        {
            m_planeBox = new BoundBoxPlane(totalCurves);

            if (m_planeBox.IsTranslation())
            {
                // 平移处理
                var relatedCurvesTrans = new List<Curve>();
                var trans = m_planeBox.TransValue;
                foreach (var curve in relatedCurves)
                {
                    var transCurve = MoveTransform(curve, trans);
                    relatedCurvesTrans.Add(transCurve);
                }

                var totalCurvesTrans = new List<Curve>();
                foreach (var curve in totalCurves)
                {
                    var transTotalCurve = MoveTransform(curve, trans);
                    totalCurvesTrans.Add(transTotalCurve);
                }

                var movePts = new List<Point3d>();
                var moveDir = new Vector3d(trans.X, trans.Y, 0);
                foreach (var pt in pts)
                {
                    var movePt = pt + moveDir;
                    movePts.Add(movePt);
                }
                m_srcLoops = TopoCalculate.MakeProfileLoopFromPoints(totalCurvesTrans, relatedCurvesTrans, movePts);
            }
            else
            {
                // 不平移处理
                m_srcLoops = TopoCalculate.MakeProfileLoopFromPoints(totalCurves, relatedCurves, pts);
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

        private PolylineLayer ConvertTopoEdges2PolylineLayer(List<List<TopoEdge>> topoLoops)
        {
            if (topoLoops == null || topoLoops.Count == 0)
                return null;

            var tmpPolylineLayers = new List<PolylineLayer>();
            var polylines = new List<Curve>();
            foreach (var loop in topoLoops)
            {
                Polyline profile = ConvertLoop2Polyline(loop);
                if (profile == null)
                    continue;

                var profileCurves = TopoUtils.Polyline2Curves(profile, false);
                var layers = GetLayersFromTopoEdges(loop);
                if (profile != null && layers != null)
                {
                    tmpPolylineLayers.Add(new PolylineLayer(layers, profileCurves, profile));
                }
            }

            if (tmpPolylineLayers.Count == 0)
                return null;

            var aimPolylineLayer = tmpPolylineLayers.First();

            if (tmpPolylineLayers.Count > 1)
            {
                for (int i = 1; i < tmpPolylineLayers.Count; i++)
                {
                    aimPolylineLayer.InnerPolylineLayers.Add(tmpPolylineLayers[i]);
                }
            }

            return aimPolylineLayer;
        }


        private List<string> GetLayersFromTopoEdges(List<TopoEdge> topoEdges)
        {
            if (topoEdges == null && topoEdges.Count == 0)
                return null;

            var layers = new List<string>();
            foreach (var edge in topoEdges)
            {
                layers.Add(edge.SrcCurve.Layer);
            }

            return layers;
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

        // 坐标转换处理
        private List<List<TopoEdge>> TransFormProfileLoops()
        {
            var outProfile = m_loopMap.Item1;
            var innerProfiles = m_loopMap.Item2;
            if (outProfile == null || outProfile.Count == 0)
            {
                return null;
            }

            var loops = new List<Profile>();
            loops.AddRange(outProfile);
            if (innerProfiles != null && innerProfiles.Count != 0)
                loops.AddRange(innerProfiles);

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

                var curLoopArea = CommonUtils.CalcuTesslateLoopArea(resEdges);
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
        class CurveNodeInner
        {
            public Point3d point;
            public Curve curve;
            public CurveNodeInner(Curve srcCurve, Point3d pt)
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

        public List<Profile> Profiles
        {
            get { return m_ProfileLoop; }
        }

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

        public List<Profile> InnerProfiles
        {
            get
            {
                return m_InnerProfileLoops;
            }
        }

        private List<Profile> m_ProfileLoop = new List<Profile>(); // 外轮廓
        private List<Profile> m_InnerProfileLoops = new List<Profile>(); // 内轮廓

        public List<Profile> TotalProfileLoops = new List<Profile>();
        private HashMap m_hashMap = new HashMap();
        private HashMap m_innerHashMap = new HashMap();
        private List<TopoEdge> m_topoEdges = new List<TopoEdge>();
        private List<TopoEdge> m_innerEdges = new List<TopoEdge>();
        private List<Curve> srcCurves = null;
        private Point3d aimPoint;
        private List<Point3d> aimPoints;

        public CalcuContainPointProfile(List<Curve> curves, Point3d pt)
        {
            srcCurves = curves;
            aimPoint = pt;
        }

        public CalcuContainPointProfile(List<Curve> curves, List<Point3d> pts)
        {
            srcCurves = curves;
            aimPoints = pts;
        }

        /// <summary>
        /// 计算所有的curveBounds 以及相互连通关系
        /// </summary>
        /// <param name="srcCurves"></param>
        private void CalculateCurveBounds(List<Curve> srcCurves)
        {
            double leftX = 0;
            double leftY = 0;
            double rightX = 0;
            double rightY = 0;
            foreach (var curve in srcCurves)
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
            if (srcBound == null || srcBound.curve == null || srcBound.IsValid == false)
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

        /// <summary>
        /// 计算右边的相邻曲线
        /// </summary>
        /// <returns></returns>
        public List<Curve> DoCalRelatedCurves()
        {
            var rightCurves = CalcuRightCurves(srcCurves);
            if (rightCurves == null)
                return null;

            var nearCurves = new List<Curve>();
            CalculateCurveBounds(srcCurves);
            foreach (var rightCurve in rightCurves)
            {
                var rightCurveBound = FindRightCurveBound(rightCurve);
                var rightNearCurves = CalcuRelatedCurvesFromCurve(rightCurveBound);
                if (rightNearCurves != null && rightNearCurves.Count != 0)
                    nearCurves.AddRange(rightNearCurves);
            }

            return nearCurves;
        }

        private CurveBound FindRightCurveBound(Curve curve)
        {
            if (curveBoundLst.Count == 0)
                return null;

            foreach (var curveBound in curveBoundLst)
            {
                if (curveBound.curve.Equals(curve))
                    return curveBound;
            }

            return null;
        }

        public void DoCalPts(List<Curve> totalCurves)
        {
            var scatterCurves = ScatterCurves.MakeNewCurves(srcCurves);

            foreach (var pt in aimPoints)
            {
                List<Curve> scatterRightCurves = CalcuRightCurvesFromPoint(scatterCurves, pt);
                if (scatterRightCurves == null || scatterRightCurves.Count == 0)
                    continue;

                List<TopoEdge> rightStartEdges = new List<TopoEdge>();
                TopoEdge startEdge = null;
                foreach (var curve in scatterCurves)
                {
                    startEdge = null;
                    TopoEdge.MakeTopoEdge(curve, m_topoEdges);
                    foreach (var rightCurve in scatterRightCurves)
                    {
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

                            rightStartEdges.Add(startEdge);
                        }
                    }

                }


                if (rightStartEdges.Count == 0)
                    continue;

                if (rightStartEdges.Count > 1)
                    SortEdges(rightStartEdges);

                foreach (var topoEdge in m_topoEdges)
                {
                    m_hashMap.Add(topoEdge);
                }

                aimPoint = pt;
                // outer
                foreach (var startRightEdge in rightStartEdges)
                {
                    if (m_ProfileLoop.Count > 0)
                        break;

                    BuildOneLoop(startRightEdge);
                }

                if (m_ProfileLoop.Count > 0)
                {
                    TotalProfileLoops.Add(m_ProfileLoop.First());
                    m_ProfileLoop.Clear();
                }

                m_hashMap.Clear();
                m_topoEdges.Clear();
            }
        }

        public void DoCal()
        {
            var scatterCurves = ScatterCurves.MakeNewCurves(srcCurves);
            var scatterRightCurves = CalcuRightCurves(scatterCurves);
            //Utils.DrawProfile(scatterRightCurves, "scatter");
            //return;
            if (scatterRightCurves == null || scatterRightCurves.Count == 0)
                return;

            List<TopoEdge> rightStartEdges = new List<TopoEdge>();
            TopoEdge startEdge = null;
            foreach (var curve in scatterCurves)
            {
                startEdge = null;
                TopoEdge.MakeTopoEdge(curve, m_topoEdges);
                foreach (var rightCurve in scatterRightCurves)
                {
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

                        rightStartEdges.Add(startEdge);
                    }
                }

            }

            if (rightStartEdges.Count == 0)
                return;

            foreach (var topoEdge in m_topoEdges)
            {
                m_hashMap.Add(topoEdge);
            }

            // outer
            foreach (var startRightEdge in rightStartEdges)
            {
                if (m_ProfileLoop.Count > 0)
                    break;

                BuildOneLoop(startRightEdge);
            }
        }

        private void SortEdges(List<TopoEdge> srcEdges)
        {
            Point2d end = new Point2d(aimPoint.X + 100000000000, aimPoint.Y);
            LineSegment2d intersectLine = new LineSegment2d(new Point2d(aimPoint.X, aimPoint.Y), end);

            for (int i = 0; i < srcEdges.Count; i++)
            {
                var curEdge = srcEdges[i];
                var curCurve = curEdge.SrcCurve;
                if (curCurve is Line line3d)
                {
                    LineSegment2d line = new LineSegment2d(new Point2d(line3d.StartPoint.X, line3d.StartPoint.Y), new Point2d(line3d.EndPoint.X, line3d.EndPoint.Y));
                    var intersectPts = line.IntersectWith(intersectLine);
                    if (intersectPts != null && intersectPts.Count() == 1)
                    {
                        var interPt = intersectPts.First();
                        curEdge.intersectPt = new Point3d(interPt.X, interPt.Y, 0);
                    }
                }
                else if (curCurve is Arc arc3d)
                {
                    var startPt = arc3d.StartPoint;
                    var endPt = arc3d.EndPoint;
                    var midPoint = arc3d.GetPointAtParameter((arc3d.StartParam + arc3d.EndParam) * 0.5);
                    var arc = new CircularArc2d(new Point2d(startPt.X, startPt.Y), new Point2d(midPoint.X, midPoint.Y), new Point2d(endPt.X, endPt.Y));

                    var intersectPts = arc.IntersectWith(intersectLine);
                    if (intersectPts != null && intersectPts.Count() == 1)
                    {
                        var interPt = intersectPts.First();
                        curEdge.intersectPt = new Point3d(interPt.X, interPt.Y, 0);
                    }
                }
            }

            srcEdges.Sort((s1, s2) => { return (s1.intersectPt - aimPoint).Length.CompareTo((s2.intersectPt - aimPoint).Length); });
        }

        public void DoCal(List<Curve> totalCurves)
        {
            var scatterCurves = ScatterCurves.MakeNewCurves(srcCurves);
            //Utils.DrawProfile(scatterCurves, "scatter");
            //return;
            var scatterRightCurves = CalcuRightCurves(scatterCurves);
            if (scatterRightCurves == null || scatterRightCurves.Count == 0)
                return;

            List<TopoEdge> rightStartEdges = new List<TopoEdge>();
            TopoEdge startEdge = null;
            foreach (var curve in scatterCurves)
            {
                startEdge = null;
                TopoEdge.MakeTopoEdge(curve, m_topoEdges);
                foreach (var rightCurve in scatterRightCurves)
                {
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

                        rightStartEdges.Add(startEdge);
                    }
                }
            }


            if (rightStartEdges.Count == 0)
                return;

            if (rightStartEdges.Count > 1)
                SortEdges(rightStartEdges);

            foreach (var topoEdge in m_topoEdges)
            {
                m_hashMap.Add(topoEdge);
            }

            // outer
            foreach (var startRightEdge in rightStartEdges)
            {
                if (m_ProfileLoop.Count > 0)
                    break;

                BuildOneLoop(startRightEdge);
            }

            // inner
            if (m_ProfileLoop.Count == 0)
                return;

            var outProfile = m_ProfileLoop.First();
            // 包含column
            var columnScatterCurves = scatterCurves.Where(p =>
            {
                if (p.Layer.ToUpper().Contains("COLU"))
                    return true;
                return false;
            }).ToList();

            if (columnScatterCurves.Count == 0)
                return;

            var relatedCurves = CalcuRelatedCurves(columnScatterCurves, outProfile.TopoEdges);
            //Utils.DrawProfile(relatedCurves, "rela");
            //return;
            CalculateLoop(relatedCurves);

            if (m_InnerProfileLoops.Count == 0)
                return;

            CalInnerProfiles(outProfile.TopoEdges);
        }

        private void CalInnerProfiles(List<TopoEdge> outProfile)
        {
            var innerProfileEdges = new List<List<TopoEdge>>(); // 内轮廓 辅助变量
            foreach (var profile in m_InnerProfileLoops)
            {
                innerProfileEdges.Add(profile.TopoEdges);
                //Utils.DrawProfile(profile.TopoEdges, "innertest");
            }

            //return;
            // 内轮廓集
            innerProfileEdges.Add(outProfile);
            var loopProfilesCal = new LoopEntity(innerProfileEdges);
            loopProfilesCal.CalcuChild();
            var loopProfiles = loopProfilesCal.RootInnerLoop;

            if (loopProfiles.Count == 0)
                return;

            // 相邻轮廓
            var innerLoopProfile = new InnerLoopProfile(loopProfiles);
            innerLoopProfile.Do();
            var tmp = innerLoopProfile.OutLoops;

            // 洞口轮廓
            m_InnerProfileLoops.Clear();
            foreach (var loop in innerLoopProfile.OutLoops)
            {
                m_InnerProfileLoops.Add(new TopoNode.Profile(loop, true));
            }
        }

        public void DoCalS(List<Curve> totalCurves)
        {
            var scatterCurves = ScatterCurves.MakeNewCurves(srcCurves);
            var layers = Utils.GetLayersFromCurves(scatterCurves);
            var scatterRightCurves = CalcuRightCurves(scatterCurves);
            //Utils.DrawProfile(scatterCurves, "scatter");
            //return;
            if (scatterRightCurves == null || scatterRightCurves.Count == 0)
                return;

            List<TopoEdge> rightStartEdges = new List<TopoEdge>();
            TopoEdge startEdge = null;
            foreach (var curve in scatterCurves)
            {
                startEdge = null;
                TopoEdge.MakeTopoEdge(curve, m_topoEdges);
                foreach (var rightCurve in scatterRightCurves)
                {
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

                        rightStartEdges.Add(startEdge);
                    }
                }

            }

            if (rightStartEdges.Count == 0)
                return;

            foreach (var topoEdge in m_topoEdges)
            {
                m_hashMap.Add(topoEdge);
            }

            foreach (var startRightEdge in rightStartEdges)
            {
                if (m_ProfileLoop.Count > 0)
                    break;

                BuildOneLoop(startRightEdge);
            }

            //if (m_ProfileLoop.Count == 0)
            //    return;

            //var totalScatterCurves = ScatterCurves.MakeNewCurves(totalCurves);
            //var profile = m_ProfileLoop.First();
            //var relatedCurves = CalcuRelatedCurves(totalScatterCurves, profile.TopoEdges);
            ////Utils.DrawProfile(relatedCurves, "rela");
            //CalculateLoop(relatedCurves);
        }

        private void CalculateLoop(List<Curve> allCurves)
        {
            foreach (var curve in allCurves)
            {
                TopoEdge.MakeTopoEdge(curve, m_innerEdges);
            }

            m_hashMap.Clear();
            foreach (var topoEdge in m_innerEdges)
            {
                m_hashMap.Add(topoEdge);
            }

            for (int i = 0; i < m_innerEdges.Count; i++)
            {
                if (m_innerEdges[i].IsUse)
                    continue;

                BuildInnerOneLoop(m_innerEdges[i]);
            }

            m_InnerProfileLoops = TopoSearch.RemoveDuplicate(m_InnerProfileLoops);
            //CalculateBound();
            //PostProcessLoop();
        }

        private List<Curve> CalcuRelatedCurves(List<Curve> srcCurves, List<TopoEdge> loop)
        {
            if (srcCurves == null || srcCurves.Count == 0)
                return null;

            var resCurves = new List<Curve>();
            foreach (var curve in srcCurves)
            {
                if (IsValidCurve(curve, loop))
                    resCurves.Add(curve);
            }

            return resCurves;
        }

        private bool IsCurveOnEdges(Curve curve, List<TopoEdge> topoEdges)
        {
            if (curve is Line line)
            {
                var ptS = line.StartPoint;
                var ptE = line.EndPoint;

                if (IsPointOnEdges(ptS, topoEdges) && IsPointOnEdges(ptE, topoEdges))
                    return true;
            }
            else if (curve is Arc arc)
            {
                var ptS = arc.StartPoint;
                var ptE = arc.EndPoint;
                var midPt = arc.GetPointAtParameter(0.5 * (arc.StartParam + arc.EndParam));

                if (IsPointOnEdges(ptS, topoEdges) && IsPointOnEdges(ptE, topoEdges) && IsPointOnEdges(midPt, topoEdges))
                    return true;
            }

            return false;
        }

        private bool IsPointOnEdges(Point3d pt, List<TopoEdge> topoEdges)
        {
            foreach (var edge in topoEdges)
            {
                if (CommonUtils.IsPointOnSegment(pt, edge))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsValidCurve(Curve curve, List<TopoEdge> topoEdges)
        {
            if (curve == null)
                return false;

            if (IsCurveOnEdges(curve, topoEdges))
                return false;

            if (curve is Line || curve is Arc)
            {
                var midPt = curve.GetPointAtParameter(0.5 * (curve.StartParam + curve.EndParam)) + new Vector3d(0, 0.12345, 0);
                if (CommonUtils.PtInLoop(topoEdges, midPt))
                    return true;
            }

            return false;
        }

        private void BuildInnerOneLoop(TopoEdge edge)
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
                    if (Math.Abs(CommonUtils.CalcuLoopArea(polys)) > 10)
                    {
                        m_InnerProfileLoops.Add(new Profile(polys, true));
                    }
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

                        if (edgeLoop.Count > 1 && Math.Abs(CommonUtils.CalcuLoopArea(polys)) > 10)
                        {
                            m_InnerProfileLoops.Add(new Profile(edgeLoop, true));
                        }
                        var nEraseCnt = polys.Count - nEraseindex;
                        polys.RemoveRange(nEraseindex, nEraseCnt);
                    }
                }
            }
        }

        private void BuildOneLoop(TopoEdge edge)
        {
            var polys = new List<TopoEdge>();
            edge.IsUse = true;
            polys.Add(edge);
            int nCount = 0;

            while (polys.Count != 0)
            {
                nCount++;
                if (nCount > 2000)
                    return;

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
                        if (CommonUtils.PtInLoop(polys, aimPoint))
                        {
                            m_ProfileLoop.Add(new Profile(polys, true));
                            return;
                        }
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
                            if (CommonUtils.PtInLoop(edgeLoop, aimPoint))
                            {
                                m_ProfileLoop.Add(new Profile(edgeLoop, true));
                                return;
                            }
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
            var curTopoEdges = m_hashMap[hashKey];
            var beforeEdges = m_hashMap[(hashKey - 1 + CommonUtils.HashMapCount) % CommonUtils.HashMapCount];
            var nextEdges = m_hashMap[(hashKey + 1) % CommonUtils.HashMapCount];
            var adjTopoEdges = new List<TopoEdge>();

            if (curTopoEdges.Count != 0)
                adjTopoEdges.AddRange(curTopoEdges);
            if (beforeEdges.Count != 0)
                adjTopoEdges.AddRange(beforeEdges);
            if (nextEdges.Count != 0)
                adjTopoEdges.AddRange(nextEdges);

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

        /// <summary>
        ///  有可能某一条右边是不符合条件， 所以计算所有的右边
        /// </summary>
        /// <param name="curves"></param>
        /// <returns></returns>
        private List<Curve> CalcuRightCurvesFromPoint(List<Curve> curves, Point3d aimPt)
        {
            double firLeftX = 0;
            double firLeftY = 0;
            double firRightX = 0;
            double firRightY = 0;
            var curveNodes = new List<CurveNodeInner>();

            Point2d end = new Point2d(aimPt.X + 100000000000, aimPt.Y);
            LineSegment2d intersectLine = new LineSegment2d(new Point2d(aimPt.X, aimPt.Y), end);
            Line lineHori = new Line(aimPt, new Point3d(end.X, end.Y, 0));
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

                if (aimPt.Y > firRightY || aimPt.Y < firLeftY)
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
                        curveNodes.Add(new CurveNodeInner(curve, new Point3d(interPt.X, interPt.Y, 0)));
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
                        curveNodes.Add(new CurveNodeInner(curve, new Point3d(interPt.X, interPt.Y, 0)));
                    }
                }
                else
                {
                    lineHori.IntersectWith(curve, Intersect.OnBothOperands, ptLst, new System.IntPtr(0), new System.IntPtr(0));
                    if (ptLst.Count != 0)
                    {
                        if (ptLst.Count == 1)
                            curveNodes.Add(new CurveNodeInner(curve, ptLst[0]));
                        else
                        {
                            var tmpPtLst = new List<Point3d>();
                            for (int i = 0; i < ptLst.Count; i++)
                                tmpPtLst.Add(ptLst[i]);
                            tmpPtLst.Sort((s1, s2) => { return s1.X.CompareTo(s2.X); });
                            curveNodes.Add(new CurveNodeInner(curve, tmpPtLst[0]));
                        }
                    }
                }
            }

            if (curveNodes.Count != 0)
            {
                curveNodes.Sort((s1, s2) => { return (s1.point - aimPt).Length.CompareTo((s2.point - aimPt).Length); });
                var rightCurves = new List<Curve>();
                curveNodes.ForEach(node => rightCurves.Add(node.curve));
                return rightCurves;
            }

            return null;
        }

        /// <summary>
        ///  有可能某一条右边是不符合条件， 所以计算所有的右边
        /// </summary>
        /// <param name="curves"></param>
        /// <returns></returns>
        List<Curve> CalcuRightCurves(List<Curve> curves)
        {
            double firLeftX = 0;
            double firLeftY = 0;
            double firRightX = 0;
            double firRightY = 0;
            var curveNodes = new List<CurveNodeInner>();

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
                        curveNodes.Add(new CurveNodeInner(curve, new Point3d(interPt.X, interPt.Y, 0)));
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
                        curveNodes.Add(new CurveNodeInner(curve, new Point3d(interPt.X, interPt.Y, 0)));
                    }
                }
                else
                {
                    lineHori.IntersectWith(curve, Intersect.OnBothOperands, ptLst, new System.IntPtr(0), new System.IntPtr(0));
                    if (ptLst.Count != 0)
                    {
                        if (ptLst.Count == 1)
                            curveNodes.Add(new CurveNodeInner(curve, ptLst[0]));
                        else
                        {
                            var tmpPtLst = new List<Point3d>();
                            for (int i = 0; i < ptLst.Count; i++)
                                tmpPtLst.Add(ptLst[i]);
                            tmpPtLst.Sort((s1, s2) => { return s1.X.CompareTo(s2.X); });
                            curveNodes.Add(new CurveNodeInner(curve, tmpPtLst[0]));
                        }
                    }
                }
            }

            if (curveNodes.Count != 0)
            {
                curveNodes.Sort((s1, s2) => { return (s1.point - aimPoint).Length.CompareTo((s2.point - aimPoint).Length); });
                var rightCurves = new List<Curve>();
                curveNodes.ForEach(node => rightCurves.Add(node.curve));
                return rightCurves;
            }

            return null;
        }

        private Curve CalcuRightCurve(List<Curve> curves)
        {
            double firLeftX = 0;
            double firLeftY = 0;
            double firRightX = 0;
            double firRightY = 0;
            var curveNodes = new List<CurveNodeInner>();

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
                        curveNodes.Add(new CurveNodeInner(curve, new Point3d(interPt.X, interPt.Y, 0)));
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
                        curveNodes.Add(new CurveNodeInner(curve, new Point3d(interPt.X, interPt.Y, 0)));
                    }
                }
                else
                {
                    lineHori.IntersectWith(curve, Intersect.OnBothOperands, ptLst, new System.IntPtr(0), new System.IntPtr(0));
                    if (ptLst.Count != 0)
                    {
                        if (ptLst.Count == 1)
                            curveNodes.Add(new CurveNodeInner(curve, ptLst[0]));
                        else
                        {
                            var tmpPtLst = new List<Point3d>();
                            for (int i = 0; i < ptLst.Count; i++)
                                tmpPtLst.Add(ptLst[i]);
                            tmpPtLst.Sort((s1, s2) => { return s1.X.CompareTo(s2.X); });
                            curveNodes.Add(new CurveNodeInner(curve, tmpPtLst[0]));
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
        private List<Profile> m_ProfileLoop = new List<Profile>(); // out Profle
        private List<Profile> m_InnerProfileLoops = new List<Profile>(); // innerProfiles

        public List<Profile> InnerProfileLoops
        {
            get { return m_InnerProfileLoops; }
        }

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

        public static List<Profile> MakeProfileLoopFromPoints(List<Curve> totalCurves, List<Curve> curves, List<Point3d> pts)
        {
            if (curves == null || curves.Count == 0)
                return null;

            var topoCal = new TopoCalculate(totalCurves, curves, pts);
            return topoCal.ProfileLoops;
        }

        public static Tuple<List<Profile>, List<Profile>> MakeProfileLoopFromPoint(List<Curve> totalCurves, List<Curve> curves, Point3d pt)
        {
            if (curves == null || curves.Count == 0)
                return null;

            var topoCal = new TopoCalculate(totalCurves, curves, pt);

            var outProfiles = topoCal.ProfileLoops;
            var innerProfiles = topoCal.InnerProfileLoops;
            var loopMaps = new Tuple<List<Profile>, List<Profile>>(outProfiles, innerProfiles);
            return loopMaps;
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
            var outEdges = profileCalcu.Profile; // out Profile

            if (outEdges == null)
                return;
            m_ProfileLoop.Add(new Profile(outEdges, true));
        }

        private TopoCalculate(List<Curve> totalCurves, List<Curve> SrcCurves, List<Point3d> pts)
        {
            m_curves = SrcCurves;

            var profileCalcu = new CalcuContainPointProfile(m_curves, pts);
            profileCalcu.DoCalPts(totalCurves);
            var totalProfileLoops = profileCalcu.TotalProfileLoops;

            if (totalProfileLoops.Count == 0)
                return;

            m_ProfileLoop.AddRange(totalProfileLoops);
        }

        private TopoCalculate(List<Curve> totalCurves, List<Curve> SrcCurves, Point3d pt)
        {
            m_curves = SrcCurves;

            var profileCalcu = new CalcuContainPointProfile(m_curves, pt);
            profileCalcu.DoCal(totalCurves);
            m_ProfileLoop.AddRange(profileCalcu.Profiles); // out Profile

            var innerProfiles = profileCalcu.InnerProfiles; // inner Profiles
            if (innerProfiles.Count != 0)
            {
                m_InnerProfileLoops.AddRange(innerProfiles);
            }
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
        private List<Curve> m_intersectCurves = null;
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

        public static List<Curve> MakeScatterCurves(List<Curve> srcCurves, List<Curve> intersectCurves)
        {
            var scatterCurves = new ScatterCurves(srcCurves, intersectCurves);
            return scatterCurves.Curves;
        }

        private ScatterCurves(List<Curve> srcCurves, List<Curve> intersectCurves)
        {
            m_curves = srcCurves;
            m_intersectCurves = intersectCurves;

            foreach (var curve in m_curves)
            {
                m_ScatterNodes.Add(new ScatterNode(curve, curve is Line));
            }

            IntersectWith();
            SortXYZPoints();
            NewCurves();
        }

        private void IntersectWith()
        {
            for (int i = 0; i < m_ScatterNodes.Count; i++)
            {
                var curCurve = m_ScatterNodes[i].srcCurve;
                for (int j = 0; j < m_intersectCurves.Count; j++)
                {
                    var nextCurve = m_intersectCurves[j];

                    if (!CommonUtils.IntersectValid(curCurve, nextCurve))
                        continue;

                    var ptLst = new Point3dCollection();
                    curCurve.IntersectWith(nextCurve, Intersect.OnBothOperands, ptLst, (IntPtr)0, (IntPtr)0);
                    if (ptLst.Count != 0)
                    {
                        foreach (Point3d pt in ptLst)
                        {
                            m_ScatterNodes[i].ptLst.Add(pt);
                        }
                    }
                }
            }
        }

        private ScatterCurves(List<Curve> curves)
        {
            m_curves = curves;

            foreach (var curve in m_curves)
            {
                m_ScatterNodes.Add(new ScatterNode(curve, curve is Line));
            }

            IntersectCurvesTole();
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

        private void IntersectCurvesTole()
        {
            for (int i = 0; i < m_ScatterNodes.Count; i++)
            {
                var curCurve = m_ScatterNodes[i].srcCurve;
                for (int j = i + 1; j < m_ScatterNodes.Count; j++)
                {
                    var nextCurve = m_ScatterNodes[j].srcCurve;

                    if (!CommonUtils.IntersectValid(curCurve, nextCurve))
                        continue;

                    var ptLst = CurveIntersectWithCurve(curCurve, nextCurve);
                    if (ptLst.Count != 0)
                    {
                        foreach (Point3d pt in ptLst)
                        {
                            PointAppend(m_ScatterNodes[i].ptLst, m_ScatterNodes[j].ptLst, pt);
                        }
                    }
                }
            }
        }


        private void PointAppend(List<Point3d> indexINode, List<Point3d> indexJNode, Point3d intersectPt, double tole = 1e-3)
        {
            bool bHas = false;
            foreach (var ptI in indexINode)
            {
                if (CommonUtils.Point3dIsEqualPoint3d(ptI, intersectPt, tole))
                {
                    bHas = true;
                    break;
                }
            }

            if (!bHas)
            {
                indexINode.Add(intersectPt);
            }

            foreach (var ptJ in indexJNode)
            {
                if (CommonUtils.Point3dIsEqualPoint3d(ptJ, intersectPt, tole))
                {
                    return;
                }
            }

            indexJNode.Add(intersectPt);
        }

        private List<Point3d> CurveIntersectWithCurve(Curve curveFir, Curve curveSec)
        {
            var ptLst = new List<Point3d>();
            if (curveFir is Line && curveSec is Line)
            {
                var lineFir = curveFir as Line;
                var lineSec = curveSec as Line;
                var lineFir3d = Line2Ge(lineFir);
                var lineSec3d = Line2Ge(lineSec);
                var intersecPts = lineFir3d.IntersectWith(lineSec3d, new Tolerance(1e-3, 1e-3));
                if (intersecPts != null)
                    ptLst = intersecPts.ToList();
            }
            else if (curveFir is Line && curveSec is Arc)
            {
                var lineFir = curveFir as Line;
                var arcSec = curveSec as Arc;
                var lineFir3d = Line2Ge(lineFir);
                var arcSec3d = Arc2Ge(arcSec);
                var intersecPts = arcSec3d.IntersectWith(lineFir3d, new Tolerance(1e-3, 1e-3));
                if (intersecPts != null)
                    ptLst = intersecPts.ToList();
            }
            else if (curveFir is Arc && curveSec is Line)
            {
                var arcFir = curveFir as Arc;
                var lineSec = curveSec as Line;
                var arcFir3d = Arc2Ge(arcFir);
                var lineSec3d = Line2Ge(lineSec);
                var intersecPts = arcFir3d.IntersectWith(lineSec3d, new Tolerance(1e-3, 1e-3));
                if (intersecPts != null)
                    ptLst = intersecPts.ToList();
            }
            else if (curveFir is Arc && curveSec is Arc)
            {
                var arcFir = curveFir as Arc;
                var arcSec = curveSec as Arc;
                var arc3dFir = Arc2Ge(arcFir);
                var arc3dSec = Arc2Ge(arcSec);
                var intersecPts = arc3dFir.IntersectWith(arc3dSec, new Tolerance(1e-3, 1e-3));
                if (intersecPts != null)
                    ptLst = intersecPts.ToList();
            }

            return ptLst;
        }

        private CircularArc3d Arc2Ge(Arc arc)
        {
            var ptS = arc.StartPoint;
            var ptE = arc.EndPoint;
            var ptMid = arc.GetPointAtParameter(0.5 * (arc.StartParam + arc.EndParam));
            var arc3d = new CircularArc3d(ptS, ptMid, ptE);
            return arc3d;
        }

        private Point2d P3Dto2D(Point3d point)
        {
            return new Point2d(point.X, point.Y);
        }

        private LineSegment3d Line2Ge(Line line)
        {
            var ptS = line.StartPoint;
            var ptE = line.EndPoint;
            var line3d = new LineSegment3d(ptS, ptE);
            return line3d;
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
                try
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
                catch
                {
                    scatterNode.ptLst.Clear();
                    // 圆弧重叠部分
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
                var layer = scatterNode.srcCurve.Layer;
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
                                newLine.Layer = layer;
                                m_geneCurves.Add(newLine);
                            }
                            else
                            {
                                var srcArc = scatterNode.srcCurve as Arc;
                                var srcArcNormal = srcArc.Normal;
                                var radius = srcArc.Radius;
                                var ptCenter = srcArc.Center;
                                var arc = CommonUtils.CreateArc(curPoint, ptCenter, nextPoint, radius);

                                //var arcNormal = arc.Normal;
                                //if (arcNormal.DotProduct(srcArcNormal) < 0)
                                //    arc = CommonUtils.CreateArc(nextPoint, ptCenter, curPoint, radius);
                                arc.Layer = layer;
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

        public void Clear()
        {
            m_hashMapEdges.Clear();
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

        public XY Negate()
        {
            return new XY(-m_x, -m_y);
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
            if (CommonUtils.IsAlmostNearZero((angleRad - Math.PI)))
                return angleRad;
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

    public class TopoEdgeCalculate
    {
        private List<Curve> m_curves = new List<Curve>();
        private List<TopoEdge> m_topoEdges = new List<TopoEdge>();
        private HashMap m_hashMap = new HashMap();
        private List<List<TopoEdge>> m_ProfileLoop = new List<List<TopoEdge>>();

        public List<List<TopoEdge>> ProfileLoops
        {
            get { return m_ProfileLoop; }
        }

        public TopoEdgeCalculate(List<TopoEdge> edges)
        {
            foreach (var edge in edges)
            {
                m_curves.Add(edge.SrcCurve);
            }
        }

        public void Do()
        {
            m_curves = CommonUtils.RemoveCollinearLines(m_curves);
            Calculate(m_curves);
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

                if (polys.Count > 1 && CommonUtils.Point3dIsEqualPoint3d(first.Start, last.End, 1e-1))
                {
                    if (Math.Abs(CommonUtils.CalcuLoopArea(polys)) > 10)
                    {
                        m_ProfileLoop.Add(polys);
                    }
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

                        if (edgeLoop.Count > 1 && Math.Abs(CommonUtils.CalcuLoopArea(polys)) > 10)
                        {
                            m_ProfileLoop.Add(edgeLoop);
                        }
                        var nEraseCnt = polys.Count - nEraseindex;
                        polys.RemoveRange(nEraseindex, nEraseCnt);
                    }
                }
            }
        }

        private TopoEdge GetNextEdgeInMaps(TopoEdge edge)
        {
            var tailPoint = edge.End;
            int hashKey = CommonUtils.HashKey(tailPoint);
            var curTopoEdges = m_hashMap[hashKey];
            var beforeEdges = m_hashMap[(hashKey - 1 + CommonUtils.HashMapCount) % CommonUtils.HashMapCount];
            var nextEdges = m_hashMap[(hashKey + 1) % CommonUtils.HashMapCount];
            var adjTopoEdges = new List<TopoEdge>();

            if (curTopoEdges.Count != 0)
                adjTopoEdges.AddRange(curTopoEdges);
            if (beforeEdges.Count != 0)
                adjTopoEdges.AddRange(beforeEdges);
            if (nextEdges.Count != 0)
                adjTopoEdges.AddRange(nextEdges);

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
    }
}
