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
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
using NFox.Cad.Collections;
using System.Threading;
using Autodesk.AutoCAD.Colors;
using TopoNode.Progress;
using System;

namespace TopoNode
{
    public class TopoNodeApp : IExtensionApplication
    {
        public void Initialize()
        {

        }

        public void Terminate()
        {

        }

        [CommandMethod("TopoNodeS", "TopoNodes", CommandFlags.Modal)]
        static public void topoNodes()
        {
            //打开需要的图层
            List<string> wallLayers = null;
            List<string> arcDoorLayers = null;
            List<string> windLayers = null;
            List<string> validLayers = null;
            List<string> beamLayers = null;
            List<string> columnLayers = null;
            var allCurveLayers = Utils.ShowThLayers(out wallLayers, out arcDoorLayers, out windLayers, out validLayers, out beamLayers, out columnLayers);

            var pickPoints = new List<Point3d>();
            List<Polyline> selectPLines = null;
            try
            {
                selectPLines = Utils.GetEntitys("请选择防火分区线", typeof(Polyline));
            }
            catch (System.Exception e)
            {

            }

            if (selectPLines == null || selectPLines.Count == 0)
                return;

            // 开始弹出进度条提示
            Progress.Progress.ShowProgress();
            Progress.Progress.SetTip("图纸预处理...");

            // 图元预处理
            var objs = Utils.PreProcess2(validLayers);

            //Utils.PostProcess(removeEntityLst);
            //return;
            Progress.Progress.SetValue(900);
            pickPoints = Utils.GetRoomPoints("AD-NAME-ROOM");

            //foreach (var pt in pickPoints)
            //    Utils.DrawPreviewPoint(pt, "pick");

            // 获取相关图层中的数据
            var srcAllCurves = Utils.GetAllCurvesFromLayerNames(allCurveLayers);// allCurves指所有能作为墙一部分的曲线
            if (srcAllCurves == null || srcAllCurves.Count == 0)
            {
                Utils.PostProcess(objs);
                Progress.Progress.HideProgress();
                return;
            }

            // wall 中的数据
            var srcWallAllCurves = Utils.GetAllCurvesFromLayerNames(wallLayers);
            if (srcWallAllCurves == null || srcWallAllCurves.Count == 0 || wallLayers.Count == 0)
            {
                Utils.PostProcess(objs);
                Progress.Progress.HideProgress();
                return;
            }

            Progress.Progress.SetValue(1000);
            Progress.Progress.SetTip("区域识别中...");
            var selectCount = selectPLines.Count;
            double beginPos = 1000.0;
            double FFStep = 5400.0 / selectCount;

            for (int i = 0; i < selectCount; i++)
            {
                double profileFindPre = FFStep / 4.0;
                double smallStep = 0.0;
                var curSelectPLine = selectPLines[i];
                var line2ds = CommonUtils.Polyline2dLines(curSelectPLine);
                if (CommonUtils.CalcuLoopArea(line2ds) < 0)
                {
                    using (var db = AcadDatabase.Active())
                    {
                        curSelectPLine.ReverseCurve();
                    }
                }
                var curSelectPoints = Utils.GetValidFPointsFromSelectPLine(pickPoints, curSelectPLine);
                if (curSelectPoints == null || curSelectPoints.Count == 0)
                    continue;
                var allCurves = Utils.GetValidCurvesFromSelectPLine(srcAllCurves, curSelectPLine);

                foreach (var pt in curSelectPoints)
                    Utils.DrawPreviewPoint(pt, "pick");

                allCurves = TopoUtils.TesslateCurve(allCurves);
                Utils.ExtendCurves(allCurves, 5);

                var wallAllCurves = Utils.GetValidCurvesFromSelectPLineNoSelf(srcWallAllCurves, curSelectPLine);
                wallAllCurves = TopoUtils.TesslateCurve(wallAllCurves);

                // wind线作为墙的一部分
                if (windLayers != null && windLayers.Count != 0)
                {
                    var windCurves = Utils.GetWindDOORCurves(windLayers);
                    windCurves = Utils.GetValidCurvesFromSelectPLineNoSelf(windCurves, curSelectPLine);
                    if (windCurves != null && windCurves.Count != 0)
                    {
                        var tesslateWindCurves = TopoUtils.TesslateCurve(windCurves);
                        Utils.ExtendCurvesWithTransaction(tesslateWindCurves, 5);
                        wallAllCurves.AddRange(tesslateWindCurves);
                        allCurves.AddRange(tesslateWindCurves);
                    }
                }

                // door线作为墙的一部分
                if (arcDoorLayers != null && arcDoorLayers.Count != 0)
                {
                    var doorCurves = Utils.GetWindDOORCurves(arcDoorLayers);
                    doorCurves = Utils.GetValidCurvesFromSelectPLineNoSelf(doorCurves, curSelectPLine);
                    if (doorCurves != null && doorCurves.Count != 0)
                    {
                        var tesslateDoorCurves = TopoUtils.TesslateCurve(doorCurves);
                        wallAllCurves.AddRange(tesslateDoorCurves);
                        allCurves.AddRange(tesslateDoorCurves);
                    }
                }

                smallStep = profileFindPre / 3.0;
                beginPos += smallStep;
                Progress.Progress.SetValue((int)beginPos);
                // door 内门中的数据
                if (arcDoorLayers != null && arcDoorLayers.Count != 0)
                {
                    var doorBounds = Utils.GetBoundsFromDOORLayerCurves(arcDoorLayers);
                    doorBounds = Utils.GetValidBoundsFromSelectPLine(doorBounds, curSelectPLine);
                    var doorInsertCurves = Utils.InsertDoorRelatedCurveDatas(doorBounds, allCurves, arcDoorLayers.First());

                    if (doorInsertCurves != null && doorInsertCurves.Count != 0)
                    {
                        allCurves.AddRange(doorInsertCurves);
                        Utils.DrawProfile(doorInsertCurves, "doorInsertCurves");
                        //removeEntityLst.AddRange(doorInsertCurves);
                    }
                }

                beginPos += smallStep;
                Progress.Progress.SetValue((int)beginPos);
                // wind 中的数据
                if (windLayers != null && windLayers.Count != 0)
                {
                    var windBounds = Utils.GetBoundsFromWINDLayerCurves(windLayers);
                    windBounds = Utils.GetValidBoundsFromSelectPLine(windBounds, curSelectPLine);
                    var windInsertCurves = Utils.InsertDoorRelatedCurveDatas(windBounds, wallAllCurves, windLayers.First());

                    if (windInsertCurves != null && windInsertCurves.Count != 0)
                    {
                        Utils.DrawProfile(windInsertCurves, "windInsertCurves");
                        allCurves.AddRange(windInsertCurves);
                    }
                }

                allCurves = CommonUtils.RemoveCollinearLines(allCurves);

                beginPos += smallStep;
                Progress.Progress.SetValue((int)beginPos);
                //Utils.DrawProfile(allCurves, "allCurves");
                //Utils.PostProcess(removeEntityLst);
                //return;
                //Utils.DrawProfile(allCurves, "allCurves");
                //return;

                var inc = (profileFindPre * 3.0) / curSelectPoints.Count;
                var hasPutPolys = new List<Tuple<Point3d, double>>();

                //var profiles = TopoUtils.MakeProfilesFromPoints(allCurves, pickPoints);

                //if (profiles != null && profiles.Count != 0)
                //{
                //    foreach (var profile in profiles)
                //    {
                //        //if (CommonUtils.HasPolylines(hasPutPolys, profile.profile))
                //        //    continue;

                //        Utils.DrawProfile(new List<Curve>() { profile.profile }, "outProfile");
                //    }
                //}

                foreach (var pt in curSelectPoints)
                {
                    beginPos += inc;
                    Progress.Progress.SetValue((int)beginPos);

                    try
                    {
                        var aimProfile = TopoUtils.MakeProfileFromPoint2(allCurves, pt);
                        if (aimProfile == null)
                            continue;

                        if (CommonUtils.HasPolylines(hasPutPolys, aimProfile.profile))
                            continue;

                        Utils.DrawProfile(new List<Curve>() { aimProfile.profile }, "outProfile");
                        // Utils.DrawTextProfile(outProfile.profileCurves, outProfile.profileLayers);
                        if (aimProfile.InnerPolylineLayers.Count != 0)
                        {
                            foreach (var innerProfile in aimProfile.InnerPolylineLayers)
                            {
                                Utils.DrawProfile(new List<Curve>() { innerProfile.profile }, "innerProfile");
                            }
                        }
                    }
                    catch (System.Exception e)
                    {
                        //Active.WriteMessage(e.Message);
                    }
                }
            }

            Utils.PostProcess(objs);
            Progress.Progress.SetValue(6500);
            Progress.Progress.HideProgress();
            //Utils.ErasePreviewPoint(objCollect);
        }

