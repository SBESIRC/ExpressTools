using System;
using GeoAPI.Geometries;
using Autodesk.AutoCAD.Geometry;
using NetTopologySuite.Algorithm;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThCADCore.NTS
{
    public static class ThCADCoreNTSPolylineExtension
    {
        public static Circle MinimumBoundingCircle(this Polyline polyline)
        {
            var mbc = new MinimumBoundingCircle(polyline.ToNTSLineString());
            return new Circle(mbc.GetCentre().ToAcGePoint3d(), Vector3d.ZAxis, mbc.GetRadius());
        }

        public static Polyline MinimumBoundingBox(this Polyline polyline)
        {
            var geometry = polyline.ToNTSLineString().Envelope;
            if (geometry is ILineString lineString)
            {
                return lineString.ToDbPolyline();
            }
            else if (geometry is ILinearRing linearRing)
            {
                return linearRing.ToDbPolyline();
            }
            else if (geometry is IPolygon polygon)
            {
                return polygon.Shell.ToDbPolyline();
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}
