using System;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using GeometryExtensions;
using TianHua.AutoCAD.Utility.ExtensionTools;
using Linq2Acad;

namespace ThEssential.Equipment
{
    public static class ThEquipmentHatchExtension
    {
        public static bool ContainsPoint(this Hatch hatch, Point3d pt)
        {
            var objIds = new ObjectIdCollection();
            using (AcadDatabase acadDatabase = AcadDatabase.Use(hatch.Database))
            {
                var boundary = hatch.TracePolylineBoundary();
                objIds.Add(acadDatabase.ModelSpace.Add(boundary));
            }
            using (AcadDatabase acadDatabase = AcadDatabase.Use(hatch.Database))
            {
                try
                {
                    using (var curves = new DBObjectCollection())
                    {
                        foreach (ObjectId objId in objIds)
                        {
                            curves.Add(acadDatabase.Element<Entity>(objId));
                        }
                        using (var regions = Region.CreateFromCurves(curves))
                        {
                            foreach (Region region in regions)
                            {
                                if (region.ContainsPoint(pt))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
                catch
                {
                }
                finally
                {
                }
            }
            return false;
        }

        public static Polyline TracePolylineBoundary(this Hatch hatch)
        {
            var segments = new PolylineSegmentCollection();
            for (int i = 0; i < hatch.NumberOfLoops; i++)
            {
                Plane plane = hatch.GetPlane();
                HatchLoop loop = hatch.GetLoopAt(i);
                foreach (Curve2d cv in loop.Curves)
                {
                    LineSegment2d line2d = cv as LineSegment2d;
                    CircularArc2d arc2d = cv as CircularArc2d;
                    EllipticalArc2d ellipse2d = cv as EllipticalArc2d;
                    NurbCurve2d spline2d = cv as NurbCurve2d;
                    if (line2d != null)
                    {
                        segments.Add(new PolylineSegment(line2d));
                    }
                    else if (arc2d != null)
                    {
                        segments.Add(new PolylineSegment(arc2d));
                    }
                    else if (ellipse2d != null)
                    {
                        NurbCurve2d nurbCurve = new NurbCurve2d(ellipse2d);
                        segments.AddRange(nurbCurve.ToPolylineSegments(plane));
                    }
                    else if (spline2d != null)
                    {
                        segments.AddRange(spline2d.ToPolylineSegments(plane));
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                }
            }
            segments.Join();
            return segments.ToPolyline();
        }
    }
}
