using Linq2Acad;
using System.Linq;
using ThWSS.Model;
using ThWSS.Utlis;
using ThWSS.Layout;
using ThCADCore.NTS;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThWSS.Bussiness
{
    public class SprayLayoutNoBeamService : SparyLayoutService
    {
        public override void LayoutSpray(List<Polyline> roomsLine, Polyline floor, SprayLayoutModel layoutModel)
        {
            foreach (var room in roomsLine)
            {
                //0.获取喷头类型
                var spraType = layoutModel.sprayType == 0 ? SprayType.SPRAYUP : SprayType.SPRAYDOWN;

                //*******预处理房间*********
                //1.处理小的凹边
                var roomBounding = GeUtils.CreateConvexPolygon(room, 1500);

                //2.去掉线上多余的点
                roomBounding = GeUtils.ReovePointOnLine(new List<Polyline>() { roomBounding }, new Tolerance(0.001, 0.001)).First();

                //3.获取柱信息
                CalColumnInfoService columnInfoService = new CalColumnInfoService();
                List<Polyline> columnPolys = columnInfoService.GetColumnStruc();

                //4.获取梁信息
                CalBeamInfoService beamInfoService = new CalBeamInfoService();
                List<Polyline> beams = beamInfoService.GetAllBeamInfo(roomBounding, floor, columnPolys, true);

                List<Polyline> polys = GeUtils.ExtendPolygons(beams, 20);
                polys.AddRange(columnPolys);

                //5.根据房间分割区域
                RegionDivisionByBeamUtils regionDivision = new RegionDivisionByBeamUtils();
                var respolys = regionDivision.DivisionRegion(roomBounding, polys);

                // 统计房间内所有喷淋
                var allSprays = new List<SprayLayoutData>();   //房间内的喷淋
                var otherSprays = new List<SprayLayoutData>(); //房间外的喷淋
                foreach (var poly in respolys)
                {
                    RegionDivisionUtils regionDivisionUtils = new RegionDivisionUtils();
                    //处理小的凹边
                    var polyBounding = GeUtils.CreateConvexPolygon(poly, 800);

                    //去掉线上多余的点
                    polyBounding = GeUtils.ReovePointOnLine(new List<Polyline>() { polyBounding }, new Tolerance(0.1, 0.1)).First();

                    //区域分割
                    var diviRooms = regionDivisionUtils.DivisionRegion(polyBounding);

                    foreach (var dRoom in diviRooms)
                    {
                        using (AcadDatabase acdb = AcadDatabase.Active())
                        {
                            acdb.ModelSpace.Add(dRoom);
                        }
                        //计算房间走向
                        var roomOOB = OrientedBoundingBox.Calculate(dRoom);

                        //计算出布置点
                        SquareLayout squareLayout = new SquareLayout(layoutModel);
                        List<List<SprayLayoutData>> layoutPts = squareLayout.Layout(dRoom, roomOOB);

                        //计算房间出房间内的点
                        List<SprayLayoutData> roomSprays = new List<SprayLayoutData>();
                        foreach (var lpts in layoutPts)
                        {
                            List<SprayLayoutData> checkPts = CalRoomSpray(dRoom, lpts, out List<SprayLayoutData> outsideSpray);
                            roomSprays.AddRange(checkPts);
                            otherSprays.AddRange(outsideSpray);
                        }
                        allSprays.AddRange(roomSprays);

                        //放置喷头
                        InsertSprayService.InsertSprayBlock(roomSprays.Select(o => o.Position).ToList(), spraType);
                    }
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

                //获取需要调整的喷淋
                var adSpray = SprayLayoutDataUtils.CalAdjustmentSpray(otherSprays, blindRegions);

                //排布盲区内喷淋和调整需要调整的喷淋
                AdjustmentLayoutSpray adjustmentLayout = new AdjustmentLayoutSpray(layoutModel);
                var sprays = adjustmentLayout.Layout(adSpray);

                //放置喷头
                InsertSprayService.InsertSprayBlock(sprays.Select(o => o.Position).ToList(), spraType);
            }
        }
    }
}
