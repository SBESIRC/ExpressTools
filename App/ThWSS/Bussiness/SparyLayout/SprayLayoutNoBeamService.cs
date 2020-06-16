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

namespace ThWSS.Bussiness.SparyLayout
{
    public class SprayLayoutNoBeamService : SparyLayoutService
    {
        public override void LayoutSpray(List<Polyline> roomsLine, SparyLayoutModel layoutModel)
        {
            foreach (var room in roomsLine)
            {
                //*******预处理房间*********
                //1.处理小的凹边
                var rommBounding = GeUtils.CreateConvexPolygon(room, 1500);

                //2.去掉线上多余的点
                rommBounding = GeUtils.ReovePointOnLine(new List<Polyline>() { rommBounding }, new Tolerance(0.001, 0.001)).First();

                //区域分割
                RegionDivisionUtils regionDivisionUtils = new RegionDivisionUtils();
                var diviRoom = regionDivisionUtils.DivisionRegion(rommBounding);
                using (AcadDatabase acdb = AcadDatabase.Active())
                {
                    foreach (var item in diviRoom)
                    {
                        acdb.ModelSpace.Add(item);
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
                        List<Point3d> checkPts = CalRoomSpray(dRoom, lpts);
                        roomPts.AddRange(checkPts);
                    }

                    //放置喷头
                    InsertSparyService.InsertSprayBlock(roomPts, SprayType.SPRAYDOWN);
                }
            }
        }
    }
}
