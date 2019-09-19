using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetARX;

namespace ThRoomBoundary.topo
{
    public class RoomData
    {
        public List<Line> Lines
        {
            get;
            set;
        }

        public Point3d Pos
        {
            get;
            set;
        }

        public double Area
        {
            get;
            set;
        }
    }

    public class RoomDataPolyline
    {
        public Polyline RoomPolyline
        {
            get;
            set;
        }

        public Point3d Pos
        {
            get;
            set;
        }

        public double Area
        {
            get;
            set;
        }

        public bool ValidData
        {
            get;
            set;
        }

        public RoomDataPolyline(Polyline polyline, Point3d pos, double area)
        {
            RoomPolyline = polyline;
            Pos = pos;
            Area = area;
            ValidData = true;
        }
    }

    /// <summary>
    /// 内部数据
    /// </summary>
    class RoomDataInner
    {
        public RoomDataInner(List<LineSegment2d> lines, Point2d pos, double area)
        {
            Lines = lines;
            Pos = pos;
            Area = area;
        }

        public List<LineSegment2d> Lines
        {
            get;
            set;
        }

        public Point2d Pos
        {
            get;
            set;
        }

        public double Area
        {
            get;
            set;
        }
    }

    class TopoUtils
    {
        /// <summary>
        /// 获取包含点的最小轮廓
        /// </summary>
        /// <param name="curves"></param>
        /// <param name="point3"></param>
        /// <returns></returns>
        public static List<RoomData> MakeStructureMinLoop(List<Curve> curves, Point3d point)
        {
            var lines = TesslateCurve2Lines(curves);
            var pt = new Point2d(point.X, point.Y);
            var profiles = TopoSearch.MakeMinProfileLoopsInner(lines, pt);

            // 二维转化为三维
            return Convert2RoomData(profiles);
        }

        /// <summary>
        /// 获取闭合轮廓
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        public static List<RoomDataPolyline> MakeSrcProfiles(List<Curve> curves, List<LineSegment2d> rectLines = null)
        {
            var lines = TesslateCurve2Lines(curves);
            var profiles = TopoSearch.MakeSrcProfileLoops(lines, rectLines);

            // 二维转化为三维
            var roomDatas = Convert2RoomData(profiles);
            return TopoUtils.Convert2RoomDataPolyline(roomDatas);
        }

        /// <summary>
        /// 数据格式转换
        /// </summary>
        /// <param name="roomDatas"></param>
        /// <returns></returns>
        public static List<RoomDataPolyline> Convert2RoomDataPolylines(List<RoomData> roomDatas)
        {
            if (roomDatas == null || roomDatas.Count == 0)
                return null;

            var roomDataPolylines = new List<RoomDataPolyline>();
            foreach (var room in roomDatas)
            {
                var polyline = new Polyline();
                polyline.Closed = true;
                for (int i = 0; i < room.Lines.Count; i++)
                {
                    var point = room.Lines[i].StartPoint;
                    polyline.AddVertexAt(i, new Point2d(point.X, point.Y), 0, 0, 0);
                }

                var roomPolyline = new RoomDataPolyline(polyline, room.Pos, room.Area);
                roomDataPolylines.Add(roomPolyline);
            }

            return roomDataPolylines;
        }

        public static List<RoomDataPolyline> Convert2RoomDataPolyline(List<RoomData> roomDatas)
        {
            if (roomDatas == null || roomDatas.Count == 0)
                return null;

            var roomDataPolylines = new List<RoomDataPolyline>();
            foreach (var room in roomDatas)
            {
                var polyline = new Polyline();
                polyline.Closed = true;
                var lines = room.Lines;
                var drawLines = new List<Line>();
                foreach (var line in lines)
                {
                    drawLines.Add(CommonUtils.LineDecVector(line, new Vector3d(0, -50000, 0)));
                }

                for (int i = 0; i < drawLines.Count; i++)
                {
                    var point = drawLines[i].StartPoint;
                    polyline.AddVertexAt(i, new Point2d(point.X, point.Y), 0, 0, 0);
                }

                var roomPolyline = new RoomDataPolyline(polyline, room.Pos, room.Area);
                roomDataPolylines.Add(roomPolyline);
            }

            return roomDataPolylines;
        }

