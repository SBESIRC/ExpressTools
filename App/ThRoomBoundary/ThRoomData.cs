using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Linq2Acad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThRoomBoundary.topo;

namespace ThRoomBoundary
{
    /// <summary>
    /// 门和相关墙数据的映射
    /// </summary>
    class DoorRelatedData
    {
        public List<LineSegment2d> DoorBound
        {
            get;
            set;
        }

        public List<LineSegment2d> WallLines
        {
            get;
            set;
        }

        public DoorRelatedData(List<LineSegment2d> doorBound, List<LineSegment2d> wallLines = null)
        {
            DoorBound = doorBound;
            WallLines = wallLines;
        }
    }

    class CollinearData
    {
        public LineSegment2d Line
        {
            get;
            set;
        }

        public bool Valid
        {
            get;
            set;
        }

        public List<LineSegment2d> RelatedCollinearLines
        {
            get;
            set;
        }

        public CollinearData(LineSegment2d line)
        {
            Line = line;
            Valid = true;
            RelatedCollinearLines = new List<LineSegment2d>();
        }

    }

    class PathNearSearch
    {
        class PathNode
        {
            public PathNode(LineSegment2d line)
            {
                m_curLine = line;
            }

            public bool m_bUse = false;
            public LineSegment2d m_curLine = null;
            public List<PathNode> m_relatedConnectNodes = new List<PathNode>();
        }

        private List<LineSegment2d> m_lines;
        private List<LineSegment2d> m_outLines = new List<LineSegment2d>();
        public List<LineSegment2d> OutLines
        {
            get { return m_outLines; }
        }

        private List<PathNode> m_ConnectedNodes = new List<PathNode>();
        public PathNearSearch(List<LineSegment2d> lines)
        {
            m_lines = lines;
        }

        public static List<LineSegment2d> MakeMergeLines(List<LineSegment2d> lines)
        {
            var pathSearch = new PathNearSearch(lines);
            pathSearch.Do();
            return pathSearch.OutLines;
        }

        public void Do()
        {
            for (int i = 0; i < m_lines.Count; i++)
            {
                m_ConnectedNodes.Add(new PathNode(m_lines[i]));
            }

            for (int i = 0; i < m_ConnectedNodes.Count; i++)
            {
                var curProfile = m_ConnectedNodes[i];
                for (int j = i + 1; j < m_ConnectedNodes.Count; j++)
                {
                    var nextProfile = m_ConnectedNodes[j];
                    if (IsLineNear(curProfile.m_curLine, nextProfile.m_curLine))
                    {
                        curProfile.m_relatedConnectNodes.Add(nextProfile);
                        nextProfile.m_relatedConnectNodes.Add(curProfile);
                    }
                }
            }

            // 相邻区域收集
            for (int j = 0; j < m_ConnectedNodes.Count; j++)
            {
                if (m_ConnectedNodes[j].m_bUse)
                    continue;

                if (m_ConnectedNodes[j].m_relatedConnectNodes.Count == 0)
                {
                    m_ConnectedNodes[j].m_bUse = true;
                    m_outLines.Add(m_ConnectedNodes[j].m_curLine);
                }
                else
                {
                    var nearLines = SearchFromOneLine(m_ConnectedNodes[j]);
                    var mergeLine = MergeLines(nearLines);
                    if (mergeLine != null)
                        m_outLines.Add(mergeLine);
                }
            }
        }

        private LineSegment2d MergeLines(List<LineSegment2d> lines)
        {
            if (lines == null || lines.Count == 0)
                return null;
            var ptLst = new List<Point2d>();
            foreach (var line in lines)
            {
                ptLst.Add(line.StartPoint);
                ptLst.Add(line.EndPoint);
            }

            //跟Y轴平行
            if (lines[0].Direction.IsParallelTo(new Vector2d(0, 1)))
            {
                ptLst.Sort((p1, p2) => { return p1.Y.CompareTo(p2.Y); });
            }
            else
            {
                ptLst.Sort((p1, p2) => { return p1.X.CompareTo(p2.X); });
            }

            Point2d ptS = ptLst.First();
            Point2d ptE = ptLst.Last();
            var outLine = new LineSegment2d(ptS, ptE);
            return outLine;
        }

        // 从一块区域开始搜索
        private List<LineSegment2d> SearchFromOneLine(PathNode SearchLine)
        {
            var line2ds = new List<LineSegment2d>();
            var InOutNodes = new List<PathNode>();
            InOutNodes.Add(SearchLine);
            while (InOutNodes.Count != 0)
            {
                var curProfile = InOutNodes.First();
                line2ds.Add(curProfile.m_curLine);
                curProfile.m_bUse = true;
                InOutNodes.RemoveAt(0);
                var childConnectNodes = curProfile.m_relatedConnectNodes;
                foreach (var connectedNode in childConnectNodes)
                {
                    if (!connectedNode.m_bUse)
                        InOutNodes.Add(connectedNode);
                }
            }

            return line2ds;
        }

        // 线与线之间是连接的
        private bool IsLineNear(LineSegment2d lineFir, LineSegment2d lineSec)
        {
            var ptFirHead = lineFir.StartPoint;
            var ptFirTail = lineFir.EndPoint;
            var ptSecHead = lineSec.StartPoint;
            var ptSecTail = lineSec.EndPoint;

            if (CommonUtils.IsAlmostNearZero(ptFirHead.GetDistanceTo(ptSecHead), 1e-2) || CommonUtils.IsAlmostNearZero(ptFirHead.GetDistanceTo(ptSecTail), 1e-2)
                || CommonUtils.IsAlmostNearZero(ptFirTail.GetDistanceTo(ptSecHead), 1e-2) || CommonUtils.IsAlmostNearZero(ptFirTail.GetDistanceTo(ptSecTail), 1e-2))
            {
                return true;
            }

            return false;
        }
    }
    /// <summary>
    ///  合并相连的直线
    /// </summary>
    class CombineLine
    {
        public List<LineSegment2d> CombineLines
        {
            get;
            set;
        }

        public List<LineSegment2d> OutLines
        {
            get;
            set;
        }

        public CombineLine(List<LineSegment2d> srcLines)
        {
            CombineLines = srcLines;
            OutLines = new List<LineSegment2d>();
        }

        public static List<LineSegment2d> MakeCombineLines(List<LineSegment2d> srcLines)
        {
            var combine = new CombineLine(srcLines);
            combine.Do();
            return combine.OutLines;
        }

        private bool IsCollinearLines(LineSegment2d lineFir, LineSegment2d lineSec)
        {
            var angle1 = lineFir.Direction.GetAngleTo(lineSec.Direction);
            var angle2 = lineFir.Direction.GetAngleTo(lineSec.Direction.Negate());
            if (CommonUtils.IsAlmostNearZero(Math.Abs(angle1), 1e-5) || CommonUtils.IsAlmostNearZero(Math.Abs(angle2), 1e-5))
            {
                var ptFirS = lineFir.StartPoint;
                var ptSecS = lineSec.StartPoint;
                var vector = ptFirS - ptSecS;
                var angle3 = lineFir.Direction.GetAngleTo(vector);
                var angle4 = lineFir.Direction.GetAngleTo(vector.Negate());
                if (CommonUtils.IsAlmostNearZero(Math.Abs(angle3), 1e-5) || CommonUtils.IsAlmostNearZero(Math.Abs(angle4), 1e-5))
                {
                    return true;
                }

            }

            return false;
        }

