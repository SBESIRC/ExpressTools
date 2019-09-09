using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThRoomBoundary.topo
{
    class ThUtils
    {
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
            return new TopoEdge(ptSadd, ptEadd);
        }
    }

    /// <summary>
    /// topo边的数据信息
    /// </summary>
    public class TopoEdge
    {
        private TopoEdge m_pair;
        private Point2d m_start = new Point2d();
        private Point2d m_end = new Point2d();
        private Vector2d m_dir = new Vector2d();
        private bool m_bUse = false;

        public bool IsUse
        {
            get { return m_bUse; }
            set { m_bUse = true; }
        }

        public TopoEdge(LineSegment2d srcLine)
        {
            m_start = srcLine.StartPoint;
            m_end = srcLine.EndPoint;
            m_dir = srcLine.Direction;
        }

        public TopoEdge(Point2d start, Point2d end)
        {
            m_start = start;
            m_end = end;
        }

        public TopoEdge Pair
        {
            get { return m_pair; }
            set { m_pair = value; }
        }

        public Point2d Start
        {
            get { return m_start; }
            set { m_start = value; }
        }

        public Point2d End
        {
            get { return m_end; }
            set { m_end = value; }
        }

        public Vector2d Dir
        {
            get { return m_dir; }
            set { m_dir = value; }
        }

        public static void MakeTopoEdge(LineSegment2d srcLine, List<TopoEdge> topoEdges)
        {
            var srcLineS = srcLine.StartPoint;
            var srcLineE = srcLine.EndPoint;
            TopoEdge curEdge = new TopoEdge(srcLineS, srcLineE);
            TopoEdge pairEdge = new TopoEdge(srcLineE, srcLineS);
            curEdge.Pair = pairEdge;
            pairEdge.Pair = curEdge;
            topoEdges.Add(curEdge);
            topoEdges.Add(pairEdge);
        }
    }

    /// <summary>
    /// 计算最小或者最大包围区域的功能处理，使用时根据功能分别调用相应的静态函数。
    /// </summary>
    class TopoSearch
    {
        private BoundBoxPlane m_planeBox = null;
        private Point2d m_point2d;
        private List<List<TopoEdge>> m_srcLoops = null;

        public List<List<TopoEdge>> SrcLoops
        {
            get { return m_srcLoops; }
        }

        private List<List<TopoEdge>> m_partialLoops = new List<List<TopoEdge>>();
        private List<AreaLoop> m_conPtLoops = new List<AreaLoop>();

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

        // 内部使用 获取点的最小包围区域
        public static List<List<LineSegment2d>> MakeMinProfileLoopsInner(List<LineSegment2d> lines, Point2d point)
        {
            var search = new TopoSearch(lines, point);
            var profileLoops = search.ContainsPtLoop();
            var tmpEdgeLoops = search.TransFormProfileLoops(profileLoops);
            return search.ConvertTopoEdges2Curve(tmpEdgeLoops);
        }

        public static List<List<LineSegment2d>> MakeSrcProfileLoops(List<LineSegment2d> lines)
        {
            var search = new TopoSearch(lines);
            
            var loops = TopoSearch.RemoveDuplicate(search.m_srcLoops);
            var tmpEdgeLoops = search.TransFormProfileLoops(loops);
            return search.ConvertTopoEdges2Curve(tmpEdgeLoops);
        }

        private TopoSearch(List<LineSegment2d> lines)
        {
            m_planeBox = new BoundBoxPlane(lines);

            if (m_planeBox.IsTranslation())
            {
                // 平移处理
                var trans = m_planeBox.TransValue;
                var tmpLines = new List<LineSegment2d>();
                foreach (var line in lines)
                {
                    var transLine = ThUtils.LineAddVector(line, trans);
                    tmpLines.Add(transLine);
                }

                m_srcLoops = TopoCalculate.MakeProfileLoop(tmpLines);
            }
            else
            {
                // 不平移处理
                m_srcLoops = TopoCalculate.MakeProfileLoop(lines);
            }

        }

        private TopoSearch(List<LineSegment2d> lines, Point2d point2d)
        {
            m_planeBox = new BoundBoxPlane(lines);

            if (m_planeBox.IsTranslation())
            {
                // 平移处理
                var trans = m_planeBox.TransValue;
                var tmpLines = new List<LineSegment2d>();
                foreach (var line in lines)
                {
                    var transLine = ThUtils.LineAddVector(line, trans);
                    tmpLines.Add(transLine);
                }

                m_srcLoops = TopoCalculate.MakeProfileLoop(tmpLines);
                m_point2d = point2d + trans;
            }
            else
            {
                // 不平移处理
                m_srcLoops = TopoCalculate.MakeProfileLoop(lines);
                m_point2d = point2d;
            }

        }

        /// <summary>
        /// 转化为CAD中的数据格式
        /// </summary>
        /// <param name="topoLoops"></param>
        /// <returns></returns>
        private List<List<LineSegment2d>> ConvertTopoEdges2Curve(List<List<TopoEdge>> topoLoops)
        {
            if (topoLoops == null)
                return null;

            var curveLoops = new List<List<LineSegment2d>>();
            foreach (var loop in topoLoops)
            {
                var profile = new List<LineSegment2d>();
                foreach (var edge in loop)
                {
                    var line = new LineSegment2d(edge.Start, edge.End);
                    profile.Add(line);
                }

                curveLoops.Add(profile);
            }

            return curveLoops;
        }

        // 坐标转换处理
        private List<List<TopoEdge>> TransFormProfileLoops(List<List<TopoEdge>> loops)
        {
            if (loops == null)
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
                    var transEdge = ThUtils.LineDecVector(edge, transValue);
                    loopEdge.Add(transEdge);
                }

                topoLoops.Add(loopEdge);
            }

            return topoLoops;
        }

        // minimal loop contains pt
        private List<List<TopoEdge>> ContainsPtLoop()
        {
            foreach (var curLoop in m_srcLoops)
            {
                if (CommonUtils.PtInLoop(curLoop, m_point2d))
                {
                    double loopArea = CommonUtils.CalcuLoopArea(curLoop);
                    m_conPtLoops.Add(new AreaLoop(curLoop, loopArea));
                }
                else
                {
                    m_partialLoops.Add(curLoop);
                }
            }

            if (m_conPtLoops.Count == 0)
                return null;
            m_conPtLoops.Sort((s1, s2) => { return Math.Abs(s1.LoopArea).CompareTo(Math.Abs(s2.LoopArea)); });
            List<TopoEdge> outLoop = m_conPtLoops[0].Loop;
            if (m_partialLoops.Count == 0)
                return new List<List<TopoEdge>>() { outLoop };

            var loops = new List<List<TopoEdge>>();
            loops.Add(outLoop);
            foreach (var partialLoop in m_partialLoops)
            {
                if (CommonUtils.OutLoopContainsInnerLoop(outLoop, partialLoop))
                    loops.Add(partialLoop);

            }

            // erase same loop and Get minimal profile
            var innerLoops = CommonUtils.GetMaxLoopInnerEdges(loops);
            var edgesLoop = new List<List<TopoEdge>>();
            edgesLoop.Add(outLoop);
            edgesLoop.AddRange(innerLoops);
            var resultTopoEdges = RemoveDuplicate(edgesLoop);
            return resultTopoEdges;
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
                var pt = curLoop.First().Start;
                for (int j = 1; j < curLoop.Count; j++)
                {
                    pt = CommonUtils.Point2dAddPoint2d(pt, curLoop[j].Start);
                }

                var ptCen = pt / curLoop.Count;
                proLoops.Add(new ProductLoop(srcEdgeLoops[i], CommonUtils.CalcuLoopArea(srcEdgeLoops[i]), ptCen));
            }

            var tmpLoop = proLoops.Distinct(new ProductLoopCom()).ToList();
            foreach (var loop in tmpLoop)
            {
                edgeLoops.Add(loop.OneLoop);
            }

            return edgeLoops;
        }
    }

    /// <summary>
    /// 计算topo环的面积
    /// </summary>
    class AreaLoop
    {
        private double m_nArea;
        public double LoopArea
        {
            get { return m_nArea; }
        }
        private List<TopoEdge> m_topoEdges;
        public List<TopoEdge> Loop
        {
            get { return m_topoEdges; }
        }

        public AreaLoop(List<TopoEdge> topoEdges, double area)
        {
            m_topoEdges = topoEdges;
            m_nArea = area;
        }
    }

    /// <summary>
    /// 记录当前环的深度
    /// </summary>
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

                    if (CommonUtils.OutLoopContainsInnerLoop(loops[j], curLoop))
                        nCount++;
                }

                if (m_nMaxDeep < nCount)
                    m_nMaxDeep = nCount;

                m_edgeLoops.Add(new EdgeLoop(curLoop, nCount));
            }

            CalcuChild();
        }

        // calculate every entity (outLoop innerLoop)
        private void CalcuChild()
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
                }
            }
        }
    }

    /// <summary>
    /// topo imp
    /// </summary>
    class TopoCalculate
    {
        private List<LineSegment2d> m_lines = null;
        private List<TopoEdge> m_topoEdges = new List<TopoEdge>();
        private HashMap m_hashMap = new HashMap();
        private List<List<TopoEdge>> m_ProfileLoop = new List<List<TopoEdge>>();
        public List<List<TopoEdge>> ProfileLoops
        {
            get { return m_ProfileLoop; }
        }

        public static List<List<TopoEdge>> MakeProfileLoop(List<LineSegment2d> lines)
        {
            var topoCal = new TopoCalculate(lines);
            return topoCal.ProfileLoops;
        }

        private TopoCalculate(List<LineSegment2d> SrcLines)
        {
            m_lines = SrcLines;
            var lines = ScatterLines.MakeNewLines(m_lines);
            Calculate(lines);
        }

        private void Calculate(List<LineSegment2d> srcLines)
        {
            foreach (var line in srcLines)
            {
                TopoEdge.MakeTopoEdge(line, m_topoEdges);
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
            var tailPoint = edge.End;
            int hashKey = CommonUtils.HashKey(tailPoint);
            var adjTopoEdges = m_hashMap[hashKey];
            if (adjTopoEdges.Count == 0)
            {
                return null;
            }

            var headPoint = edge.Start;
            var curDir = new XY(edge.Dir.X, edge.Dir.Y);
            var clockWiseMatchs = new List<ClockWiseMatch>();
            for (int i = 0; i < adjTopoEdges.Count; i++)
            {
                var curEdge = adjTopoEdges[i];
                var curPtHead = curEdge.Start;
                var curPtTail = curEdge.End;

                if (curEdge.IsUse || (CommonUtils.Point2dIsEqualPoint2d(headPoint, curPtTail, 1e-3) && CommonUtils.Point2dIsEqualPoint2d(tailPoint, curPtHead, 1e-3)))
                    continue;

                if (CommonUtils.Point2dIsEqualPoint2d(tailPoint, curEdge.Start, 1e-3))
                {
                    var clockEdge = new ClockWiseMatch(curEdge);
                    clockEdge.Angle = curDir.CalAngle(clockEdge.Dir);
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

                if (CommonUtils.Point2dIsEqualPoint2d(first.Start, last.End, 1e-3) && !CommonUtils.IsAlmostNearZero(CommonUtils.CalcuLoopArea(polys)))
                {
                    m_ProfileLoop.Add(polys);
                    break;
                }

                // 摘除环，继续寻找
                for (int i = 0; i < polys.Count - 1; i++)
                {
                    var Cedge = polys[i];
                    var point = Cedge.End;
                    if (CommonUtils.Point2dIsEqualPoint2d(point, last.End, 1e-3))
                    {
                        var k = i + 1;
                        var nEraseindex = k;
                        var edgeLoop = new List<TopoEdge>();
                        for (; k < polys.Count; k++)
                        {
                            edgeLoop.Add(polys[k]);
                        }

                        m_ProfileLoop.Add(edgeLoop);
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
        public XY Dir
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
            m_dir = new XY(edge.Dir.X, edge.Dir.Y);
        }
    }

    /// <summary>
    /// Scatter srcLine for Build loop
    /// </summary>
    class ScatterLines
    {
        private List<LineSegment2d> m_lines;
        private List<List<Point2d>> m_pointsLst = new List<List<Point2d>>();
        private List<LineSegment2d> m_geneLines = new List<LineSegment2d>();

        public List<LineSegment2d> Lines
        {
            get { return m_geneLines; }
        }

        public static List<LineSegment2d> MakeNewLines(List<LineSegment2d> srcLines)
        {
            var scatterLines = new ScatterLines(srcLines);
            return scatterLines.Lines;
        }


        private ScatterLines(List<LineSegment2d> lines)
        {
            m_lines = lines;

            foreach (var line in m_lines)
            {
                var linePoints = new List<Point2d>();
                linePoints.Add(line.StartPoint);
                linePoints.Add(line.EndPoint);
                m_pointsLst.Add(linePoints);
            }

            IntersectLines();
            SortXYZPoints();
            NewLines();
        }

        /// <summary>
        /// 求交点
        /// </summary>
        private void IntersectLines()
        {
            for (int i = 0; i < m_lines.Count; i++)
            {
                var curLine = m_lines[i];
                for (int j = i + 1; j < m_lines.Count; j++)
                {
                    var nextLine = m_lines[j];

                    var intersectPts = curLine.IntersectWith(nextLine);
                    if (intersectPts != null && intersectPts.Count() == 1)
                    {
                        m_pointsLst[i].AddRange(intersectPts);
                        m_pointsLst[j].AddRange(intersectPts);
                    }
                }
            }
        }

        /// <summary>
        /// 排序
        /// </summary>
        private void SortXYZPoints()
        {
            foreach (var points in m_pointsLst)
            {
                var firstPoint = points.First();
                points.Sort((s1, s2) => { return s1.GetDistanceTo(firstPoint).CompareTo(s2.GetDistanceTo(firstPoint)); });
            }
        }

        /// <summary>
        /// 生成新的直线
        /// </summary>
        private void NewLines()
        {
            foreach (var points in m_pointsLst)
            {
                for (int i = 0; i < points.Count; i++)
                {
                    var curPoint = points[i];
                    var curPointXY = new Point2d(curPoint.X, curPoint.Y);
                    if (i + 1 == points.Count)
                        break;

                    var nextPoint = points[i + 1];
                    var nextPointXY = new Point2d(nextPoint.X, nextPoint.Y);
                    try
                    {
                        if ((curPointXY - nextPointXY).Length > 1)
                        {
                            var newLine = new LineSegment2d(curPointXY, nextPointXY);
                            m_geneLines.Add(newLine);
                        }
                    }
                    catch (Exception ex)
                    {

                    }

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
        public BoundBoxPlane(List<LineSegment2d> lines)
        {
            CalcuBoundBox(lines);
        }

        public void CalcuBoundBox(List<LineSegment2d> lines)
        {
            if (lines.Count == 0)
                return;

            var ptLst = new List<XY>();
            foreach (var line in lines)
            {
                var pxHead = line.StartPoint;
                var pxEnd = line.EndPoint;
                var pt1 = new XY(pxHead.X, pxHead.Y);
                var pt2 = new XY(pxEnd.X, pxEnd.Y);
                ptLst.Add(pt1);
                ptLst.Add(pt2);
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
            var offsetX = 0 - m_leftBottom.X;
            var offsetY = 0 - m_leftBottom.Y;
            if (offsetX < 0 || offsetY < 0)
            {
                TransValue = new Vector2d(offsetX, offsetY);
                return true;
            }

            return false;
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
    class XY
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
