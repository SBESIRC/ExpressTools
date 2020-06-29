using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThStructureCheck.Common.Interface;
using ThStructureCheck.Common.Service;

namespace ThStructureCheck.Common.Model
{
    public abstract class BeamGeometry
    {
        public Coordinate StartPoint { get; set; }
        public Coordinate EndPoint { get; set; }
        public double Ecc { get; set; }
        public double Rotation { get; set; }
        public double B { get; set; }
        public double H { get; set; }
        public abstract Polyline DrawCenterLine();
    }
    public class LineBeamGeometry: BeamGeometry,IEntity
    {
        public override Polyline DrawCenterLine()
        {
            Polyline polyline = null;
            Vector3d offsetVec = GeometricCalculation.GetOffsetDirection(StartPoint.Coord, EndPoint.Coord);
            Point3d sp = StartPoint.Coord + offsetVec.GetNormal().MultiplyBy(this.Ecc);
            Point3d ep = EndPoint.Coord + offsetVec.GetNormal().MultiplyBy(this.Ecc);
            if(sp.DistanceTo(ep)>0.0)
            {
                polyline = new Polyline();
                polyline.AddVertexAt(0, new Point2d(sp.X, sp.Y), 0.0, 0.0, 0.0);
                polyline.AddVertexAt(1, new Point2d(ep.X, ep.Y), 0.0, 0.0, 0.0);
                polyline.Closed = false;
            }
            return polyline;
        }
        public Entity Draw()
        {
            Polyline polyline = null;
            Vector3d offsetVec = GeometricCalculation.GetOffsetDirection(StartPoint.Coord, EndPoint.Coord);
            Point3d sp = StartPoint.Coord + offsetVec.GetNormal().MultiplyBy(this.Ecc);
            Point3d ep = EndPoint.Coord + offsetVec.GetNormal().MultiplyBy(this.Ecc);
            Point3d pt1 = sp + offsetVec.GetNormal().MultiplyBy(this.B / 2.0);
            Point3d pt2 = ep + offsetVec.GetNormal().MultiplyBy(this.B / 2.0);
            Point3d pt4 = sp - offsetVec.GetNormal().MultiplyBy(this.B / 2.0);
            Point3d pt3 = ep - offsetVec.GetNormal().MultiplyBy(this.B / 2.0);
            polyline.AddVertexAt(0, new Point2d(pt1.X, pt1.Y), 0.0, 0.0, 0.0);
            polyline.AddVertexAt(1, new Point2d(pt2.X, pt2.Y), 0.0, 0.0, 0.0);
            polyline.AddVertexAt(2, new Point2d(pt3.X, pt3.Y), 0.0, 0.0, 0.0);
            polyline.AddVertexAt(3, new Point2d(pt4.X, pt4.Y), 0.0, 0.0, 0.0);
            polyline.Closed = true;
            return polyline;
        }
    }
}
