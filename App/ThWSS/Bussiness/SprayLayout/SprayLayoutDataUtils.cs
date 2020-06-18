using System.Linq;
using ThCADCore.NTS;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThWSS.Bussiness
{
    public class SprayLayoutDataUtils
    {
        public static DBObjectCollection Radii(List<SprayLayoutData> sprays)
        {
            var objs = new DBObjectCollection();
            foreach(var curve in sprays.Select(o => o.Radii))
            {
                objs.Add(curve);
            }

            return objs.Union();
        }
    }
}
