using ThWSS.Model;
using System.Linq;
using ThWSS.Bussiness;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThWSS.Engine
{
    public class ThSprayLayoutWorker
    {
        public void DoLayout(ThRoom room, Polyline floor, SprayLayoutModel layoutModel)
        {
            var frames = room.Properties.Values.Cast<Polyline>().ToList();
            SparyLayoutService service = null;
            if (layoutModel.UseBeam)
            {
                service = new SprayLayoutByBeamService();
            }
            else
            {
                service = new SprayLayoutNoBeamService();
            }
            service.LayoutSpray(frames, floor, layoutModel);
        }
    }
}