        public void Do()
        {
            var relatedDatas = new List<CollinearData>();
            foreach (var line in CombineLines)
            {
                relatedDatas.Add(new CollinearData(line));
            }

            for (int i = 0; i < relatedDatas.Count; i++)
            {
                if (!relatedDatas[i].Valid)
                    continue;

                for (int j = i + 1; j < relatedDatas.Count; j++)
                {
                    if (!relatedDatas[j].Valid)
                        continue;

                    //共线
                    if (IsCollinearLines(relatedDatas[i].Line, relatedDatas[j].Line))
                    {
                        relatedDatas[i].RelatedCollinearLines.Add(((LineSegment2d)relatedDatas[j].Line.Clone()));
                        relatedDatas[j].Valid = false;
                    }
                }
            }

            // 有共线则生成共线数据
            foreach (var data in relatedDatas)
            {
                if (data.Valid)
                {
                    if (data.RelatedCollinearLines.Count == 0)
                    {
                        OutLines.Add(data.Line);
                    }
                    else
                    {
                        var lines = CombineData(data);
                        if (lines.Count != 0)
                            OutLines.AddRange(lines);
                    }
                }
            }
        }

        /// <summary>
        /// 合并相邻的数据
        /// </summary>
        /// <param name="CollinearData"></param>
        /// <returns></returns>
        private List<LineSegment2d> CombineData(CollinearData data)
        {
            var lines = RemoveOverlaySection(data);
            var outLines = PathNearSearch.MakeMergeLines(lines);
            return outLines;
        }

        //删除重复边
        private List<LineSegment2d> RemoveOverlaySection(CollinearData data)
        {
            var srcLines = new List<LineSegment2d>();
            srcLines.Add(data.Line);
            srcLines.AddRange(data.RelatedCollinearLines);
            var lineNodes = new List<LineNode>();
            srcLines.ForEach(l => lineNodes.Add(new LineNode(l)));
            List<LineNode> outLineNodes = null;
            CommonUtils.RemoveCollinearLines(lineNodes, out outLineNodes);
            var aimLines = new List<LineSegment2d>();
            outLineNodes.ForEach(p => aimLines.Add(p.CurLine));
            return aimLines;
        }
    }

    public class LineNode
    {
        public LineSegment2d CurLine
        {
            get;
            set;
        }

        public string LayerName
        {
            get;
            set;
        }

        public LineNode(LineSegment2d line2d, string layerName = null)
        {
            CurLine = line2d;
            LayerName = layerName;
        }
    }

    class ConnectedNode
    {
        public ConnectedNode(List<LineSegment2d> relatedCurvesNode)
        {
            m_curNode = relatedCurvesNode;
        }

        public bool m_bUse = false;
        public List<LineSegment2d> m_curNode = null;
        public List<ConnectedNode> m_relatedConnectNodes = new List<ConnectedNode>();
    }

    /// <summary>
    /// 连通区域的外包框
    /// </summary>
    class ConnectedBoxBound
    {
        private List<List<LineSegment2d>> m_curvesLst;
        private List<List<LineSegment2d>> m_outBounds = new List<List<LineSegment2d>>();
        public List<List<LineSegment2d>> OutBounds
        {
            get { return m_outBounds; }
        }

        private List<ConnectedNode> m_ConnectedNodes = new List<ConnectedNode>();
        public ConnectedBoxBound(List<List<LineSegment2d>> curvesLst)
        {
            m_curvesLst = curvesLst;
        }

        /// <summary>
        /// 生成相邻区域外包轮廓
        /// </summary>
        /// <param name="curvesLst"></param>
        /// <returns></returns>
        public static List<List<LineSegment2d>> MakeRelatedBoundary(List<List<LineSegment2d>> curvesLst)
        {
            var boxBound = new ConnectedBoxBound(curvesLst);
            var relatedCurves = boxBound.Do();
            boxBound.CalculateBounds(relatedCurves);
            return boxBound.OutBounds;
        }

        public static List<List<LineSegment2d>> MakeRelatedCurves(List<List<LineSegment2d>> curvesLst)
        {
            var boxBound = new ConnectedBoxBound(curvesLst);
            return boxBound.Do();
        }

        public List<List<LineSegment2d>> Do()
        {
            for (int i = 0; i < m_curvesLst.Count; i++)
            {
                m_ConnectedNodes.Add(new ConnectedNode(m_curvesLst[i]));
            }

            for (int i = 0; i < m_ConnectedNodes.Count; i++)
            {
                var curProfile = m_ConnectedNodes[i];
                for (int j = i + 1; j < m_ConnectedNodes.Count; j++)
                {
                    var nextProfile = m_ConnectedNodes[j];
                    if (IsAreaConnectArea(curProfile.m_curNode, nextProfile.m_curNode))
                    {
                        curProfile.m_relatedConnectNodes.Add(nextProfile);
                        nextProfile.m_relatedConnectNodes.Add(curProfile);
                    }
                }
            }

            var connectedAreaLst = new List<List<LineSegment2d>>();
            // 相邻区域收集
            for (int j = 0; j < m_ConnectedNodes.Count; j++)
            {
                if (m_ConnectedNodes[j].m_bUse)
                    continue;

                if (m_ConnectedNodes[j].m_relatedConnectNodes.Count == 0)
                {
                    m_ConnectedNodes[j].m_bUse = true;
                    connectedAreaLst.Add(m_ConnectedNodes[j].m_curNode);
                }
                else
                {
                    var loops = SearchFromOneArea(m_ConnectedNodes[j]);
                    connectedAreaLst.Add(loops);
                }
            }

            return connectedAreaLst;
        }

        /// <summary>
        /// 计算相邻区域的外包框
        /// </summary>
        /// <param name="connectedAreaLst"></param>
        public void CalculateBounds(List<List<LineSegment2d>> connectedAreaLst)
        {
            foreach (var connectCurves in connectedAreaLst)
            {
                // 经过外扩的
                var boundLines = ThRoomUtils.GetBoundFromLine2ds(connectCurves);
                if (boundLines != null && boundLines.Count != 0)
                {
                    m_outBounds.Add(boundLines);
                }
            }
        }

        // 从一块区域开始搜索
        private List<LineSegment2d> SearchFromOneArea(ConnectedNode Searchprofile)
        {
            var line2ds = new List<LineSegment2d>();
            var InOutNodes = new List<ConnectedNode>();
            InOutNodes.Add(Searchprofile);
            while (InOutNodes.Count != 0)
            {
                var curProfile = InOutNodes.First();
                line2ds.AddRange(curProfile.m_curNode);
                curProfile.m_bUse = true;
                InOutNodes.RemoveAt(0);
                var childConnectNodes = curProfile.m_relatedConnectNodes;
                foreach (var connectedNode in childConnectNodes)
                {
                    if (!connectedNode.m_bUse)
                        InOutNodes.Add(connectedNode);
                }
            }

            return line2ds;
        }

        /// <summary>
        /// 重叠
        /// </summary>
        /// <param name="lineFir"></param>
        /// <param name="lineSec"></param>
        /// <returns></returns>
        private bool IsOverlap(LineSegment2d lineFir, LineSegment2d lineSec)
        {
            var ptFirS = lineFir.StartPoint;
            var ptFirE = lineFir.EndPoint;
            var ptSecS = lineSec.StartPoint;
            var ptSecE = lineSec.EndPoint;
            if (CommonUtils.IsPointOnSegment(ptFirS, lineSec) || CommonUtils.IsPointOnSegment(ptFirE, lineSec)
               || CommonUtils.IsPointOnSegment(ptSecS, lineFir) || CommonUtils.IsPointOnSegment(ptSecE, lineFir))
                return true;
            return false;
        }

