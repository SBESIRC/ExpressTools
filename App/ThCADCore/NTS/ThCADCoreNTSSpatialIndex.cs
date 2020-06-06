using System;
using GeoAPI.Geometries;
using NetTopologySuite.Index.Strtree;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThCADCore.NTS
{
    public class ThCADCoreNTSSpatialIndex
    {
        private STRtree<IGeometry> Engine { get; set; }
        public ThCADCoreNTSSpatialIndex(DBObjectCollection objs)
        {
            Engine = new STRtree<IGeometry>();
            Initialize(objs);
        }

        private void Initialize(DBObjectCollection objs)
        {
            foreach(Curve obj in objs)
            {
                if (obj is Line line)
                {
                    AddGeometry(line.ToNTSLineString());
                }
                else if (obj is Polyline polyline)
                {
                    AddGeometry(polyline.ToNTSLineString());
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
        }

        private void AddGeometry(IGeometry geometry)
        {
            Engine.Insert(geometry.EnvelopeInternal, geometry);
        }

        private void RemoveGeometry(IGeometry geometry)
        {
            Engine.Remove(geometry.EnvelopeInternal, geometry);
        }

        public DBObjectCollection Query(Polyline window)
        {
            var objs = new DBObjectCollection();
            var frame = window.ToNTSPolygon();
            foreach(var geometry in Engine.Query(frame.EnvelopeInternal))
            {
                if (geometry is ILineString lineString)
                {
                    objs.Add(lineString.ToDbPolyline());
                }
                else if (geometry is ILinearRing linearRing)
                {
                    objs.Add(linearRing.ToDbPolyline());
                }
                else if (geometry is IPolygon polygon)
                {
                    objs.Add(polygon.Shell.ToDbPolyline());
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            return objs;
        }
    }
}
