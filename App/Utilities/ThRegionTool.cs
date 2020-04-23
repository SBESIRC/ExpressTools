using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.BoundaryRepresentation;

namespace TianHua.AutoCAD.Utility.ExtensionTools
{
    public static class ThRegionTool
    {
        ///<summary>
        /// Returns whether a Region contains a Point3d.
        ///</summary>
        ///<param name="pt">A points to test against the Region.</param>
        ///<returns>A Boolean indicating whether the Region contains
        /// the point.
        /// </returns>
        public static bool ContainsPoint(this Region reg, Point3d pt)
        {
            using (var brep = new Brep(reg))
            {
                var pc = new PointContainment();
                using (var brepEnt = brep.GetPointContainment(pt, out pc))
                {
                    return pc != PointContainment.Outside;
                }
            }
        }

        ///<summary>
        /// Returns whether a Region contains a set of Point3ds.
        ///</summary>
        ///<param name="pts">An array of points to test against the Region.</param>
        ///<returns>A Boolean indicating whether the Region contains
        /// all the points.
        /// </returns>
        public static bool ContainsPoints(this Region reg, Point3dCollection ptc)
        {
            var pts = new Point3d[ptc.Count];
            ptc.CopyTo(pts, 0);
            return reg.ContainsPoints(pts);
        }

        ///<summary>
        /// Returns whether a Region contains a set of Point3ds.
        ///</summary>
        ///<param name="pts">An array of points to test against the Region.</param>
        ///<returns>A Boolean indicating whether the Region contains
        /// all the points.
        /// </returns>
        public static bool ContainsPoints(this Region reg, Point3d[] pts)
        {
            using (var brep = new Brep(reg))
            {
                foreach (var pt in pts)
                {
                    var pc = new PointContainment();
                    using (var brepEnt = brep.GetPointContainment(pt, out pc))
                    {
                        if (pc == PointContainment.Outside)
                            return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 获取Region的顶点
        /// </summary>
        /// <param name="region"></param>
        /// <returns></returns>
        public static Point3dCollection Vertices(this Region region)
        {
            var vertices = new Point3dCollection();
            if (!region.IsNull)
            {
                using (var brepRegion = new Brep(region))
                {
                    foreach (var face in brepRegion.Faces)
                    {
                        foreach (var loop in face.Loops)
                        {
                            foreach (var vertex in loop.Vertices)
                            {
                                vertices.Add(vertex.Point);
                            }
                        }
                    }
                }
            }
            return vertices;
        }
    }
}
