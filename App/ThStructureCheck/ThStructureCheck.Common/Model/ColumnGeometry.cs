using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThStructureCheck.Common.Interface;

namespace ThStructureCheck.Common.Model
{
    public abstract class ColumnGeometry
    {
        public Coordinate Origin { get; set; }
        public double EccX { get; set; }
        public double EccY { get; set; }
        public double Rotation { get; set; }
        public double B { get; set; }
        public double H { get; set; }        
    }
    public class RectangleColumnGeometry: ColumnGeometry,IEntity
    {        
        public Entity Draw()
        {
            Polyline polyline = null;
            Point3d cenPt = Origin.Coord;
            cenPt = cenPt + new Vector3d(EccX, EccY, 0.0);
            Point3dCollection pts = new Point3dCollection();
            Point3d pt1 = cenPt + new Vector3d(B / 2.0, H / 2.0, 0.0);
            Point3d pt2 = cenPt + new Vector3d(-B / 2.0, H / 2.0, 0.0);
            Point3d pt3 = cenPt + new Vector3d(-B / 2.0, -H / 2.0, 0.0);
            Point3d pt4 = cenPt + new Vector3d(B / 2.0, -H / 2.0, 0.0);
            pts.Add(pt1);
            pts.Add(pt2);
            pts.Add(pt3);
            pts.Add(pt4);
            polyline = CadTool.CreatePolyline(pts);
            Matrix3d mt = Matrix3d.Rotation(this.Rotation, Vector3d.ZAxis, cenPt);
            polyline.TransformBy(mt);
            return polyline;
        }
    }

}
