using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThWSS.Model;
using ThWSS.Utlis;

namespace ThWSS.Bussiness
{
    public abstract class SparyLayoutService
    {
        public virtual void LayoutSpray(List<Polyline> roomsLine, SprayLayoutModel layoutModel) { }

        /// <summary>
        /// 计算出房间内的喷淋的布置点
        /// </summary>
        /// <param name="room"></param>
        /// <param name="layoutPts"></param>
        /// <returns></returns>
        public List<SprayLayoutData> CalRoomSpray(Polyline room, List<SprayLayoutData> sprays)
        {
            var roomSprays = new List<SprayLayoutData>();
            foreach (var spray in sprays)
            {
                var res = GeUtils.CheckPointInPolyline(room, spray.Position, 1.0E-4);
                if (res == 1)
                {
                    roomSprays.Add(spray);
                }
            }
            return roomSprays;
        }
    }
}