        // 区域之间有连通的
        private bool IsAreaConnectArea(List<LineSegment2d> curvesFir, List<LineSegment2d> curvesSec)
        {
            foreach (var curveFir in curvesFir)
            {
                foreach (var curveSec in curvesSec)
                {
                    var intersectPts = curveFir.IntersectWith(curveSec);
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

    class ThRoomUtils
    {
        /// <summary>
        /// 门和墙的曲线连接
        /// </summary>
        /// <param name="doorBounds"></param>
        /// <param name="wallCurves"></param>
        public static List<Curve> InsertDoorRelatedCurveDatas(List<List<LineSegment2d>> doorBounds, List<Curve> wallCurves, string layer)
        {
            if ((doorBounds == null || doorBounds.Count == 0) || (wallCurves == null || wallCurves.Count == 0))
                return null;

            var curves = new List<Curve>();
            foreach (var doorBound in doorBounds)
            {
                // 相关数据内部或者相交
                var relatedCurves = BoundRelatedBoundCurves(doorBound, wallCurves);
                if (relatedCurves == null || relatedCurves.Count == 0)
                    continue;

                // 根据相关数据进行连接处理
                // 收尾相连且共线的线段进行合并直线处理
                var combineCurves = CombineCollinearLines(relatedCurves);
                var insertCurves = InsertConnectCurves(combineCurves, layer);
                if (insertCurves != null && insertCurves.Count != 0)
                {
                    curves.AddRange(insertCurves);
                }
            }

            return curves;
        }

        /// <summary>
        /// 合并直线处理
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        public static List<LineSegment2d> CombineCollinearLines(List<LineSegment2d> lines)
        {
            if (lines == null || lines.Count == 0)
                return null;

            var combines = CombineLine.MakeCombineLines(lines);
            return combines;
        }

        /// <summary>
        /// 是否有平行共线的数据，如果没有，则进行相交处理
        /// </summary>
        /// <param name="relatedDatas"></param>
        /// <returns> 没有平行共线的处理则返回true</returns>
        public static bool IsNotHaveCollinearDatas(List<CollinearData> relatedDatas)
        {
            foreach (var relatedData in relatedDatas)
            {
                // 只要有false则说明有共线的处理
                if (!relatedData.Valid)
                    return false;
            }

            return true;
        }

        public static LineSegment2d ExtendLine(LineSegment2d line)
        {
            var ptS = line.StartPoint;
            var ptE = line.EndPoint;
            ptS = ptS - line.Direction * 1000000;
            ptE = ptE + line.Direction * 1000000;
            return new LineSegment2d(ptS, ptE);
        }

        /// <summary>
        /// 一块区域和另一块区域的连接处理
        /// </summary>
        /// <param name="line2dsFir"></param>
        /// <param name="line2dsSec"></param>
        /// <returns></returns>
        public static List<Curve> ConnectLinesWithsLines(List<LineSegment2d> line2dsFir, List<LineSegment2d> line2dsSec, string layerName = null)
        {
            //DrawCurves(line2dsFir);
            //DrawCurves(line2dsSec);
            var curves = new List<Curve>();
            foreach (var firLine in line2dsFir)
            {
                foreach (var secLine in line2dsSec)
                {
                    if (IsParallel(firLine, secLine))
                        continue;

                    // 非平行
                    var firLineExtend = ExtendLine(firLine);
                    var secLineExtend = ExtendLine(secLine);
                    var intersectPoints = firLineExtend.IntersectWith(secLineExtend);

                    if (intersectPoints != null && intersectPoints.Count() == 1)
                    {
                        Point2d pt = intersectPoints.First();
                        var ptFirHead = firLine.StartPoint;
                        var ptFirEnd = firLine.EndPoint;
                        var ptSecHead = secLine.StartPoint;
                        var ptSecEnd = secLine.EndPoint;
                        Line line = null;
                        if (CommonUtils.IsPointOnSegment(pt, firLine, 1e-1))
                        {
                            if (pt.GetDistanceTo(ptSecHead) < pt.GetDistanceTo(ptSecEnd))
                                line = new Line(new Point3d(pt.X, pt.Y, 0), new Point3d(ptSecHead.X, ptSecHead.Y, 0));
                            else
                                line = new Line(new Point3d(pt.X, pt.Y, 0), new Point3d(ptSecEnd.X, ptSecEnd.Y, 0));
                        }
                        else if (CommonUtils.IsPointOnSegment(pt, secLine, 1e-1))
                        {
                            if (pt.GetDistanceTo(ptFirHead) < pt.GetDistanceTo(ptFirEnd))
                                line = new Line(new Point3d(pt.X, pt.Y, 0), new Point3d(ptFirHead.X, ptFirHead.Y, 0));
                            else
                                line = new Line(new Point3d(pt.X, pt.Y, 0), new Point3d(ptFirEnd.X, ptFirEnd.Y, 0));
                        }

                        if (line != null)
                        {
                            if (layerName != null)
                                line.Layer = layerName;

                            // 剔除重复的增加线
                            if (!IsInValidLine(curves, line))
                                curves.Add(line);
                        }
                    }

                }
            }

            return curves;
        }

        public static bool IsInValidLine(List<Curve> curves, Line line)
        {
            if (curves.Count == 0)
                return false;

            foreach (var curve in curves)
            {
                if (curve is Line)
                {
                    var addLine = curve as Line;
                    if (CommonUtils.IsAlmostNearZero(addLine.Length - line.Length, 1))
                    {
                        var ptAddS = addLine.StartPoint;
                        var ptAddE = addLine.EndPoint;
                        var ptS = line.StartPoint;
                        var ptE = line.EndPoint;
                        if ((CommonUtils.Point3dIsEqualPoint3d(ptAddS, ptS, 0.1) && CommonUtils.Point3dIsEqualPoint3d(ptAddE, ptE, 0.1))
                            || (CommonUtils.Point3dIsEqualPoint3d(ptAddS, ptE, 0.1) && CommonUtils.Point3dIsEqualPoint3d(ptAddE, ptS, 0.1)))
                            return true;
                    }
                }
            }

            return false;
        }

        //共线数据生成
        public static List<Curve> InsertConnectCurves(List<LineSegment2d> line2ds, string layer = null)
        {
            var relatedDatas = new List<CollinearData>();
            foreach (var line in line2ds)
            {
                relatedDatas.Add(new CollinearData(line));
            }

            for (int i = 0; i < relatedDatas.Count; i++)
            {
                if (!relatedDatas[i].Valid)
                    continue;

                for (int j = i + 1; j < relatedDatas.Count; j++)
                {
                    if (!relatedDatas[j].Valid)
                        continue;

                    //共线
                    if (IsCollinearLines(relatedDatas[i].Line, relatedDatas[j].Line))
                    {
                        relatedDatas[i].RelatedCollinearLines.Add(((LineSegment2d)relatedDatas[j].Line.Clone()));
                        relatedDatas[j].Valid = false;
                    }
                }
            }

            // 如果没有平行线，则做相交处理
            if (IsNotHaveCollinearDatas(relatedDatas))
            {
                var innerLinesLst = new List<List<LineSegment2d>>();
                relatedDatas.ForEach(p =>
                {
                    innerLinesLst.Add(new List<LineSegment2d>() { p.Line });
                });

                var relatedCurvesLst = ConnectedBoxBound.MakeRelatedCurves(innerLinesLst);
                if (relatedCurvesLst.Count == 2)
                {
                    return ConnectLinesWithsLines(relatedCurvesLst.First(), relatedCurvesLst.Last(), layer);
                }
                else
                    return null;
            }
            else
            {
                // 有共线则生成共线数据
                var curves = new List<Curve>();
                foreach (var data in relatedDatas)
                {
                    if (data.Valid)
                    {
                        var line = MakeLineFromCollinears(data);
                        if (line != null)
                        {
                            if (layer != null)
                                line.Layer = layer;
                            curves.Add(line);
                        }
                    }
                }

                return curves;
            }
        }

        /// <summary>
        /// 由共线数据集生成新的直线
        /// </summary>
        /// <param name="collinearLines"></param>
        /// <returns></returns>
        public static Line MakeLineFromCollinears(CollinearData collinearData)
        {
            if (collinearData.RelatedCollinearLines == null || collinearData.RelatedCollinearLines.Count == 0)
                return null;

            var ptLst = new List<Point2d>();
            foreach (var line2d in collinearData.RelatedCollinearLines)
            {
                ptLst.Add(line2d.StartPoint);
                ptLst.Add(line2d.EndPoint);
            }

            var curLine = collinearData.Line;
            ptLst.Add(curLine.StartPoint);
            ptLst.Add(curLine.EndPoint);

            Tolerance tolerance = new Tolerance(1e-4, 0);
            //跟Y轴平行
            if (collinearData.Line.Direction.IsParallelTo(new Vector2d(0, 1), tolerance))
            {
                ptLst.Sort((p1, p2) => { return p1.Y.CompareTo(p2.Y); });
            }
            else
            {
                ptLst.Sort((p1, p2) => { return p1.X.CompareTo(p2.X); });
            }

            Point2d ptS;
            Point2d ptE;
            if (ptLst.Count > 3)
            {
                ptS = ptLst[1];
                ptE = ptLst[2];
            }
            else
            {
                ptS = ptLst.First();
                ptE = ptLst.Last();
            }

            var line = new Line(new Point3d(ptS.X, ptS.Y, 0), new Point3d(ptE.X, ptE.Y, 0));
            return line;
        }

        public static bool IsParallel(LineSegment2d lineFir, LineSegment2d lineSec)
        {
            var angle1 = lineFir.Direction.GetAngleTo(lineSec.Direction);
            var angle2 = lineFir.Direction.GetAngleTo(lineSec.Direction.Negate());
            if (CommonUtils.IsAlmostNearZero(Math.Abs(angle1), 1e-5) || CommonUtils.IsAlmostNearZero(Math.Abs(angle2), 1e-5))
                return true;
            return false;
        }

        /// <summary>
        /// 共线处理
        /// </summary>
        /// <param name="lineFir"></param>
        /// <param name="lineSec"></param>
        /// <returns></returns>
        public static bool IsCollinearLines(LineSegment2d lineFir, LineSegment2d lineSec)
        {
            var angle1 = lineFir.Direction.GetAngleTo(lineSec.Direction);
            var angle2 = lineFir.Direction.GetAngleTo(lineSec.Direction.Negate());
            if (CommonUtils.IsAlmostNearZero(Math.Abs(angle1), 1e-5) || CommonUtils.IsAlmostNearZero(Math.Abs(angle2), 1e-5))
            {
                var ptFirS = lineFir.StartPoint;
                var ptSecS = lineSec.StartPoint;
                var vector = ptFirS - ptSecS;
                var angle3 = lineFir.Direction.GetAngleTo(vector);
                var angle4 = lineFir.Direction.GetAngleTo(vector.Negate());
                if (CommonUtils.IsAlmostNearZero(Math.Abs(angle3), 1e-5) || CommonUtils.IsAlmostNearZero(Math.Abs(angle4), 1e-5))
                {
                    // 共线直线之间的距离不能太小
                    var ptFirE = lineFir.EndPoint;
                    var ptSecE = lineSec.EndPoint;
                    if (ptSecS.GetDistanceTo(ptFirS) > 10 && ptSecS.GetDistanceTo(ptFirE) > 10 && ptSecE.GetDistanceTo(ptFirS) > 10 && ptSecE.GetDistanceTo(ptFirE) > 10)
                        return true;
                }

            }

            return false;
        }

        // 直线处理
        public static List<LineSegment2d> BoundRelatedBoundCurves(List<LineSegment2d> doorBound, List<Curve> wallCurves)
        {
            var relatedLines = new List<LineSegment2d>();
            foreach (var curve in wallCurves)
            {
                if (curve is Line)
                {
                    var line = curve as Line;
                    var line2d = new LineSegment2d(new Point2d(line.StartPoint.X, line.StartPoint.Y), new Point2d(line.EndPoint.X, line.EndPoint.Y));
                    if (IsLinesIntersectWithLine(doorBound, line2d) || (CommonUtils.PointInnerEntity(doorBound, line2d.StartPoint) && CommonUtils.PointInnerEntity(doorBound, line2d.EndPoint)))
                    {
                        relatedLines.Add(line2d);
                    }
                }
            }

            return relatedLines;
        }

        /// <summary>
        /// 绘制
        /// </summary>
        /// <param name="curves"></param>
        public static void DrawCurves(List<Curve> curves)
        {
            using (AcadDatabase acad = AcadDatabase.Active())
            {
                IntegerCollection intCol = new IntegerCollection();
                foreach (var curve in curves)
                {
                    curve.Color = Color.FromRgb(0, 255, 0);
                    Autodesk.AutoCAD.GraphicsInterface.TransientManager tm = Autodesk.AutoCAD.GraphicsInterface.TransientManager.CurrentTransientManager;

                    tm.AddTransient(curve, Autodesk.AutoCAD.GraphicsInterface.TransientDrawingMode.Highlight, 128, intCol);
                }
            }
        }

        public static void DrawCurves(List<LineSegment2d> lines)
        {
            var curves = new List<Curve>();
            foreach (var line in lines)
            {
                var ptS2d = line.StartPoint;
                var ptE2d = line.EndPoint;
                var ptS = new Point3d(ptS2d.X, ptS2d.Y, 0);
                var ptE = new Point3d(ptE2d.X, ptE2d.Y, 0);
                curves.Add(new Line(ptS, ptE));
            }

            using (AcadDatabase acad = AcadDatabase.Active())
            {
                IntegerCollection intCol = new IntegerCollection();
                foreach (var curve in curves)
                {
                    curve.Color = Color.FromRgb(0, 255, 0);
                    Autodesk.AutoCAD.GraphicsInterface.TransientManager tm = Autodesk.AutoCAD.GraphicsInterface.TransientManager.CurrentTransientManager;

                    tm.AddTransient(curve, Autodesk.AutoCAD.GraphicsInterface.TransientDrawingMode.Highlight, 128, intCol);
                }
            }
        }

        /// <summary>
        /// 绘制
        /// </summary>
        /// <param name="curves"></param>
        public static void DrawCurvesAdd(List<Curve> curves)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                foreach (var curve in curves)
                {
                    // curve.UpgradeOpen();
                    curve.Color = Color.FromRgb(0, 255, 255);
                    // 添加到modelSpace中

                    acadDatabase.ModelSpace.Add(curve);
                }
            }
        }

        public static void DrawCurvesAdd(List<LineNode> lineNodes)
        {
            var line2ds = new List<LineSegment2d>();
            var layerNames = new List<string>();
            lineNodes.ForEach(p =>
            {
                line2ds.Add(p.CurLine);
                layerNames.Add(p.LayerName);
            });
            DrawCurvesAdd(line2ds, layerNames);
        }

        /// <summary>
        /// 绘制
        /// </summary>
        /// <param name="curves"></param>
        public static void DrawCurvesAdd(List<LineSegment2d> line2ds, List<string> layerNames = null)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var curves = new List<Curve>();
                foreach (var line2d in line2ds)
                {
                    var start = line2d.StartPoint;
                    var endPt = line2d.EndPoint;
                    var startPoint = new Point3d(start.X, start.Y, 0);
                    var endPoint = new Point3d(endPt.X, endPt.Y, 0);
                    var line = new Line(startPoint, endPoint);
                    curves.Add(line);
                }

                if (layerNames != null)
                {
                    for (int i = 0; i < layerNames.Count; i++)
                    {
                        curves[i].Layer = layerNames[i];
                    }
                }

                foreach (var curve in curves)
                {
                    curve.Color = Color.FromRgb(0, 255, 255);
                    // 添加到modelSpace中

                    acadDatabase.ModelSpace.Add(curve);
                }
            }
        }

