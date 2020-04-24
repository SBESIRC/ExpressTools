using GeoAPI.Geometries;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.BoundaryRepresentation;

namespace ThCADCore.NTS
{
    public static class ThCADCoreNTSRegionExtension
    {
        public static Region Union(this Region pRegion, Region sRegion)
        {
            var pGeometry = pRegion.ToNTSGeometry();
            var sGeometry = sRegion.ToNTSGeometry();
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
            var pGeometry = pRegion.ToNTSGeometry();
            var sGeometry = sRegion.ToNTSGeometry();
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
            var pGeometry = pRegion.ToNTSGeometry();
            var sGeometry = sRegion.ToNTSGeometry();
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

            return null;
        }

        public static IGeometry Intersect(this Region pRegion, Region sRegion)
        {
            var pGeometry = pRegion.ToNTSGeometry() as IPolygon;
            var sGeometry = sRegion.ToNTSGeometry() as IPolygon;
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

        public static Point3dCollection Vertices(this Region region)
        {
            var vertices = new Point3dCollection();
            if (!region.IsNull)
            {
                using (var brepRegion = new Brep(region))
                {
                    foreach (var face in brepRegion.Faces)
                    {
                        foreach (var loop in face.Loops)
                        {
                            foreach (var vertex in loop.Vertices)
                            {
                                vertices.Add(vertex.Point);
                            }
                        }
                    }
                }
            }
            return vertices;
        }
    }
}
