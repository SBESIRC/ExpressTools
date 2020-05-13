using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Linq2Acad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThWSS.LayoutRule;
using ThWSS.Utlis;

namespace ThWSS.Bussiness
{
    public class SprayLayoutService
    {
        public void LayoutSpray(List<Polyline> roomsLine)
        {
            foreach (var room in roomsLine)
            {
                //计算房间走向
                var roomOOB = OrientedBoundingBox.Calculate(room);
                using (AcadDatabase acdb = AcadDatabase.Active())
                {
                    acdb.ModelSpace.Add(roomOOB);
                }

                //计算出布置点
                SquareLayout squareLayout = new SquareLayout();
                List<List<Point3d>> layoutPts = squareLayout.Layout(roomOOB);

                //计算房间出房间内的点
                List<Point3d> roomPts = new List<Point3d>();
                foreach (var lpts in layoutPts)
                {
                    List<Point3d> checkPts = CalRoomSpray(room, lpts);
                    roomPts.AddRange(checkPts);
                }

                InsertSparyService.InsertSprayBlock(roomPts, SprayType.SPRAYDOWN);
            }
        }

        /// <summary>
        /// 计算出房间内的喷淋的布置点
        /// </summary>
        /// <param name="room"></param>
        /// <param name="layoutPts"></param>
        /// <returns></returns>
        private List<Point3d> CalRoomSpray(Polyline room, List<Point3d> layoutPts)
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
