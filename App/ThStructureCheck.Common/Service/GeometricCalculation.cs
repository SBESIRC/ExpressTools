using Autodesk.AutoCAD.Geometry;
using ThStructureCheck.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using ThStructureCheck.Common.Interface;

namespace ThStructureCheck.Common.Service
{
    public class GeometricCalculation
    {
        /// <summary>
        /// 获取偏移方向
        /// </summary>
        /// <param name="startPt"></param>
        /// <param name="endPt"></param>
        /// <returns></returns>
        public static Vector3d GetOffsetDirection(Point3d startPt, Point3d endPt)
        {
            double rangeAngle = 1.0;
            Vector3d lineVec = startPt.GetVectorTo(endPt);
            //与X轴平行 1度范围内
            double rad = lineVec.GetAngleTo(Vector3d.XAxis);
            double angle = Utils.RadToAng(rad);
            if (angle <= rangeAngle)
            {
                return Vector3d.YAxis;
            }

            //与Y轴平行 1度范围内
            rad = lineVec.GetAngleTo(Vector3d.YAxis);
            angle = Utils.RadToAng(rad);
            if (angle <= rangeAngle)
            {
                return Vector3d.XAxis.Negate();
            }

            Plane plane = new Plane(Point3d.Origin, Vector3d.ZAxis);
            double counterClockAngle = lineVec.AngleOnPlane(plane);
            counterClockAngle %= (Math.PI * 2);
            counterClockAngle = Utils.RadToAng(counterClockAngle);
            if ((counterClockAngle > 0.0 && counterClockAngle < 90) ||
                (counterClockAngle > 180.0 && counterClockAngle < 270.0))
            {
                double minX = Math.Min(startPt.X, endPt.X);
                double minY = Math.Min(startPt.Y, endPt.Y);

                double maxX = Math.Max(startPt.X, endPt.X);
                double maxY = Math.Max(startPt.Y, endPt.Y);

                Point3d leftDownPt = new Point3d(minX, minY, 0);
                Point3d rightUpPt = new Point3d(maxX, maxY, 0);
                Vector3d vec = leftDownPt.GetVectorTo(rightUpPt);
                return vec.GetPerpendicularVector();
            }

            if ((counterClockAngle > 90.0 && counterClockAngle < 180.0) ||
                (counterClockAngle > 270.0 && counterClockAngle < 360.0))
            {
                double minX = Math.Min(startPt.X, endPt.X);
                double minY = Math.Min(startPt.Y, endPt.Y);

                double maxX = Math.Max(startPt.X, endPt.X);
                double maxY = Math.Max(startPt.Y, endPt.Y);

                Point3d rightDownPt = new Point3d(maxX, minY, 0);
                Point3d leftUpPt = new Point3d(minX, maxY, 0);
                Vector3d vec = leftUpPt.GetVectorTo(rightDownPt);
                return vec.GetPerpendicularVector();
            }
            plane.Dispose();
            return lineVec.GetPerpendicularVector();
        }
    }
}
