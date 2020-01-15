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
using System.IO;

namespace ThSpray
{
    public class ConnectedNode
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
    public class ConnectedBoxBound
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
                var boundLines = GetBoundFromLine2ds(connectCurves);
                if (boundLines != null && boundLines.Count != 0)
                {
                    m_outBounds.Add(boundLines);
                }
            }
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
                    if (!connectedNode.m_bUse && !InOutNodes.Contains(connectedNode))
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
    class Utils
    {
        public static int INDEX = 0;
        /// <summary>
        /// 获取所有curves
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// 插入喷淋图块
        /// </summary>
        /// <param name="insertPts"></param>
        public static void InsertSprayBlock(List<Point3d> insertPts, SprayType type)
        {
            Utils.CreateLayer("W-FRPT-SPRL", Color.FromRgb(191, 255, 0));

            string propName = "上喷";
            if (type == SprayType.SPRAYDOWN)
                propName = "下喷";
            using (var db = AcadDatabase.Active())
            {
                var filePath = Path.Combine(ThCADCommon.SupportPath(), "SprayBlockUp.dwg");
                db.Database.ImportBlocksFromDwg(filePath);
                foreach (var insertPoint in insertPts)
                {
                    var blockId = db.ModelSpace.ObjectId.InsertBlockReference("W-FRPT-SPRL", "喷头", insertPoint, new Scale3d(1, 1, 1), 0);
                    var props = blockId.GetDynProperties();
                    foreach (DynamicBlockReferenceProperty prop in props)
                    {
                        // 如果动态属性的名称与输入的名称相同
                        prop.Value = propName;
                    }
                }
            }
        }

        /// <summary>
        /// 生成房间内所有的可布置轮廓，经过距离裁剪后的轮廓
        /// </summary>
        /// <param name="beams"></param>
        /// <param name="roomCurves"></param>
        /// <returns></returns>
        public static List<Curve> MakeValidProfiles(List<Curve> srcBeams, Polyline roomPolyline)
        {
            if (srcBeams == null || srcBeams.Count == 0)
                return null;

            // 生成相关梁数据
            var roomCurves = TopoUtils.Polyline2dLines(roomPolyline);
            var beams = TopoUtils.TesslateCurve(srcBeams);
            var relatedBeams = GetValidBeams(beams, roomCurves);
            var offsetPolylines = new List<Curve>();
            var line2ds = CommonUtils.Polyline2dLines(roomPolyline);
            if (CommonUtils.CalcuLoopArea(line2ds) < 0)
                roomPolyline.ReverseCurve();
            var dbRoom = roomPolyline.GetOffsetCurves(-100);
            for (int i = 0; i < dbRoom.Count; i++)
                offsetPolylines.Add(dbRoom[i] as Polyline);

            // 梁轮廓提取
            var polylines = BeamPath.MakeBeamLoop(relatedBeams, roomCurves);


            if (polylines != null && polylines.Count != 0)
            {
                foreach (var offset in polylines)
                {
                    var poly = offset as Polyline;
                    var lines = CommonUtils.Polyline2dLines(poly);
                    if (CommonUtils.CalcuLoopArea(lines) < 0)
                        poly.ReverseCurve();
                    var db = poly.GetOffsetCurves(-600);
                    for (int i = 0; i < db.Count; i++)
                    {
                        offsetPolylines.Add(db[i] as Polyline);
                    }
                }
            }

            TriUtils.DrawCurvesAdd(offsetPolylines);
            return polylines;
        }

        /// <summary>
        /// 延长curve的长度
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="length"></param>
        public static void ExtendCurve(Curve curve, double length)
        {
            try
            {
                if (curve is Line)
                {
                    var line = curve as Line;
                    var startPoint = line.StartPoint;
                    var endPoint = line.EndPoint;
                    var dir = line.GetFirstDerivative(startPoint);
                    var ptHead = startPoint - dir.GetNormal() * length;
                    var ptTail = endPoint + dir.GetNormal() * length;

                    line.Extend(true, ptHead);
                    line.Extend(false, ptTail);
                }
                else if (curve is Arc)
                {
                    var arc = curve as Arc;
                    var radius = arc.Radius;
                    var startParam = arc.StartParam;
                    var endParam = arc.EndParam;
                    var param = Math.Asin(length * 0.5 / radius) * 2;
                    arc.Extend(startParam - param);
                    arc.Extend(endParam + param);
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// 延长指定长度
        /// </summary>
        /// <param name="curves"></param>
        /// <param name="length"></param>
        public static void ExtendCurves(List<Curve> curves, double length)
        {
            foreach (var curve in curves)
                ExtendCurve(curve, length);
        }

        /// <summary>
        /// 删除无效的，即很窄的轮廓
        /// </summary>
        /// <param name="beamLoops"></param>
        /// <param name="minSprayOffset"></param>
        /// <returns></returns>
        public static List<Curve> EraseInvalidLoops(List<Curve> beamLoops, double minSprayOffset)
        {
            // 梁轮廓内部数据提取
            var validLoops = new List<Curve>();
            for (int i = 0; i < beamLoops.Count; i++)
            {
                var poly = beamLoops[i] as Polyline;

                var lines = CommonUtils.Polyline2dLines(poly);
                if (CommonUtils.CalcuLoopArea(lines) < 0)
                    poly.ReverseCurve();

                var db = poly.GetOffsetCurves(-minSprayOffset);
                for (int j = 0; j < db.Count; j++)
                {
                    validLoops.Add(db[j] as Polyline);
                }
            }

            return validLoops;
        }

        /// <summary>
        /// 墙的内部可布置梁轮廓
        /// </summary>
        /// <param name="allCurves"></param>
        /// <param name="roomPolyline"></param>
        /// <returns></returns>
        public static List<Point3d> MakeInnerProfiles(List<Curve> allCurves, Polyline roomPolyline, PlaceData userData)
        {
            if (allCurves == null || allCurves.Count == 0)
                return null;

            // 生成相关梁数据
            var roomCurves = TopoUtils.Polyline2dLines(roomPolyline);
            var curves = TopoUtils.TesslateCurve(allCurves);
            var relatedCurves = GetValidBeams(curves, roomCurves);
            if (relatedCurves == null || relatedCurves.Count == 0)
                return null;

            // 偏移数据
            var offsetPolylines = new List<Curve>();
            var line2ds = CommonUtils.Polyline2dLines(roomPolyline);
            if (CommonUtils.CalcuLoopArea(line2ds) < 0)
                roomPolyline.ReverseCurve();

            var offsetRoomPolylines = new List<Curve>();
            var dbRoom = roomPolyline.GetOffsetCurves(-userData.minWallGap);
            for (int i = 0; i < dbRoom.Count; i++)
                offsetRoomPolylines.Add(dbRoom[i] as Polyline);
            if (offsetRoomPolylines.Count != 0 && relatedCurves.Count != 0)
            {
                relatedCurves.AddRange(TopoUtils.Polyline2dLines(offsetRoomPolylines.First() as Polyline));
            }
            var beamLoops = TopoUtils.MakeSrcProfilesNoTes(relatedCurves);
            var validLoops = EraseInvalidLoops(beamLoops, userData.minBeamGap);

            var insertPtS = PlaceSpray.CalcuInsertPos(TopoUtils.Polyline2dLines(roomPolyline), validLoops, userData.minBeamGap, userData.maxWallGap, userData.maxSprayGap);

            return insertPtS;
        }

        public class PlaceSpray
        {
            private List<Curve> m_roomSrcCurves; // 房间的原始边界线
            private List<List<Curve>> m_beamLoops; // 梁轮廓
            private double m_wallSprayOffset; // 喷淋距墙的最小位置
            private double m_maxWallSprayOffset; // 喷淋距墙的最大位置值
            private double m_sprayGap; // 喷淋之间最大间距
            private Line m_splitLeftEdge;
            private Line m_splitBottomEdge;

            private double m_offsetAdd = 43.52; // 水平或者垂直方向上面增加的向量偏移值

            // 原始房间包围盒的上下左右边
            private Line m_roomRight;
            private Line m_roomTop;
            private Line m_roomLeft;
            private Line m_roomBottom;

            private List<Line> m_hLines; // 水平切割线
            private List<Line> m_vLines; // 垂直切割线

            private List<Line> boundCurves; // 边界轮廓
            public PlaceSpray(List<Curve> srcRoomCurves, List<List<Curve>> beamLoops, double minWallSprayOffset, double maxWallSprayOffset, double sprayGap)
            {
                m_roomSrcCurves = srcRoomCurves;
                m_beamLoops = beamLoops;
                m_wallSprayOffset = minWallSprayOffset;
                m_sprayGap = sprayGap;
                m_maxWallSprayOffset = maxWallSprayOffset;
            }

            public static List<Point3d> CalcuInsertPos(List<Curve> srcRoomCurves, List<Curve> loops, double minWallSprayOffset, double maxWallSprayOffset, double sprayGap)
            {
                var beamLoops = new List<List<Curve>>();
                foreach (var loop in loops)
                {
                    if (loop is Polyline)
                        beamLoops.Add(TopoUtils.Polyline2dLines(loop as Polyline));
                }

                var placeSpray = new PlaceSpray(srcRoomCurves, beamLoops, minWallSprayOffset, maxWallSprayOffset, sprayGap);
                placeSpray.CalStartEdges(); // 开始边
                placeSpray.CalcuVHLines(); // 水平，垂直线，并考虑是否平移至最下边和最左边
                placeSpray.CutFromMaxSprayOffset(); // 两边截断处理
                var ptsLst = placeSpray.DoPlace(); // 放置插入点计算

                var insertPts = new List<Point3d>();
                if (ptsLst != null && ptsLst.Count != 0)
                {
                    foreach (var pts in ptsLst)
                    {
                        insertPts.AddRange(pts);
                    }
                }
                return insertPts;
            }


            /// <summary>
            /// 计算开始边
            /// </summary>
            private void CalStartEdges()
            {
                // 原始房间的外接矩形框
                var srcRoomEdgeCal = new EdgeCalcu(m_roomSrcCurves, m_wallSprayOffset);
                srcRoomEdgeCal.Do();

                // 房间外包框曲线
                boundCurves = srcRoomEdgeCal.BoundCurves;

                // 左边和底边的第一条分割偏移边
                m_splitLeftEdge = srcRoomEdgeCal.LeftEdge; // 从下到上的边
                m_splitBottomEdge = srcRoomEdgeCal.BottomEdge; // 从左到右的边

                // 原始分界边的右边和上边
                m_roomRight = srcRoomEdgeCal.SrcRightEdge;
                m_roomTop = srcRoomEdgeCal.SrcTopEdge;

                m_roomLeft = srcRoomEdgeCal.SrcLeftEdge;
                m_roomBottom = srcRoomEdgeCal.SrcBottomEdge;

                // 所有梁轮廓的边界值计算
                var beamCurves = new List<Curve>();
                foreach (var curves in m_beamLoops)
                {
                    beamCurves.AddRange(curves);
                }
                var beamLoopEdgeCal = new EdgeCalcu(beamCurves, m_wallSprayOffset);
                beamLoopEdgeCal.Do();
                var leftEdge = beamLoopEdgeCal.SrcLeftEdge;
                var vecHori = new Vector3d(m_offsetAdd, 0, 0);
                var leftPtS = leftEdge.StartPoint;
                var leftPtE = leftEdge.EndPoint;
                m_splitLeftEdge = new Line(leftPtS + vecHori, leftPtE + vecHori);

                var bottomEdge = beamLoopEdgeCal.SrcBottomEdge;
                var vecVer = new Vector3d(0, m_offsetAdd, 0);
                var bottomPtS = bottomEdge.StartPoint;
                var bottomPtE = bottomEdge.EndPoint;
                m_splitBottomEdge = new Line(bottomPtS + vecVer, bottomPtE + vecVer);
            }


            private List<Point3d> LineWithLoop(Line line, List<Curve> curLoop)
            {
                var ptLst = new List<Point3d>();
                for (int j = 0; j < curLoop.Count; j++)
                {
                    var curve = curLoop[j];

                    if (!CommonUtils.IntersectValid(line, curve))
                        continue;

                    var tmpPtLst = new Point3dCollection();
                    line.IntersectWith(curve, Intersect.OnBothOperands, tmpPtLst, (IntPtr)0, (IntPtr)0);
                    if (tmpPtLst.Count != 0 && tmpPtLst.Count < 3)
                    {
                        foreach (Point3d pt in tmpPtLst)
                        {
                            bool bInLst = false;
                            for (int m = 0; m < ptLst.Count; m++)
                            {
                                var curPt = ptLst[m];
                                if (CommonUtils.Point3dIsEqualPoint3d(pt, curPt))
                                {
                                    bInLst = true;
                                    break;
                                }
                            }

                            if (!bInLst)
                                ptLst.Add(pt);
                        }
                    }
                }

                return ptLst;
            }
            /// <summary>
            /// 计算水平可插入的交线
            /// </summary>
            /// <param name="line"></param>
            /// <param name="loops"></param>
            /// <returns></returns>
            private List<Line> CalculateHoriLinesWithLoops(Line line, List<List<Curve>> loops)
            {
                var ptLst = new List<Point3d>();
                var lines = new List<Line>();

                for (int i = 0; i < loops.Count; i++)
                {
                    ptLst.Clear();
                    var curLoop = loops[i];
                    ptLst = LineWithLoop(line, curLoop);

                    // 跟每个环的交线加入到水平线中去
                    if (ptLst.Count > 1)
                    {
                        // 尖点处理
                        ptLst = ptLst.OrderBy(p => p.X).ToList();

                        for (int index = 0; index < ptLst.Count - 1; index++)
                        {
                            var curPt = ptLst[index];
                            var nextPt = ptLst[index + 1];
                            var midPt = new Point3d((curPt.X + nextPt.X) * 0.5, (curPt.Y + nextPt.Y) * 0.5, 0);

                            if (CommonUtils.PtInLoop(curLoop, midPt))
                                lines.Add(new Line(curPt, nextPt));
                        }
                    }
                }

                lines = lines.OrderBy(p => p.StartPoint.X).ToList();
                return lines;
            }

            /// <summary>
            /// 计算垂直方向的线段集
            /// </summary>
            /// <param name="line"></param>
            /// <param name="loops"></param>
            /// <returns></returns>
            private List<Line> CalculateVerLinesWithLoops(Line line, List<List<Curve>> loops)
            {
                var ptLst = new List<Point3d>();
                var lines = new List<Line>();

                for (int i = 0; i < loops.Count; i++)
                {
                    ptLst.Clear();
                    var curLoop = loops[i];
                    ptLst = LineWithLoop(line, curLoop);

                    // 跟每个环的交线加入到垂直线中去
                    if (ptLst.Count > 1)
                    {
                        // 尖点处理
                        ptLst = ptLst.OrderBy(p => p.Y).ToList();

                        for (int index = 0; index < ptLst.Count - 1; index++)
                        {
                            var curPt = ptLst[index];
                            var nextPt = ptLst[index + 1];
                            var midPt = new Point3d((curPt.X + nextPt.X) * 0.5, (curPt.Y + nextPt.Y) * 0.5, 0);

                            if (CommonUtils.PtInLoop(curLoop, midPt))
                                lines.Add(new Line(curPt, nextPt));
                        }
                    }
                }

                lines = lines.OrderBy(p => p.StartPoint.Y).ToList();
                return lines;
            }

            private void CalcuVHLines()
            {
                double YValue = m_sprayGap;
                // 计算水平分割边

                //var drawCurves = new List<Curve>();
                //foreach (var curves in m_beamLoops)
                //{
                //    drawCurves.AddRange(curves);
                //}
                ////Utils.DrawProfile(drawCurves, "horizon");
                m_hLines = CalculateHoriLinesWithLoops(m_splitBottomEdge, m_beamLoops);
                // 计算垂直分割边
                Line vSplitLine = null;
                var extendValue = 10000000;
                for (int i = 0; i < m_hLines.Count; i++)
                {
                    var ptS = m_hLines[i].StartPoint;
                    var vec = new Vector3d(0, extendValue, 0);
                    var vLine = new Line(ptS - vec, ptS + vec);

                    var roomPtLst = LineWithLoop(vLine, m_roomSrcCurves);
                    if (roomPtLst.Count == 2)
                    {
                        var length = (roomPtLst.First() - roomPtLst.Last()).Length;

                        // 没有大的空洞出现
                        if (CommonUtils.IsAlmostNearZero(Math.Abs(length - m_roomLeft.Length), 600))
                        {
                            vSplitLine = vLine;
                            break;
                        }
                    }
                }

                if (vSplitLine != null)
                {
                    var hVec = new Vector3d(m_offsetAdd, 0, 0);
                    var vSplitS = vSplitLine.StartPoint;
                    var vSplitE = vSplitLine.EndPoint;
                    var vOffLine = new Line(vSplitS + hVec, vSplitE + hVec);
                    //Utils.DrawProfile(new List<Curve> { vOffLine }, "voff");
                    m_vLines = CalculateVerLinesWithLoops(vOffLine, m_beamLoops);
                }
                // 判断水平边可能是否为缺少边
                var splitHoriS = m_splitBottomEdge.StartPoint;
                var splitHoriE = m_splitBottomEdge.EndPoint;
                var vecHori = new Vector3d(extendValue, 0, 0);
                var splitHoriLine = new Line(splitHoriS - vecHori, splitHoriE + vecHori);
                var horiPtLst = LineWithLoop(splitHoriLine, m_roomSrcCurves);
                Line hSplitLine = null;
                if (horiPtLst.Count == 2)
                {
                    var length = (horiPtLst.First() - horiPtLst.Last()).Length;
                    if (!CommonUtils.IsAlmostNearZero(Math.Abs(length - m_roomBottom.Length), 600))
                    {
                        // 左下角有空洞的情形
                        for (int j = 0; j < m_vLines.Count; j++)
                        {
                            var ptS = m_vLines[j].StartPoint;
                            var vec = new Vector3d(extendValue, 0, 0);
                            var horiLine = new Line(ptS - vec, ptS + vec);
                            var hRoomPtLst = LineWithLoop(horiLine, m_roomSrcCurves);
                            if (hRoomPtLst.Count == 2)
                            {
                                var hLength = (hRoomPtLst.First() - hRoomPtLst.Last()).Length;
                                if (CommonUtils.IsAlmostNearZero(Math.Abs(hLength - m_roomBottom.Length), 600))
                                {
                                    hSplitLine = horiLine;
                                    m_hLines = CalculateHoriLinesWithLoops(hSplitLine, m_beamLoops);
                                    break;
                                }
                            }
                        }
                    }
                }

                if (m_hLines.Count == 0 || m_vLines.Count == 0)
                    return;
                //Utils.DrawProfile(m_hLines.ToList<Curve>(), "m_hlines");
                //Utils.DrawProfile(m_vLines.ToList<Curve>(), "m_vLines");
                // 处理至最左边和最下边的位置
                MoveVHLines();
            }

            /// <summary>
            /// 处理至最左边和最下边的位置
            /// </summary>
            private void MoveVHLines()
            {
                var tmpHLines = new List<Line>();
                var curHY = m_hLines.First().StartPoint.Y;
                // 不等 则移动到下面
                if (!CommonUtils.IsAlmostNearZero(Math.Abs(curHY - m_roomBottom.StartPoint.Y - m_wallSprayOffset - m_offsetAdd), 600))
                {
                    var vecV = new Vector3d(0, curHY - m_wallSprayOffset - m_offsetAdd, 0);
                    foreach (var line in m_hLines)
                    {
                        var hPtS = line.StartPoint;
                        var hPtE = line.EndPoint;
                        tmpHLines.Add(new Line(hPtS - vecV, hPtE - vecV));
                    }
                    m_hLines = tmpHLines;
                }

                // 不等 则移动到左边
                var tmpVLines = new List<Line>();
                var curVX = m_vLines.First().StartPoint.X;
                if (!CommonUtils.IsAlmostNearZero(Math.Abs(curVX - m_roomLeft.StartPoint.X - m_wallSprayOffset - m_offsetAdd), 600))
                {
                    var vecH = new Vector3d(curVX - m_wallSprayOffset - m_offsetAdd, 0, 0);
                    foreach (var line in m_vLines)
                    {
                        var vPtS = line.StartPoint;
                        var vPtE = line.EndPoint;
                        tmpVLines.Add(new Line(vPtS - vecH, vPtE - vecH));
                    }

                    m_vLines = tmpVLines;
                }

                //Utils.DrawProfile(m_hLines.ToList<Curve>(), "m_hlines");
                //Utils.DrawProfile(m_vLines.ToList<Curve>(), "m_vLines");
            }

            /// <summary>
            /// 计算水平裁剪
            /// </summary>
            private void CutFromMaxSprayOffsetHLines()
            {
                var leftMaxX = m_roomLeft.StartPoint.X + m_maxWallSprayOffset;
                var rightMaxX = m_roomRight.StartPoint.X - m_maxWallSprayOffset;

                var tmpHLines = new List<Line>();
                if ((m_roomRight.StartPoint.X - m_roomLeft.StartPoint.X) < 2 * m_maxWallSprayOffset)
                {
                    var firstLine = m_hLines.First();
                    var lastLine = m_hLines.Last();
                    var ptFir = firstLine.StartPoint;
                    var ptEnd = lastLine.EndPoint;
                    var midPt = new Point3d((ptFir.X + ptEnd.X) * 0.5, ptFir.Y, 0);
                    var sVLine = new Line(midPt, midPt + new Vector3d(1, 0, 0) * 2);
                    tmpHLines.Add(sVLine);
                    m_hLines.Clear();
                    m_hLines = tmpHLines;
                    return;
                }

                for (int i = 0; i < m_hLines.Count; i++)
                {
                    // 当前
                    var curHLine = m_hLines[i];
                    var curPtS = curHLine.StartPoint;
                    var curPtE = curHLine.EndPoint;
                    if (m_hLines.Count == 1)
                    {
                        // 四种情况处理
                        if (curPtS.X > leftMaxX && curPtE.X < rightMaxX)
                        {
                            var nPtS = new Point3d(leftMaxX, curPtS.Y, 0);
                            var nPtE = curPtE;
                            tmpHLines.Add(new Line(nPtS, nPtE));
                        }
                        else if (curPtS.X > leftMaxX && curPtE.X > rightMaxX)
                        {
                            var nPtS = curPtS;
                            var nPtE = new Point3d(rightMaxX, curPtE.Y, 0);
                            tmpHLines.Add(new Line(nPtS, nPtE));
                        }
                        else if (curPtS.X < leftMaxX && curPtE.X > rightMaxX)
                        {
                            var nPtS = new Point3d(leftMaxX, curPtS.Y, 0);
                            var nPtE = new Point3d(rightMaxX, curPtE.Y, 0);
                            tmpHLines.Add(new Line(nPtS, nPtE));
                        }
                        else
                        {
                            tmpHLines.Add(curHLine);
                        }
                    }
                    else if (i < m_hLines.Count - 1)
                    {
                        // 至倒数第二段的情形
                        // 下一条
                        var nextHLine = m_hLines[i + 1];
                        var nextPtS = nextHLine.StartPoint;
                        var nextPtE = nextHLine.EndPoint;
                        if (curPtS.X < leftMaxX && curPtE.X > leftMaxX)
                        {
                            // 当前与左边相交 收集
                            var nPtS = new Point3d(leftMaxX, curPtS.Y, 0);
                            tmpHLines.Add(new Line(nPtS, curPtE));
                        }
                        else if (curPtE.X < leftMaxX && nextPtS.X > leftMaxX)
                        {
                            // 当前是被包含的最后一个线段 收集
                            var nPtS = curPtE;
                            var nPtE = curPtE + new Vector3d(10, 0, 0);
                            tmpHLines.Add(new Line(nPtS, nPtE));
                        }
                        if (curPtS.X > leftMaxX && curPtE.X < rightMaxX)
                        {
                            // 当前段在左右两条边的中间 收集
                            tmpHLines.Add(curHLine);
                        }
                        if (curPtS.X < rightMaxX && curPtE.X > rightMaxX)
                        {
                            // 当前段与尾段相交 收集
                            var nPtS = curPtS;
                            var nPtE = new Point3d(rightMaxX, curPtS.Y, 0);
                            tmpHLines.Add(new Line(nPtS, nPtE));
                        }
                        if (curPtE.X < rightMaxX && nextPtS.X > rightMaxX)
                        {
                            // 收集next段
                            var nPtS = nextPtS;
                            var nPtE = nextPtS + new Vector3d(2, 0, 0);
                            tmpHLines.Add(new Line(nPtS, nPtE)); // 最后一段检查长度，小于10的只要放置一个点
                        }
                    }
                    else if (i == m_hLines.Count - 1)
                    {
                        // 最后一段的情形
                        if (curPtS.X < rightMaxX && curPtE.X > rightMaxX)
                        {
                            var nPtS = curPtS;
                            var nPtE = new Point3d(rightMaxX, curPtE.Y, 0);
                            tmpHLines.Add(new Line(nPtS, nPtE));
                        }
                        else if (curPtE.X < rightMaxX)
                        {
                            tmpHLines.Add(curHLine);
                        }
                    }
                }

                m_hLines.Clear();
                m_hLines = tmpHLines;
            }

            /// <summary>
            /// 计算垂直裁剪
            /// </summary>
            private void CutFromMaxSprayOffsetVLines()
            {
                var bottomMaxY = m_roomBottom.StartPoint.Y + m_maxWallSprayOffset;
                var topMaxY = m_roomTop.StartPoint.Y - m_maxWallSprayOffset;
                var tmpVLines = new List<Line>();

                if ((m_roomTop.StartPoint.Y - m_roomBottom.StartPoint.Y) < 2 * m_maxWallSprayOffset)
                {
                    var firstLine = m_vLines.First();
                    var lastLine = m_vLines.Last();
                    var ptFir = firstLine.StartPoint;
                    var ptEnd = lastLine.EndPoint;
                    var midPt = new Point3d(ptFir.X, (ptFir.Y + ptEnd.Y) * 0.5, 0);
                    var sVLine = new Line(midPt, midPt + new Vector3d(0, 1, 0) * 2);
                    tmpVLines.Add(sVLine);
                    m_vLines.Clear();
                    m_vLines = tmpVLines;
                    return;
                }
                

                for (int i = 0; i < m_vLines.Count; i++)
                {
                    // 当前
                    var curVLine = m_vLines[i];
                    var curPtS = curVLine.StartPoint;
                    var curPtE = curVLine.EndPoint;
                    if (m_vLines.Count == 1)
                    {
                        // 四种情况处理
                        if (curPtS.Y > bottomMaxY && curPtE.Y < topMaxY)
                        {
                            var nPtS = new Point3d(curPtS.X, bottomMaxY, 0);
                            var nPtE = curPtE;
                            tmpVLines.Add(new Line(nPtS, nPtE));
                        }
                        else if (curPtS.Y > bottomMaxY && curPtE.Y > topMaxY)
                        {
                            var nPtS = curPtS;
                            var nPtE = new Point3d(curPtE.X, topMaxY, 0);
                            tmpVLines.Add(new Line(nPtS, nPtE));
                        }
                        else if (curPtS.Y < bottomMaxY && curPtE.Y > topMaxY)
                        {
                            var nPtS = new Point3d(curPtS.X, bottomMaxY, 0);
                            var nPtE = new Point3d(curPtE.X, topMaxY, 0);
                            tmpVLines.Add(new Line(nPtS, nPtE));
                        }
                        else
                        {
                            tmpVLines.Add(curVLine);
                        }
                    }
                    else if (i < m_vLines.Count - 1)
                    {
                        // 至倒数第二段的情形
                        // 下一条
                        var nextVLine = m_vLines[i + 1];
                        var nextPtS = nextVLine.StartPoint;
                        var nextPtE = nextVLine.EndPoint;
                        if (curPtS.Y < bottomMaxY && curPtE.Y > bottomMaxY)
                        {
                            // 当前与左边相交 收集
                            var nPtS = new Point3d(curPtS.X, bottomMaxY, 0);
                            tmpVLines.Add(new Line(nPtS, curPtE));
                        }
                        else if (curPtE.Y < bottomMaxY && nextPtS.Y > bottomMaxY)
                        {
                            // 当前是被包含的最后一个线段 收集
                            var nPtS = curPtE;
                            var nPtE = curPtE + new Vector3d(0, 10, 0);
                            tmpVLines.Add(new Line(nPtS, nPtE));
                        }
                        if (curPtS.Y > bottomMaxY && curPtE.Y < topMaxY)
                        {
                            // 当前段在左右两条边的中间 收集
                            tmpVLines.Add(curVLine);
                        }
                        if (curPtS.Y < topMaxY && curPtE.Y > topMaxY)
                        {
                            // 当前段与尾段相交 收集
                            var nPtS = curPtS;
                            var nPtE = new Point3d(curPtS.X, topMaxY, 0);
                            tmpVLines.Add(new Line(nPtS, nPtE));
                        }
                        if (curPtE.Y < topMaxY && nextPtS.Y > topMaxY)
                        {
                            // 收集next段
                            var nPtS = nextPtS;
                            var nPtE = nextPtS + new Vector3d(0, 2, 0);
                            tmpVLines.Add(new Line(nPtS, nPtE)); // 最后一段检查长度，小于10的只要放置一个点
                        }
                    }
                    else if (i == m_vLines.Count - 1)
                    {
                        // 最后一段的情形
                        if (curPtS.Y < topMaxY && curPtE.Y > topMaxY)
                        {
                            var nPtS = curPtS;
                            var nPtE = new Point3d(curPtS.X, topMaxY, 0);
                            tmpVLines.Add(new Line(nPtS, nPtE));
                        }
                        else if (curPtE.Y < topMaxY)
                        {
                            tmpVLines.Add(curVLine);
                        }
                    }
                }

                m_vLines.Clear();
                m_vLines = tmpVLines;
            }

            /// <summary>
            /// 四周切掉最大距墙值
            /// </summary>
            private void CutFromMaxSprayOffset()
            {
                CutFromMaxSprayOffsetHLines();
                CutFromMaxSprayOffsetVLines();
                //Utils.DrawProfile(m_hLines.ToList<Curve>(), "m_hlines");
                //Utils.DrawProfile(m_vLines.ToList<Curve>(), "m_vLines");
            }

            /// <summary>
            /// 计算水平第一行
            /// </summary>
            /// <returns></returns>
            private List<Point3d> CalcuHPoints()
            {
                var hPtLst = new List<Point3d>();
                for (int i = 0; i < m_hLines.Count; i++)
                {
                    var curHLine = m_hLines[i];
                    var firPtS = curHLine.StartPoint;
                    var firPtE = curHLine.EndPoint;
                    if (i == m_hLines.Count - 1)
                    {
                        // 最后一段
                        if (curHLine.Length < 10)
                        {
                            hPtLst.Add(new Point3d(firPtS.X, firPtS.Y, 0));
                        }
                        else
                        {
                            var xWid = curHLine.Length;
                            var ratio = xWid / m_sprayGap;
                            int nCount = (int)ratio;

                            // 多出一点点的情形，不是少于的情形
                            if (CommonUtils.IsAlmostNearZero(Math.Abs(ratio - nCount), 1e-4))
                                nCount -= 1;

                            if (nCount == 0)
                            {
                                hPtLst.Add(new Point3d(firPtS.X, firPtS.Y, 0));
                            }
                            else
                            {
                                nCount++;
                                var midStep = (int)(xWid / nCount);
                                var gapAdd = (midStep / 50) * 50;

                                for (int j = 0; j < nCount; j++)
                                {
                                    var curX = firPtS.X + j * gapAdd;
                                    hPtLst.Add(new Point3d(curX, firPtS.Y, 0));
                                }
                            }

                            hPtLst.Add(new Point3d(firPtE.X, firPtE.Y, 0));
                        }
                    }
                    else
                    {
                        // 非最后一段
                        var nextLine = m_hLines[i + 1];
                        var nextPtS = nextLine.StartPoint;
                        var xWid = nextPtS.X - firPtS.X;
                        var ratio = xWid / m_sprayGap;
                        int nCount = (int)ratio;

                        // 多出一点点的情形，不是少于的情形
                        if (CommonUtils.IsAlmostNearZero(Math.Abs(ratio - nCount), 1e-4))
                            nCount -= 1;

                        if (nCount == 0)
                        {
                            hPtLst.Add(new Point3d(firPtS.X, firPtS.Y, 0));
                        }
                        else
                        {
                            nCount++;
                            var midStep = (int)(xWid / nCount);
                            var gapAdd = (midStep / 50) * 50;

                            for (int j = 0; j < nCount; j++)
                            {
                                var curX = firPtS.X + j * gapAdd;
                                hPtLst.Add(new Point3d(curX, firPtS.Y, 0));
                            }
                        }
                    }
                }

                if (hPtLst.Count < 3)
                    return hPtLst;

                var resPtLst = new List<Point3d>();
                resPtLst.Add(hPtLst.First());
                for (int i = 1; i < hPtLst.Count - 1; i++)
                {
                    var beforePt = hPtLst[i - 1];
                    var nextPt = hPtLst[i + 1];
                    var curSpt = hPtLst[i];
                    var midPt = new Point3d((beforePt.X + nextPt.X) * 0.5, (beforePt.Y + nextPt.Y) * 0.5, 0);
                    bool bPointIn = false;
                    foreach (var line in m_hLines)
                    {
                        var ptSX = line.StartPoint.X;
                        var ptEX = line.EndPoint.X;
                        if (midPt.X >= ptSX && midPt.X <= ptEX)
                        {
                            bPointIn = true;
                            break;
                        }
                    }
                    if (bPointIn)
                    {
                        hPtLst[i] = midPt;
                        resPtLst.Add(midPt);
                    }
                    else
                    {
                        resPtLst.Add(curSpt);
                    }
                }

                resPtLst.Add(hPtLst.Last());

                // 水平坐标点规整
                resPtLst = NormalizeHPoints(resPtLst, m_hLines);
                return resPtLst;
            }

            /// <summary>
            /// 归一化水平点
            /// </summary>
            /// <param name="hPtLst"></param>
            /// <param name="range"></param>
            /// <returns></returns>
            private List<Point3d> NormalizeHPoints(List<Point3d> hPtLst, List<Line> rangeLines)
            {
                var resPtLst = new List<Point3d>();
                resPtLst.Add(hPtLst.First());
                for (int i = 1; i < hPtLst.Count - 1; i++)
                {
                    var beforePt = hPtLst[i - 1];
                    var curSpt = hPtLst[i];

                    // 当前点的左右两个点
                    int nRatio = (int)((curSpt.X - beforePt.X) / 50.0);
                    int nNext = nRatio + 1;

                    var ratioPoint = new Point3d(beforePt.X + nRatio * 50, curSpt.Y, 0);
                    var nextPt = new Point3d(beforePt.X + nNext * 50, curSpt.Y, 0);
                    Point3d midPt = Point3d.Origin;

                    bool bPointIn = false;
                    foreach (var line in m_hLines)
                    {
                        var ptSX = line.StartPoint.X;
                        var ptEX = line.EndPoint.X;

                        if ((ratioPoint.X >= ptSX && ratioPoint.X <= ptEX))
                        {
                            bPointIn = true;
                            midPt = ratioPoint;
                            break;
                        }
                        else if ((nextPt.X >= ptSX && nextPt.X <= ptEX))
                        {
                            bPointIn = true;
                            midPt = nextPt;
                            break;
                        }
                    }
                    if (bPointIn)
                    {
                        hPtLst[i] = midPt;
                        resPtLst.Add(midPt);
                    }
                    else
                    {
                        resPtLst.Add(curSpt);
                    }
                }

                resPtLst.Add(hPtLst.Last());
                return resPtLst;
            }

            private List<Point3d> NormalizeVPoints(List<Point3d> vPtLst, List<Line> rangeLines)
            {
                var resPtLst = new List<Point3d>();
                resPtLst.Add(vPtLst.First());
                for (int i = 1; i < vPtLst.Count - 1; i++)
                {
                    var beforePt = vPtLst[i - 1];
                    var curSpt = vPtLst[i];

                    // 当前点的上下两个点
                    int nRatio = (int)((curSpt.Y - beforePt.Y) / 50.0);
                    int nNext = nRatio + 1;

                    var ratioPoint = new Point3d(beforePt.X, beforePt.Y + nRatio * 50, 0);
                    var nextPt = new Point3d(beforePt.X, beforePt.Y + nNext * 50, 0);
                    Point3d midPt = Point3d.Origin;

                    bool bPointIn = false;
                    foreach (var line in m_vLines)
                    {
                        var ptSY = line.StartPoint.Y;
                        var ptEY = line.EndPoint.Y;

                        if ((ratioPoint.Y >= ptSY && ratioPoint.Y <= ptEY))
                        {
                            bPointIn = true;
                            midPt = ratioPoint;
                            break;
                        }
                        else if ((nextPt.Y >= ptSY && nextPt.Y <= ptEY))
                        {
                            bPointIn = true;
                            midPt = nextPt;
                            break;
                        }
                    }
                    if (bPointIn)
                    {
                        vPtLst[i] = midPt;
                        resPtLst.Add(midPt);
                    }
                    else
                    {
                        resPtLst.Add(curSpt);
                    }
                }

                resPtLst.Add(vPtLst.Last());
                return resPtLst;
            }

            /// <summary>
            /// 计算垂直的点
            /// </summary>
            /// <returns></returns>
            private List<Point3d> CalcuVPoints()
            {
                var vPtLst = new List<Point3d>();
                for (int i = 0; i < m_vLines.Count; i++)
                {
                    var curVLine = m_vLines[i];
                    var firPtS = curVLine.StartPoint;
                    var firPtE = curVLine.EndPoint;
                    if (i == m_vLines.Count - 1)
                    {
                        // 最后一段
                        if (curVLine.Length < 10)
                        {
                            vPtLst.Add(new Point3d(firPtS.X, firPtS.Y, 0));
                        }
                        else
                        {
                            var yWid = curVLine.Length;
                            var ratio = yWid / m_sprayGap;
                            int nCount = (int)ratio;

                            // 多出一点点的情形，不是少于的情形
                            if (CommonUtils.IsAlmostNearZero(Math.Abs(ratio - nCount), 1e-4))
                                nCount -= 1;

                            if (nCount == 0)
                            {
                                vPtLst.Add(new Point3d(firPtS.X, firPtS.Y, 0));
                            }
                            else
                            {
                                nCount++;
                                var midStep = (int)(yWid / nCount);
                                var gapAdd = (midStep / 50) * 50;

                                for (int j = 0; j < nCount; j++)
                                {
                                    var curY = firPtS.Y + j * gapAdd;
                                    vPtLst.Add(new Point3d(firPtS.X, curY, 0));
                                }
                            }

                            vPtLst.Add(new Point3d(firPtE.X, firPtE.Y, 0));
                        }
                    }
                    else
                    {
                        // 非最后一段
                        var nextLine = m_vLines[i + 1];
                        var nextPtS = nextLine.StartPoint;
                        var yWid = nextPtS.Y - firPtS.Y;
                        var ratio = yWid / m_sprayGap;
                        int nCount = (int)ratio;

                        // 多出一点点的情形，不是少于的情形
                        if (CommonUtils.IsAlmostNearZero(Math.Abs(ratio - nCount), 1e-4))
                            nCount -= 1;

                        if (nCount == 0)
                        {
                            vPtLst.Add(new Point3d(firPtS.X, firPtS.Y, 0));
                        }
                        else
                        {
                            nCount++;
                            var midStep = (int)(yWid / nCount);
                            var gapAdd = (midStep / 50) * 50;

                            for (int j = 0; j < nCount; j++)
                            {
                                var curY = firPtS.Y + j * gapAdd;
                                vPtLst.Add(new Point3d(firPtS.X, curY, 0));
                            }
                        }
                    }
                }

                if (vPtLst.Count < 3)
                    return vPtLst;

                var resPtLst = new List<Point3d>();
                resPtLst.Add(vPtLst.First());
                for (int i = 1; i < vPtLst.Count - 1; i++)
                {
                    var beforePt = vPtLst[i - 1];
                    var nextPt = vPtLst[i + 1];
                    var curSpt = vPtLst[i];
                    var midPt = new Point3d((beforePt.X + nextPt.X) * 0.5, (beforePt.Y + nextPt.Y) * 0.5, 0);
                    bool bPointIn = false;
                    foreach (var line in m_vLines)
                    {
                        if (CommonUtils.IsPointOnLine(midPt, line))
                        {
                            bPointIn = true;
                            break;
                        }
                    }
                    if (bPointIn)
                    {
                        vPtLst[i] = midPt;
                        resPtLst.Add(midPt);
                    }
                    else
                    {
                        resPtLst.Add(curSpt);
                    }
                }
                resPtLst.Add(vPtLst.Last());
                // 垂直方向规整
                resPtLst = NormalizeVPoints(resPtLst, m_vLines);
                return resPtLst;
            }

            /// <summary>
            /// 开始布置
            /// </summary>
            private List<List<Point3d>> DoPlace()
            {
                var hPtLst = CalcuHPoints(); // 水平分割点
                var vPtLst = CalcuVPoints(); // 垂直分割点
                var dbCol = new DBObjectCollection();

                var curPtY = hPtLst.First().Y;
                var ptsLst = new List<List<Point3d>>();

                foreach (var vPt in vPtLst)
                {
                    var vY = vPt.Y;
                    var moveValue = vY - curPtY;
                    var movePtLst = new List<Point3d>();
                    if (CommonUtils.IsAlmostNearZero(Math.Abs(moveValue), 10))
                    {
                        ptsLst.Add(hPtLst);
                    }
                    else
                    {
                        foreach (var hPt in hPtLst)
                        {
                            var movePt = hPt + new Vector3d(0, moveValue, 0);
                            movePtLst.Add(movePt);
                        }
                        ptsLst.Add(movePtLst);
                    }
                }

                // 有效的放置点
                var validPtsLst = ErasePoints(m_roomSrcCurves, ptsLst);
                //foreach (var drawPts in validPtsLst)
                //    foreach (var drawPt in drawPts)
                //        Utils.DrawPreviewPoint(new DBObjectCollection(), drawPt);
                return validPtsLst;
            }

            /// <summary>
            /// 删除无效点
            /// </summary>
            /// <param name="srcCurves"></param>
            /// <param name="ptsLst"></param>
            /// <returns></returns>
            private List<List<Point3d>> ErasePoints(List<Curve> srcCurves, List<List<Point3d>> ptsLst)
            {
                var validPtsLst = new List<List<Point3d>>();

                foreach (var pts in ptsLst)
                {
                    var validPtLst = new List<Point3d>();
                    foreach (var pt in pts)
                    {
                        if (CommonUtils.PtInLoop(srcCurves, pt))
                            validPtLst.Add(pt);
                    }

                    validPtsLst.Add(validPtLst);
                }

                return validPtsLst;
            }
        }

        /// <summary>
        /// 计算多段线的垂直开始边和水平开始边
        /// </summary>
        public class EdgeCalcu
        {
            private List<Curve> m_srcCurves;
            private double m_offset = 0.0;

            public Line LeftEdge;
            public Line BottomEdge;
            public Line SrcRightEdge;
            public Line SrcTopEdge;

            public Line SrcLeftEdge;
            public Line SrcBottomEdge;

            public List<Line> BoundCurves;
            public EdgeCalcu(List<Curve> curves, double offset)
            {
                m_srcCurves = curves;
                m_offset = offset;
            }

            public void Do()
            {
                var box = new BoundBoxPlane(m_srcCurves);

                // 左边
                var leftEdge = box.CalculateLeftEdge();
                var leftPtS = leftEdge.StartPoint;
                var leftPtE = leftEdge.EndPoint;
                LeftEdge = new Line(leftPtS + new Vector3d(m_offset, 0, 0), leftPtE + new Vector3d(m_offset, 0, 0));

                // 底边
                var bottomEdge = box.CalculateBottomEdge();
                var bottomPtS = bottomEdge.StartPoint;
                var bottomPtE = bottomEdge.EndPoint;
                BottomEdge = new Line(bottomPtS + new Vector3d(0, m_offset, 0), bottomPtE + new Vector3d(0, m_offset, 0));

                // 房间的右边
                SrcRightEdge = box.CalculateRightEdge();
                // 房间的上边
                SrcTopEdge = box.CalculateTopEdge();

                // 房间的左边
                SrcLeftEdge = box.CalculateLeftEdge();

                // 房间的下边
                SrcBottomEdge = box.CalculateBottomEdge();
                // 房间的边界原始curves
                BoundCurves = box.CalcuBoundCurves();
            }
        }

        /// <summary>
        /// 梁数据生成
        /// </summary>
        public class BeamPath
        {
            // 梁节点，端点度数计算
            public class BeamNode
            {
                public Curve srcCurve = null;
                public bool bVlalid = true;
                public Point3d StartPoint; // 开始点
                public Point3d EndPoint;   // 结束点
                public BeamNode(Curve curve, bool valid = true)
                {
                    srcCurve = curve;
                    bVlalid = valid;
                    StartPoint = srcCurve.StartPoint;
                    EndPoint = srcCurve.EndPoint;
                }

                public List<BeamNode> startNodes = new List<BeamNode>();
                public List<BeamNode> endNodes = new List<BeamNode>();
                public List<BeamNode> totalNodes = new List<BeamNode>();
            }

            private List<Curve> srcBeams = null;
            private List<BeamNode> beamNodes = new List<BeamNode>();

            private List<BeamNode> startBeamNodes = new List<BeamNode>(); // 度数为1的开始点

            public List<List<BeamNode>> unclosedBeamNodes = new List<List<BeamNode>>(); // 开环路径
            public List<List<BeamNode>> closedBeamNodes = new List<List<BeamNode>>(); // 闭环路径

            public BeamPath(List<Curve> curves)
            {
                srcBeams = curves;
            }

            public static List<Curve> MakeBeamLoop(List<Curve> curves, List<Curve> roomCurves)
            {
                var pathCal = new BeamPath(curves);
                pathCal.Do();

                var unclosedCurves = pathCal.TransCurves(pathCal.unclosedBeamNodes);
                // 区域分割生成所需要的内部点
                var ptLst = SpaceSplit.MakeSplitPoints(unclosedCurves);
                if (unclosedCurves != null && unclosedCurves.Count != 0)
                {
                    foreach (var poly in unclosedCurves)
                    {
                        if (poly is Polyline)
                        {
                            var polyCurves = TopoUtils.Polyline2dLines(poly as Polyline);
                            if (polyCurves != null && polyCurves.Count != 0)
                                roomCurves.AddRange(polyCurves);
                        }
                    }
                }

                var polylines = new List<Curve>();
                var closedCurves = pathCal.TransClosedCurves(pathCal.closedBeamNodes);

                if (closedCurves != null && closedCurves.Count != 0)
                    polylines.AddRange(closedCurves);

                foreach (var beamPoint in ptLst)
                {
                    var profile = TopoUtils.MakeProfileFromPoint(roomCurves, beamPoint);
                    if (profile != null && profile.Count != 0)
                    {
                        polylines.AddRange(profile);
                    }
                }

                return polylines;
            }

            public void Do()
            {
                CalRelation();
                SearchFromBeamNodes();

                for (int i = 0; i < beamNodes.Count; i++)
                {
                    var curBeamNode = beamNodes[i];
                    if (curBeamNode.bVlalid == false)
                        continue;

                    var path = new List<BeamNode>();
                    SearchFromOneNode(curBeamNode, path, closedBeamNodes);
                }
            }

            /// <summary>
            /// 转闭合多段线
            /// </summary>
            /// <param name="beamNodes"></param>
            /// <param name="bClosed"></param>
            /// <returns></returns>
            public List<Curve> TransClosedCurves(List<List<BeamNode>> beamNodes)
            {
                if (beamNodes == null || beamNodes.Count == 0)
                    return null;

                var polys = new List<Curve>();

                for (int i = 0; i < beamNodes.Count; i++)
                {
                    var curves = new List<Curve>();
                    var nodes = beamNodes[i];
                    if (nodes.Count < 2)
                        continue;

                    var poly = new Polyline();
                    poly.Closed = true;
                    Point3d endPoint = new Point3d(0, 0, 0);
                    Point3d startPoint = new Point3d(0, 0, 0);
                    for (int j = 0; j < nodes.Count; j++)
                    {
                        var node = nodes[j];
                        var curve = node.srcCurve;
                        var curPtS = node.StartPoint;
                        var curPtE = node.EndPoint;
                        bool positive = true; // 正数
                        if (j == 0)
                        {
                            var nextNode = nodes[j + 1];
                            var nextPtS = nextNode.StartPoint;
                            var nextPtE = nextNode.EndPoint;

                            if (CommonUtils.Point3dIsEqualPoint3d(curPtE, nextPtS, 1E-3) || CommonUtils.Point3dIsEqualPoint3d(curPtE, nextPtE, 1e-3))
                            {
                                startPoint = curPtS;
                                endPoint = curPtE;
                            }
                            else if (CommonUtils.Point3dIsEqualPoint3d(curPtS, nextPtS, 1E-3) || CommonUtils.Point3dIsEqualPoint3d(curPtS, nextPtE, 1e-3))
                            {
                                startPoint = curPtE;
                                endPoint = curPtS;
                                positive = false;
                            }

                            if (curve is Line)
                            {
                                poly.AddVertexAt(j, new Point2d(startPoint.X, startPoint.Y), 0, 0, 0);
                            }
                            else if (curve is Arc)
                            {
                                var arc = curve as Arc;
                                var bulge = Math.Tan(arc.TotalAngle / 4.0);
                                if (positive == false)
                                    bulge *= -1;
                                poly.AddVertexAt(j, new Point2d(startPoint.X, startPoint.Y), bulge, 0, 0);
                            }
                        }
                        else
                        {
                            if (CommonUtils.Point3dIsEqualPoint3d(endPoint, node.StartPoint, 1e-3))
                            {
                                startPoint = node.StartPoint;
                                endPoint = node.EndPoint;
                            }
                            else if (CommonUtils.Point3dIsEqualPoint3d(endPoint, node.EndPoint, 1e-3))
                            {
                                startPoint = node.EndPoint;
                                endPoint = node.StartPoint;
                                positive = false;
                            }

                            if (curve is Line)
                                poly.AddVertexAt(j, new Point2d(startPoint.X, startPoint.Y), 0, 0, 0);
                            else if (curve is Arc)
                            {
                                var arc = curve as Arc;
                                var bulge = Math.Tan(arc.TotalAngle / 4.0);
                                if (positive == false)
                                    bulge *= -1;
                                poly.AddVertexAt(j, new Point2d(startPoint.X, startPoint.Y), bulge, 0, 0);
                            }
                        }
                    }
                    polys.Add(poly);
                }

                return polys;
            }


            /// <summary>
            /// 转化非闭合多段线
            /// </summary>
            /// <param name="beamNodes"></param>
            /// <returns></returns>
            public List<Curve> TransCurves(List<List<BeamNode>> beamNodes)
            {
                if (beamNodes == null || beamNodes.Count == 0)
                    return null;

                var polys = new List<Curve>();

                for (int i = 0; i < beamNodes.Count; i++)
                {
                    var curves = new List<Curve>();
                    var nodes = beamNodes[i];
                    if (nodes.Count < 2)
                        continue;

                    var poly = new Polyline();
                    poly.Closed = false;
                    Point3d endPoint = new Point3d(0, 0, 0);
                    Point3d startPoint = new Point3d(0, 0, 0);
                    for (int j = 0; j < nodes.Count; j++)
                    {
                        var node = nodes[j];
                        var curve = node.srcCurve;
                        if (j == 0)
                        {
                            if (node.startNodes.Count == 0)
                            {
                                // 尾点连接下一条边，则不需要反向
                                startPoint = node.StartPoint;
                                endPoint = node.EndPoint;
                                if (curve is Line)
                                    poly.AddVertexAt(j, new Point2d(startPoint.X, startPoint.Y), 0, 0, 0);
                                else if (curve is Arc)
                                {
                                    // 逆时针圆弧
                                    var arc = curve as Arc;
                                    var bulge = Math.Tan(arc.TotalAngle / 4.0);
                                    poly.AddVertexAt(j, new Point2d(startPoint.X, startPoint.Y), bulge, 0, 0);
                                }
                            }
                            else if (node.endNodes.Count == 0)
                            {
                                // 原始曲线的开始点连接下一条边，则需要反向
                                startPoint = node.EndPoint;
                                endPoint = node.StartPoint;
                                if (curve is Line)
                                    poly.AddVertexAt(j, new Point2d(startPoint.X, startPoint.Y), 0, 0, 0);
                                else if (curve is Arc)
                                {
                                    var arc = curve as Arc;
                                    var bulge = -Math.Tan(arc.TotalAngle / 4.0);
                                    poly.AddVertexAt(j, new Point2d(startPoint.X, startPoint.Y), bulge, 0, 0);
                                }
                            }
                        }
                        else
                        {
                            bool positive = true; // 负数
                            if (CommonUtils.Point3dIsEqualPoint3d(endPoint, node.StartPoint, 1e-3))
                            {
                                startPoint = node.StartPoint;
                                endPoint = node.EndPoint;
                            }
                            else if (CommonUtils.Point3dIsEqualPoint3d(endPoint, node.EndPoint, 1e-3))
                            {
                                startPoint = node.EndPoint;
                                endPoint = node.StartPoint;
                                positive = false;
                            }

                            if (curve is Line)
                                poly.AddVertexAt(j, new Point2d(startPoint.X, startPoint.Y), 0, 0, 0);
                            else if (curve is Arc)
                            {
                                var arc = curve as Arc;
                                var bulge = Math.Tan(arc.TotalAngle / 4.0);
                                if (positive == false)
                                    bulge *= -1;
                                poly.AddVertexAt(j, new Point2d(startPoint.X, startPoint.Y), bulge, 0, 0);
                            }

                            if (j == nodes.Count - 1)
                            {
                                poly.AddVertexAt(j + 1, new Point2d(endPoint.X, endPoint.Y), 0, 0, 0);
                            }
                        }
                    }
                    polys.Add(poly);
                }

                return polys;
            }

            public void CalRelation()
            {
                foreach (var srcBeam in srcBeams)
                {
                    beamNodes.Add(new BeamNode(srcBeam));
                }

                // 节点的度
                for (int i = 0; i < beamNodes.Count; i++)
                {
                    var curBeamNode = beamNodes[i];
                    var firPtS = curBeamNode.StartPoint;
                    var firPtE = curBeamNode.EndPoint;

                    for (int j = i + 1; j < beamNodes.Count; j++)
                    {
                        var nextBeamNode = beamNodes[j];
                        var secPtS = nextBeamNode.StartPoint;
                        var secPtE = nextBeamNode.EndPoint;

                        if (CommonUtils.Point3dIsEqualPoint3d(firPtS, secPtS, 1E-3))
                        {
                            curBeamNode.startNodes.Add(nextBeamNode);
                            nextBeamNode.startNodes.Add(curBeamNode);
                        }
                        else if (CommonUtils.Point3dIsEqualPoint3d(firPtS, secPtE, 1e-3))
                        {
                            curBeamNode.startNodes.Add(nextBeamNode);
                            nextBeamNode.endNodes.Add(curBeamNode);
                        }
                        else if (CommonUtils.Point3dIsEqualPoint3d(firPtE, secPtS, 1e-3))
                        {
                            curBeamNode.endNodes.Add(nextBeamNode);
                            nextBeamNode.startNodes.Add(curBeamNode);
                        }
                        else if (CommonUtils.Point3dIsEqualPoint3d(firPtE, secPtE, 1e-3))
                        {
                            curBeamNode.endNodes.Add(nextBeamNode);
                            nextBeamNode.endNodes.Add(curBeamNode);
                        }
                    }

                    // 度数为1 的点
                    if (curBeamNode.startNodes.Count == 0 || curBeamNode.endNodes.Count == 0)
                    {
                        startBeamNodes.Add(curBeamNode);
                    }

                    if (curBeamNode.startNodes.Count != 0)
                        curBeamNode.totalNodes.AddRange(curBeamNode.startNodes);
                    if (curBeamNode.endNodes.Count != 0)
                        curBeamNode.totalNodes.AddRange(curBeamNode.endNodes);
                }
            }


            private void SearchFromBeamNodes()
            {
                // 从度数为1的点开始搜索
                for (int i = 0; i < startBeamNodes.Count; i++)
                {
                    var curBeamNode = startBeamNodes[i];
                    if (curBeamNode.bVlalid == false)
                        continue;

                    var path = new List<BeamNode>();
                    SearchFromOneNode(curBeamNode, path, unclosedBeamNodes);
                }
            }

            private bool IsEndNode(BeamNode node)
            {
                foreach (var viewNode in node.totalNodes)
                {
                    if (viewNode.bVlalid)
                        return false;
                }

                return true;
            }

            private void SearchFromOneNode(BeamNode curNode, List<BeamNode> path, List<List<BeamNode>> searchPath)
            {
                if (IsEndNode(curNode))
                {
                    path.Add(curNode);
                    curNode.bVlalid = false;
                    searchPath.Add(path);
                    return;
                }

                curNode.bVlalid = false;
                path.Add(curNode);

                foreach (var node in curNode.totalNodes)
                {
                    if (node.bVlalid == false)
                        continue;

                    var resPath = new List<BeamNode>();
                    resPath.AddRange(path);

                    SearchFromOneNode(node, resPath, searchPath);
                }
            }
        }

        public static List<Curve> GetValidBeams(List<Curve> beams, List<Curve> roomCurves)
        {
            if (beams == null || beams.Count == 0 || roomCurves == null || roomCurves.Count == 0)
                return null;

            var relatedCurves = new List<Curve>();
            var intersectCurves = new List<Curve>();
            foreach (var beam in beams)
            {
                if (LoopContainCurve(roomCurves, beam))
                    relatedCurves.Add(beam);
                else if (CurveIntersectWithLoop(beam, roomCurves))
                {
                    intersectCurves.Add(beam);
                }
            }

            intersectCurves.AddRange(roomCurves);
            // 裁剪掉框线外的曲线
            var newCurves = ScatterCurves.MakeNewCurves(intersectCurves);

            if (newCurves != null)
            {
                foreach (var curve in newCurves)
                {
                    var ptMid = curve.GetPointAtParameter((curve.StartParam + curve.EndParam) * 0.5);
                    if (CommonUtils.PtInLoop(roomCurves, ptMid))
                        relatedCurves.Add(curve);
                }
            }

            return relatedCurves;
        }


        public static bool CurveIntersectWithLoop(Curve srcCurve, List<Curve> loop)
        {
            foreach (var curve in loop)
            {
                if (!CommonUtils.IntersectValid(srcCurve, curve))
                    continue;

                var ptLst = new Point3dCollection();
                srcCurve.IntersectWith(curve, Intersect.OnBothOperands, ptLst, (IntPtr)0, (IntPtr)0);
                if (ptLst.Count != 0)
                {
                    return true;
                }
            }

            return false;
        }


        public static bool CurveIntersectWithLoop(Curve srcCurve, List<Curve> loop, out Point3d intersectPt, out Curve intersectCurve)
        {
            foreach (var curve in loop)
            {
                if (!CommonUtils.IntersectValid(srcCurve, curve))
                    continue;

                var ptLst = new Point3dCollection();
                srcCurve.IntersectWith(curve, Intersect.OnBothOperands, ptLst, (IntPtr)0, (IntPtr)0);
                if (ptLst.Count != 0)
                {
                    intersectPt = ptLst[0];
                    intersectCurve = curve;
                    return true;
                }
            }

            intersectPt = Point3d.Origin;
            intersectCurve = null;
            return false;
        }
        public static bool LoopContainCurve(List<Curve> loop, Curve curve)
        {
            if (CommonUtils.PtInLoop(loop, curve.StartPoint) && CommonUtils.PtInLoop(loop, curve.EndPoint))
                return true;
            else
                return false;
        }

        public static List<Curve> Line2d2Curve(List<LineSegment2d> line2ds)
        {
            var curves = new List<Curve>();
            foreach (var line2d in line2ds)
            {
                var ptS = line2d.StartPoint;
                var ptE = line2d.EndPoint;
                var pt3S = new Point3d(ptS.X, ptS.Y, 0);
                var pt3E = new Point3d(ptE.X, ptE.Y, 0);
                var line = new Line(pt3S, pt3E);
                curves.Add(line);
            }

            return curves;
        }

        public static void CreateGroup(List<List<TopoEdge>> topoEdges, string groupName, string showName)
        {

            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            Utils.CreateLayer(showName, Color.FromRgb(255, 0, 0));

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                // Get the group dictionary from the drawing
                DBDictionary gd = (DBDictionary)tr.GetObject(db.GroupDictionaryId, OpenMode.ForRead);

                foreach (var loop in topoEdges)
                {
                    var loopcurves = new List<Curve>();
                    foreach (var edge in loop)
                    {
                        loopcurves.Add(edge.SrcCurve);
                    }

                    //Random random = new Random();
                    //int value = random.Next(100000000);
                    Utils.INDEX++;
                    string grpName = groupName + Utils.INDEX;
                    //do
                    //{
                    //    inputGrpName = groupName + value;
                    //    try
                    //    {
                    //        if (gd.Contains(inputGrpName))
                    //            ed.WriteMessage("\nA group with this name already exists.");
                    //        else
                    //            grpName = inputGrpName;
                    //    }
                    //    catch
                    //    {
                    //        ed.WriteMessage("\nInvalid group name.");
                    //    }
                    //} while (grpName == "");

                    // Create our new group...
                    Group grp = new Group("Profile group", true);
                    var color = Color.FromRgb(255, 255, 0);
                    grp.SetColor(color);
                    // Add the new group to the dictionary
                    gd.UpgradeOpen();
                    ObjectId grpId = gd.SetAt(grpName, grp);
                    tr.AddNewlyCreatedDBObject(grp, true);
                    // Open the model-space
                    BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                    BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                    // Add some lines to the group to form a square
                    // (the entities belong to the model-space)

                    ObjectIdCollection ids = new ObjectIdCollection();
                    foreach (Entity ent in loopcurves)
                    {
                        ent.Layer = showName;
                        ObjectId id = ms.AppendEntity(ent);
                        ids.Add(id);
                        tr.AddNewlyCreatedDBObject(ent, true);
                    }

                    grp.InsertAt(0, ids);
                }

                tr.Commit();
            }
        }

        public static void DrawPreviewPoint(DBObjectCollection objCol, Point3d pt, double length = 500)
        {
            var dbCollection = new DBObjectCollection();
            var half = length * 0.5;
            var curveFirStart = pt + new Vector3d(1, 1, 0).GetNormal() * half;
            var curveFirEnd = curveFirStart - new Vector3d(1, 1, 0).GetNormal() * length;
            var curFir = new Line(curveFirStart, curveFirEnd);
            objCol.Add(curFir);

            var curveSecStart = pt + new Vector3d(-1, 1, 0).GetNormal() * half;
            var curveSecEnd = curveSecStart - new Vector3d(-1, 1, 0).GetNormal() * length;
            var curveSec = new Line(curveSecStart, curveSecEnd);
            objCol.Add(curveSec);
            IntegerCollection intCol = new IntegerCollection();
            using (AcadDatabase acad = AcadDatabase.Active())
            {
                Autodesk.AutoCAD.GraphicsInterface.TransientManager tm = Autodesk.AutoCAD.GraphicsInterface.TransientManager.CurrentTransientManager;
                tm.AddTransient(curFir, Autodesk.AutoCAD.GraphicsInterface.TransientDrawingMode.DirectTopmost, 128, intCol);
                tm.AddTransient(curveSec, Autodesk.AutoCAD.GraphicsInterface.TransientDrawingMode.DirectTopmost, 128, intCol);
            }
        }

        public static void ErasePreviewPoint(DBObjectCollection dbCol)
        {
            using (AcadDatabase acad = AcadDatabase.Active())
            {
                Autodesk.AutoCAD.GraphicsInterface.TransientManager tm = Autodesk.AutoCAD.GraphicsInterface.TransientManager.CurrentTransientManager;
                foreach (DBObject obj in dbCol)
                {
                    tm.EraseTransient(obj, new IntegerCollection());
                    obj.Dispose();
                }
            }
        }

        /// <summary>
        /// 创建新的图层
        /// </summary>
        /// <param name="allLayers"></param>
        /// <param name="aimLayer"></param>
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
        /// 显示
        /// </summary>
        /// <param name="curvesLst"></param>
        public static void ShowCurves(List<List<Curve>> curvesLst)
        {
            var polylines = Convert2Polyline(curvesLst);
            DisplayBoundaryProfile(polylines, "polyline");
        }

        /// <summary>
        /// 界面显示
        /// </summary>
        /// <param name="roomDataPolylines"></param>
        /// <param name="layerName"></param>
        public static void DisplayBoundaryProfile(List<Polyline> boundaryPolylines, string boundaryLayerName)
        {
            if (boundaryPolylines == null || boundaryPolylines.Count == 0)
                return;

            using (var db = AcadDatabase.Active())
            {
                CreateLayer(boundaryLayerName, Color.FromRgb(255, 0, 0));

                foreach (var polyline in boundaryPolylines)
                {
                    var objectPolylineId = db.ModelSpace.Add(polyline);
                    db.ModelSpace.Element(objectPolylineId, true).Layer = boundaryLayerName;
                }
            }
        }

        /// <summary>
        /// 界面显示
        /// </summary>
        /// <param name="roomDataPolylines"></param>
        /// <param name="layerName"></param>
        public static void DrawProfile(List<Curve> curves, string LayerName, Color color = null)
        {
            if (curves == null || curves.Count == 0)
                return;

            using (var db = AcadDatabase.Active())
            {
                if (color == null)
                    CreateLayer(LayerName, Color.FromRgb(255, 0, 0));
                else
                    CreateLayer(LayerName, color);

                foreach (var curve in curves)
                {
                    var objectCurveId = db.ModelSpace.Add(curve.Clone() as Curve);
                    db.ModelSpace.Element(objectCurveId, true).Layer = LayerName;
                }
            }
        }

        // 绘图，用于测试点的位置
        public static void DrawLinesWithTransaction(List<TopoEdge> edges, string layerName = null)
        {
            if (edges == null || edges.Count == 0)
                return;
            using (AcadDatabase acad = AcadDatabase.Active())
            {
                Document doc = Application.DocumentManager.MdiActiveDocument;
                if (layerName != null)
                    CreateLayer(layerName, Color.FromRgb(255, 0, 0));

                foreach (var edge in edges)
                {
                    var ptS = edge.Start;
                    var ptE = edge.End;
                    var line = new Line(new Point3d(ptS.X, ptS.Y, 0), new Point3d(ptE.X, ptE.Y, 0));
                    line.Color = Color.FromRgb(0, 255, 255);
                    if (layerName != null)
                        line.Layer = layerName;
                    // 添加到modelSpace中
                    AcHelper.DocumentExtensions.AddEntity<Line>(doc, line);
                }
            }
        }

        /// <summary>
        /// 格式转换
        /// </summary>
        /// <param name="curvesLst"></param>
        /// <returns></returns>
        public static List<Polyline> Convert2Polyline(List<List<Curve>> curvesLst)
        {
            if (curvesLst == null || curvesLst.Count == 0)
                return null;

            var polylineLst = new List<Polyline>();
            foreach (var curves in curvesLst)
            {
                var polyline = new Polyline();
                polyline.Closed = true;

                for (int i = 0; i < curves.Count; i++)
                {
                    var point = curves[i].StartPoint;
                    polyline.AddVertexAt(i, new Point2d(point.X, point.Y), 0, 0, 0);
                }

                polylineLst.Add(polyline);
            }

            return polylineLst;
        }

        /// <summary>
        /// 获取block中的所有曲线数据
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public static List<Curve> GetCurvesFromBlock(BlockReference block, bool bSetLayer = true)
        {
            DBObjectCollection collection = new DBObjectCollection();
            block.Explode(collection);
            var blockCurves = new List<Curve>();
            using (var db = AcadDatabase.Active())
            {
                foreach (var obj in collection)
                {
                    if (obj is Curve)
                    {
                        var curve = obj as Curve;
                        if (bSetLayer)
                            curve.Layer = block.Layer;
                        blockCurves.Add(curve);
                    }
                    else if (obj is BlockReference)
                    {
                        var blockReference = obj as BlockReference;
                        if (bSetLayer)
                            blockReference.Layer = block.Layer;
                        var childCurves = GetCurvesFromBlock(blockReference, bSetLayer);
                        if (childCurves.Count != 0)
                            blockCurves.AddRange(childCurves);
                    }
                }
            }

            return blockCurves;
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

                        try
                        {
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
                        catch
                        {
                            // 有些block炸开的时候会抛出eCannotScaleNonUniformly异常
                            // 如果这个block 不能炸开，不作处理
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

        /// <summary>
        /// 数据打撒成直线段
        /// </summary>
        /// <param name="curves"></param>
        /// <returns></returns>
        public static List<LineSegment2d> TesslateCurve2Lines(List<Curve> curves)
        {
            var lines = new List<LineSegment2d>();
            foreach (var curve in curves)
            {
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
                    var lineNodes = Polyline2Lines(polyline as Polyline);
                    if (lineNodes != null)
                        lines.AddRange(lineNodes);
                }
                else if (curve is Circle)
                {
                    var circle = curve as Circle;
                    var spline = circle.Spline;
                    var polyline = spline.ToPolyline();
                    var lineNodes = Polyline2Lines(polyline as Polyline);
                    if (lineNodes != null)
                        lines.AddRange(lineNodes);
                }
                else if (curve is Ellipse)
                {
                    var ellipse = curve as Ellipse;
                    var polyline = ellipse.Spline.ToPolyline();
                    var lineNodes = Polyline2Lines(polyline as Polyline);
                    if (lineNodes != null)
                        lines.AddRange(lineNodes);
                }
                else if (curve is Polyline)
                {
                    var lineNodes = Polyline2Lines(curve as Polyline);
                    if (lineNodes != null)
                        lines.AddRange(lineNodes);
                }
                else if (curve is Spline)
                {
                    var polyline = (curve as Spline).ToPolyline();
                    if (polyline is Polyline)
                    {
                        var lineNodes = Polyline2Lines(polyline as Polyline);
                        if (lineNodes != null)
                            lines.AddRange(lineNodes);
                    }
                }
            }

            return lines;
        }

        /// <summary>
        /// 多段线转换为直线
        /// </summary>
        /// <param name="polyline"></param>
        /// <returns></returns>
        public static List<LineSegment2d> GetLineFromPolyline(Polyline polyline)
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
                }
            }

            return lines;
        }

        /// <summary>
        /// 多段线转换为直线
        /// </summary>
        /// <param name="polyline"></param>
        /// <returns></returns>
        public static List<LineSegment2d> Polyline2Lines(Polyline polyline)
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
                            var lineNodes = Polyline2Lines(pline as Polyline);
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
                            var lineNodes = Polyline2Lines(pline as Polyline);
                            if (lineNodes != null)
                                lines.AddRange(lineNodes);
                        }
                    }
                }
            }

            return lines;
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
            var lineNodes = Utils.TesslateCurve2Lines(curves);
            var ptLst = new List<XY>();
            foreach (var line in lineNodes)
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
        /// 获取指定图层中连通区域的包围盒边界处理
        /// </summary>
        /// <param name="layerName"></param>
        /// <returns></returns>
        public static List<List<LineSegment2d>> GetBoundsFromLayerBlocksAndCurves(List<string> layerNames)
        {
            var blockBounds = new List<List<LineSegment2d>>();
            using (var db = AcadDatabase.Active())
            {
                // 块轮廓
                var blocks = db.ModelSpace.OfType<BlockReference>();
                foreach (var block in blocks)
                {
                    if (ValidBlock(block, layerNames))
                    {
                        var blockCurves = GetCurvesFromBlock(block);
                        var lines = GetBoundFromCurves(blockCurves);
                        if (lines == null || lines.Count == 0)
                            continue;

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

                // 线数据处理
                if (layerCurves != null && layerCurves.Count != 0)
                {
                    foreach (var curve in layerCurves)
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
                            var curveBounds = Utils.GetRectFromBound(curve.Bounds.Value);
                            blockBounds.Add(curveBounds);
                        }
                    }
                }
            }

            // 生成相邻框的数据，框边缘是外扩的， 上面的框边缘都是原始的，不经过外扩的数据
            var connectBounds = ConnectedBoxBound.MakeRelatedBoundary(blockBounds);
            return connectBounds;
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

        public static List<Entity> PreProcessCurDwg(List<string> validLayers)
        {
            var resEntityLst = new List<Entity>();
            double progressPos = 5;
            // 本图纸数据块处理
            using (var db = AcadDatabase.Active())
            {
                var blockRefs = db.CurrentSpace.OfType<BlockReference>().Where(p => p.Visible).ToList();
                var incre = 10.0 / blockRefs.Count;
                foreach (var blockReference in blockRefs)
                {
                    var layerId = blockReference.LayerId;
                    if (layerId == null || !layerId.IsValid)
                        continue;

                    LayerTableRecord layerTableRecord = db.Element<LayerTableRecord>(layerId);
                    if (layerTableRecord.IsOff)
                        continue;

                    var blockId = blockReference.BlockTableRecord;
                    var blockRefRecord = db.Element<BlockTableRecord>(blockId);
                    if (blockRefRecord.IsFromExternalReference)
                        continue;
                    progressPos += 0.4;
                    progressPos += incre;
                    var entityLst = GetEntityFromBlock(blockReference);
                    if (entityLst != null && entityLst.Count > 1)
                    {
                        foreach (var entity in entityLst)
                        {
                            // 除了块，其余的可以用图层确定是否收集，块的图层内部数据也可能符合要求
                            if (!entity.Equals(blockReference) && IsValidLayer(entity, validLayers))
                            {
                                db.CurrentSpace.Add(entity);
                                resEntityLst.Add(entity);
                            }
                        }
                    }
                }
            }

            return resEntityLst;
        }

        public static bool IsValidLayer(Entity entity, List<string> validLayers)
        {
            if (validLayers.Count == 0)
                return false;
            if (entity is BlockReference)
                return true;

            foreach (var layer in validLayers)
            {
                if (entity.Layer.Contains(layer))
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

        public static void PostProcess(List<Entity> removeEntityLst)
        {
            using (var db = AcadDatabase.Active())
            {
                if (removeEntityLst != null && removeEntityLst.Count != 0)
                {
                    foreach (var entity in removeEntityLst)
                    {
                        var openEntity = db.Element<Entity>(entity.Id, true);
                        openEntity.Erase(true);
                    }
                }
            }
        }

        public static bool IsValidBlockReference(BlockReference block, List<LineSegment2d> rectLines)
        {
            var blockCurves = GetCurvesFromBlock(block, false);
            var lines = GetBoundFromCurves(blockCurves);
            if (lines == null || lines.Count == 0)
                return false;

            if (CommonUtils.OutLoopContainsInnerLoop(rectLines, lines) || IsLinesIntersectWithLines(lines, rectLines))
                return true;

            return false;
        }

        /// <summary>
        /// 从block单个数据
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public static List<Entity> GetEntityFromBlock(BlockReference block)
        {
            if (block == null || !block.Visible)
                return null;

            var entityLst = new List<Entity>();
            var blockReferences = new List<BlockReference>();
            blockReferences.Add(block);
            while (blockReferences.Count > 0)
            {
                var curBlock = blockReferences.First();
                blockReferences.RemoveAt(0);

                if (curBlock.Visible)
                {
                    var entitysInBlock = FromSingleBlock(curBlock, ref blockReferences);
                    if (entitysInBlock != null && entitysInBlock.Count != 0)
                    {
                        entityLst.AddRange(entitysInBlock);
                    }
                }
            }

            return entityLst;
        }

        /// <summary>
        /// 判断单个能否炸开，以及返回的数据
        /// </summary>
        /// <param name="block"></param>
        /// <param name="childReferences"></param>
        /// <returns></returns>
        public static List<Entity> FromSingleBlock(BlockReference block, ref List<BlockReference> childReferences)
        {
            if (block == null)
                return null;

            var entityLst = new List<Entity>();
            try
            {
                var dbCollection = new DBObjectCollection();
                block.Explode(dbCollection);

                if (IsHasBlockReference(dbCollection)) // 内部包含块
                {
                    foreach (var obj in dbCollection)
                    {
                        if (obj is Entity)
                        {
                            if (obj is BlockReference)
                            {
                                var childBlock = obj as BlockReference;
                                if (childBlock.Visible)
                                    childReferences.Add(childBlock);
                            }
                            else
                            {
                                var entity = obj as Entity;
                                if (entity.Visible)
                                    entityLst.Add(entity);
                            }
                        }
                    }
                }
                else if (IsCanExplode(dbCollection)) // 内部不包含块且曲线所在的图层名包含3个以上
                {
                    foreach (var obj in dbCollection)
                    {
                        if (obj is Entity)
                        {
                            var entity = obj as Entity;
                            if (entity.Visible)
                                entityLst.Add(entity);
                        }
                    }
                }
                else // 内部不包含块且曲线所在的图层名小于3个图层
                {
                    // 此时这个块不被炸开作为一个整体。
                    if (block.Visible)
                        entityLst.Add(block);
                }
            }
            catch
            {
                return null;
            }

            return entityLst;
        }

        /// <summary>
        /// 是否含有块
        /// </summary>
        /// <param name="dbCollection"></param>
        /// <returns></returns>
        public static bool IsHasBlockReference(DBObjectCollection dbCollection)
        {
            if (dbCollection == null || dbCollection.Count == 0)
                return false;

            foreach (var obj in dbCollection)
            {
                if (obj is BlockReference)
                {
                    var curBlock = obj as BlockReference;
                    if (curBlock.Visible)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 一个块中是否有多3个图层
        /// </summary>
        /// <param name="dbCollection"></param>
        /// <returns></returns>
        public static bool IsCanExplode(DBObjectCollection dbCollection)
        {
            var layerNames = new List<string>();
            foreach (var obj in dbCollection)
            {
                if (obj is Curve)
                {
                    var curve = obj as Curve;
                    if (curve.Visible)
                    {
                        var layerName = curve.Layer;
                        if (!layerNames.Contains(layerName))
                        {
                            layerNames.Add(layerName);
                            if (layerNames.Count > 3)
                                return true;
                        }
                    }
                }
            }

            return false;
        }

        public static List<Entity> PreProcessXREF(List<string> validLayers)
        {
            var resEntityLst = new List<Entity>();
            // 外部参照
            double progressPos = 17;
            using (var db = AcadDatabase.Active())
            {
                var refs = db.XRefs;
                var incre = 10.0 / refs.Count();

                foreach (var xblock in refs)
                {
                    if (xblock.Block.XrefStatus == XrefStatus.Resolved)
                    {
                        ObjectIdCollection idCollection = new ObjectIdCollection();
                        BlockTableRecord blockTableRecord = xblock.Block;
                        List<BlockReference> blockReferences = blockTableRecord.GetAllBlockReferences(true, true).ToList();
                        for (int i = 0; i < blockReferences.Count(); i++)
                        {
                            var blockReference = blockReferences[i];
                            progressPos += 0.4;
                            progressPos += incre;
                            var entityLst = GetEntityFromBlock(blockReference);
                            if (entityLst != null && entityLst.Count != 0)
                            {
                                foreach (var entity in entityLst)
                                {
                                    if (!entity.Equals(blockReference))
                                    {
                                        try
                                        {
                                            if (entity is DBText || entity is MText)
                                            {
                                                continue;
                                            }
                                            if (entity is BlockReference)
                                            {
                                                // 块里面的数据可能是有效单元， 剔除文字的影响
                                                var reference = entity as BlockReference;
                                                var dbCollection = new DBObjectCollection();
                                                reference.Explode(dbCollection);
                                                foreach (var part in dbCollection)
                                                {
                                                    if (part is DBText || part is MText)
                                                    {
                                                        continue;
                                                    }
                                                    else if (part is Entity)
                                                    {
                                                        var partEntity = part as Entity;
                                                        partEntity.Layer = reference.Layer;
                                                        db.CurrentSpace.Add(partEntity);
                                                        resEntityLst.Add(partEntity);
                                                    }
                                                }
                                            }
                                            else if (IsValidLayer(entity, validLayers))
                                            {
                                                db.CurrentSpace.Add(entity);
                                                resEntityLst.Add(entity);
                                            }
                                        }
                                        catch
                                        { }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return resEntityLst;
        }

        /// <summary>
        /// 图纸预处理
        /// </summary>
        public static List<Entity> PreProcess(List<string> validLayers)
        {
            var resEntityLst = new List<Entity>();
            var curEntityLst = PreProcessCurDwg(validLayers);

            if (curEntityLst.Count != 0)
                resEntityLst.AddRange(curEntityLst);

            var xRefEntityLst = PreProcessXREF(validLayers);
            if (xRefEntityLst.Count != 0)
                resEntityLst.AddRange(xRefEntityLst);
            return resEntityLst;
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

        public class CollinearData
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
                        if (!connectedNode.m_bUse && !InOutNodes.Contains(connectedNode))
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
                var lines = new List<LineSegment2d>();
                lines.Add(data.Line);
                lines.AddRange(data.RelatedCollinearLines);
                var outLines = PathNearSearch.MakeMergeLines(lines);
                return outLines;
            }
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
        public static List<string> ShowThLayers(out List<string> wallLayers, out List<string> doorLayers, out List<string> windLayers, out List<string> allValidLayers, out List<string> beamLayers, out List<string> columnLayers)
        {
            var allCurveLayers = new List<string>();
            wallLayers = new List<string>();
            doorLayers = new List<string>();
            windLayers = new List<string>();
            allValidLayers = new List<string>();
            beamLayers = new List<string>();
            columnLayers = new List<string>();
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
                         || layer.Name.Contains("AE-STRU") || layer.Name.Contains("COLU") || layer.Name.Contains("HDWR"))
                    {
                        allCurveLayers.Add(layer.Name);
                    }

                    if (layer.Name.Contains("AE-WALL") || layer.Name.Contains("HDWR"))
                        wallLayers.Add(layer.Name);

                    if (layer.Name.Contains("AE-DOOR-INSD"))
                        doorLayers.Add(layer.Name);

                    if (layer.Name.Contains("AE-WIND"))
                        windLayers.Add(layer.Name);

                    if (layer.Name.Contains("BEAM"))
                        beamLayers.Add(layer.Name);

                    if (layer.Name.Contains("COLU"))
                        columnLayers.Add(layer.Name);
                }

                allValidLayers.Add("AE-WALL");
                allValidLayers.Add("AD-NAME-ROOM");
                allValidLayers.Add("AE-STRU");
                allValidLayers.Add("COLU");
                allValidLayers.Add("AE-DOOR-INSD");
                allValidLayers.Add("AE-WIND");

                allValidLayers.Add("HDWR");

                foreach (var lName in closeLayerNames)
                {
                    db.Layers.Element(lName, true).IsOff = true;
                }
            }

            return allCurveLayers;
        }
    }
}
