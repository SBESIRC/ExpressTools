using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Linq;
using ThWSS.Bussiness;
using ThWSS.Bussiness.SparyLayout;
using ThWSS.Model;

namespace ThWSS.Engine
{
    public class ThSprayLayoutWorker
    {
        public void DoLayout(ThRoom room, SparyLayoutModel layoutModel)
        {
            var polygon = room.Properties.Values.Cast<Polyline>().ToList();

            SparyLayoutService sprayLayoutService = new SparyLayoutByBeamService();
            sprayLayoutService.LayoutSpray(polygon, layoutModel);
        }
    }
}
