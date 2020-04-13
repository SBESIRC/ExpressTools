using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetARX;
using Linq2Acad;

namespace TopoNode
{
    class TopoUtils
    {
        /// <summary>
        /// 获取闭合轮廓
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        public static List<Curve> MakeSrcProfiles(List<Curve> curves)
        {
            var tmpCurves = TesslateCurve(curves);
            var profiles = TopoSearch.MakeSrcProfileLoops(tmpCurves);
            return profiles;
        }

        public static List<Curve> MakeSrcProfilesNoTes(List<Curve> curves)
        {
            var tmpCurves = CommonUtils.RemoveCollinearLines(curves);
            var profiles = TopoSearch.MakeSrcProfileLoops(tmpCurves);
            return profiles;
        }

        public static List<PolylineLayer> MakeProfileFromPoint(List<Curve> srcCurves, Point3d pt)
        {
            //预处理
            var profileCalcu = new CalcuContainPointProfile(srcCurves, pt);
            var relatedCurves = profileCalcu.DoCalRelatedCurves();
            if (relatedCurves == null)
                return null;

            var layers = Utils.GetLayersFromCurves(relatedCurves);
            //Utils.DrawProfile(relatedCurves, "related");
            //return null;
            var profileLayer = TopoSearch.MakeSrcProfileLoopsLayerFromPoint(relatedCurves, pt);
            return profileLayer;
        }

        class IntersectCurves
        {
            private List<UseCurve> useCurves = new List<UseCurve>();

            public List<Curve> CurvesIntersect
            {
                get;
                set;
            } = new List<Curve>();

            public List<Curve> CurvesNoIntersect
            {
                get;
                set;
            } = new List<Curve>();

            class UseCurve
            {
                public UseCurve(Curve curve, bool intersect = false)
                {
                    curCurve = curve;
                    curveIntersect = intersect;
                }

                public bool curveIntersect;
                public Curve curCurve;
            }

            public static void MakeIntersectCurves(List<Curve> curves, ref List<Curve> noIntersect, ref List<Curve> intersect)
            {
                var curvesCal = new IntersectCurves(curves);
                curvesCal.CalculateCurves();
                if (curvesCal.CurvesNoIntersect.Count != 0)
                    noIntersect.AddRange(curvesCal.CurvesNoIntersect);

                if (curvesCal.CurvesIntersect.Count != 0)
                    intersect.AddRange(curvesCal.CurvesIntersect);
            }

            public IntersectCurves(List<Curve> curves)
            {
                foreach (var curve in curves)
                {
                    useCurves.Add(new UseCurve(curve, false));
                }
            }

            private void CalculateCurves()
            {
                for (int i = 0; i < useCurves.Count(); i++)
                {
                    var firCurve = useCurves[i];
                    if (firCurve.curveIntersect || !firCurve.curCurve.Closed)
                        continue;

                    // 当前的都需要和后面的进行比较
                    for (int j = 0; j < useCurves.Count(); j++)
                    {
                        if (i == j)
                            continue;

                        var secCurve = useCurves[j];
                        if (Intersect(firCurve, secCurve))
                        {
                            firCurve.curveIntersect = true;
                            secCurve.curveIntersect = true;
                        }
                    }
                }

                foreach (var curve in useCurves)
                {
                    if (curve.curveIntersect || !curve.curCurve.Closed)
                    {
                        // 非闭合数据也将参与后续的拓扑计算
                        CurvesIntersect.Add(curve.curCurve);
                    }
                    else
                    {
                        CurvesNoIntersect.Add(curve.curCurve);
                    }
                }
            }

            private bool Intersect(UseCurve firCurve, UseCurve secCurve)
            {
                Point3dCollection ptLst = new Point3dCollection();
                firCurve.curCurve.IntersectWith(secCurve.curCurve, Autodesk.AutoCAD.DatabaseServices.Intersect.OnBothOperands, ptLst, new System.IntPtr(0), new System.IntPtr(0));
                if (ptLst.Count != 0)
                {
                    return true;
                }

                return false;
            }

