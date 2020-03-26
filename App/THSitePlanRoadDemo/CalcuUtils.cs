using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetARX;

namespace THSitePlanRoadDemo
{
    class CalcuUtils
    {
        /// <summary>
        /// 直线
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        public static List<LineSegment2d> RemoveCollinearLines(List<LineSegment2d> lines)
        {
            var outLines = new List<LineSegment2d>();
            var eraseLines = CoEdgeErase.MakeCoEdgeErase(lines);
            outLines.AddRange(eraseLines);
            return outLines;
        }

        public static List<Curve> RemoveOverlap(List<Curve> curves)
        {
            var resCurves = new List<Curve>();

            var line2ds = new List<LineSegment2d>();
            var arcs = new List<Arc>();
            foreach (var curve in curves)
            {
                if (curve is Line line)
                {
                    var ptS = line.StartPoint;
                    var ptE = line.EndPoint;
                    var line2d = new LineSegment2d(new Point2d(ptS.X, ptS.Y), new Point2d(ptE.X, ptE.Y));
                    line2ds.Add(line2d);
                }
                else if (curve is Arc arc)
                {
                    var arcPlane = new Arc();
                    Point3d ptS = arc.StartPoint;
                    var ptCenter = arc.Center;
                    var ptE = arc.EndPoint;

                    var ptSZO = new Point3d(ptS.X, ptS.Y, 0);
                    var ptCenterZ0 = new Point3d(ptCenter.X, ptCenter.Y, 0);
                    var ptEZ0 = new Point3d(ptE.X, ptE.Y, 0);
                    arcPlane.CreateArcSCE(ptSZO, ptCenterZ0, ptEZ0);
                    arcs.Add(arcPlane);
                }
            }

            var resLine2ds = RemoveCollinearLines(line2ds);
            var resArcs = RemoveArcs(arcs);

            var drawCurves = line2dToCurves(resLine2ds);
            ExtendUtils.DrawProfile(drawCurves, "drawC");
            return resCurves;
        }
    
        public static List<Curve> line2dToCurves(List<LineSegment2d> line2ds)
        {
            if (line2ds == null || line2ds.Count == 0)
                return null;

            var curves = new List<Curve>();
            foreach (var line2d in line2ds)
            {
                var line = Line2Curve(line2d);
                curves.Add(line);
            }
            return curves;
            
        }

        public static Line Line2Curve(LineSegment2d line2d)
        {
            var ptS = line2d.StartPoint;
            var ptE = line2d.EndPoint;
            var line = new Line(new Point3d(ptS.X, ptS.Y, 0), new Point3d(ptE.X, ptE.Y, 0));
            return line;
        }

        /// <summary>
        /// 圆弧
        /// </summary>
        /// <param name="arcs"></param>
        /// <returns></returns>
        public static List<Curve> RemoveArcs(List<Arc> arcs)
        {
            var resArcs = new List<Curve>();

            return resArcs;
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
            public static List<LineSegment2d> MakeCoEdgeErase(List<LineSegment2d> line2ds)
            {
                var edgeErase = new CoEdgeErase(line2ds);
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

                        if (curEdge.IsErase)
                            break;

                        if (nextEdge.IsErase)
                            continue;

                        var nextLine = nextEdge.CoLine;
                        var nextLinePtS = nextLine.StartPoint;
                        var nextLinePtE = nextLine.EndPoint;

                        // 平行线
                        if (CommonUtils.IsAlmostNearZero(CommonUtils.CalAngle(CommonUtils.Vector2XY(curLine.Direction), CommonUtils.Vector2XY(nextLine.Direction)), 1e-6)
                           || CommonUtils.IsAlmostNearZero(CommonUtils.CalAngle(CommonUtils.Vector2XY(curLine.Direction), CommonUtils.Vector2XY(nextLine.Direction.Negate())), 1e-6))
                        {
                            // 重合线
                            if ((CommonUtils.Point2dIsEqualPoint2d(curLinePtS, nextLinePtS) && CommonUtils.Point2dIsEqualPoint2d(curLinePtE, nextLinePtE))
                            || (CommonUtils.Point2dIsEqualPoint2d(curLinePtS, nextLinePtE) && CommonUtils.Point2dIsEqualPoint2d(curLinePtE, nextLinePtS)))
                            {
                                curEdge.IsErase = true;
                            }
                            else if (CommonUtils.IsPointOnSegment(nextLinePtS, curLine, 1e-3) && CommonUtils.IsPointOnSegment(nextLinePtE, curLine, 1e-3)) // 完全包含线nextLine
                            {
                                nextEdge.IsErase = true;
                            }
                            else if (CommonUtils.IsPointOnSegment(curLinePtS, nextLine, 1e-3) && CommonUtils.IsPointOnSegment(curLinePtE, nextLine, 1e-3)) // 完全包含线curLine
                            {
                                curEdge.IsErase = true;
                            }
                            else if (CommonUtils.IsPointOnSegment(nextLinePtS, curLine, 1e-3) || (CommonUtils.IsPointOnSegment(nextLinePtE, curLine, 1e-3))) // 部分包含线
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
                var line2ds = new List<LineSegment2d>();
                for (int i = 0; i < m_coEdges.Count; i++)
                {
                    if (m_coEdges[i].IsErase)
                        continue;

                    line2ds.Add(m_coEdges[i].CoLine);
                }

                return line2ds;
            }
        }
    }
}
