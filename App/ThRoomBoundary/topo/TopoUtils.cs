using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThRoomBoundary.topo
{
    class TopoUtils
    {
        /// <summary>
        /// 获取包含点的最小轮廓,topo算法的上层进行数据的处理，处理为直线段
        /// </summary>
        /// <param name="curves"></param>
        /// <param name="point3"></param>
        /// <returns></returns>
        public static List<List<LineSegment2d>> MakeStructureMinLoop(List<LineSegment2d> lines, Point2d point)
        {
            var profiles = TopoSearch.MakeMinProfileLoopsInner(lines, point);
            return profiles;
        }

        /// <summary>
        /// 获取包含点的最小轮廓
        /// </summary>
        /// <param name="curves"></param>
        /// <param name="point3"></param>
        /// <returns></returns>
        public static List<List<Line>> MakeStructureMinLoop(List<Curve> curves, Point3d point)
        {
            var lines = TesslateCurve2Lines(curves);
            var pt = new Point2d(point.X, point.Y);
            var profiles = TopoSearch.MakeMinProfileLoopsInner(lines, pt);

            // 二维转化为三维
            return LineSegment2d2LineProfiles(profiles);
        }

        /// <summary>
        /// 数据格式的转换
        /// </summary>
        /// <param name="profileLst"></param>
        /// <returns></returns>
        public static List<List<Line>> LineSegment2d2LineProfiles(List<List<LineSegment2d>> profileLst)
        {
            if (profileLst == null)
                return null;
            var loopLst = new List<List<Line>>();
            foreach (var profiles in profileLst)
            {
                var lines = new List<Line>();
                foreach (var lineSegment2d in profiles)
                {
                    var ptS = lineSegment2d.StartPoint;
                    var ptE = lineSegment2d.EndPoint;
                    var line = new Line(new Point3d(ptS.X, ptS.Y, 0), new Point3d(ptE.X, ptE.Y, 0));
                    lines.Add(line);
                }

                loopLst.Add(lines);
            }

            return loopLst;
        }

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

                }
                else if (curve is Circle)
                {

                }
                else if (curve is Ellipse)
                {

                }
                else if (curve is Polyline)
                {
                    var lineSegment2ds = Polyline2Lines(curve as Polyline);
                    if (lineSegment2ds != null)
                        lines.AddRange(lineSegment2ds);
                }
                else if (curve is Spline)
                {

                }
            }

            return lines;
        }

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
                        var arc3d = polyline.GetArcSegmentAt(i);
                    }
               }
            }
            else
            {
                for (int j  = 0; j < polyline.NumberOfVertices - 1; j++)
                {
                    var bulge = polyline.GetBulgeAt(j);
                    if (CommonUtils.IsAlmostNearZero(bulge))
                    {
                        var line2d = polyline.GetLineSegment2dAt(j);
                        lines.Add(line2d);
                    }
                    else
                    {
                        var arc3d = polyline.GetArcSegmentAt(j);
                    }
                }
            }

            return lines;
        }

        /// <summary>
        /// 获取闭合轮廓
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        public static List<List<LineSegment2d>> MakeSrcProfiles(List<LineSegment2d> lines)
        {
            var profiles = TopoSearch.MakeSrcProfileLoops(lines);
            return profiles;
        }
    }

    //class MinProfile
    //{
    //    private List<List<LineSegment2d>> m_LineLoops = new List<List<LineSegment2d>>();
    //    public List<List<LineSegment2d>> LineLoops
    //    {
    //        get { return m_LineLoops; }
    //    }

    //    public MinProfile(List<LineSegment2d> lines, Point2d point)
    //    {
    //        var profiles = TopoSearch.MakeMinProfileLoopsInner(lines, point);
    //        var wLineLoops = new List<List<LineSegment2d>>();
    //        if (profiles != null)
    //        {
    //            m_LineLoops.AddRange(profiles);
    //        }
    //    }
    //}
}
