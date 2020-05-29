using System;
using GeoAPI.Geometries;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThCADCore.NTS
{
    public static class ThCADCoreNTSRegionExtension
    {
        public static Region Union(this Region pRegion, Region sRegion)
        {
            var pGeometry = pRegion.ToNTSPolygon();
            var sGeometry = sRegion.ToNTSPolygon();
            if (pGeometry == null || sGeometry == null)
            {
                return null;
            }

            // 检查是否相交
            if (!pGeometry.Intersects(sGeometry))
            {
                return null;
            }

            // 若相交，则计算共用部分
            var rGeometry = pGeometry.Union(sGeometry);
            if (rGeometry is IPolygon polygon)
            {
                return polygon.ToDbRegion();
            }

            return null;
        }

        public static Region Intersection(this Region pRegion, Region sRegion)
        {
            var pGeometry = pRegion.ToNTSPolygon();
            var sGeometry = sRegion.ToNTSPolygon();
            if (pGeometry == null || sGeometry == null)
            {
                return null;
            }

            // 检查是否相交
            if (!pGeometry.Intersects(sGeometry))
            {
                return null;
            }

            // 若相交，则计算相交部分
            var rGeometry = pGeometry.Intersection(sGeometry);
            if (rGeometry is IPolygon polygon)
            {
                return polygon.ToDbRegion();
            }

            return null;
        }

        public static Region Difference(this Region pRegion, Region sRegion)
        {
            var pGeometry = pRegion.ToNTSPolygon();
            var sGeometry = sRegion.ToNTSPolygon();
            if (pGeometry == null || sGeometry == null)
            {
                return null;
            }

            // 检查是否相交
            if (!pGeometry.Intersects(sGeometry))
            {
                return null;
            }

            // 若相交，则计算在pRegion，但不在sRegion的部分
            var rGeometry = pGeometry.Difference(sGeometry);
            if (rGeometry is IPolygon polygon)
            {
                return polygon.ToDbRegion();
            }
            else if (rGeometry is IMultiPolygon mPolygon)
            {
                // 暂时不考虑MPolygon的情况
                return null;
            }
            else
            {
                // 为止情况，抛出异常
                throw new NotSupportedException();
            }
        }

        public static IGeometry Intersect(this Region pRegion, Region sRegion)
        {
            var pGeometry = pRegion.ToNTSPolygon();
            var sGeometry = sRegion.ToNTSPolygon();
            if (pGeometry == null || sGeometry == null)
            {
                return null;
            }

            // 检查是否相交
            if (!pGeometry.Intersects(sGeometry))
            {
                return null;
            }

            // 若相交，则计算相交部分
            return pGeometry.Intersection(sGeometry);
        }
    }
}
