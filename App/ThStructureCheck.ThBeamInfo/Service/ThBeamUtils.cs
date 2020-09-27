using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThStructureCheck.Common;
using ThStructureCheck.YJK.Model;

namespace ThStructureCheck.ThBeamInfo.Service
{
    public static class ThBeamUtils
    {
        private static double angleRange = 1.0; //在这个范围视为平行
        public static bool ThreePointsCollinear(Point a, Point b, Point c)
        {
            return CadTool.ThreePointsCollinear(a, b, c, angleRange);
        }
    }
}
