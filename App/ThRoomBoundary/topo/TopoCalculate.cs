using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThRoomBoundary.topo
{
    /// <summary>
    /// topo边的数据信息
    /// </summary>
    public class TopoEdge
    {
        private TopoEdge m_pair;
        private Point2d m_start = new Point2d();
        private Point2d m_end = new Point2d();
        private XY m_dir = null;
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
            m_dir = CommonUtils.Vector2XY(srcLine.Direction);
        }

        public TopoEdge(Point2d start, Point2d end, XY dir)
        {
            m_start = start;
            m_end = end;
            m_dir = dir;
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

        public XY Dir
        {
            get { return m_dir; }
            set { m_dir = value; }
        }

        public static void MakeTopoEdge(LineSegment2d srcLine, List<TopoEdge> topoEdges)
        {
            var srcLineS = srcLine.StartPoint;
            var srcLineE = srcLine.EndPoint;
            TopoEdge curEdge = new TopoEdge(srcLineS, srcLineE, CommonUtils.Vector2XY(srcLine.Direction));
            TopoEdge pairEdge = new TopoEdge(srcLineE, srcLineS, CommonUtils.Vector2XY(srcLine.Direction.Negate()));
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

        /// <summary>
        /// 内部使用 获取点的最小包围区域
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static List<RoomDataInner> MakeMinProfileLoopsInner(List<LineSegment2d> lines, Point2d point)
        {
            var search = new TopoSearch(lines, point);
            var profileLoops = search.ContainsPtLoop();
            var tmpEdgeLoops = search.TransFormProfileLoops(profileLoops);
            return search.ConvertTopoEdges2Curve(tmpEdgeLoops);
        }

        /// <summary>
        /// 获取轮廓
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        public static List<RoomDataInner> MakeSrcProfileLoops(List<LineSegment2d> lines, List<LineSegment2d> rectLines = null)
        {
            var search = new TopoSearch(lines);

            var loops = TopoSearch.RemoveDuplicate(search.m_srcLoops);
            var tmpEdgeLoops = search.TransFormProfileLoops(loops);
            if (rectLines == null)
                return search.ConvertTopoEdges2Curve(tmpEdgeLoops);
            else
            {
                var validEdgeLoops = MakeTopoEdgeProfilesFromRect(tmpEdgeLoops, rectLines);
                return search.ConvertTopoEdges2Curve(validEdgeLoops);
            }
        }

        /// <summary>
        /// 删选轮廓数据
        /// </summary>
        /// <param name="profiles"></param>
        /// <param name="rectLines"></param>
        /// <returns></returns>
        public static List<List<TopoEdge>> MakeTopoEdgeProfilesFromRect(List<List<TopoEdge>> profiles, List<LineSegment2d> rectLines)
        {
            if (profiles == null || profiles.Count == 0)
                return null;

            var topoEdgeProfiles = new List<List<TopoEdge>>();
            foreach (var profile in profiles)
            {
                if (CommonUtils.OutLoopContainsInnerLoop(rectLines, profile))
                    topoEdgeProfiles.Add(profile);
            }

            return topoEdgeProfiles;
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
                    var transLine = CommonUtils.LineAddVector(line, trans);
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
                    var transLine = CommonUtils.LineAddVector(line, trans);
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
        private List<RoomDataInner> ConvertTopoEdges2Curve(List<List<TopoEdge>> topoLoops)
        {
            if (topoLoops == null || topoLoops.Count == 0)
                return null;

            var curveLoops = new List<RoomDataInner>();
            foreach (var loop in topoLoops)
            {
                var profile = new List<LineSegment2d>();
                foreach (var edge in loop)
                {
                    var line = new LineSegment2d(edge.Start, edge.End);
                    profile.Add(line);
                }

                var pos = CommonUtils.CalculateLineSegment2dPos(profile);
                var area = CommonUtils.CalcuLoopArea(profile);
                curveLoops.Add(new RoomDataInner(profile, pos, area));
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
                    var transEdge = CommonUtils.LineDecVector(edge, transValue);
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
            var tmpTopoEdges = RemoveDuplicate(edgesLoop);

            // inner loops combine
            var innerLoopProfile = new InnerLoopProfile(tmpTopoEdges);
            innerLoopProfile.Do();
            return innerLoopProfile.OutLoops;
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

        private List<List<TopoEdge>> m_nearLoops = new List<List<TopoEdge>>();
        private List<Profile> m_profiles = new List<Profile>();
        public InnerLoopProfile(List<List<TopoEdge>> loops)
        {
            m_srcLoops = loops;
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
                var curves = TopoCalculate.MakeNoScatterProfile(edges);
                curves.Sort((s1, s2) => { return Math.Abs(CommonUtils.CalcuLoopArea(s1)).CompareTo(Math.Abs(CommonUtils.CalcuLoopArea(s2))); });

                m_outLoops.Add(curves.Last());
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
        /// 重叠
        /// </summary>
        /// <param name="lineFir"></param>
        /// <param name="lineSec"></param>
        /// <returns></returns>
        private bool IsOverlap(TopoEdge lineFir, TopoEdge lineSec)
        {
            var ptFirS = lineFir.Start;
            var ptFirE = lineFir.End;
            var ptSecS = lineSec.Start;
            var ptSecE = lineSec.End;
            if (CommonUtils.IsPointOnSegment(ptFirS, lineSec) || CommonUtils.IsPointOnSegment(ptFirE, lineSec)
               || CommonUtils.IsPointOnSegment(ptSecS, lineFir) || CommonUtils.IsPointOnSegment(ptSecE, lineFir))
                return true;
            return false;
        }

        // 环与环有公共区域
        private bool LoopIntersectLoop(List<TopoEdge> loopFir, List<TopoEdge> loopSec)
        {
            foreach (var curveFir in loopFir)
            {
                LineSegment2d lineFir = new LineSegment2d(curveFir.Start, curveFir.End);
                foreach (var curveSec in loopSec)
                {
                    LineSegment2d lineSec = new LineSegment2d(curveSec.Start, curveSec.End);
                    var intersectPts = lineFir.IntersectWith(lineSec);
                    if (intersectPts != null && intersectPts.Count() == 1)
                    {
                        // intersect
                        return true;
                    }
                    else
                    {
                        // not intersect
                        if (IsOverlap(curveFir, curveSec))
                            return true;
                    }
                }
            }

            return false;
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
            List<LineSegment2d> removeLines = null;
            CommonUtils.RemoveCollinearLines(lines, out removeLines);
            if (removeLines == null)
                return null;
            var topoCal = new TopoCalculate(removeLines);
            return topoCal.ProfileLoops;
        }

        private TopoCalculate(List<LineSegment2d> SrcLines)
        {
            m_lines = SrcLines;
            var lines = ScatterLines.MakeNewLines(m_lines);
            Calculate(lines);
        }

        public static List<List<TopoEdge>> MakeNoScatterProfile(List<TopoEdge> edges)
        {
            var topoCal = new TopoCalculate(edges);
            return topoCal.ProfileLoops;
        }

        private TopoCalculate(List<TopoEdge> edges)
        {
            var lines = new List<LineSegment2d>();
            foreach (var edge in edges)
            {
                lines.Add(new LineSegment2d(edge.Start, edge.End));
            }

            m_lines = lines;
            Calculate(m_lines);
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

                if (curEdge.IsUse || (CommonUtils.Point2dIsEqualPoint2d(headPoint, curPtTail, 1e-1) && CommonUtils.Point2dIsEqualPoint2d(tailPoint, curPtHead, 1e-1)))
                    continue;

                if (CommonUtils.Point2dIsEqualPoint2d(tailPoint, curEdge.Start, 1e-1))
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

                    var intersectPts = curLine.IntersectWith(nextLine, new Tolerance(1e-3, 1e-3));
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
                        if ((curPointXY - nextPointXY).Length > 1e-5)
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

    class CoEdge
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

        private LineSegment2d m_coLine = null;
        private List<CoEdge> m_relevantLines = null; // 相关联的线段
        public List<CoEdge> RelevantLines
        {
            get { return m_relevantLines; }
        }

        public LineSegment2d CoLine
        {
            get { return m_coLine; }
            set { m_coLine = value; }
        }

        public CoEdge(LineSegment2d line)
        {
            m_coLine = line;
            m_relevantLines = new List<CoEdge>();
        }
    }

    /// <summary>
    /// 共边处理
    /// </summary>
    class CoEdgeErase
    {
        private List<CoEdge> m_coEdges = null;
        private CoEdgeErase(List<LineSegment2d> lines)
        {
            m_coEdges = new List<CoEdge>();
            for (int i = 0; i < lines.Count; i++)
            {
                m_coEdges.Add(new CoEdge(lines[i]));
            }
        }

        // 接口调用
        public static List<LineSegment2d> MakeCoEdgeErase(List<LineSegment2d> lines)
        {
            var edgeErase = new CoEdgeErase(lines);
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

                var curLine = curEdge.CoLine;
                var curLinePtS = curLine.StartPoint;
                var curLinePtE = curLine.EndPoint;
                for (int j = i + 1; j < m_coEdges.Count; j++)
                {
                    var nextEdge = m_coEdges[j];
                    if (curEdge.IsErase || nextEdge.IsErase)
                        continue;

                    var nextLine = nextEdge.CoLine;
                    var nextLinePtS = nextLine.StartPoint;
                    var nextLinePtE = nextLine.EndPoint;
                    if (CommonUtils.IsAlmostNearZero(CommonUtils.CalAngle(CommonUtils.Vector2XY(curLine.Direction), CommonUtils.Vector2XY(nextLine.Direction)), 1e-6)
                       || CommonUtils.IsAlmostNearZero(CommonUtils.CalAngle(CommonUtils.Vector2XY(curLine.Direction), CommonUtils.Vector2XY(nextLine.Direction.Negate())), 1e-6))
                    {
                        // 重合线
                        if ((CommonUtils.Point2dIsEqualPoint2d(curLinePtS, nextLinePtS) && CommonUtils.Point2dIsEqualPoint2d(curLinePtE, nextLinePtE))
                        || (CommonUtils.Point2dIsEqualPoint2d(curLinePtS, nextLinePtE) && CommonUtils.Point2dIsEqualPoint2d(curLinePtE, nextLinePtS)))
                        {
                            nextEdge.IsErase = true;
                        }
                        else if (CommonUtils.IsPointOnSegment(nextLinePtS, curLine, 1e-9) && CommonUtils.IsPointOnSegment(nextLinePtE, curLine, 1e-9)) // 完全包含线nextLine
                        {
                            nextEdge.IsErase = true;
                        }
                        else if (CommonUtils.IsPointOnSegment(curLinePtS, nextLine, 1e-9) && CommonUtils.IsPointOnSegment(curLinePtE, nextLine, 1e-9)) // 完全包含线curLine
                        {
                            curEdge.IsErase = true;
                        }
                        else if (CommonUtils.IsPointOnSegment(nextLinePtS, curLine, 1e-6) || (CommonUtils.IsPointOnSegment(nextLinePtE, curLine, 1e-6))) // 部分包含线
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

                    var curLine = curEdge.CoLine; // 可能会不断修改curEdge中line的值
                    var releEdge = curReleEdges[j];
                    if (releEdge.IsErase)
                        continue;

                    var curLinePtS = curLine.StartPoint;
                    var curLinePtE = curLine.EndPoint;
                    var releLine = releEdge.CoLine;
                    var releLinePtS = releLine.StartPoint;
                    var releLinePtE = releLine.EndPoint;

                    // 当前段被相关重合段裁剪
                    if (CommonUtils.IsPointOnSegment(releLinePtS, curLine) && !CommonUtils.IsPointOnSegment(releLinePtE, curLine)) // 部分包含当前线条
                    {
                        if (CommonUtils.IsPointOnSegment(curLinePtS, releLine))
                        {
                            curEdge.CoLine = new LineSegment2d(releLinePtS, curLinePtE); // 裁剪当前线段
                        }
                        else if (CommonUtils.IsPointOnSegment(curLinePtE, releLine))
                        {
                            curEdge.CoLine = new LineSegment2d(releLinePtS, curLinePtS);
                        }

                        curEdge.ChangeCount++;
                        continue;
                    }
                    else if (CommonUtils.IsPointOnSegment(releLinePtE, curLine) && !CommonUtils.IsPointOnSegment(releLinePtS, curLine)) // 部分包含当前线条
                    {
                        if (CommonUtils.IsPointOnSegment(curLinePtS, releLine))
                        {
                            curEdge.CoLine = new LineSegment2d(releLinePtE, curLinePtE);
                        }
                        else if (CommonUtils.IsPointOnSegment(curLinePtE, releLine))
                        {
                            curEdge.CoLine = new LineSegment2d(releLinePtE, curLinePtS);
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
        private List<LineSegment2d> Traverse2Lines()
        {
            var lines = new List<LineSegment2d>();
            for (int i = 0; i < m_coEdges.Count; i++)
            {
                if (m_coEdges[i].IsErase)
                    continue;

                lines.Add(m_coEdges[i].CoLine);
            }

            return lines;
        }
    }
}
