using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using AcHelper;
using GeometryExtensions;

namespace ThEssential.Align
{
    public static class ThAlignMTextExtension
    {
        private static Extents3d GeometricExtentsImpl(this MText mText)
        {
            // WCS下相对于ECS的包围框
            var collection = mText.GetBoundingPoints();

            // UCS下相对于ECS的包围框
            var wcs2ucs = Active.Editor.WCS2UCS();
            var pointCollection = new Point3dCollection()
            {
                // 左下角
                collection[2].TransformBy(wcs2ucs),
                // 左上角
                collection[0].TransformBy(wcs2ucs),
                // 右上角
                collection[1].TransformBy(wcs2ucs),
                // 右下角
                collection[3].TransformBy(wcs2ucs),
            };

            return pointCollection.ToExtents3d();
        }
    }
}
