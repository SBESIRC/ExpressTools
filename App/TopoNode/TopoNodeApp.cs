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

            // 开始弹出进度条提示
            Progress.Progress.ShowProgress();

            // 图元预处理
            var removeEntityLst = Utils.PreProcess2(validLayers);
            pickPoints = Utils.GetRoomPoints("AD-NAME-ROOM");

            foreach (var pt in pickPoints)
                Utils.DrawPreviewPoint(pt, "pick");
            //return;

            // 获取相关图层中的数据
            var allCurves = Utils.GetAllCurvesFromLayerNames(allCurveLayers);// allCurves指所有能作为墙一部分的曲线
            var layerCurves = Utils.GetLayersFromCurves(allCurves);
            if (allCurves == null || allCurves.Count == 0)
            {
                Utils.PostProcess(removeEntityLst);
                Progress.Progress.HideProgress();
                return;
            }

            allCurves = TopoUtils.TesslateCurve(allCurves);
            Utils.ExtendCurves(allCurves, 3);

            // wall 中的数据
            var wallAllCurves = Utils.GetAllCurvesFromLayerNames(wallLayers);
            if (wallAllCurves == null || wallAllCurves.Count == 0 || wallLayers.Count == 0)
            {
                Utils.PostProcess(removeEntityLst);
                Progress.Progress.HideProgress();
                return;
            }
            Progress.Progress.SetValue(40);
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

            Progress.Progress.SetValue(45);
            var layerNames = Utils.GetLayersFromCurves(allCurves);

            layerNames = Utils.GetLayersFromCurves(allCurves);
            allCurves = CommonUtils.RemoveCollinearLines(allCurves);
            layerNames = Utils.GetLayersFromCurves(allCurves);
            Utils.DrawProfileAndText(allCurves);
            Utils.PostProcess(removeEntityLst);
            //return;
            //Utils.DrawProfile(allCurves, "outerFile");
            //return;
            Progress.Progress.SetValue(60);

            var progress = 61.0;
            var inc = 30.0 / pickPoints.Count;
            var hasPutPolys = new List<Tuple<Point3d, double>>();
            foreach (var pt in pickPoints)
            {
                try
                {
                    progress += inc;
                    Progress.Progress.SetValue((int)progress);

                    var profile = TopoUtils.MakeProfileFromPoint(allCurves, pt);
                    if (profile == null || profile.Count == 0)
                        continue;

                    var outProfile = profile.First();

                    if (CommonUtils.HasPolylines(hasPutPolys, outProfile.profile))
                        continue;

                    Utils.DrawProfile(new List<Curve>() { outProfile.profile }, "outProfile");
                    Utils.DrawTextProfile(outProfile.profileCurves, outProfile.profileLayers);
                }
                catch
                { }
            }

            Utils.PostProcess(removeEntityLst);
            Progress.Progress.HideProgress();
            //Utils.ErasePreviewPoint(objCollect);
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
            Utils.ExtendCurves(allCurves, 3);
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
                    var profile = TopoUtils.MakeProfileFromPoint(allCurves, pt);
                    if (profile == null || profile.Count == 0)
                        continue;

                    var outProfile = profile.First();
                    if (CommonUtils.HasPolylines(hasPutPolys, outProfile.profile))
                        continue;

                    Utils.DrawProfile(new List<Curve>() { outProfile.profile }, "outProfile");
                    Utils.DrawTextProfile(outProfile.profileCurves, outProfile.profileLayers);
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
