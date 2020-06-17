using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThWSS.Bussiness.SprayLayout
{
    public class SprayLayoutData
    {
        public Curve Radii { get; set; }
        public Point3d Position { get; set; }
    }
}
