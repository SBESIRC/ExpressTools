using System;
using System.Linq;
using GeoAPI.Geometries;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using NetTopologySuite.Operation.Union;
using NetTopologySuite.Operation.Polygonize;
using NetTopologySuite.Utilities;

namespace ThSitePlan.NTS
{
    public static class ThSitePlanNTSDbExtension
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

        public static Line ToDbLine(this NetTopologySuite.Geometries.LineSegment segment)
        {
            return new Line()
            {
                StartPoint = segment.P0.ToAcGePoint3d(),
                EndPoint = segment.P1.ToAcGePoint3d()
            };
        }

        public static ILineString ToNTSLineString(this Polyline polyLine)
        {
            var points = new List<Coordinate>();
            for(int i = 0; i < polyLine.NumberOfVertices; i++)
            {
                switch(polyLine.GetSegmentType(i))
                {
                    case SegmentType.Line:
                        {
                            points.Add(polyLine.GetPoint3dAt(i).ToNTSCoordinate());
                        }
                        break;
                    default:
                        throw new NotSupportedException();

                };
            }
            if (polyLine.Closed)
            {
                points.Add(points[0]);
            }

            if (points.Count > 1)
            {
                return ThSitePlanNTSService.Instance.GeometryFactory.CreateLineString(points.ToArray());
            }
            else
            {
                return null; 
            }
        }

        public static ILineString ToNTSLineString(this Line line)
        {
            var points = new List<Coordinate>();
            points.Add(line.StartPoint.ToNTSCoordinate());
            points.Add(line.EndPoint.ToNTSCoordinate());
            if (points.Count > 1)
            {
                return ThSitePlanNTSService.Instance.GeometryFactory.CreateLineString(points.ToArray());
            }
            else
            {
                return null;
            }
        }

        public static ILineString ToNTSLineString(this Arc arc, int numPoints)
        {
            var shapeFactory = new GeometricShapeFactory(ThSitePlanNTSService.Instance.GeometryFactory)
            {
                Centre = arc.Center.ToNTSCoordinate(),
                Size = 2 * arc.Radius,
                NumPoints = numPoints
            };
            return shapeFactory.CreateArc(arc.StartAngle, arc.TotalAngle);
        }

        public static IGeometry ToNTSNodedLineStrings(this DBObjectCollection lines)
        {
            IGeometry nodedLineString = ThSitePlanNTSService.Instance.GeometryFactory.CreateLineString();
            foreach(DBObject obj in lines)
            {
                if (obj is Line line)
                {
                    // 暂时只支持最简单的线段
                    nodedLineString = nodedLineString.Union(line.ToNTSLineString());
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            return nodedLineString;
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
    }
}
