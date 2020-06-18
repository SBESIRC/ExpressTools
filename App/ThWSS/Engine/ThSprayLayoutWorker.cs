using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Linq;
using ThWSS.Bussiness;
using ThWSS.Model;

namespace ThWSS.Engine
{
    public class ThSprayLayoutWorker
    {
        public void DoLayout(ThRoom room, SprayLayoutModel layoutModel)
        {
            var polygon = room.Properties.Values.Cast<Polyline>().ToList();

            SparyLayoutService sprayLayoutService = new SprayLayoutByBeamService();
            sprayLayoutService.LayoutSpray(polygon, layoutModel);
        }
    }
}
