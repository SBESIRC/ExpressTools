using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThWSS.Model;
using ThWSS.Utlis;

namespace ThWSS.Bussiness.SparyLayout
{
    public abstract class SparyLayoutService
    {
        public virtual void LayoutSpray(List<Polyline> roomsLine, SparyLayoutModel layoutModel) { }

        /// <summary>
        /// 计算出房间内的喷淋的布置点
        /// </summary>
        /// <param name="room"></param>
        /// <param name="layoutPts"></param>
        /// <returns></returns>
        public List<Point3d> CalRoomSpray(Polyline room, List<Point3d> layoutPts)
        {
            List<Point3d> roomP = new List<Point3d>();
            foreach (var pt in layoutPts)
            {
                var res = GeUtils.CheckPointInPolyline(room, pt, 1.0E-4);
                if (res == 1)
                {
                    roomP.Add(pt);
                }
            }

            return roomP;
        }
    }
}