            private bool PointsValid(Point3dCollection ptLst1, Point3dCollection ptLst2)
            {
                for (int i = 0; i < ptLst1.Count; i++)
                {
                    if (PointValid(ptLst1[i], ptLst2))
                    {
                        return true;
                    }
                }

                return false;
            }

            private bool PointValid(Point3d pt, Point3dCollection ptLst)
            {
                for (int i = 0; i < ptLst.Count; i++)
                {
                    if (CommonUtils.Point3dIsEqualPoint3d(ptLst[i], pt))
                        return false;
                }

                return true;
            }
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

        public static List<Arc> Circle2Arcs(Circle circle)
        {
            var arcs = new List<Arc>();
            var arc1 = new Arc(circle.Center, circle.Radius, 0, Math.PI);
            var arc2 = new Arc(circle.Center, circle.Radius, Math.PI, Math.PI * 2);
            arc1.Layer = circle.Layer;
            arc2.Layer = circle.Layer;
            arcs.Add(arc1);
            arcs.Add(arc2);
            return arcs;
        }

        /// <summary>
        /// 数据打撒
        /// </summary>
        /// <param name="curves"></param>
        /// <returns></returns>
        public static List<Curve> TesslateCurve(List<Curve> curves)
        {
            var outCurves = new List<Curve>();
            foreach (var curve in curves)
            {
                if (curve is Line)
                {
                    outCurves.Add(curve);
                }
                else if (curve is Arc)
                {
                    outCurves.Add(curve);
                }
                else if (curve is Circle)
                {
                    var circle = curve as Circle;
                    var arcs = Circle2Arcs(circle);
                    if (arcs != null)
                        outCurves.AddRange(arcs);
                }
                else if (curve is Ellipse)
                {
                    var ellipse = curve as Ellipse;
                    var polyline = ellipse.Spline.ToPolyline();
                    polyline.Layer = curve.Layer;
                    var lineNodes = Polyline2Curves(polyline as Polyline);
                    if (lineNodes != null)
                        outCurves.AddRange(lineNodes);
                }
                else if (curve is Polyline)
                {
                    var lineNodes = Polyline2Curves(curve as Polyline);
                    if (lineNodes != null)
                        outCurves.AddRange(lineNodes);
                }
                else if (curve is Spline)
                {
                    var polyline = (curve as Spline).ToPolyline();
                    polyline.Layer = curve.Layer;
                    if (polyline is Polyline)
                    {
                        var lineNodes = Polyline2Curves(polyline as Polyline);
                        if (lineNodes != null)
                            outCurves.AddRange(lineNodes);
                    }
                }
            }

            return outCurves;
        }

        public static List<Curve> Polyline2Curves(Polyline polyline, bool copyLayer = true)
        {
            if (polyline == null)
                return null;

            var curves = new List<Curve>();
            if (polyline.Closed)
            {
                for (int i = 0; i < polyline.NumberOfVertices; i++)
                {
                    var bulge = polyline.GetBulgeAt(i);
                    if (CommonUtils.IsAlmostNearZero(bulge))
                    {
                        LineSegment3d line3d = polyline.GetLineSegmentAt(i);
                        var line = new Line(line3d.StartPoint, line3d.EndPoint);
                        if (copyLayer)
                            line.Layer = polyline.Layer;
                        curves.Add(line);
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
                            if (copyLayer)
                                arc.Layer = polyline.Layer;
                            curves.Add(arc);
                        }
                    }
                }
            }
            else
            {
                for (int j = 0; j < polyline.NumberOfVertices - 1; j++)
                {
                    try
                    {
                        var bulge = polyline.GetBulgeAt(j);
                        if (CommonUtils.IsAlmostNearZero(bulge))
                        {
                            LineSegment3d line3d = polyline.GetLineSegmentAt(j);
                            var line = new Line(line3d.StartPoint, line3d.EndPoint);
                            if (copyLayer)
                                line.Layer = polyline.Layer;
                            curves.Add(line);
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
                                if (copyLayer)
                                    arc.Layer = polyline.Layer;
                                curves.Add(arc);
                            }
                        }
                    }
                    catch
                    { }
                }
            }

            return curves;
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