        /// <summary>
        /// 绘制
        /// </summary>
        /// <param name="curves"></param>
        public static void DrawCurvesAddWithLayer(List<LineSegment2d> line2ds, List<string> layerNames = null)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            var curves = new List<Curve>();
            foreach (var line2d in line2ds)
            {
                var start = line2d.StartPoint;
                var endPt = line2d.EndPoint;
                var startPoint = new Point3d(start.X, start.Y, 0);
                var endPoint = new Point3d(endPt.X, endPt.Y, 0);
                var line = new Line(startPoint, endPoint);
                curves.Add(line);
            }

            foreach (var curve in curves)
            {
                if (layerNames != null)
                    curve.Layer = layerNames.First();
                curve.Color = Color.FromRgb(0, 255, 255);
                // 添加到modelSpace中
                AcHelper.DocumentExtensions.AddEntity<Curve>(doc, curve);
            }
        }

        // 绘图，用于测试点的位置
        public static void DrawLinesWithTransaction(List<TopoEdge> edges)
        {
            using (AcadDatabase acad = AcadDatabase.Active())
            {
                Document doc = Application.DocumentManager.MdiActiveDocument;
                foreach (var edge in edges)
                {
                    var ptS = edge.Start;
                    var ptE = edge.End;
                    var line = new Line(new Point3d(ptS.X, ptS.Y, 0), new Point3d(ptE.X, ptE.Y, 0));
                    line.Color = Color.FromRgb(0, 255, 255);
                    // 添加到modelSpace中
                    AcHelper.DocumentExtensions.AddEntity<Line>(doc, line);
                }
            }
        }

