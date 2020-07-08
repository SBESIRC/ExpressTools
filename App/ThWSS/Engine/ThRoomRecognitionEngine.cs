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
using ThWSS.Beam;

namespace ThWSS.Engine
{
    public class ThRoom : ThModelElement
    {
        public override Dictionary<string, object> Properties { get; set; }
    }

    public class ThRoomRecognitionEngine : ThModeltRecognitionEngine, IDisposable
    {
        public override List<ThModelElement> Elements { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public ThRoomRecognitionEngine()
        {
            Elements = new List<ThModelElement>();
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        public void Dispose()
        {
            //
        }

        public override bool Acquire(Database database, Polyline floor, ObjectId frame)
        {
            using (var acadDatabase = AcadDatabase.Active())
            {
                var selectPLines = new List<Polyline>()
                {
                    acadDatabase.Element<Polyline>(frame, true),
                };

                var extendLength = 100;
                // 开始弹出进度条提示
                Progress.ShowProgress();
                Progress.SetTip("图纸预处理...");

                Progress.SetValue(900);
                var pickTextNodes = Utils.GetRoomTextNodes("AD-NAME-ROOM");

                // 获取相关图层中的数据
                // allCurves指所有能作为墙一部分的曲线
                var srcAllCurves = Utils.GetAllCurvesFromLayerNames(ThRoomLayerManager.Instance.AllLayers());
                if (srcAllCurves == null || srcAllCurves.Count == 0)
                {
                    Progress.HideProgress();
                    return false;
                }

                // wall 中的数据
                var srcWallAllCurves = Utils.GetAllCurvesFromLayerNames(ThRoomLayerManager.Instance.WallLayers());
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

                    var curSelectTextNodes = Utils.GetValidFRoomNodeFromSelectPLine(pickTextNodes, curSelectPLine);
                    if (curSelectTextNodes == null || curSelectTextNodes.Count == 0)
                        continue;

                    var allCurves = Utils.GetValidCurvesFromSelectPLine(srcAllCurves, curSelectPLine);

                    foreach (var textNode in curSelectTextNodes)
                        Utils.DrawPreviewPoint(textNode.textPoint, "pick");

                    allCurves = TopoUtils.TesslateCurve(allCurves);
                    allCurves = Utils.ExtendCurves(allCurves, extendLength);

                    var wallAllCurves = Utils.GetValidCurvesFromSelectPLineNoSelf(srcWallAllCurves, curSelectPLine);
                    wallAllCurves = TopoUtils.TesslateCurve(wallAllCurves);

                    // wind线作为墙的一部分
                    if (ThRoomLayerManager.Instance.WindowLayers().Count != 0)
                    {
                        var windCurves = Utils.GetWindDOORCurves(ThRoomLayerManager.Instance.WindowLayers());
                        windCurves = Utils.GetValidCurvesFromSelectPLineNoSelf(windCurves, curSelectPLine);
                        if (windCurves != null && windCurves.Count != 0)
                        {
                            var tesslateWindCurves = TopoUtils.TesslateCurve(windCurves);
                            tesslateWindCurves = Utils.ExtendCurves(tesslateWindCurves, extendLength);
                            wallAllCurves.AddRange(tesslateWindCurves);
                            allCurves.AddRange(tesslateWindCurves);
                        }
                    }

                    // door线作为墙的一部分
                    if (ThRoomLayerManager.Instance.DoorLayers().Count != 0)
                    {
                        var doorCurves = Utils.GetWindDOORCurves(ThRoomLayerManager.Instance.DoorLayers());
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
                    if (ThRoomLayerManager.Instance.DoorLayers().Count != 0)
                    {
                        var doorBounds = Utils.GetBoundsFromDOORLayerCurves(ThRoomLayerManager.Instance.DoorLayers());
                        doorBounds = Utils.GetValidBoundsFromSelectPLine(doorBounds, curSelectPLine);
                        var doorInsertCurves = Utils.InsertDoorRelatedCurveDatas(doorBounds, allCurves, ThRoomLayerManager.Instance.DoorLayers().First());

                        if (doorInsertCurves != null && doorInsertCurves.Count != 0)
                        {
                            doorInsertCurves = Utils.ExtendCurves(doorInsertCurves, extendLength);
                            allCurves.AddRange(doorInsertCurves);
                            Utils.DrawProfile(doorInsertCurves, "doorInsertCurves");
                        }
                    }

                    beginPos += smallStep;
                    Progress.SetValue((int)beginPos);
                    // wind 中的数据
                    if (ThRoomLayerManager.Instance.WindowLayers().Count != 0)
                    {
                        var windBounds = Utils.GetBoundsFromWINDLayerCurves(ThRoomLayerManager.Instance.WindowLayers());
                        windBounds = Utils.GetValidBoundsFromSelectPLine(windBounds, curSelectPLine);
                        var windInsertCurves = Utils.InsertDoorRelatedCurveDatas(windBounds, wallAllCurves, ThRoomLayerManager.Instance.WindowLayers().First());

                        if (windInsertCurves != null && windInsertCurves.Count != 0)
                        {
                            windInsertCurves = Utils.ExtendCurves(windInsertCurves, extendLength);
                            Utils.DrawProfile(windInsertCurves, "windInsertCurves");
                            allCurves.AddRange(windInsertCurves);
                        }
                    }

                    allCurves = CommonUtils.RemoveCollinearLines(allCurves);

                    beginPos += smallStep;
                    Progress.SetValue((int)beginPos);
                    var inc = (profileFindPre * 3.0) / curSelectTextNodes.Count;
                    var hasPutPolys = new List<Tuple<Point3d, double>>();

                    int roomIndex = 0;
                    foreach (var selectTextNode in curSelectTextNodes)
                    {
                        beginPos += inc;
                        Progress.SetValue((int)beginPos);

                        try
                        {
                            var aimProfile = TopoUtils.MakeProfileFromPoint2(allCurves, selectTextNode.textPoint);
                            if (aimProfile == null)
                                continue;

                            if (CommonUtils.HasPolylines(hasPutPolys, aimProfile.profile))
                                continue;

                            // 包含水和井且面积大于3平米，则布置
                            var roomTextName = selectTextNode.textString;
                            if (roomTextName.Contains("水") && roomTextName.Contains("井"))
                            {
                                if ((Math.Abs(aimProfile.profile.Area) / 1e6) < 3)
                                    continue;
                            }

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
                            Utils.DrawProfile(new List<Curve>() { aimProfile.profile }, ThWSSCommon.AreaOutlineLayer);
                        }
                        catch
                        {
                            //Active.WriteMessage(e.Message);
                        }
                    }
                }

                Progress.SetValue(6500);
                Progress.HideProgress();
                return true;
            }
        }

        public override bool Acquire(Database database, Polyline floor, ObjectIdCollection frames)
        {
            var plines = new DBObjectCollection();
            using (var acadDatabase = AcadDatabase.Use(database))
            {
                // 为了确保选择的多段线可以形成封闭的房间轮廓
                // 把多段线的Closed状态设置成true
                foreach (ObjectId frame in frames)
                {
                    var pline = acadDatabase.Element<Polyline>(frame);
                    var clone = pline.GetTransformedCopy(Matrix3d.Identity) as Polyline;
                    clone.Closed = true;
                    plines.Add(clone);
                }
            }
            return Acquire(database, floor, plines);
        }

        public override bool Acquire(Database database, Polyline floor, DBObjectCollection frames)
        {
            // 获取房间轮廓
            using (var acadDatabase = AcadDatabase.Use(database))
            {
                int roomIndex = 0;
                foreach (Polyline frame in frames)
                {
                    ThRoom thRoom = new ThRoom()
                    {
                        Properties = new Dictionary<string, object>()
                        {
                            { string.Format("ThRoom{0}", roomIndex++), frame }
                        }
                    };
                    Elements.Add(thRoom);
                }
            }

            // 获取房间内的柱
            using (var columnEngine = new ThColumnRecognitionEngine(database))
            {
                columnEngine.Acquire(database, floor, frames);
                Elements.AddRange(columnEngine.Elements);
            }

            return true;
        }

        public override bool Acquire(ThModelElement element)
        {
            throw new NotImplementedException();
        }
    }
}
