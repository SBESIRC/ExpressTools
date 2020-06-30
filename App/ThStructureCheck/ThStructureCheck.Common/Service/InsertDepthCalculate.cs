using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThStructureCheck.Common.Interface;
using ThStructureCheck.Common.Model;

namespace ThStructureCheck.Common.Service
{
    public class InsertDepthCalculate
    {
        private IEntity beam;
        private IEntity linkEntity;
        private double insertDepth = 0.0;
        /// <summary>
        /// 梁插入到连接物体的深度
        /// </summary>
        public double InsertDepth => insertDepth;
        public InsertDepthCalculate(IEntity beam,IEntity linkEntity)
        {
            this.beam = beam;
            this.linkEntity = linkEntity;
        }
        public void Calculate()
        {
            if(this.linkEntity==null || this.beam ==null)
            {
                return;
            }
            if ((this.linkEntity is RectangleColumnGeometry || this.linkEntity is LineWallGeometry) &&
                this.beam is LineBeamGeometry)
            {
               this.insertDepth = GetLineBeamRecEntInsertDepth();
            }
        }
        /// <summary>
        /// 获取梁钢筋插入到柱子或墙的深度
        /// </summary>
        /// <param name="lineEnt"></param>
        /// <param name="beam"></param>
        /// <returns></returns>
        private double GetLineBeamRecEntInsertDepth()
        {
            double dis = 0.0;
            Entity linkEntity = this.linkEntity.Draw();
            LineBeamGeometry lineBeamGeo = beam as LineBeamGeometry;
            Entity beamEntity = lineBeamGeo.DrawCenterLine();
            Point3dCollection pts = new Point3dCollection();
            linkEntity.IntersectWith(beamEntity, Intersect.OnBothOperands, pts, IntPtr.Zero, IntPtr.Zero);
            if (pts.Count == 0)
            {
                return dis;
            }
            List<Point3d> recPts = CadTool.GetPolylinePts(linkEntity as Polyline);
            Point2dCollection rec2dPts = new Point2dCollection();
            recPts.ForEach(i => rec2dPts.Add(new Point2d(i.X, i.Y)));
            Point2d startPt = new Point2d(lineBeamGeo.StartPoint.Coord.X, lineBeamGeo.StartPoint.Coord.Y);
            Point2d endPt = new Point2d(lineBeamGeo.EndPoint.Coord.X, lineBeamGeo.EndPoint.Coord.Y);
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
