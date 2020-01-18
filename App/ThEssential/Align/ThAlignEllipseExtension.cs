using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using AcHelper;
using GeometryExtensions;

namespace ThEssential.Align
{
    public static class ThAlignEllipseExtension
    {
        private static Extents3d GeometricExtentsImpl(this Ellipse ellipse)
        {
            var wcs2ucs = Active.Editor.WCS2UCS();
            var geEllipse = new EllipticalArc3d(
                ellipse.Center.TransformBy(wcs2ucs), 
                ellipse.MajorAxis.TransformBy(wcs2ucs), 
                ellipse.MinorAxis.TransformBy(wcs2ucs), 
                ellipse.MajorRadius, 
                ellipse.MinorRadius);
            return new Extents3d(
                geEllipse.OrthoBoundBlock.GetMinimumPoint(), 
                geEllipse.OrthoBoundBlock.GetMaximumPoint());
        }
    }
}
