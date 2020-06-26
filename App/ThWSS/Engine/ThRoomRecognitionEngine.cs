using System;
using TopoNode;
using AcHelper;
using Linq2Acad;
using System.Linq;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using TopoNode.Progress;
using ThWSS.Column;

namespace ThWSS.Engine
{
    public class ThRoom : ThModelElement
    {
        public override Dictionary<string, object> Properties { get; set; }
    }

    public class ThRoomRecognitionEngine : ThModeltRecognitionEngine
    {
        public override List<ThModelElement> Elements { get; set; }

        public override bool Acquire(Database database, ObjectId polygon)
        {
            Elements = new List<ThModelElement>();
            using (var dbManager = new ThRoomDbManager(Active.Database))
            using (var acadDatabase = AcadDatabase.Active())
            {
                var selectPLines = new List<Polyline>()
                {
                    acadDatabase.Element<Polyline>(polygon, true),
                };

                // 开始弹出进度条提示
                Progress.ShowProgress();
                Progress.SetTip("图纸预处理...");

                //Utils.PostProcess(removeEntityLst);
                //return;
                Progress.SetValue(900);
                var pickPoints = Utils.GetRoomPoints("AD-NAME-ROOM");

                //foreach (var pt in pickPoints)
                //    Utils.DrawPreviewPoint(pt, "pick");

                // 获取相关图层中的数据
                // allCurves指所有能作为墙一部分的曲线
                var srcAllCurves = Utils.GetAllCurvesFromLayerNames(dbManager.LayerManger.AllLayers());
                if (srcAllCurves == null || srcAllCurves.Count == 0)
                {
                    Progress.HideProgress();
                    return false;
                }

                // wall 中的数据
                var srcWallAllCurves = Utils.GetAllCurvesFromLayerNames(dbManager.LayerManger.WallLayers());
                if (srcWallAllCurves == null || srcWallAllCurves.Count == 0)
                {
                    Progress.HideProgress();
                    return false;
                }

                Progress.SetValue(1000);
                Progress.SetTip("区域识别中...");
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
                    if (dbManager.LayerManger.WindowLayers().Count != 0)
                    {
                        var windCurves = Utils.GetWindDOORCurves(dbManager.LayerManger.WindowLayers());
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
                    if (dbManager.LayerManger.DoorLayers().Count != 0)
                    {
                        var doorCurves = Utils.GetWindDOORCurves(dbManager.LayerManger.DoorLayers());
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
                    Progress.SetValue((int)beginPos);
                    // door 内门中的数据
                    if (dbManager.LayerManger.DoorLayers().Count != 0)
                    {
                        var doorBounds = Utils.GetBoundsFromDOORLayerCurves(dbManager.LayerManger.DoorLayers());
                        doorBounds = Utils.GetValidBoundsFromSelectPLine(doorBounds, curSelectPLine);
                        var doorInsertCurves = Utils.InsertDoorRelatedCurveDatas(doorBounds, allCurves, dbManager.LayerManger.DoorLayers().First());

                        if (doorInsertCurves != null && doorInsertCurves.Count != 0)
                        {
                            allCurves.AddRange(doorInsertCurves);
                            Utils.DrawProfile(doorInsertCurves, "doorInsertCurves");
                            //removeEntityLst.AddRange(doorInsertCurves);
                        }
                    }

                    beginPos += smallStep;
                    Progress.SetValue((int)beginPos);
                    // wind 中的数据
                    if (dbManager.LayerManger.WindowLayers().Count != 0)
                    {
                        var windBounds = Utils.GetBoundsFromWINDLayerCurves(dbManager.LayerManger.WindowLayers());
                        windBounds = Utils.GetValidBoundsFromSelectPLine(windBounds, curSelectPLine);
                        var windInsertCurves = Utils.InsertDoorRelatedCurveDatas(windBounds, wallAllCurves, dbManager.LayerManger.WindowLayers().First());

                        if (windInsertCurves != null && windInsertCurves.Count != 0)
                        {
                            Utils.DrawProfile(windInsertCurves, "windInsertCurves");
                            allCurves.AddRange(windInsertCurves);
                        }
                    }

                    allCurves = CommonUtils.RemoveCollinearLines(allCurves);

                    beginPos += smallStep;
                    Progress.SetValue((int)beginPos);
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

                    int roomIndex = 0;
                    foreach (var pt in curSelectPoints)
                    {
                        beginPos += inc;
                        Progress.SetValue((int)beginPos);

                        try
                        {
                            var aimProfile = TopoUtils.MakeProfileFromPoint2(allCurves, pt);
                            if (aimProfile == null)
                                continue;

                            if (CommonUtils.HasPolylines(hasPutPolys, aimProfile.profile))
                                continue;

                            // 获取房间轮廓
                            ThRoom thRoom = new ThRoom()
                            {
                                Properties = new Dictionary<string, object>()
                                {
                                    { string.Format("ThRoom{0}", roomIndex++),  aimProfile.profile }
                                }
                            };
                            Elements.Add(thRoom);

                            int columnIndex = 0;
                            foreach (var plineDic in aimProfile.InnerPolylineLayers)
                            {
                                if (plineDic.profileLayers.Where(x => x.ToUpper().Contains("S_COLU")).Count() > 0)
                                {
                                    ThColumn thColumn = new ThColumn()
                                    {
                                        Properties = new Dictionary<string, object>()
                                        {
                                            { string.Format("ThColumn{0}", columnIndex++), plineDic.profile }
                                        }
                                    };
                                    Elements.Add(thColumn);
                                }
                            }

                            // 输出房间轮廓曲线
                            Utils.DrawProfile(new List<Curve>() { aimProfile.profile }, "outProfile");
                            // Utils.DrawTextProfile(outProfile.profileCurves, outProfile.profileLayers);
                            //if (aimProfile.InnerPolylineLayers.Count != 0)
                            //{
                            //    foreach (var innerProfile in aimProfile.InnerPolylineLayers)
                            //    {
                            //        Utils.DrawProfile(new List<Curve>() { innerProfile.profile }, "innerProfile");
                            //    }
                            //}
                        }
                        catch
                        {
                            //Active.WriteMessage(e.Message);
                        }
                    }
                }

                Progress.SetValue(6500);
                Progress.HideProgress();
                //Utils.ErasePreviewPoint(objCollect);
            }

            return true;
        }

        public override bool Acquire(Database database, ObjectIdCollection frames)
        {
            using (var dbManager = new ThColumnDbManager(database))
            using (var acadDatabase = AcadDatabase.Use(database))
            {
                Elements = new List<ThModelElement>();

                // 获取房间轮廓
                int roomIndex = 0;
                foreach (ObjectId frame in frames)
                {
                    var pline = acadDatabase.Element<Polyline>(frame);
                    ThRoom thRoom = new ThRoom()
                    {
                        Properties = new Dictionary<string, object>()
                        {
                            { string.Format("ThRoom{0}", roomIndex++),  pline.GetTransformedCopy(Matrix3d.Identity) as Polyline }
                        }
                    };
                    Elements.Add(thRoom);
                }

                // 获取房间内的柱
                using (var columnEngine = new ThColumnRecognitionEngine(dbManager))
                {
                    columnEngine.Acquire(database, frames);
                    Elements.AddRange(columnEngine.Elements);
                    //foreach (var column in columnEngine.Elements)
                    //{
                    //    acadDatabase.ModelSpace.Add(column.Properties.First().Value as Polyline);
                    //}
                }

                return true;
            }
        }


        public override bool Acquire(Database database, DBObjectCollection frames)
        {
            using (var dbManager = new ThColumnDbManager(database))
            using (var acadDatabase = AcadDatabase.Use(database))
            {
                Elements = new List<ThModelElement>();

                // 获取房间轮廓
                int roomIndex = 0;
                foreach (Polyline frame in frames)
                {
                    ThRoom thRoom = new ThRoom()
                    {
                        Properties = new Dictionary<string, object>()
                        {
                            { string.Format("ThRoom{0}", roomIndex++),  frame.GetTransformedCopy(Matrix3d.Identity) as Polyline }
                        }
                    };
                    Elements.Add(thRoom);
                }

                // 获取房间内的柱
                using (var columnEngine = new ThColumnRecognitionEngine(dbManager))
                {
                    columnEngine.Acquire(database, frames);
                    Elements.AddRange(columnEngine.Elements);
                    //foreach(var column in columnEngine.Elements)
                    //{
                    //    acadDatabase.ModelSpace.Add(column.Properties.First().Value as Polyline);
                    //}
                }

                return true;
            }
        }

        public override bool Acquire(ThModelElement element)
        {
            throw new NotImplementedException();
        }
    }
}
