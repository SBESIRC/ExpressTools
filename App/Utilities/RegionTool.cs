using Autodesk.AutoCAD.BoundaryRepresentation;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TianHua.AutoCAD.Utility.ExtensionTools
{
    public static class RegionTool
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


    }
}
