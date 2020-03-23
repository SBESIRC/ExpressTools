using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace THSitePlanRoadDemo
{
    class CalcuUtils
    {
        /// <summary>
        /// 删除共边
        /// </summary>
        /// <param name="lines"></param>
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
            return resCurves;
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