        [CommandMethod("TopoPick", "TopoPick", CommandFlags.Modal)]
        static public void topoNodePick()
        {
            //打开需要的图层
            List<string> wallLayers = null;
            List<string> arcDoorLayers = null;
            List<string> windLayers = null;
            List<string> validLayers = null;
            List<string> beamLayers = null;
            List<string> columnLayers = null;
            var allCurveLayers = Utils.ShowThLayers(out wallLayers, out arcDoorLayers, out windLayers, out validLayers, out beamLayers, out columnLayers);

            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
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


            // 图元预处理
            var removeEntityLst = Utils.PreProcess2(validLayers);

            // 获取相关图层中的数据
            var allCurves = Utils.GetAllCurvesFromLayerNames(allCurveLayers);// allCurves指所有能作为墙一部分的曲线
            var layerCurves = Utils.GetLayersFromCurves(allCurves);
            if (allCurves == null || allCurves.Count == 0)
            {
                Utils.PostProcess(removeEntityLst);
                return;
            }

            allCurves = TopoUtils.TesslateCurve(allCurves);
            Utils.ExtendCurves(allCurves, 5);
            // wall 中的数据
            var wallAllCurves = Utils.GetAllCurvesFromLayerNames(wallLayers);
            if (wallAllCurves == null || wallAllCurves.Count == 0 || wallLayers.Count == 0)
            {
                Utils.PostProcess(removeEntityLst);
                return;
            }

            wallAllCurves = TopoUtils.TesslateCurve(wallAllCurves);
            // door 内门中的数据
            if (arcDoorLayers != null && arcDoorLayers.Count != 0)
            {
                var doorBounds = Utils.GetBoundsFromLayerBlocksAndCurves(arcDoorLayers);
                var doorInsertCurves = Utils.InsertDoorRelatedCurveDatas(doorBounds, wallAllCurves, arcDoorLayers.First());
                if (doorInsertCurves != null && doorInsertCurves.Count != 0)
                {
                    layerCurves = Utils.GetLayersFromCurves(doorInsertCurves);
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
                    layerCurves = Utils.GetLayersFromCurves(windInsertCurves);
                    allCurves.AddRange(windInsertCurves);
                }
            }

            var layerNames = Utils.GetLayersFromCurves(allCurves);
            layerNames = Utils.GetLayersFromCurves(allCurves);
            allCurves = CommonUtils.RemoveCollinearLines(allCurves);
            layerNames = Utils.GetLayersFromCurves(allCurves);
            //Utils.DrawProfile(allCurves, "all");
            //return;
            ////Utils.DrawProfileAndText(allCurves, Color.FromRgb(0, 255, 0));
            //return;

            //Utils.ExtendCurves(allCurves, 0.5);
            var hasPutPolys = new List<Tuple<Point3d, double>>();
            foreach (var pt in pickPoints)
            {
                try
                {
                    var aimProfile = TopoUtils.MakeProfileFromPoint2(allCurves, pt);
                    if (aimProfile == null)
                        continue;

                    if (CommonUtils.HasPolylines(hasPutPolys, aimProfile.profile))
                        continue;

                    Utils.DrawProfile(new List<Curve>() { aimProfile.profile }, "outProfile");
                    Utils.DrawTextProfile(aimProfile.profileCurves, aimProfile.profileLayers);
                    if (aimProfile.InnerPolylineLayers.Count != 0)
                    {
                        foreach (var innerProfile in aimProfile.InnerPolylineLayers)
                        {
                            Utils.DrawProfile(new List<Curve>() { innerProfile.profile }, "innerProfile");
                            Utils.DrawTextProfile(innerProfile.profileCurves, innerProfile.profileLayers);
                        }
                    }
                }
                catch
                { }
            }

            Utils.PostProcess(removeEntityLst);
            Utils.ErasePreviewPoint(objCollect);
            //var profiles = TopoUtils.MakeProfileFromPoint(allCurves, pt);
            ////var profiles = TopoSearch.MakeSrcProfileLayerLoops(allCurves);
            //if (profiles == null || profiles.Count == 0)
            //    return;

            //foreach (var polylineLayer in profiles)
            //{
            //    Utils.DrawProfile(new List<Curve>() { polylineLayer.profile }, "outProfile");
            //    Utils.DrawTextProfile(polylineLayer.profileCurves, polylineLayer.profileLayers);
            //}

            //// 梁数据
            //var beamCurves = Utils.GetAllCurvesFromLayerNames(beamLayers);
            ////柱子数据
            //var columnCurves = Utils.GetAllCurvesFromLayerNames(columnLayers);
            //// 需要剔除的柱子轮廓数据
            ////var columnTesCurves = TopoUtils.TesslateCurve(columnCurves);
            ////var columnLoops = TopoUtils.MakeSrcProfilesNoTes(columnTesCurves);

            //var innerCurves = new List<Curve>();
            //if (beamCurves != null && beamCurves.Count != 0)
            //    innerCurves.AddRange(beamCurves);
            //if (columnCurves != null && columnCurves.Count != 0)
            //    innerCurves.AddRange(columnCurves);

        }