        // 绘图，用于测试点的位置
        public static void DrawLinesWithTransaction(List<LineSegment2d> lines)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            foreach (var line2d in lines)
            {
                var ptS = line2d.StartPoint;
                var ptE = line2d.EndPoint;
                var line = new Line(new Point3d(ptS.X, ptS.Y, 0), new Point3d(ptE.X, ptE.Y, 0));
                line.Color = Color.FromRgb(0, 255, 255);
                // 添加到modelSpace中
                AcHelper.DocumentExtensions.AddEntity<Line>(doc, line);
            }
        }

        /// <summary>
        /// 获取字体
        /// </summary>
        /// <returns></returns>
        public static ObjectId GetIdFromSymbolTable()
        {
            Database dbH = HostApplicationServices.WorkingDatabase;
            using (Transaction trans = dbH.TransactionManager.StartTransaction())
            {
                TextStyleTable textTableStyle = (TextStyleTable)trans.GetObject(dbH.TextStyleTableId, OpenMode.ForWrite);

                if (textTableStyle.Has("黑体"))
                {
                    ObjectId idres = textTableStyle["黑体"];
                    if (!idres.IsErased)
                        return idres;
                }
            }

            return ObjectId.Null;
        }

        /// <summary>
        /// 绘制
        /// </summary>
        /// <param name="curves"></param>
        public static void DrawTopoEdges(List<TopoEdge> curves)
        {
            using (AcadDatabase acad = AcadDatabase.Active())
            {
                IntegerCollection intCol = new IntegerCollection();
                foreach (var curve in curves)
                {
                    var ptS = curve.Start;
                    var ptE = curve.End;
                    var line = new Line(new Point3d(ptS.X, ptS.Y, 0), new Point3d(ptE.X, ptE.Y, 0));
                    line.Color = Color.FromRgb(0, 255, 0);
                    Autodesk.AutoCAD.GraphicsInterface.TransientManager tm = Autodesk.AutoCAD.GraphicsInterface.TransientManager.CurrentTransientManager;

                    tm.AddTransient(line, Autodesk.AutoCAD.GraphicsInterface.TransientDrawingMode.Highlight, 128, intCol);
                }
            }
        }

        // 绘图，用于测试点的位置
        public static void DrawPointWithTransaction(Point2d pt)
        {
            var drawPoint = new Point3d(pt.X, pt.Y, 0);
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Line lineX = new Line(drawPoint, drawPoint + new Vector3d(1, 0, 0) * 10000);
            Line lineY = new Line(drawPoint, drawPoint + new Vector3d(0, 1, 0) * 10000);

            // 添加到modelSpace中
            AcHelper.DocumentExtensions.AddEntity<Line>(doc, lineX);
            AcHelper.DocumentExtensions.AddEntity<Line>(doc, lineY);
        }

        /// <summary>
        /// 获取指定图层中的所有曲线
        /// </summary>
        /// <param name="layerName"></param>
        /// <returns></returns>
        public static List<Curve> GetAllCurvesFromLayerName(string layerName)
        {
            var curves = new List<Curve>();
            var curveIds = new List<ObjectId>();
            using (var db = AcadDatabase.Active())
            {
                var res = db.ModelSpace.OfType<Curve>().Where(p => p.Layer == layerName);
                foreach (var text in res)
                {
                    curveIds.Add(text.ObjectId);
                }
            }

            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database dataBase = doc.Database;

            using (Transaction dataGetTrans = dataBase.TransactionManager.StartTransaction())
            {
                // 图片多段线
                foreach (var curveId in curveIds)
                {
                    Curve curve = (Curve)dataGetTrans.GetObject(curveId, OpenMode.ForRead);

                    curves.Add((Curve)curve.Clone());
                }

                if (curves.Count == 0)
                    return null;
            }

            return curves;
        }

        /// <summary>
        /// 获取指定图层中的所有曲线
        /// </summary>
        /// <param name="layerName"></param>
        /// <returns></returns>
        public static List<Curve> GetAllCurvesFromLayerNames(List<string> layerNames)
        {
            var curves = new List<Curve>();
            var curveIds = new List<ObjectId>();
            var blockCurves = new List<Curve>();
            using (var db = AcadDatabase.Active())
            {

                // curve 读取
                var res = db.ModelSpace.OfType<Curve>().Where(p =>
                {
                    foreach (var layerName in layerNames)
                    {
                        if (p.Layer == layerName)
                            return true;

                    }

                    return false;
                });

                foreach (var text in res)
                {
                    curveIds.Add(text.ObjectId);
                }

                var blocks = db.ModelSpace.OfType<BlockReference>();
                foreach (var block in blocks)
                {
                    if (ValidBlock(block, layerNames))
                    {
                        var blockRelatedCurves = GetCurvesFromBlock(block);
                        if (blockRelatedCurves.Count != 0)
                            blockCurves.AddRange(blockRelatedCurves);
                    }
                    else
                    {
                        DBObjectCollection collection = new DBObjectCollection();
                        block.Explode(collection);
                        foreach (var obj in collection)
                        {
                            if (obj is Curve)
                            {
                                var curve = obj as Curve;
                                if (ValidLayer(curve.Layer, layerNames))
                                    blockCurves.Add(curve);
                            }
                        }
                    }
                }

            }

            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database dataBase = doc.Database;

            using (Transaction dataGetTrans = dataBase.TransactionManager.StartTransaction())
            {
                // 图片多段线
                foreach (var curveId in curveIds)
                {
                    Curve curve = (Curve)dataGetTrans.GetObject(curveId, OpenMode.ForRead);

                    curves.Add((Curve)curve.Clone());
                }
            }

            if (blockCurves.Count != 0)
                curves.AddRange(blockCurves);
            return curves;
        }

        /// <summary>
        /// 获取指定图层块中的所有曲线
        /// </summary>
        /// <param name="layerName"></param>
        /// <returns></returns>
        public static List<List<LineSegment2d>> GetBoundsFromLayerBlocks(List<string> layerNames, List<LineSegment2d> rectLines)
        {
            var blockBounds = new List<List<LineSegment2d>>();
            using (var db = AcadDatabase.Active())
            {
                var blocks = db.ModelSpace.OfType<BlockReference>();
                foreach (var block in blocks)
                {
                    if (ValidBlock(block, layerNames))
                    {
                        var blockCurves = GetCurvesFromBlock(block);
                        var lines = GetBoundFromCurves(blockCurves);
                        if (lines == null || lines.Count == 0)
                            continue;

                        if (CommonUtils.OutLoopContainsInnerLoop(rectLines, lines))
                            blockBounds.Add(lines);
                    }
                }
            }

            return blockBounds;
        }

        /// <summary>
        /// 获取指定图层中连通区域的包围盒边界处理
        /// </summary>
        /// <param name="layerName"></param>
        /// <returns></returns>
        public static List<List<LineSegment2d>> GetBoundsFromLayerBlocksAndCurves(List<string> layerNames, List<LineSegment2d> rectLines)
        {
            var blockBounds = new List<List<LineSegment2d>>();

            using (var db = AcadDatabase.Active())
            {
                var blocks = db.ModelSpace.OfType<BlockReference>();
                foreach (var block in blocks)
                {
                    if (ValidBlock(block, layerNames))
                    {
                        var blockCurves = GetCurvesFromBlock(block);
                        var lines = GetBoundFromCurves(blockCurves);
                        if (lines == null || lines.Count == 0)
                            continue;

                        if (CommonUtils.OutLoopContainsInnerLoop(rectLines, lines))
                            blockBounds.Add(lines);
                    }
                }

                var layerCurves = db.ModelSpace.OfType<Curve>().Where(l =>
                {
                    foreach (var layerName in layerNames)
                    {
                        if (l.Layer == layerName)
                            return true;
                    }

                    return false;
                }).ToList<Curve>();

                var validCurves = GetValidCurvesFromSelectArea(layerCurves, rectLines);

                foreach (var curve in validCurves)
                {
                    if (curve is Line)
                    {
                        var line = curve as Line;
                        var ptS = new Point2d(line.StartPoint.X, line.StartPoint.Y);
                        var ptE = new Point2d(line.EndPoint.X, line.EndPoint.Y);
                        blockBounds.Add(new List<LineSegment2d>() { new LineSegment2d(ptS, ptE) });
                    }
                    else
                    {
                        var curveBounds = ThRoomUtils.GetRectFromBound(curve.Bounds.Value);
                        blockBounds.Add(curveBounds);
                    }
                }
            }

            // 生成相邻框的数据，框边缘是外扩的， 上面的框边缘都是原始的，不经过外扩的数据
            var connectBounds = ConnectedBoxBound.MakeRelatedBoundary(blockBounds);
            return connectBounds;
        }

        /// <summary>
        ///  计算curves的外包边界
        /// </summary>
        /// <param name="curves"></param>
        /// <returns></returns>
        public static List<LineSegment2d> GetBoundFromCurves(List<Curve> curves)
        {
            if (curves == null || curves.Count == 0)
                return null;

            var boundLines = new List<LineSegment2d>();
            var lineNodes = TopoUtils.TesslateCurve2Lines(curves);
            var ptLst = new List<XY>();
            foreach (var lineNode in lineNodes)
            {
                var line = lineNode.CurLine;
                var pxHead = line.StartPoint;
                var pxEnd = line.EndPoint;
                var ptS = new XY(pxHead.X, pxHead.Y);
                var ptE = new XY(pxEnd.X, pxEnd.Y);
                ptLst.Add(ptS);
                ptLst.Add(ptE);
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

            var pt1 = new Point2d(leftBottom.X, leftBottom.Y);
            var pt3 = new Point2d(rightTop.X, rightTop.Y);
            var pt2 = new Point2d(pt3.X, pt1.Y);
            var pt4 = new Point2d(pt1.X, pt3.Y);

            var line1 = new LineSegment2d(pt1, pt2);
            var line2 = new LineSegment2d(pt2, pt3);
            var line3 = new LineSegment2d(pt3, pt4);
            var line4 = new LineSegment2d(pt4, pt1);
            boundLines.Add(line1);
            boundLines.Add(line2);
            boundLines.Add(line3);
            boundLines.Add(line4);
            return boundLines;
        }

        /// <summary>
        ///  计算外包边界
        /// </summary>
        /// <param name="line2ds"></param>
        /// <returns></returns>
        public static List<LineSegment2d> GetBoundFromLine2ds(List<LineSegment2d> line2ds)
        {
            if (line2ds == null || line2ds.Count == 0)
                return null;

            var boundLines = new List<LineSegment2d>();
            var ptLst = new List<XY>();
            foreach (var line in line2ds)
            {
                var pxHead = line.StartPoint;
                var pxEnd = line.EndPoint;
                var ptS = new XY(pxHead.X, pxHead.Y);
                var ptE = new XY(pxEnd.X, pxEnd.Y);
                ptLst.Add(ptS);
                ptLst.Add(ptE);
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

            leftBottom = leftBottom - new XY(10, 10);
            rightTop = rightTop + new XY(10, 10);
            var pt1 = new Point2d(leftBottom.X, leftBottom.Y);
            var pt3 = new Point2d(rightTop.X, rightTop.Y);
            var pt2 = new Point2d(pt3.X, pt1.Y);
            var pt4 = new Point2d(pt1.X, pt3.Y);

            var line1 = new LineSegment2d(pt1, pt2);
            var line2 = new LineSegment2d(pt2, pt3);
            var line3 = new LineSegment2d(pt3, pt4);
            var line4 = new LineSegment2d(pt4, pt1);
            boundLines.Add(line1);
            boundLines.Add(line2);
            boundLines.Add(line3);
            boundLines.Add(line4);
            return boundLines;
        }

        /// <summary>
        /// 获取block中的所有曲线数据
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public static List<Curve> GetCurvesFromBlock(BlockReference block)
        {
            DBObjectCollection collection = new DBObjectCollection();
            block.Explode(collection);
            var blockCurves = new List<Curve>();
            foreach (var obj in collection)
            {
                if (obj is Curve)
                {
                    var curve = obj as Curve;
                    curve.Layer = block.Layer;
                    blockCurves.Add(curve);
                }
                else if (obj is BlockReference)
                {
                    var blockReference = obj as BlockReference;
                    blockReference.Layer = block.Layer;
                    var childCurves = GetCurvesFromBlock(blockReference);
                    if (childCurves.Count != 0)
                        blockCurves.AddRange(childCurves);
                }
            }

            return blockCurves;
        }

        /// <summary>
        /// 是否是有效的块
        /// </summary>
        /// <param name="block"></param>
        /// <param name="layerNames"></param>
        /// <returns></returns>
        public static bool ValidBlock(BlockReference block, List<string> layerNames)
        {
            foreach (var layerName in layerNames)
            {
                if (block.Layer == layerName)
                    return true;
            }

            return false;
        }

        public static bool ValidCurve(Curve curve, List<string> layerNames)
        {
            foreach (var layerName in layerNames)
            {
                if (curve.Layer == layerName)
                    return true;
            }

            return false;
        }
        /// <summary>
        /// 是否是有效的curve
        /// </summary>
        /// <param name="curveLayer"></param>
        /// <param name="layerNames"></param>
        /// <returns></returns>
        public static bool ValidLayer(string curveLayer, List<string> layerNames)
        {
            foreach (var layerName in layerNames)
            {
                if (curveLayer == layerName)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 获取文本
        /// </summary>
        /// <param name="layerName"></param>
        /// <returns></returns>
        public static List<DBText> GetAllTextFromLayerName(string layerName)
        {
            var texts = new List<DBText>();
            var textIds = new List<ObjectId>();
            using (var db = AcadDatabase.Active())
            {
                var res = db.ModelSpace.OfType<DBText>().Where(p => p.Layer == layerName);
                foreach (var text in res)
                {
                    textIds.Add(text.ObjectId);
                }
            }

            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database dataBase = doc.Database;

            using (Transaction dataGetTrans = dataBase.TransactionManager.StartTransaction())
            {
                // 图片多段线
                foreach (var textId in textIds)
                {
                    DBText curve = (DBText)dataGetTrans.GetObject(textId, OpenMode.ForRead);

                    texts.Add((DBText)curve.Clone());
                }

                if (texts.Count == 0)
                    return null;
            }

            return texts;
        }

        /// <summary>
        /// 获取框选范围的线的集合
        /// </summary>
        /// <param name="ptFir"></param>
        /// <param name="ptSec"></param>
        /// <returns></returns>
        public static List<LineSegment2d> GetRectangleFromPoint(Point3d ptFir, Point3d ptSec)
        {
            var ptLst = new List<XY>();
            var ptFir2d = new XY(ptFir.X, ptFir.Y);
            var ptSec2d = new XY(ptSec.X, ptSec.Y);
            ptLst.Add(ptFir2d);
            ptLst.Add(ptSec2d);
            var leftBottom = new XY(ptFir2d.X, ptFir2d.Y);
            var rightTop = new XY(ptFir2d.X, ptFir2d.Y);
            if (leftBottom.X > ptSec2d.X)
                leftBottom.X = ptSec2d.X;
            if (leftBottom.Y > ptSec2d.Y)
                leftBottom.Y = ptSec2d.Y;

            if (rightTop.X < ptSec2d.X)
                rightTop.X = ptSec2d.X;
            if (rightTop.Y < ptSec2d.Y)
                rightTop.Y = ptSec2d.Y;
            if (leftBottom.IsEqualTo(rightTop))
                return null;
            var recPt1 = new Point2d(leftBottom.X, leftBottom.Y);
            var recPt2 = new Point2d(rightTop.X, leftBottom.Y);
            var recPt3 = new Point2d(rightTop.X, rightTop.Y);
            var recPt4 = new Point2d(leftBottom.X, rightTop.Y);
            var lines = new List<LineSegment2d>();
            lines.Add(new LineSegment2d(recPt1, recPt2));
            lines.Add(new LineSegment2d(recPt2, recPt3));
            lines.Add(new LineSegment2d(recPt3, recPt4));
            lines.Add(new LineSegment2d(recPt4, recPt1));
            return lines;
        }

        /// <summary>
        /// 矩形框选择后的轮廓面积线
        /// </summary>
        /// <param name="roomDatas"></param>
        /// <param name="rectLines"></param>
        /// <returns></returns>
        public static List<RoomDataPolyline> MakeValidProfilesFromSelectRect(List<RoomData> roomDatas, List<LineSegment2d> rectLines)
        {
            if (roomDatas == null || roomDatas.Count == 0)
                return null;

            if (rectLines == null || rectLines.Count == 0)
                return null;

            var validRoomDatas = new List<RoomData>();
            foreach (var room in roomDatas)
            {
                if (room.Lines != null && room.Lines.Count != 0)
                {
                    if (ProfileInRectLines(room.Lines, rectLines))
                    {
                        validRoomDatas.Add(room);
                    }
                }
            }

            if (validRoomDatas.Count == 0)
                return null;

            var roomDataPolylines = TopoUtils.Convert2RoomDataPolyline(validRoomDatas);
            return roomDataPolylines;
        }

        public static bool ProfileInRectLines(List<Line> lines, List<LineSegment2d> rectLines)
        {
            foreach (var line in lines)
            {
                var startPoint3d = line.StartPoint;
                var startPoint2d = new Point2d(startPoint3d.X, startPoint3d.Y);
                if (!CommonUtils.PointInnerEntity(rectLines, startPoint2d))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 界面显示
        /// </summary>
        /// <param name="roomDataPolylines"></param>
        /// <param name="layerName"></param>
        public static void DisplayRoomProfile(List<RoomDataPolyline> roomDataPolylines, string layerName)
        {
            // 设置字体样式
            var textId = ThRoomUtils.GetIdFromSymbolTable();
            using (var db = AcadDatabase.Active())
            {
                var layers = db.Layers;
                LayerTableRecord layerRecord = null;
                foreach (var layer in layers)
                {
                    if (layer.Name.Equals(layerName))
                    {
                        layerRecord = db.Layers.Element(layerName);
                        break;
                    }
                }

                // 创建新的图层
                if (layerRecord == null)
                {
                    layerRecord = db.Layers.Create(layerName);
                    layerRecord.Color = Color.FromRgb(255, 0, 0);
                    layerRecord.IsPlottable = false;
                }

                foreach (var room in roomDataPolylines)
                {
                    if (room.ValidData)
                    {
                        if (!CommonUtils.IsAlmostNearZero(room.RoomPolyline.Area, 2600000))
                        {
                            var area = (int)(room.RoomPolyline.Area * 1e-6);
                            var pos = room.Pos;
                            var dbText = new DBText();
                            if (textId != ObjectId.Null)
                                dbText.TextStyleId = textId;
                            dbText.TextString = area.ToString() + "m²";
                            dbText.Position = pos;
                            dbText.Height = 200;
                            dbText.Thickness = 1;
                            dbText.WidthFactor = 1;
                            var objectTextId = db.ModelSpace.Add(dbText);
                            db.ModelSpace.Element(objectTextId, true).Layer = layerName;
                            var objectPolylineId = db.ModelSpace.Add(room.RoomPolyline);
                            db.ModelSpace.Element(objectPolylineId, true).Layer = layerName;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获取选择矩形框线
        /// </summary>
        /// <returns></returns>
        public static List<LineSegment2d> GetSelectRectLines()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            PromptPointOptions ppo = new PromptPointOptions("\n请框选需要生成房间线的范围");
            ppo.AllowNone = false;
            PromptPointResult ppr = ed.GetPoint(ppo);
            if (ppr.Status != PromptStatus.OK)
                return null;

            Point3d first = ppr.Value;
            PromptCornerOptions pco = new PromptCornerOptions("\n框选结束", first);
            ppr = ed.GetCorner(pco);
            if (ppr.Status != PromptStatus.OK)
                return null;
            Point3d second = ppr.Value;

            //框线范围
            var rectLines = ThRoomUtils.GetRectangleFromPoint(first, second);
            return rectLines;
        }

        /// <summary>
        /// 获取矩形框截取后的curves
        /// </summary>
        /// <param name="allCurves"></param>
        /// <param name="rectLines"></param>
        /// <returns></returns>
        public static List<Curve> GetValidCurvesFromSelectArea(List<Curve> allCurves, List<LineSegment2d> rectLines)
        {
            if (allCurves == null || allCurves.Count == 0)
                return null;

            var validCurves = new List<Curve>();
            foreach (var srcCurve in allCurves)
            {
                if (IsValidCurve(srcCurve, rectLines))
                    validCurves.Add(srcCurve);
            }

            return validCurves;
        }

        public static bool IsValidCurve(Curve curve, List<LineSegment2d> rectLines)
        {
            var bound = curve.Bounds.Value;
            if (bound == null)
                return false;


            var srcLine2ds = GetRectFromBound(bound);
            if (IsLinesIntersectWithLines(srcLine2ds, rectLines) || CommonUtils.OutLoopContainsInnerLoop(rectLines, srcLine2ds))
            {
                return true;
            }

            return false;
        }


        public static bool IsLinesIntersectWithLines(List<LineSegment2d> srcLines, List<LineSegment2d> rectLines)
        {
            foreach (var rectLine in rectLines)
            {
                foreach (var line in srcLines)
                {
                    var intersectPts = line.IntersectWith(rectLine);
                    if (intersectPts != null && intersectPts.Count() != 0)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 线是否与曲线集相交
        /// </summary>
        /// <param name="srcLines"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        public static bool IsLinesIntersectWithLine(List<LineSegment2d> srcLines, LineSegment2d line)
        {
            foreach (var srcline in srcLines)
            {
                var intersectPts = srcline.IntersectWith(line);
                if (intersectPts != null && intersectPts.Count() != 0)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 可扩展的矩形范围， vector 为空值时， 则是原始的范围
        /// </summary>
        /// <param name="bound"></param>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static List<LineSegment2d> GetRectFromBound(Extents3d bound, Vector3d? vector = null)
        {
            Point3d minPoint;
            Point3d maxPoint;
            if (vector != null)
            {
                minPoint = bound.MinPoint - vector.Value;
                maxPoint = bound.MaxPoint + vector.Value;
            }
            else
            {
                minPoint = bound.MinPoint;
                maxPoint = bound.MaxPoint;
            }

            var recPt1 = new Point2d(minPoint.X, minPoint.Y);
            var recPt2 = new Point2d(maxPoint.X, minPoint.Y);
            var recPt3 = new Point2d(maxPoint.X, maxPoint.Y);
            var recPt4 = new Point2d(minPoint.X, maxPoint.Y);
            var lines = new List<LineSegment2d>();
            if (!CommonUtils.IsAlmostNearZero(recPt1.GetDistanceTo(recPt2), 1e-4))
                lines.Add(new LineSegment2d(recPt1, recPt2));
            if (!CommonUtils.IsAlmostNearZero(recPt2.GetDistanceTo(recPt3), 1e-4))
                lines.Add(new LineSegment2d(recPt2, recPt3));
            if (!CommonUtils.IsAlmostNearZero(recPt3.GetDistanceTo(recPt4), 1e-4))
                lines.Add(new LineSegment2d(recPt3, recPt4));
            if (!CommonUtils.IsAlmostNearZero(recPt4.GetDistanceTo(recPt1), 1e-4))
                lines.Add(new LineSegment2d(recPt4, recPt1));
            return lines;
        }
        /// <summary>
        /// 打开需要显示的图层
        /// </summary>
        public static List<string> ShowLayers()
        {
            var aimLayers = new List<string>();
            using (var db = AcadDatabase.Active())
            {
                var layers = db.Layers;
                var closeLayerNames = new List<string>();
                foreach (var layer in layers)
                {
                    if (!layer.Name.Contains("WALL") && !layer.Name.Contains("AE-DOOR") && !layer.Name.Contains("AD-NAME") && !layer.Name.Contains("S_COLU")
                         && !layer.Name.Contains("WINDOW") && !layer.Name.Contains("天华面积框线"))
                    {
                        closeLayerNames.Add(layer.Name);
                    }

                    if (layer.Name.Contains("WALL") || layer.Name.Contains("AE-DOOR") || layer.Name.Contains("AD-NAME") || layer.Name.Contains("S_COLU")
                         || layer.Name.Contains("WINDOW"))
                    {
                        aimLayers.Add(layer.Name);
                    }
                }

                foreach (var lName in closeLayerNames)
                {
                    db.Layers.Element(lName, true).IsOff = true;
                }
            }

            return aimLayers;
        }

        /// <summary>
        /// 打开需要显示的图层
        /// </summary>
        public static List<string> ShowThLayers(out List<string> wallLayers, out List<string> doorLayers, out List<string> windLayers)
        {
            var allCurveLayers = new List<string>();
            wallLayers = new List<string>();
            doorLayers = new List<string>();
            windLayers = new List<string>();
            using (var db = AcadDatabase.Active())
            {
                var layers = db.Layers;
                var closeLayerNames = new List<string>();
                foreach (var layer in layers)
                {
                    //if (!layer.Name.Contains("AE-DOOR-INSD") && !layer.Name.Contains("AE-WALL") && !layer.Name.Contains("AE-WIND") && !layer.Name.Contains("AD-NAME-ROOM")
                    //     && !layer.Name.Contains("AE-STRU") && !layer.Name.Contains("S_COLU") && !layer.Name.Contains("天华面积框线"))
                    //{
                    //    closeLayerNames.Add(layer.Name);
                    //}

                    if (layer.Name.Contains("AE-WALL") || layer.Name.Contains("AD-NAME-ROOM")
                         || layer.Name.Contains("AE-STRU") || layer.Name.Contains("S_COLU"))
                    {
                        allCurveLayers.Add(layer.Name);
                    }

                    if (layer.Name.Contains("AE-WALL"))
                        wallLayers.Add(layer.Name);

                    if (layer.Name.Contains("AE-DOOR-INSD"))
                        doorLayers.Add(layer.Name);

                    if (layer.Name.Contains("AE-WIND"))
                        windLayers.Add(layer.Name);
                }

                foreach (var lName in closeLayerNames)
                {
                    db.Layers.Element(lName, true).IsOff = true;
                }
            }

            return allCurveLayers;
        }

        /// <summary>
        /// 扣减墙本生的轮廓
        /// </summary>
        /// <param name="allRoomDataPolylines"></param>
        /// <param name="wallRoomDataPolyines"></param>
        /// <returns></returns>
        public static void MakeValidRoomDataPolylines(List<RoomDataPolyline> allRoomDataPolylines, List<RoomDataPolyline> wallRoomDataPolyines)
        {
            if (wallRoomDataPolyines == null || wallRoomDataPolyines.Count == 0)
                return;

            foreach (var curAllRoomData in allRoomDataPolylines)
            {
                if (curAllRoomData.ValidData)
                {
                    foreach (var curWallRoomData in wallRoomDataPolyines)
                    {
                        if (IsRoomDataSame(curAllRoomData, curWallRoomData))
                            break;
                    }
                }

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="srcRoomData"> 所有的轮廓中的一个</param>
        /// <param name="desRoomData"> 墙的轮廓中的一个</param>
        /// <returns></returns>
        public static bool IsRoomDataSame(RoomDataPolyline srcRoomData, RoomDataPolyline desRoomData)
        {
            if (desRoomData == null)
                return false;

            if (CommonUtils.IsAlmostNearZero(Math.Abs(srcRoomData.Area) - Math.Abs(desRoomData.Area), 100)
                && CommonUtils.IsAlmostNearZero(srcRoomData.RoomPolyline.Length - desRoomData.RoomPolyline.Length, 2))
            {
                srcRoomData.ValidData = false;
                return true;
            }

            return false;
        }

        public static List<Curve> MoveCurves(List<Curve> curves, double moveValue = -40000)
        {
            var moveCurves = new List<Curve>();
            foreach (var curve in curves)
            {
                if (curve is Line)
                {
                    var line = curve as Line;
                    var ptS = line.StartPoint + new Vector3d(0, moveValue, 0);
                    var ptE = line.EndPoint + new Vector3d(0, moveValue, 0);
                    moveCurves.Add(new Line(ptS, ptE));
                }
            }

            return moveCurves;
        }
    }
}
