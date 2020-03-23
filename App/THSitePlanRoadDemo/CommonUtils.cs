using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace THSitePlanRoadDemo
{
    class CommonUtils
    {
        /// 零值判断
        public static bool IsAlmostNearZero(double val, double tolerance = 1e-9)
        {
            if (val > -tolerance && val < tolerance)
                return true;
            return false;
        }

        /// <summary>
        /// 调整因为精度问题导致的值范围
        /// </summary>
        /// <param name="rad"></param>
        /// <returns></returns>
        public static double CutRadRange(double rad)
        {
            if (IsAlmostNearZero(rad - 1))
                return 1;
            else if (IsAlmostNearZero(rad + 1))
                return -1;
            return rad;
        }

        /// <summary>
        /// 判断两个点是否相等
        /// </summary>
        /// <param name="ptFirst"></param>
        /// <param name="ptSecond"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static bool Point2dIsEqualPoint2d(Point2d ptFirst, Point2d ptSecond, double tolerance = 1e-6)
        {
            if (ptFirst.GetDistanceTo(ptSecond) < tolerance)
                return true;
            return false;
        }

        /// <summary>
        /// 空间夹角计算
        /// </summary>
        /// <param name="dir1"></param>
        /// <param name="dir2"></param>
        /// <returns></returns>
        public static double CalAngle(XY dir1, XY dir2)
        {
            double val = dir1.X * dir2.X + dir1.Y * dir2.Y;
            double tmp = Math.Sqrt(Math.Pow(dir1.X, 2) + Math.Pow(dir1.Y, 2)) * Math.Sqrt(Math.Pow(dir2.X, 2) + Math.Pow(dir2.Y, 2));
            double angleRad = Math.Acos(CommonUtils.CutRadRange(val / tmp));
            return angleRad;
        }

        public static XY Vector2XY(Vector2d vec)
        {
            return new XY(vec.X, vec.Y);
        }

        /// <summary>
        /// 点在线段上面
        /// </summary>
        /// <param name="point"></param>
        /// <param name="line"></param>
        /// <param name="tole"></param>
        /// <returns></returns>
        public static bool IsPointOnSegment(Point2d point, LineSegment2d line, double tole = 1e-8)
        {
            var ptS = line.StartPoint;
            var ptE = line.EndPoint;
            var lengthS = (ptS - point).Length;
            var lengthE = (ptE - point).Length;
            var lengthDiff = lengthS + lengthE - (ptS - ptE).Length;
            if (CommonUtils.IsAlmostNearZero(lengthDiff, tole))
                return true;

            return false;
        }
    }
}
