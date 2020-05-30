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
        public static ICollection<IGeometry> Polygonize(this DBObjectCollection lines)
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
            if(geometry == null)
            {
                return boundaries;
            }
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

        public static DBObjectCollection Outline(this DBObjectCollection lines)
        {
            var objs = new DBObjectCollection();
            var polygonizer = new Polygonizer();
            polygonizer.Add(lines.ToNTSNodedLineStrings());
            var geometry = CascadedPolygonUnion.Union(polygonizer.GetPolygons());
            if (geometry == null)
            {
                return objs;
            }
            if (geometry is IPolygon polygon)
            {
                objs.Add(polygon.Shell.ToDbPolyline());
            }
            else if (geometry is IMultiPolygon mPolygon)
            {
                foreach (IPolygon subPolygon in mPolygon.Geometries)
                {
                    objs.Add(subPolygon.Shell.ToDbPolyline());
                }
            }
            else
            {
                throw new NotSupportedException();
            }
            return objs;
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
