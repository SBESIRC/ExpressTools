using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThWSS.Bussiness;

namespace ThWSS.LayoutRule
{
    interface LayoutInterface
    {
        /// <summary>
        /// 排布
        /// </summary>
        /// <param name="polyline">传入矩形polygon</param>
        List<List<SprayLayoutData>> Layout(Polyline room, Polyline polyline, bool noBeam = true);
    }
}
