using System;
using GeoAPI.Geometries;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThCADCore.NTS
{
    public static class ThCADCoreNTSOperation
    {
        public static DBObjectCollection Trim(this Polyline polyline, Polyline curve)
        {
            var objs = new DBObjectCollection();
            var other = curve.ToNTSLineString();
            var polygon = polyline.ToNTSPolygon();
            var result = polygon.Intersection(other);
            if (result is IMultiLineString lineStrings)
            {
                foreach (ILineString lineString in lineStrings.Geometries)
                {
                    objs.Add(lineString.ToDbPolyline());
                }
            }
            else
            {
                throw new NotSupportedException();
            }
            return objs;
        }
    }
}
