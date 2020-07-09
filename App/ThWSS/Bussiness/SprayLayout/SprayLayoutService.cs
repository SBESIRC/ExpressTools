﻿using Linq2Acad;
using ThWSS.Model;
using ThWSS.Utlis;
using System.Linq;
using ThWSS.Layout;
using ThCADCore.NTS;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThWSS.Bussiness
{
    public abstract class SparyLayoutService
    {
        public virtual void CleanSpray(List<Polyline> roomsLine) { }
        public virtual void LayoutSpray(List<Polyline> roomsLine, Polyline floor, SprayLayoutModel layoutModel) { }

        /// <summary>
        /// 计算出房间内的喷淋的布置点
        /// </summary>
        /// <param name="room"></param>
        /// <param name="layoutPts"></param>
        /// <returns></returns>
        public List<SprayLayoutData> CalRoomSpray(Polyline room, List<SprayLayoutData> sprays, out List<SprayLayoutData> outsideSpray)
        {
            outsideSpray = new List<SprayLayoutData>();
            var roomSprays = new List<SprayLayoutData>();
            foreach (var spray in sprays)
            {
                if(room.PointInPolygon(spray.Position) == LocateStatus.Interior)
                {
                    roomSprays.Add(spray);
                }
                else
                {
                    outsideSpray.Add(spray);
                }
            }
            return roomSprays;
        }

        /// <summary>
        /// 清除房间内的喷淋
        /// </summary>
        /// <param name="roomsLine"></param>
        public void DoCleanSpray(List<Polyline> roomsLine)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var objs = new DBObjectCollection();
                var sprays = acadDatabase.ModelSpace
                    .OfType<BlockReference>()
                    .Where(o => o.GetEffectiveName() == ThWSSCommon.SprayBlockName);
                foreach (var room in roomsLine)
                {
                    sprays.Where(o => room.PointInPolygon(o.Position) == LocateStatus.Interior)
                        .ForEachDbObject(o => objs.Add(o));
                }
                foreach (BlockReference spray in objs)
                {
                    spray.UpgradeOpen();
                    spray.Erase();
                }
            }
        }
    }
}
