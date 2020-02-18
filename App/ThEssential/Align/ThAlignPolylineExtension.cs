using System.Collections.Generic;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using AcHelper;
using Dreambuild.AutoCAD;
using GeometryExtensions;

namespace ThEssential.Align
{
    public static class ThAlignPolylineExtension
    {
        private static Point2d GetMidpoint(this Curve2d curve)
        {
            double length = curve.GetLength(
                curve.GetParameterOf(curve.StartPoint),
                curve.GetParameterOf(curve.EndPoint)
                );
            double parameter = curve.GetParameterAtLength(
                curve.GetParameterOf(curve.StartPoint), 
                length / 2.0,
                true);
            return curve.EvaluatePoint(parameter);
        }

        /// <summary>
        /// 平行于UCS坐标系的包围圈
        /// </summary>
        /// <param name="polyline"></param>
        /// <returns>包围圈在UCS下的范围</returns>
        private static Extents3d GeometricExtentsImpl(this Polyline polyline)
        {
            var curves = new List<Curve3d>();
            var wcs2ucs = Active.Editor.WCS2UCS();
            var segments = new PolylineSegmentCollection(polyline);
            foreach(var segment in segments)
            {
                if (segment.IsLinear)
                {
                    var lineSegment = segment.ToLineSegment();
                    curves.Add(new LineSegment3d(
                        lineSegment.StartPoint.ToPoint3d().TransformBy(wcs2ucs),
                        lineSegment.EndPoint.ToPoint3d().TransformBy(wcs2ucs)));
                }
                else
                {
                    var circularArc = segment.ToCircularArc();
                    curves.Add(new CircularArc3d(
                        circularArc.StartPoint.ToPoint3d().TransformBy(wcs2ucs),
                        circularArc.GetMidpoint().ToPoint3d().TransformBy(wcs2ucs),
                        circularArc.EndPoint.ToPoint3d().TransformBy(wcs2ucs)
                        ));
                }
            }
            var geCompositeCurve = new CompositeCurve3d(curves.ToArray());
            return new Extents3d(
                geCompositeCurve.OrthoBoundBlock.GetMinimumPoint(),
                geCompositeCurve.OrthoBoundBlock.GetMaximumPoint());
        }
    }
}
