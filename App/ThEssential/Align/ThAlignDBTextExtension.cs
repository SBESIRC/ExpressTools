using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using AcHelper;
using GeometryExtensions;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThEssential.Align
{
    public static class ThAlignDBTextExtension
    {
        private static Extents3d GeometricExtentsImpl(this DBText dBText)
        {
            // WCS下相对于ECS的包围框
            var points = new Point3d[4];
            dBText.GetTextBoxCorners(out points[0], out points[1], out points[2], out points[3]);

            // UCS下相对于ECS的包围框
            var wcs2ucs = Active.Editor.WCS2UCS();
            var pointCollection = new Point3dCollection()
            {
                // 左下角
                points[0].TransformBy(wcs2ucs),
                // 左上角
                points[2].TransformBy(wcs2ucs),
                // 右上角
                points[1].TransformBy(wcs2ucs),
                // 右下角
                points[3].TransformBy(wcs2ucs),
            };

            return pointCollection.ToExtents3d();
        }
    }
}
