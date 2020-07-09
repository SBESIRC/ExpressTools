using Linq2Acad;
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
    class SprayLayoutByBeamService : SparyLayoutService
    {
        public override void CleanSpray(List<Polyline> roomsLine)
        {
            DoCleanSpray(roomsLine);
        }

        public override void LayoutSpray(List<Polyline> roomsLine, Polyline floor, SprayLayoutModel layoutModel)
        {
            foreach (var room in roomsLine)
            {
                // 获取梁信息
                CalBeamInfoService beamInfoService = new CalBeamInfoService();
                List<Polyline> beams = beamInfoService.GetAllBeamInfo(room, floor);
                using (AcadDatabase acdb = AcadDatabase.Active())
                {
                    foreach (var beam in beams)
                    {
                        //acdb.ModelSpace.Add(lineBeam.BeamBoundary);
                    }
                }

                // 获取柱信息
                CalColumnInfoService columnInfoService = new CalColumnInfoService();
                List<Polyline> columnPolys = columnInfoService.GetColumnStruc();

                List<Polyline> polys = GeUtils.ExtendPolygons(beams, 20);
                polys.AddRange(columnPolys);

                // 根据房间分割区域
                RegionDivisionByBeamUtils regionDivision = new RegionDivisionByBeamUtils();
                var respolys = regionDivision.DivisionRegion(room, polys);
                using (AcadDatabase acdb = AcadDatabase.Active())
                {
                    foreach (var poly in respolys)
                    {
                        //acdb.ModelSpace.Add(poly);
                    }
                }

                foreach (var poly in respolys)
                {
                    RegionDivisionUtils regionDivisionUtils = new RegionDivisionUtils();

                    //处理小的凹边
                    var polyBounding = GeUtils.CreateConvexPolygon(poly, 800);
                   
                    //去掉线上多余的点
                    polyBounding = GeUtils.ReovePointOnLine(new List<Polyline>() { polyBounding }, new Tolerance(0.1, 0.1)).First();
                    
                    //区域分割
                    var diviRoom = regionDivisionUtils.DivisionRegion(polyBounding);

                    var allSprays = new List<SprayLayoutData>();   //房间内的喷淋
                    var otherSprays = new List<SprayLayoutData>(); //房间外的喷淋
                    foreach (var dRoom in diviRoom)
                    {
                        //过滤无效区域
                        if (regionDivisionUtils.CalInvalidPolygon(dRoom))
                        {
                            continue;
                        }
                        using (AcadDatabase acdb = AcadDatabase.Active())
                        {
                            acdb.ModelSpace.Add(dRoom);
                        }
                        //去掉线上多余的点
                        var dRoomRes = GeUtils.ReovePointOnLine(new List<Polyline>() { dRoom }, new Tolerance(0.1, 0.1)).First();

                        //计算房间走向
                        var roomOOB = OrientedBoundingBox.Calculate(dRoom);

                        //计算出布置点
                        SquareLayout squareLayout = new SquareLayout(layoutModel);
                        List<List<SprayLayoutData>> layoutPts = squareLayout.Layout(dRoomRes, roomOOB);

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
                        using (AcadDatabase acadDatabase = AcadDatabase.Active())
                        {
                            foreach (Entity ent in radiis)
                            {
                                ent.ColorIndex = 2;
                                acadDatabase.ModelSpace.Add(ent);
                            }
                        }

                        //根据房间面积和喷淋的保护半径，计算保护盲区
                        var blindRegions = poly.Difference(radiis);
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
                        allSprays.AddRange(sprays);

                        //细节后处理
                        DetailsHandleService detailsHandle = new DetailsHandleService(layoutModel);
                        allSprays = detailsHandle.AdjustmentSpray(poly, allSprays);

                        //放置喷头
                        var spraType = layoutModel.sprayType == 0 ? SprayType.SPRAYUP : SprayType.SPRAYDOWN;
                        InsertSprayService.InsertSprayBlock(allSprays.Select(o => o.Position).ToList(), spraType);
                    }
                }
            }
        }
    }
}