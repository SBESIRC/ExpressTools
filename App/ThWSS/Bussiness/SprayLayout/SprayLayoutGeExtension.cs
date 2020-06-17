using ThCADCore.NTS;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThWSS.Bussiness.SprayLayout
{
    public static class SprayLayoutGeExtension
    {
        public static bool IsInRegion(this SprayLayoutData spray, Polyline region)
        {
            return region.PointInPolygon(spray.Position) == LocateStatus.Interior;
        }
    }
}
