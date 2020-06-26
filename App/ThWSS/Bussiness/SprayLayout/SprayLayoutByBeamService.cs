using Linq2Acad;
using ThWSS.Layout;
using ThWSS.Model;
using ThWSS.Utlis;
using System.Linq;
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
                List<ThStructure.BeamInfo.Model.Beam> beamInfo = beamInfoService.GetAllBeamInfo(roomBounding, floor);

                //4.获取柱信息
                CalColumnInfoService columnInfoService = new CalColumnInfoService();
                List<Polyline> columnPolys = columnInfoService.GetColumnStruc();

                List<Polyline> polys = ExtendPolygons(beamInfo.Select(x => x.BeamBoundary).ToList(), 20);
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
                
                //获取房间线
                List<Line> roomLines = new List<Line>();
                for (int i = 0; i < room.NumberOfVertices; i++)
                {
                    roomLines.Add(new Line(room.GetPoint3dAt(i), room.GetPoint3dAt((i + 1) % room.NumberOfVertices)));
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

                        //去掉线上多余的点
                        var dRoomRes = GeUtils.ReovePointOnLine(new List<Polyline>() { dRoom }, new Tolerance(0.1, 0.1)).First();
                        
                        //计算出布置点
                        SquareLayoutByBeam squareLayout = new SquareLayoutByBeam(layoutModel);
                        List<List<SprayLayoutData>> layoutPts = squareLayout.Layout(roomLines, dRoomRes, beamInfo);

                        //计算房间出房间内的点
                        List<SprayLayoutData> roomSprays = new List<SprayLayoutData>();
                        foreach (var lpts in layoutPts)
                        {
                            List<SprayLayoutData> checkPts = CalRoomSpray(dRoom, lpts);
                            checkPts = CalRoomSpray(room, checkPts);
                            roomSprays.AddRange(checkPts);
                        }

                        //计算出柱内的喷淋
                        Dictionary<Polyline, List<SprayLayoutData>> ptInColmns;
                        var columnSpray = columnInfoService.CalColumnSpray(columnPolys, roomSprays, out ptInColmns);

                        //将孤立柱打入图层
                        columnInfoService.SetStandAloneColumnInLayer(ptInColmns.Keys.ToList());

                        //放置喷头
                        InsertSprayService.InsertSprayBlock(columnSpray.Select(o => o.Position).ToList(), SprayType.SPRAYDOWN);
                    }
                }
            }
        }

        private List<Polyline> ExtendPolygons(List<Polyline> polys, double offset)
        {
            List<Polyline> resPoly = new List<Polyline>();
            foreach (var pls in polys)
            {
                List<Line> lines = new List<Line>();
                for (int i = 0; i < pls.NumberOfVertices; i++)
                {
                    lines.Add(new Line(pls.GetPoint3dAt(i), pls.GetPoint3dAt((i + 1) % pls.NumberOfVertices)));
                }

                lines = lines.OrderByDescending(x => x.Length).ToList();
                Polyline polyline = new Polyline() { Closed = true };
                polyline.AddVertexAt(0, (lines[0].StartPoint - lines[0].Delta.GetNormal() * offset).ToPoint2D(), 0, 0, 0);
                polyline.AddVertexAt(1, (lines[0].EndPoint + lines[0].Delta.GetNormal() * offset).ToPoint2D(), 0, 0, 0);
                polyline.AddVertexAt(2, (lines[1].StartPoint - lines[1].Delta.GetNormal() * offset).ToPoint2D(), 0, 0, 0);
                polyline.AddVertexAt(3, (lines[1].EndPoint + lines[1].Delta.GetNormal() * offset).ToPoint2D(), 0, 0, 0);
                resPoly.Add(polyline);
            }
            return resPoly;
        }
    }
}