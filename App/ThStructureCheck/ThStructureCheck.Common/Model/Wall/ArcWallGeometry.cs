using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThStructureCheck.Common.Interface;
using ThStructureCheck.Common.Service;

namespace ThStructureCheck.Common.Model.Wall
{
    public class ArcWallGeometry : WallGeometry, IEntity
    {
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
            Point3dCollection pts = new Point3dCollection();
            pts.Add(pt1);
            pts.Add(pt2);
            pts.Add(pt3);
            pts.Add(pt4);
            polyline = CadTool.CreatePolyline(pts);
            return polyline;
        }
    }
}
