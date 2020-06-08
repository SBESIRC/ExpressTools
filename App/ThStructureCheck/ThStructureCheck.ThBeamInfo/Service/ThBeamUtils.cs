using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThStructureCheck.YJK.Model;

namespace ThStructureCheck.ThBeamInfo.Service
{
    public static class ThBeamUtils
    {
        /// <summary>
        /// 把YJK的Point转成AutoCAD.Geomtry.Point3d
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Point3d PointToPoint3d(Point point)
        {
            return new Point3d(point.X,point.Y,point.Z);
        }
    }
}
