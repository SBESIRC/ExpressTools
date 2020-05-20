using System;
using System.Linq;
using GeoAPI.Geometries;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using NetTopologySuite.Operation.Union;
using NetTopologySuite.Operation.Polygonize;
using NetTopologySuite.Utilities;

namespace ThCADCore.NTS
{
    public static class ThCADCoreNTSDbExtension
    {
        public static Polyline ToDbPolyline(this ILineString lineString)
        {
            var pline = new Polyline();
            for(int i = 0; i < lineString.Coordinates.Length; i++)
            {
                pline.AddVertexAt(i, lineString.Coordinates[i].ToAcGePoint2d(), 0, 0, 0);
            }
            pline.Closed = lineString.StartPoint.EqualsExact(lineString.EndPoint);
            return pline;
        }

        public static Polyline ToDbPolyline(this ILinearRing linearRing)
        {
            var pline = new Polyline()
            {
                Closed = true
            };
            for (int i = 0; i < linearRing.Coordinates.Length; i++)
            {
                pline.AddVertexAt(i, linearRing.Coordinates[i].ToAcGePoint2d(), 0, 0, 0);
            }
            return pline;
        }

        public static List<Polyline> ToDbPolylines(this IPolygon polygon)
        {
            var plines = new List<Polyline>();
            plines.Add(polygon.Shell.ToDbPolyline());
            foreach(ILinearRing hole in polygon.Holes)
            {
                plines.Add(hole.ToDbPolyline());
            }
            return plines;
        }

        public static List<Polyline> ToDbPolylines(this IMultiLineString geometries)
        {
            var plines = new List<Polyline>();
            foreach(var geometry in geometries.Geometries)
            {
                if (geometry is ILineString lineString)
                {
                    plines.Add(lineString.ToDbPolyline());
                }
                else if (geometry is ILinearRing linearRing)
                {
                    plines.Add(linearRing.ToDbPolyline());
                }
                else if (geometry is IPolygon polygon)
                {
                    plines.AddRange(polygon.ToDbPolylines());
                }
                else if (geometry is IMultiLineString multiLineString)
                {
                    plines.AddRange(multiLineString.ToDbPolylines());
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            return plines;
        }

        public static Line ToDbLine(this NetTopologySuite.Geometries.LineSegment segment)
        {
            return new Line()
            {
                StartPoint = segment.P0.ToAcGePoint3d(),
                EndPoint = segment.P1.ToAcGePoint3d()
            };
        }

        public static Region ToDbRegion(this IPolygon polygon)
        {
            // 暂时不考虑有“洞”的情况
            var curves = new DBObjectCollection();
            curves.Add(polygon.Shell.ToDbPolyline());
            return Region.CreateFromCurves(curves)[0] as Region;
        }

        public static IGeometry ToNTSLineString(this Polyline polyLine)
        {
            var points = new List<Coordinate>();
            for (int i = 0; i < polyLine.NumberOfVertices; i++)
            {
                // 暂时不考虑“圆弧”的情况
                points.Add(polyLine.GetPoint3dAt(i).ToNTSCoordinate());
            }
            if (polyLine.Closed)
            {
                // 闭合的情况
                points.Add(polyLine.GetPoint3dAt(0).ToNTSCoordinate());
                return ThCADCoreNTSService.Instance.GeometryFactory.CreateLinearRing(points.ToArray());
            }
            else
            {
                // 不闭合的情况
                return ThCADCoreNTSService.Instance.GeometryFactory.CreateLineString(points.ToArray());
            }
        }

        public static ILineString ToNTSLineString(this Line line)
        {
            var points = new List<Coordinate>();
            points.Add(line.StartPoint.ToNTSCoordinate());
            points.Add(line.EndPoint.ToNTSCoordinate());
            if (points.Count > 1)
            {
                return ThCADCoreNTSService.Instance.GeometryFactory.CreateLineString(points.ToArray());
            }
            else
            {
                return null;
            }
        }

        public static IGeometry ToNTSGeometry(this Region region)
        {
            var coordinates = new List<Coordinate>(region.Vertices().ToNTSCoordinates());
            // A LinearRing containing n coordinates is implemented with an array
            // of Coordinates containing n+1 points, and coord[0] = coord[n]
            coordinates.Add(coordinates[0]);
            return ThCADCoreNTSService.Instance.GeometryFactory.CreatePolygon(coordinates.ToArray());
        }

        public static ILineString ToNTSLineString(this Arc arc, int numPoints)
        {
            var shapeFactory = new GeometricShapeFactory(ThCADCoreNTSService.Instance.GeometryFactory)
            {
                Centre = arc.Center.ToNTSCoordinate(),
                Size = 2 * arc.Radius,
                NumPoints = numPoints
            };
            return shapeFactory.CreateArc(arc.StartAngle, arc.TotalAngle);
        }

        /// <summary>
        /// 按弦长细化
        /// </summary>
        /// <param name="arc"></param>
        /// <param name="chord"></param>
        /// <returns></returns>
        public static ILineString TessellateWithChord(this Arc arc, double chord)
        {
            // 根据弦长，半径，计算对应的弧长
            // Chord Length = 2 * Radius * sin(angle / 2.0)
            // Arc Length = Radius * angle (angle in radians)
            if (chord > 2 * arc.Radius )
            {
                return null;
            }

            double radius = arc.Radius;
            double angle = 2 * Math.Asin(chord / (2 * radius));
            return arc.TessellateWithArc(radius * angle);
        }

        /// <summary>
        /// 按弧长细化
        /// </summary>
        /// <param name="arc"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static ILineString TessellateWithArc(this Arc arc, double length)
        {
            if (arc.Length < length)
            {
                return null;
            }

            return arc.ToNTSLineString(Convert.ToInt32(Math.Floor(arc.Length / length)) + 1);
        }

        public static IGeometry ToNTSNodedLineStrings(this DBObjectCollection curves, double chord = 5.0)
        {
            IGeometry nodedLineString = ThCADCoreNTSService.Instance.GeometryFactory.CreateLineString();
            foreach(DBObject curve in curves)
            {
                if (curve is Line line)
                {
                    nodedLineString = nodedLineString.Union(line.ToNTSLineString());
                }
                else if (curve is Polyline polyline)
                {
                    nodedLineString = nodedLineString.Union(polyline.ToNTSLineString());
                }
                else if (curve is Arc arc)
                {
                    nodedLineString = nodedLineString.Union(arc.TessellateWithChord(chord));
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            return nodedLineString;
        }

        public static List<IGeometry> ToNTSLineStrings(this DBObjectCollection curves, double chord = 5.0)
        {
            var geometries = new List<IGeometry>();
            foreach(DBObject curve in curves)
            {
                if (curve is Line line)
                {
                    geometries.Add(line.ToNTSLineString());
                }
                else if (curve is Arc arc)
                {
                    geometries.Add(arc.TessellateWithChord(chord));
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            return geometries;
        }

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
    }
}
