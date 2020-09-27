using Linq2Acad;
using ThWSS.Model;
using ThWSS.Utlis;
using System.Linq;
using ThWSS.Layout;
using ThCADCore.NTS;
using TopoNode.Progress;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThWSS.Bussiness
{
    public class SprayLayoutNoBeamService : SparyLayoutService
    {
        public override void CleanSpray(ThRoom room)
        {
            DoCleanSpray(room.Properties.Values.Cast<Polyline>().ToList());
        }

        public override void LayoutSpray(ThRoom room, Polyline floor, SprayLayoutModel layoutModel)
        {
            foreach (var outline in room.Properties.Values.Cast<Polyline>())
            {
                // 提取柱信息
                Progress.SetValue(0);
                CalColumnInfoService columnInfoService = new CalColumnInfoService();
                List<Polyline> columnPolys = columnInfoService.GetAllColumnInfo(outline, floor);

                // 提取梁信息
                Progress.SetValue(2000);
                CalBeamInfoService beamInfoService = new CalBeamInfoService();
                List<Polyline> beams = beamInfoService.GetAllBeamInfo(outline, floor, columnPolys, true);

                // 由于存在绘图不规范，梁线和柱线没有在“几何”意义上搭接起来
                // 为了处理这样的情况，这里采用了“Dissolve（溶解）”的思路
                // 通过扩大梁的范围使梁和柱搭接起来
                List<Polyline> polys = GeUtils.ExtendPolygons(beams, 20);
                polys.AddRange(columnPolys);


                // 在房间区域内按照梁和柱的搭接关系，分割房间区域
                Progress.SetValue(3000);
                RegionDivisionByBeamUtils regionDivision = new RegionDivisionByBeamUtils();
                var respolys = regionDivision.DivisionRegion(outline, polys);

                // 为每一个分割后的“子区域”布置喷头
                Progress.SetValue(5000);
                foreach (var poly in respolys)
                {
                    //处理小的凹边
                    var polyBounding = GeUtils.CreateConvexPolygon(poly, 1500);

                    //去掉线上多余的点
                    polyBounding = GeUtils.ReovePointOnLine(new List<Polyline>() { polyBounding }, new Tolerance(0.1, 0.1)).First();

                    //区域分割
                    RegionDivisionUtils regionDivisionUtils = new RegionDivisionUtils();
                    var diviRooms = regionDivisionUtils.DivisionRegion(polyBounding);

                    // 统计房间内所有喷淋
                    var allSprays = new List<SprayLayoutData>();   //房间内的喷淋
                    var otherSprays = new List<SprayLayoutData>(); //房间外的喷淋
                    foreach (var dRoom in diviRooms)
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
                            List<SprayLayoutData> checkPts = CalRoomSpray(dRoom, lpts, out List<SprayLayoutData> outsideSpray);
                            roomSprays.AddRange(checkPts);
                            otherSprays.AddRange(outsideSpray);
                        }
                        allSprays.AddRange(roomSprays);
                    }

                    if (allSprays.Count > 0)
                    {
                        // 计算房间内的所有喷淋的保护半径
                        var radiis = SprayLayoutDataUtils.Radii(allSprays);

                        //根据房间面积和喷淋的保护半径，计算保护盲区
                        var blindRegions = poly.Difference(radiis);
                        using (AcadDatabase acadDatabase = AcadDatabase.Active())
                        {
                            foreach (Entity ent in blindRegions)
                            {
                                acadDatabase.ModelSpace.Add(ent);
                                ent.LayerId = acadDatabase.Database.CreateSprayLayoutBlindRegionLayer();
                            }
                        }

                        //获取需要调整的喷淋
                        var adSpray = SprayLayoutDataUtils.CalAdjustmentSpray(otherSprays, blindRegions);

                        //排布盲区内喷淋和调整需要调整的喷淋
                        AdjustmentLayoutSpray adjustmentLayout = new AdjustmentLayoutSpray(layoutModel);
                        var sprays = adjustmentLayout.Layout(adSpray);
                        allSprays.AddRange(sprays);

                        //细节后处理
                        DetailsHandleService detailsHandle = new DetailsHandleService(layoutModel);
                        allSprays = detailsHandle.AdjustmentSpray(poly, allSprays);

                        //放置喷头
                        var spraType = layoutModel.sprayType == 0 ? SprayType.SPRAYUP : SprayType.SPRAYDOWN;
                        InsertSprayService.InsertSprayBlock(allSprays.Select(o => o.Position).ToList(), spraType);

                        // 更新进度条窗口状态
                        Progress.SetValue(5000 + 5000 / respolys.Count * respolys.IndexOf(poly));
                    }
                }
            }
        }
    }
}
