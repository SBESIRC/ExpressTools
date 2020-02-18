using System;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using GeometryExtensions;
using DotNetARX;
using Dreambuild.AutoCAD;

namespace ThEssential.Distribute
{
    public static class ThDistributeExtent3dExtension
    {
        public static double Width(this Extents3d extents)
        {
            return Math.Abs(extents.MaxPoint.X - extents.MinPoint.X);
        }

        public static double Height(this Extents3d extents)
        {
            return Math.Abs(extents.MaxPoint.Y - extents.MinPoint.Y);
        }

        public static Polyline ToPolyline(this Extents3d extents)
        {
            var points = new Point3d[4]
            {
                extents.MinPoint,
                extents.MinPoint + Vector3d.YAxis * extents.Height(),
                extents.MaxPoint,
                extents.MaxPoint - Vector3d.YAxis * extents.Height()
            };

            var pline = new Polyline()
            {
                Closed = true
            };

            pline.CreatePolyline(new Point3dCollection(points));
            return pline;
        }
    }
}
