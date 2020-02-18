using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using AcHelper;
using GeometryExtensions;

namespace ThEssential.Align
{
    public static class ThAlignArcExtension
    {
        private static Point3d GetMidpoint(this Curve curve)
        {
            double d1 = curve.GetDistanceAtParameter(curve.StartParam);
            double d2 = curve.GetDistanceAtParameter(curve.EndParam);
            return curve.GetPointAtDist(d1 + ((d2 - d1) / 2.0));
        }

        private static Extents3d GeometricExtentsImpl(this Arc arc)
        {
            var wcs2Ucs = Active.Editor.WCS2UCS();
            var geArc = new CircularArc3d(
                arc.StartPoint.TransformBy(wcs2Ucs), 
                arc.GetMidpoint().TransformBy(wcs2Ucs), 
                arc.EndPoint.TransformBy(wcs2Ucs));
            return new Extents3d(
                geArc.OrthoBoundBlock.GetMinimumPoint(),
                geArc.OrthoBoundBlock.GetMaximumPoint());
        }
    }
}
