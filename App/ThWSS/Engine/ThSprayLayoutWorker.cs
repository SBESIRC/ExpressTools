using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Linq;
using ThWSS.Bussiness;

namespace ThWSS.Engine
{
    public class ThSprayLayoutWorker
    {
        public void DoLayout(ThRoom room)
        {
            var polygon = room.Properties.Values.Cast<Polyline>().ToList();

            SprayLayoutService sprayLayoutService = new SprayLayoutService();
            sprayLayoutService.LayoutSpray(polygon);
        }
    }
}
