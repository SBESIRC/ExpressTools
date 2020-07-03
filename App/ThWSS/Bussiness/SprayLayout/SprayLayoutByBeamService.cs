﻿using Linq2Acad;
using ThWSS.Layout;
using ThWSS.Model;
using ThWSS.Utlis;
using System.Linq;
using ThStructure.BeamInfo.Model;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThWSS.Bussiness
{
    class SprayLayoutByBeamService : SparyLayoutService
    {
        public override void LayoutSpray(List<Polyline> roomsLine, Polyline floor, SprayLayoutModel layoutModel)
        {
            foreach (var room in roomsLine)
            {
                //*******预处理房间*********
                //1.处理小的凹边
                var roomBounding = GeUtils.CreateConvexPolygon(room, 1500);

                //2.去掉线上多余的点
                roomBounding = GeUtils.ReovePointOnLine(new List<Polyline>() { roomBounding }, new Tolerance(0.001, 0.001)).First();

                //3.获取梁信息
                CalBeamInfoService beamInfoService = new CalBeamInfoService();
                List<Polyline> beams = beamInfoService.GetAllBeamInfo(roomBounding, floor);
                using (AcadDatabase acdb = AcadDatabase.Active())
                {
                    foreach (var beam in beams)
                    {
                        //acdb.ModelSpace.Add(lineBeam.BeamBoundary);
                    }
                }

                //4.获取柱信息
                CalColumnInfoService columnInfoService = new CalColumnInfoService();
                List<Polyline> columnPolys = columnInfoService.GetColumnStruc();

                List<Polyline> polys = GeUtils.ExtendPolygons(beams, 20);
                polys.AddRange(columnPolys);

                //5.根据房间分割区域
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
                        SquareLayoutWithRay squareLayout = new SquareLayoutWithRay(layoutModel);
                        List<List<SprayLayoutData>> layoutPts = squareLayout.Layout(dRoomRes, roomOOB);

                        //计算房间出房间内的点
                        List<SprayLayoutData> roomSprays = new List<SprayLayoutData>();
                        foreach (var lpts in layoutPts)
                        {
                            List<SprayLayoutData> checkPts = CalRoomSpray(dRoom, lpts, out List<SprayLayoutData> outsideSpray);
                            //checkPts = CalRoomSpray(room, checkPts);
                            roomSprays.AddRange(checkPts);
                        }

                        //计算出柱内的喷淋
                        Dictionary<Polyline, List<SprayLayoutData>> ptInColmns;
                        var columnSpray = columnInfoService.CalColumnSpray(columnPolys, roomSprays, out ptInColmns);

                        //放置喷头
                        var spraType = layoutModel.sprayType == 0 ? SprayType.SPRAYUP : SprayType.SPRAYDOWN;
                        InsertSprayService.InsertSprayBlock(columnSpray.Select(o => o.Position).ToList(), spraType);
                    }
                }
            }
        }
    }
}