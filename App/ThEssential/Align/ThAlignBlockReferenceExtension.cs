using Autodesk.AutoCAD.DatabaseServices;
using AcHelper;
using GeometryExtensions;

namespace ThEssential.Align
{
    public static class ThAlignBlockReferenceExtension
    {
        private static Extents3d GeometricExtentsImpl(this BlockReference blockReference)
        {
            var wcs2Ucs = Active.Editor.WCS2UCS();
            using (var clone = (BlockReference)blockReference.GetTransformedCopy(wcs2Ucs))
            {
                return clone.GeometryExtentsBestFit();
            }
        }
    }
}
