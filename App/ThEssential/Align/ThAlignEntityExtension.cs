using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using AcHelper;
using GeometryExtensions;

namespace ThEssential.Align
{
    public static class ThAlignEntityExtension
    {
        public static Extents3d GeometricExtentsEx(this Entity entity)
        {
            if (entity is Polyline polyline)
            {
                return polyline.GeometricExtentsImpl();
            }
            else if (entity is Circle circle)
            {
                return circle.GeometricExtentsImpl();
            }
            else if (entity is Arc arc)
            {
                return arc.GeometricExtentsImpl();
            }
            else if (entity is Ellipse ellipse)
            {
                return ellipse.GeometricExtentsImpl();
            }
            else if (entity is DBText dBText)
            {
                return dBText.GeometricExtentsImpl();
            }
            else if (entity is MText mText)
            {
                return mText.GeometricExtentsImpl();
            }
            else if (entity is BlockReference blockReference)
            {
                return blockReference.GeometricExtentsImpl();
            }
            else
            {
                return entity.GeometricExtentsImpl();
            }
        }

        private static Extents3d GeometricExtentsImpl(this Entity entity)
        {
            var wcs2Ucs = Active.Editor.WCS2UCS();
            using (var clone = entity.GetTransformedCopy(wcs2Ucs))
            {
                return clone.GeometricExtents;
            }
        }

        public static void Align(this Entity entity, AlignMode mode, Point3d point)
        {
            var extents = entity.GeometricExtentsEx();
            var displacement = extents.Displacement(mode, point);
            var wcsDisplacement = displacement.TransformBy(Active.Editor.UCS2WCS());
            entity.TransformBy(Matrix3d.Displacement(wcsDisplacement));
        }
    }
}
