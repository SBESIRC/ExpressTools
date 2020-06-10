﻿using System;
using GeoAPI.Geometries;
using NetTopologySuite.Operation.Buffer;
using Autodesk.AutoCAD.DatabaseServices;
using NetTopologySuite.Operation.Linemerge;

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

        public static DBObjectCollection Merge(this DBObjectCollection lines)
        {
            var merger = new LineMerger();
            merger.Add(lines.ToNTSNodedLineStrings());
            var lineStrings = new DBObjectCollection();
            foreach (var geometry in merger.GetMergedLineStrings())
            {
                if (geometry is ILineString lineString)
                {
                    lineStrings.Add(lineString.ToDbPolyline());
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            return lineStrings;
        }

        public static DBObjectCollection Buffer(this Polyline polyline, double distance)
        {
            var objs = new DBObjectCollection();
            var polygon = polyline.ToNTSPolygon();
            var buffer = BufferOp.Buffer(polygon, distance);
            if (buffer.IsEmpty)
            {
                return objs;
            }
            if (buffer is IPolygon bufferPolygon)
            {
                objs.Add(bufferPolygon.Shell.ToDbPolyline());
            }
            else if (buffer is IMultiPolygon mPolygon)
            {
                foreach(IPolygon item in mPolygon.Geometries)
                {
                    objs.Add(item.Shell.ToDbPolyline());
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
