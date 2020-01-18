using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using AcHelper;
using GeometryExtensions;

namespace ThEssential.Align
{
    public static class ThAlignCircleExtension
    {
        /// <summary>
        /// 平行于UCS坐标系的包围圈
        /// </summary>
        /// <param name="Circle"></param>
        /// <returns>包围圈在UCS下的范围</returns>
        private static Extents3d GeometricExtentsImpl(this Circle circle)
        {
            var wcs2ucs = Active.Editor.WCS2UCS();
            var geCircle = new CircularArc3d(
                circle.Center.TransformBy(wcs2ucs),
                circle.Normal.TransformBy(wcs2ucs),
                circle.Radius);
            return new Extents3d(
                geCircle.OrthoBoundBlock.GetMinimumPoint(),
                geCircle.OrthoBoundBlock.GetMaximumPoint());
        }
    }
}
