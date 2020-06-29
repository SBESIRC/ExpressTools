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
        public static double GetInsertBeamDis(IEntity colunmOrWall,IEntity beam)
        {
            if((colunmOrWall is RectangleColumnGeometry || colunmOrWall is LineWallGeometry) &&
                beam is LineBeamGeometry lineBeamGeometry)
            {
                return GetInsertBeamDis(colunmOrWall, lineBeamGeometry);
            }
            return 0.0;
        }
        /// <summary>
        /// 获取梁钢筋插入到柱子或墙的深度
        /// </summary>
        /// <param name="lineEnt"></param>
        /// <param name="beam"></param>
        /// <returns></returns>
        public static double GetInsertBeamDis(IEntity lineEnt, LineBeamGeometry beam)
        {
            double dis = 0.0;
            Entity columnEntity = lineEnt.Draw();
            Entity beamEntity = beam.DrawCenterLine();
            if (columnEntity == null || beamEntity == null)
            {
                return dis;
            }
            Point3dCollection pts = new Point3dCollection();
            columnEntity.IntersectWith(beamEntity, Intersect.OnBothOperands, pts, IntPtr.Zero, IntPtr.Zero);
            if (pts.Count == 0)
            {
                return dis;
            }
            List<Point3d> recPts = CadTool.GetPolylinePts(columnEntity as Polyline);
            Point2dCollection rec2dPts = new Point2dCollection();
            recPts.ForEach(i => rec2dPts.Add(new Point2d(i.X, i.Y)));
            Point2d startPt = new Point2d(beam.StartPoint.Coord.X, beam.StartPoint.Coord.Y);
            Point2d endPt = new Point2d(beam.EndPoint.Coord.X, beam.EndPoint.Coord.Y);
            Point2d basePt = startPt;
            if (CadTool.IsPointInPolyline(rec2dPts, startPt))
            {
                basePt = startPt;
            }
            else if (CadTool.IsPointInPolyline(rec2dPts, endPt))
            {
                basePt = endPt;
            }
            else
            {
                return dis;
            }
            if (pts.Count == 1)
            {
                dis = basePt.GetDistanceTo(new Point2d(pts[0].X, pts[0].Y));
            }
            else if (pts.Count == 2)
            {
                foreach (Point3d pt in pts)
                {
                    dis = basePt.GetDistanceTo(new Point2d(pt.X, pt.Y));
                    if (dis <= 1.0)
                    {
                        continue;
                    }
                }
            }
            return dis;
        }
    }
}
