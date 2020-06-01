using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Linq2Acad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThWSS.LayoutRule;
using ThWSS.Model;
using ThWSS.Utlis;

namespace ThWSS.Bussiness
{
    public class SprayLayoutService
    {
        public void LayoutSpray(List<Polyline> roomsLine, SparyLayoutModel layoutModel)
        {
            foreach (var room in roomsLine)
            {
                //预处理房间
                var rommBounding = GeUtils.CreateConvexPolygon(room, 1500);

                //区域分割
                RegionDivisionUtils regionDivisionUtils = new RegionDivisionUtils();
                var diviRoom =regionDivisionUtils.DivisionRegion(rommBounding);
                using (AcadDatabase acdb = AcadDatabase.Active())
                {
                    //acdb.ModelSpace.Add(rommBounding);
                    foreach (var item in diviRoom)
                    {
                        //acdb.ModelSpace.Add(item);
                    }
                }
                //continue;

                foreach (var dRoom in diviRoom)
                {
                    //计算房间走向
                    var roomOOB = OrientedBoundingBox.Calculate(dRoom);

                    //计算出布置点
                    SquareLayout squareLayout = new SquareLayout(layoutModel);
                    List<List<Point3d>> layoutPts = squareLayout.Layout(dRoom, roomOOB);

                    //计算房间出房间内的点
                    List<Point3d> roomPts = new List<Point3d>();
                    foreach (var lpts in layoutPts)
                    {
                        //List<Point3d> checkPts = CalRoomSpray(room, lpts);
                        roomPts.AddRange(lpts);
                    }

                    //放置喷头
                    InsertSparyService.InsertSprayBlock(roomPts, SprayType.SPRAYDOWN);
                }
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
