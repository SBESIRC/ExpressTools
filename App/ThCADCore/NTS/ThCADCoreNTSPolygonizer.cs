using System;
using System.Linq;
using GeoAPI.Geometries;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using NetTopologySuite.Operation.Union;
using NetTopologySuite.Operation.Polygonize;

namespace ThCADCore.NTS
{
    public static class ThCADCoreNTSPolygonizer
    {
        public static DBObjectCollection PolygonizeLines(this DBObjectCollection lines)
        {
            var polygonizer = new Polygonizer();
            polygonizer.Add(lines.ToNTSNodedLineStrings());
            try
            {
                var objs = new DBObjectCollection();
                var polygons = polygonizer.GetPolygons();
                var solution = UnaryUnionOp.Union(polygons.ToList());
                if (solution is ILineString lineString)
                {
                    objs.Add(lineString.ToDbPolyline());
                }
                else if (solution is ILinearRing linearRing)
                {
                    objs.Add(linearRing.ToDbPolyline());
                }
                else if (solution is IPolygon polygon)
                {
                    foreach (var pline in polygon.ToDbPolylines())
                    {
                        objs.Add(pline);
                    }
                }
                else
                {
                    throw new NotSupportedException();
                }
                return objs;
            }
            catch
            {
                // 未知错误
                return new DBObjectCollection();
            }
        }

        public static ICollection<IGeometry> GetGeometries(this DBObjectCollection lines)
        {
            var polygonizer = new Polygonizer();
            polygonizer.Add(lines.ToNTSNodedLineStrings());
            return polygonizer.GetPolygons();
        }

        public static DBObjectCollection Polygons(this DBObjectCollection lines)
        {
            var objs = new DBObjectCollection();
            var polygonizer = new Polygonizer();
            polygonizer.Add(lines.ToNTSNodedLineStrings());
            foreach (IPolygon polygon in polygonizer.GetPolygons())
            {
                objs.Add(polygon.Shell.ToDbPolyline());
            }
            return objs;
        }

        public static DBObjectCollection Boundaries(this DBObjectCollection lines)
        {
            var polygons = new List<IPolygon>();
            var polygonizer = new Polygonizer();
            var boundaries = new DBObjectCollection();
            polygonizer.Add(lines.ToNTSNodedLineStrings());
            var geometry = UnaryUnionOp.Union(polygonizer.GetPolygons().ToList());
            if (geometry is IMultiPolygon mPolygon)
            {
                foreach (var item in mPolygon.Geometries)
                {
                    if (item is IPolygon polygon)
                    {
                        polygons.Add(polygon);
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                }
            }
            else if (geometry is IPolygon polygon)
            {
                polygons.Add(polygon);
            }
            else
            {
                throw new NotSupportedException();
            }
            foreach (var item in polygons)
            {
                boundaries.Add(item.Shell.ToDbPolyline());
            }
            return boundaries;
        }

        public static DBObjectCollection FindLoops(this DBObjectCollection lines)
        {
            var polygons = new List<IPolygon>();
            var polygonizer = new Polygonizer();
            var loops = new DBObjectCollection();
            polygonizer.Add(lines.ToNTSNodedLineStrings());
            var geometries = polygonizer.GetPolygons().ToList();
            foreach (var geometry in geometries)
            {
                if (geometry is IMultiPolygon mPolygon)
                {
                    foreach (var item in mPolygon.Geometries)
                    {
                        if (item is IPolygon polygon)
                        {
                            polygons.Add(polygon);
                        }
                        else
                        {
                            throw new NotSupportedException();
                        }
                    }
                }
                else if (geometry is IPolygon polygon)
                {
                    polygons.Add(polygon);
                }
                else
                {
                    continue;
                }
            }
            foreach (var item in polygons)
            {
                loops.Add(item.Shell.ToDbPolyline());
            }
            return loops;
        }
    }
}