        [CommandMethod("TopoNode", "TopoNode", CommandFlags.Modal)]
        static public void TopoNode()
        {
            #region
            //打开需要的图层
            //List<string> wallLayers = null;
            //List<string> arcDoorLayers = null;
            //List<string> windLayers = null;
            //List<string> validLayers = null;
            //List<string> beamLayers = null;
            //List<string> columnLayers = null;
            //var allCurveLayers = Utils.ShowThLayers(out wallLayers, out arcDoorLayers, out windLayers, out validLayers, out beamLayers, out columnLayers);
            //var previewCurves = new List<Curve>();
            #endregion
            var allCurves = Utils.GetAllCurves();

            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
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
            #region 注释
            //// 图元预处理
            //var removeEntityLst = Utils.PreProcess(validLayers);

            //// 获取相关图层中的数据
            //var allCurves = Utils.GetAllCurvesFromLayerNames(allCurveLayers);// allCurves指所有能作为墙一部分的曲线

            //if (allCurves == null || allCurves.Count == 0)
            //{
            //    Utils.PostProcess(removeEntityLst);
            //    return;
            //}

            //// wall 中的数据
            //var wallAllCurves = Utils.GetAllCurvesFromLayerNames(wallLayers);
            //if (wallAllCurves == null || wallAllCurves.Count == 0 || wallLayers.Count == 0)
            //{
            //    Utils.PostProcess(removeEntityLst);
            //    return;
            //}

            //// door 内门中的数据
            //if (arcDoorLayers != null && arcDoorLayers.Count != 0)
            //{
            //    var doorBounds = Utils.GetBoundsFromLayerBlocksAndCurves(arcDoorLayers);
            //    var doorInsertCurves = Utils.InsertDoorRelatedCurveDatas(doorBounds, wallAllCurves, arcDoorLayers.First());
            //    if (doorInsertCurves != null && doorInsertCurves.Count != 0)
            //    {
            //        allCurves.AddRange(doorInsertCurves);
            //    }
            //}

            //// wind 中的数据
            //if (windLayers != null && windLayers.Count != 0)
            //{
            //    var windBounds = Utils.GetBoundsFromLayerBlocksAndCurves(windLayers);

            //    var windInsertCurves = Utils.InsertDoorRelatedCurveDatas(windBounds, wallAllCurves, windLayers.First());

            //    if (windInsertCurves != null && windInsertCurves.Count != 0)
            //    {
            //        allCurves.AddRange(windInsertCurves);
            //    }
            //}

            #endregion

            var layerNames = Utils.GetLayersFromCurves(allCurves);
            allCurves = TopoUtils.TesslateCurve(allCurves);
            layerNames = Utils.GetLayersFromCurves(allCurves);
            allCurves = CommonUtils.RemoveCollinearLines(allCurves);
            layerNames = Utils.GetLayersFromCurves(allCurves);
            //Utils.DrawProfileAndText(allCurves);
            //return;
            var hasPutPolylines = new List<Polyline>();
            foreach (var pt in pickPoints)
            {
                if (CommonUtils.PtInPolylines(hasPutPolylines, pt))
                    continue;

                var testProfile = ThirdUtils.MakeProfileFromPoint(allCurves, pt);
                //if (testProfile != null)
                //{
                //    Utils.DrawProfile(testProfile, "test");
                //}
            }

            //// 梁数据
            //var beamCurves = Utils.GetAllCurvesFromLayerNames(beamLayers);
            ////柱子数据
            //var columnCurves = Utils.GetAllCurvesFromLayerNames(columnLayers);
            //// 需要剔除的柱子轮廓数据
            ////var columnTesCurves = TopoUtils.TesslateCurve(columnCurves);
            ////var columnLoops = TopoUtils.MakeSrcProfilesNoTes(columnTesCurves);

            //var innerCurves = new List<Curve>();
            //if (beamCurves != null && beamCurves.Count != 0)
            //    innerCurves.AddRange(beamCurves);
            //if (columnCurves != null && columnCurves.Count != 0)
            //    innerCurves.AddRange(columnCurves);

        }
    }
}
