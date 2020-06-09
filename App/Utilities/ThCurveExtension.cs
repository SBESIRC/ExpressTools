using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TianHua.AutoCAD.Utility.ExtensionTools
{
    public static class ThCurveExtension
    {
        /// <summary>
        /// 获取曲线的中点
        /// </summary>
        /// <param name="curve"></param>
        /// <returns></returns>
        public static Point3d GetMidpoint(this Curve curve)
        {
            double d1 = curve.GetDistanceAtParameter(curve.StartParam);
            double d2 = curve.GetDistanceAtParameter(curve.EndParam);
            return curve.GetPointAtDist(d1 + ((d2 - d1) / 2.0));
        }


        // Make sure the pt1 and pt2 are on the Curve before calling this method.
        //  https://spiderinnet1.typepad.com/blog/2012/10/autocad-net-isonpoint3d-curvegetclosestpointto-curvegetparameteratpoint.html
        public static double GetLength(this Curve ent, Point3d pt1, Point3d pt2)
        {
            double dist1 = ent.GetDistanceAtParameter(ent.GetParameterAtPoint(ent.GetClosestPointTo(pt1, false)));
            double dist2 = ent.GetDistanceAtParameter(ent.GetParameterAtPoint(ent.GetClosestPointTo(pt2, false)));

            return Math.Abs(dist1 - dist2);
        }

        public static double BulgeFromCurve(this Curve cv, bool clockwise)
        {
            double bulge = 0.0;
            Arc a = cv as Arc;
            if (a != null)
            {
                double newStart;
                // The start angle is usually greater than the end,
                // as arcs are all counter-clockwise.
                // (If it isn't it's because the arc crosses the
                // 0-degree line, and we can subtract 2PI from the
                // start angle.)
                if (a.StartAngle > a.EndAngle)
                    newStart = a.StartAngle - 8 * Math.Atan(1);
                else
                    newStart = a.StartAngle;

                // Bulge is defined as the tan of
                // one fourth of the included angle
                bulge = Math.Tan((a.EndAngle - newStart) / 4);
                // If the curve is clockwise, we negate the bulge
                if (clockwise)
                    bulge = -bulge;

            }
            return bulge;
        }

        /// <summary>
        /// Computes the offset distance between two lines 
        /// provided they meet all of the following criteria:
        ///
        ///    The lines are parallel
        ///    The lines are not colinear
        ///    The lines are adjacent
        ///    Neither endpoint of one line is
        ///    coincident with either endpoint
        ///    of the other line.
        ///    
        /// If all criteria are satisfied the method returns
        /// the sum of the squares of the x & y components 
        /// of the vector between the lines (slightly faster 
        /// than computing the distance, and has the same 
        /// result for relative comparisons).
        /// 
        /// If the lines don't meet the criteria, the method
        /// returns a negative value.
        /// </summary>
        /// 
        /// https://forums.autodesk.com/t5/net/finding-closest-parallel-lines-in-a-drawing/td-p/7326426
        public static double GetOffset(Line a, Line b)
        {
            Point2d a1 = AsPoint2d(a.StartPoint);
            Point2d a2 = AsPoint2d(a.EndPoint);
            Point2d b1 = AsPoint2d(b.StartPoint);
            Point2d b2 = AsPoint2d(b.EndPoint);

            double result = -1.0;

            // reject lines with coincident endpoints
            if (a1.IsEqualTo(b1) || a1.IsEqualTo(b2) || a2.IsEqualTo(b1) || a2.IsEqualTo(b2))
                return result;

            var line1 = new LineSegment2d(a1, a2);
            var line2 = new LineSegment2d(b1, b2);

            // reject non-parallel lines
            if (!line1.IsParallelTo(line2))
                return result;

            // reject colinear lines
            if (line1.IsColinearTo(line2))
                return result;

            var cp = line1.GetClosestPointTo(line2);
            Point2d cp1 = cp[0].Point;
            Point2d cp2 = cp[1].Point;

            Vector2d vx = cp1.GetVectorTo(cp2);

            // reject non-adjacent lines:
            if (vx.IsPerpendicularTo(line1.Direction))
                result = vx.LengthSqrd;

            return result;
        }

        public static Point2d AsPoint2d(this Point3d p)
        {
            return new Point2d(p.X, p.Y);
        }
    }
}