        /// <summary>
        /// 数据格式的转换
        /// </summary>
        /// <param name="profileLst"></param>
        /// <returns></returns>
        public static List<RoomData> Convert2RoomData(List<RoomDataInner> roomDataInner)
        {
            if (roomDataInner == null)
                return null;

            var roomDatas = new List<RoomData>();
            foreach (var dataInner in roomDataInner)
            {
                var roomData = new RoomData();
                roomData.Lines = Convert2Lines(dataInner.Lines);
                roomData.Pos = new Point3d(dataInner.Pos.X, dataInner.Pos.Y, 0);
                roomData.Area = dataInner.Area;
                roomDatas.Add(roomData);
            }

            return roomDatas;
        }

        /// <summary>
        /// 二维数据转化为三维数据
        /// </summary>
        /// <param name="line2ds"></param>
        /// <returns></returns>
        public static List<Line> Convert2Lines(List<LineSegment2d> line2ds)
        {
            if (line2ds == null)
                return null;

            var outLines = new List<Line>();
            foreach (var line2d in line2ds)
            {
                var ptS = line2d.StartPoint;
                var ptE = line2d.EndPoint;
                var line = new Line(new Point3d(ptS.X, ptS.Y, 0), new Point3d(ptE.X, ptE.Y, 0));
                outLines.Add(line);
            }

            return outLines;
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
                    var lineSegment2ds = Polyline2Lines(polyline as Polyline);
                    if (lineSegment2ds != null)
                        lines.AddRange(lineSegment2ds);
                }
                else if (curve is Circle)
                {
                    var circle = curve as Circle;
                    var spline = circle.Spline;
                    var polyline = spline.ToPolyline();
                    var lineSegment2ds = Polyline2Lines(polyline as Polyline);
                    if (lineSegment2ds != null)
                        lines.AddRange(lineSegment2ds);
                }
                else if (curve is Ellipse)
                {
                    var ellipse = curve as Ellipse;
                    var polyline = ellipse.Spline.ToPolyline();
                    var lineSegment2ds = Polyline2Lines(polyline as Polyline);
                    if (lineSegment2ds != null)
                        lines.AddRange(lineSegment2ds);
                }
                else if (curve is Polyline)
                {
                    var lineSegment2ds = Polyline2Lines(curve as Polyline);
                    if (lineSegment2ds != null)
                        lines.AddRange(lineSegment2ds);
                }
                else if (curve is Spline)
                {
                    var polyline = (curve as Spline).ToPolyline();
                    if (polyline is Polyline)
                    {
                        var lineSegment2ds = Polyline2Lines(polyline as Polyline);
                        if (lineSegment2ds != null)
                            lines.AddRange(lineSegment2ds);
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
                            var lineSegment2ds = Polyline2Lines(pline as Polyline);
                            if (lineSegment2ds != null)
                                lines.AddRange(lineSegment2ds);
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
                            var lineSegment2ds = Polyline2Lines(pline as Polyline);
                            if (lineSegment2ds != null)
                                lines.AddRange(lineSegment2ds);
                        }
                    }
                }
            }

            return lines;
        }

        /// <summary>
        /// 数据格式转换
        /// </summary>
        /// <param name="curves"></param>
        /// <returns></returns>
        public static List<LineSegment2d> ConvertToLineSegment2d(List<Curve2d> curves)
        {
            if (curves == null || curves.Count == 0)
                return null;

            var lines = new List<LineSegment2d>();
            foreach (var curve in curves)
            {
                lines.Add(new LineSegment2d(curve.StartPoint, curve.EndPoint));
            }

            return lines;
        }
    }
}
