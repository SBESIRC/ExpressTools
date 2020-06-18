using Linq2Acad;
using System.Linq;
using ThWSS.Model;
using ThWSS.Utlis;
using ThCADCore.NTS;
using ThWSS.LayoutRule;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThWSS.Bussiness
{
    public class SprayLayoutNoBeamService : SparyLayoutService
    {
        public override void LayoutSpray(List<Polyline> roomsLine, SprayLayoutModel layoutModel)
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

                // 统计房间内所有喷淋
                var allSprays = new List<SprayLayoutData>();
                foreach (var dRoom in diviRoom)
                {
                    //计算房间走向
                    var roomOOB = OrientedBoundingBox.Calculate(dRoom);

                    //计算出布置点
                    SquareLayout squareLayout = new SquareLayout(layoutModel);
                    List<List<SprayLayoutData>> layoutPts = squareLayout.Layout(dRoom, roomOOB);

                    //计算房间出房间内的点
                    List<SprayLayoutData> roomSprays = new List<SprayLayoutData>();
                    foreach (var lpts in layoutPts)
                    {
                        List<SprayLayoutData> checkPts = CalRoomSpray(dRoom, lpts);
                        roomSprays.AddRange(checkPts);
                    }
                    allSprays.AddRange(roomSprays);

                    //放置喷头
                    InsertSprayService.InsertSprayBlock(roomSprays.Select(o => o.Position).ToList(), SprayType.SPRAYDOWN);
                }

                // 计算房间内的所有喷淋的保护半径
                var radiis = SprayLayoutDataUtils.Radii(allSprays);
                using (AcadDatabase acadDatabase = AcadDatabase.Active())
                {
                    foreach (Entity ent in radiis)
                    {
                        ent.ColorIndex = 2;
                        acadDatabase.ModelSpace.Add(ent);
                    }
                }

                //根据房间面积和喷淋的保护半径，计算保护盲区
                var blindRegions = room.Difference(radiis);
                using (AcadDatabase acadDatabase = AcadDatabase.Active())
                {
                    foreach (Entity ent in blindRegions)
                    {
                        ent.ColorIndex = 3;
                        acadDatabase.ModelSpace.Add(ent);
                    }
                }

                //对于每个保护盲区，计算出布置点
            }
        }
    }
}
