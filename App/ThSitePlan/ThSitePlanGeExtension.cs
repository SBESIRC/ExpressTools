using System;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThSitePlan
{
    public static class ThSitePlanGeExtension
    {
        public static Point3d Offset(this Vector3d vector)
        {
            return new Point3d(vector.ToArray());
        }

        public static double Width(this Extents3d extents)
        {
            return Math.Abs(extents.MaxPoint.X - extents.MinPoint.X);
        }

        public static double Height(this Extents3d extents)
        {
            return Math.Abs(extents.MaxPoint.Y - extents.MinPoint.Y);
        }
    }
}
