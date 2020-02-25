using Linq2Acad;
using DotNetARX;
using AcHelper;
using System.Linq;
using Autodesk.AutoCAD.Runtime;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System.Windows.Forms;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
using NFox.Cad.Collections;
using System.Threading;

namespace ThSpray
{
    public class ThSprayApp : IExtensionApplication
    {
        public void Initialize()
        {
        }

        public void Terminate()
        {
        }

        [CommandMethod("TIANHUACAD", "THSPC", CommandFlags.Modal)]
        static public void SprayDo()
        {
            //打开需要的图层
            List<string> wallLayers = null;
            List<string> arcDoorLayers = null;
            List<string> windLayers = null;
            List<string> validLayers = null;
            List<string> beamLayers = null;
            List<string> columnLayers = null;
            var allCurveLayers = Utils.ShowThLayers(out wallLayers, out arcDoorLayers, out windLayers, out validLayers, out beamLayers, out columnLayers);
            var previewCurves = new List<Curve>();
            again:
            var sprayForm = new SprayParam();
            if (AcadApp.ShowModalDialog(sprayForm) != DialogResult.OK)
            {
                return;
            }

            var userData = SprayParam.placeData;
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            if (userData.putType == PutType.CHOOSECURVE)
            {
                Active.WriteMessage("请选择需要布置喷头的房间框线：");
                List<Polyline> roomPolylines = new List<Polyline>();
                PromptSelectionResult result;
                PromptSelectionOptions options = new PromptSelectionOptions()
                {
                    AllowDuplicates = false,
                    RejectObjectsOnLockedLayers = true,
                };

                var filterlist = OpFilter.Bulid(
                    o => o.Dxf((int)DxfCode.Start) == "LWPOLYLINE");

                result = Active.Editor.GetSelection(options, filterlist);
                if (result.Status == PromptStatus.OK)
                {
                    Utils.CollectPLines(result, roomPolylines);
                }

                if (roomPolylines.Count == 0)
                {
                    Active.WriteMessage("没有选择到有效的多段线，可能是多段线未闭合引起的");
                    return;
                }

                ProgressDialog.ShowProgress();
                // 梁数据
                var beamCurves = Utils.GetAllCurvesFromLayerNames(beamLayers);

                //柱子数据
                var columnCurves = Utils.GetAllCurvesFromLayerNames(columnLayers);
                var innerCurves = new List<Curve>();
                if (beamCurves != null && beamCurves.Count != 0)
                    innerCurves.AddRange(beamCurves);
                if (columnCurves != null && columnCurves.Count != 0)
                    innerCurves.AddRange(columnCurves);

                ProgressDialog.SetValue(15);
                roomPolylines = Utils.NormalizePolylines(roomPolylines);
                var inc = 75 / roomPolylines.Count;
                var curStep = 20;
                foreach (var roomPoly in roomPolylines)
                {
                    // 内梁
                    var insertPts = Utils.MakeInnerProfiles(innerCurves, roomPoly, userData);
                    if (insertPts == null || insertPts.Count == 0)
                        continue;

                    // 插入块
                    Utils.InsertSprayBlock(insertPts, userData.type);
                    curStep += inc;
                    ProgressDialog.SetValue(curStep);
                }

                ProgressDialog.HideProgress();
            }
            else if (userData.putType == PutType.DRAWCURVE)
            {
                var pickPoints = new List<Point3d>();
                Active.WriteMessage("绘制房间框线，指定起点:");
                PromptPointOptions ppo = new PromptPointOptions("\n请点击");
                ppo.AllowNone = true;

                ppo.Keywords.Add("U", "U", "放弃(U)");
                ppo.Keywords.Add("E", "E", "退出(E)");
                while (true)
                {
                    if (pickPoints.Count != 0)
                    {
                        ppo.UseBasePoint = true;
                        ppo.UseDashedLine = true;
                        ppo.BasePoint = pickPoints.Last();
                    }

                    PromptPointResult ppr = ed.GetPoint(ppo);
                    if (ppr.Status == PromptStatus.None)
                    {
                        // 绘制首尾段
                        if (pickPoints.Count > 1)
                        {
                            var firstPt = pickPoints.First();
                            var lastPt = pickPoints.Last();
                            if (!CommonUtils.Point3dIsEqualPoint3d(firstPt, lastPt, 1))
                            {
                                var line = new Line(lastPt, firstPt);
                                Utils.AddProfile(new List<Curve>() { line }, "preview");
                                previewCurves.Add(line);
                            }
                        }
                        break;
                    }
                    if (ppr.Status == PromptStatus.Cancel)
                    {
                        Utils.EraseProfile(previewCurves);
                        previewCurves.Clear();
                        Active.WriteMessage("取消操作");
                        return;
                    }
                    
                    var pickPoint = ppr.Value;
                    if ((ppr.Status == PromptStatus.OK) || ppr.Status == PromptStatus.Keyword)
                    {
                        if (ppr.StringResult == "U")
                        {
                            pickPoints.Remove(pickPoints.Last());
                            if (previewCurves.Count != 0)
                            {
                                var lastEraseCurve = previewCurves.Last();
                                previewCurves.Remove(lastEraseCurve);
                                Utils.EraseProfile(new List<Curve>() { lastEraseCurve });
                            }
                        }
                        else if (ppr.StringResult == "E")
                        {
                            Utils.EraseProfile(previewCurves);
                            previewCurves.Clear();
                            goto again;
                        }
                        else
                        {
                            pickPoints.Add(pickPoint);
                            if (pickPoints.Count != 1 && !CommonUtils.Point3dIsEqualPoint3d(ppo.BasePoint, pickPoint, 1))
                            {
                                var line = new Line(ppo.BasePoint, pickPoint);
                                Utils.AddProfile(new List<Curve>() { line }, "preview");
                                previewCurves.Add(line);
                            }
                        }
                    }
                }

                // 选点结束后的判断处理
                if (pickPoints.Count < 4)
                {
                    Utils.EraseProfile(previewCurves);
                    previewCurves.Clear();
                    Active.WriteMessage("绘制多段线错误");
                    return;
                }

                var poly = Utils.Pts2Polyline(pickPoints);
                poly = Utils.NormalizePolyline(poly);
                if (poly == null)
                {
                    Utils.EraseProfile(previewCurves);
                    previewCurves.Clear();
                    return;
                }

                ProgressDialog.ShowProgress();
                Utils.EraseProfile(previewCurves);
                previewCurves.Clear();
                // 梁数据
                var beamCurves = Utils.GetAllCurvesFromLayerNames(beamLayers);

                ProgressDialog.SetValue(15);
                //柱子数据
                var columnCurves = Utils.GetAllCurvesFromLayerNames(columnLayers);
                var innerCurves = new List<Curve>();
                if (beamCurves != null && beamCurves.Count != 0)
                    innerCurves.AddRange(beamCurves);
                if (columnCurves != null && columnCurves.Count != 0)
                    innerCurves.AddRange(columnCurves);
                ProgressDialog.SetValue(20);
                // 内梁
                var insertPts = Utils.MakeInnerProfiles(innerCurves, poly, userData);
                if (insertPts == null || insertPts.Count == 0)
                {
                    ProgressDialog.HideProgress();
                    Active.WriteMessage("绘制多段线错误");
                    return;
                }

                // 插入块
                Utils.InsertSprayBlock(insertPts, userData.type);
                ProgressDialog.HideProgress();
            }
            else if (userData.putType == PutType.PICKPOINT)
            {

                var pickPoints = new List<Point3d>();
                var objCollect = new DBObjectCollection();
                var tip = "请点击需要布置喷头房间内的一点，共计";
                while (true)
                {
                    var message = tip + pickPoints.Count().ToString() + "个";
                    Active.WriteMessage(message);
                    PromptPointOptions ppo = new PromptPointOptions("\n请点击");
                    ppo.AllowNone = true;
                    PromptPointResult ppr = ed.GetPoint(ppo);
                    if (ppr.Status == PromptStatus.None)
                        break;
                    if (ppr.Status == PromptStatus.Cancel)
                    {
                        Utils.ErasePreviewPoint(objCollect);
                        Active.WriteMessage("取消操作");
                        return;
                    }

                    var pickPoint = ppr.Value;
                    pickPoints.Add(pickPoint);
                    Utils.DrawPreviewPoint(objCollect, pickPoint);
                }

                // 开始布置提示
                Active.WriteMessage("开始布置...");

                // 开始弹出进度条提示
                ProgressDialog.ShowProgress();
                // 图元预处理
                var removeEntityLst = Utils.PreProcess(validLayers);

                // 获取相关图层中的数据
                var allCurves = Utils.GetAllCurvesFromLayerNames(allCurveLayers);// allCurves指所有能作为墙一部分的曲线
                
                if (allCurves == null || allCurves.Count == 0)
                {
                    Utils.PostProcess(removeEntityLst);
                    ProgressDialog.HideProgress();
                    return;
                }

                // wall 中的数据
                var wallAllCurves = Utils.GetAllCurvesFromLayerNames(wallLayers);
                if (wallAllCurves == null || wallAllCurves.Count == 0 || wallLayers.Count == 0)
                {
                    Utils.PostProcess(removeEntityLst);
                    ProgressDialog.HideProgress();
                    return;
                }

                // door 内门中的数据
                if (arcDoorLayers != null && arcDoorLayers.Count != 0)
                {
                    var doorBounds = Utils.GetBoundsFromLayerBlocksAndCurves(arcDoorLayers);
                    var doorInsertCurves = Utils.InsertDoorRelatedCurveDatas(doorBounds, wallAllCurves, arcDoorLayers.First());
                    if (doorInsertCurves != null && doorInsertCurves.Count != 0)
                    {
                        allCurves.AddRange(doorInsertCurves);
                    }
                }

                // wind 中的数据
                if (windLayers != null && windLayers.Count != 0)
                {
                    var windBounds = Utils.GetBoundsFromLayerBlocksAndCurves(windLayers);

                    var windInsertCurves = Utils.InsertDoorRelatedCurveDatas(windBounds, wallAllCurves, windLayers.First());

                    if (windInsertCurves != null && windInsertCurves.Count != 0)
                    {
                        allCurves.AddRange(windInsertCurves);
                    }
                }

                ProgressDialog.SetValue(15);
                allCurves = TopoUtils.TesslateCurve(allCurves);
                allCurves = CommonUtils.RemoveCollinearLines(allCurves);

                pickPoints = CommonUtils.ErasePointsOnCurves(pickPoints, allCurves);
                // 梁数据
                var beamCurves = Utils.GetAllCurvesFromLayerNames(beamLayers);
                //柱子数据
                var columnCurves = Utils.GetAllCurvesFromLayerNames(columnLayers);
                // 需要剔除的柱子轮廓数据
                //var columnTesCurves = TopoUtils.TesslateCurve(columnCurves);
                //var columnLoops = TopoUtils.MakeSrcProfilesNoTes(columnTesCurves);

                var innerCurves = new List<Curve>();
                if (beamCurves != null && beamCurves.Count != 0)
                    innerCurves.AddRange(beamCurves);
                if (columnCurves != null && columnCurves.Count != 0)
                    innerCurves.AddRange(columnCurves);

                ProgressDialog.SetValue(20);
                var inc = 75 / pickPoints.Count;
                var curStep = 20;

                var hasPutPolylines = new List<Polyline>();
                foreach (var pt in pickPoints)
                {
                    if (CommonUtils.PtInPolylines(hasPutPolylines, pt))
                        continue;
                    var profile = TopoUtils.MakeProfileFromPoint(allCurves, pt);
                    if (profile == null || profile.Count == 0)
                        continue;

                    // 内梁
                    var insertPts = Utils.MakeInnerProfiles(innerCurves, profile.First() as Polyline, userData);
                    if (insertPts == null || insertPts.Count == 0)
                        continue;

                    hasPutPolylines.Add(profile.First() as Polyline);
                    // 插入块
                    Utils.InsertSprayBlock(insertPts, userData.type);
                    curStep = inc + curStep;
                    ProgressDialog.SetValue(curStep);
                }

                Utils.PostProcess(removeEntityLst);
                Utils.ErasePreviewPoint(objCollect);
                ProgressDialog.HideProgress();
            }

            Active.WriteMessage("布置完成");
        }
    }
}
