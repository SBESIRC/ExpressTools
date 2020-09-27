using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThWSS.Bussiness
{
    public class SprayLayoutData
    {
        public Curve Radii { get; set; }
        public Point3d Position { get; set; }
        public Vector3d mainDir { get; set; }    //排布主方向
        public Vector3d otherDir { get; set; }   //排布次方向
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
