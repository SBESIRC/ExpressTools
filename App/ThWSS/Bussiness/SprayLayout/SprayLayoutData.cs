using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThWSS.Bussiness
{
    public class SprayLayoutData
    {
        public Curve Radii { get; set; }
        public Point3d Position { get; set; }
        public static SprayLayoutData Create(Point3d pos, Curve radii)
        {
            return new SprayLayoutData()
            {
                Radii = radii,
                Position = pos,
            };
        }
    }
}
