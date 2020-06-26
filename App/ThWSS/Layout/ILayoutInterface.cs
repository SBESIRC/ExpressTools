using ThWSS.Bussiness;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThWSS.Layout
{
    public interface ILayoutInterface
    {
        /// <summary>
        /// 排布
        /// </summary>
        /// <param name="polyline">传入矩形polygon</param>
        List<List<SprayLayoutData>> Layout(Polyline room, Polyline polyline, bool noBeam = true);
    }
}
