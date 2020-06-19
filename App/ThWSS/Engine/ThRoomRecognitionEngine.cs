using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using TopoNode;

namespace ThWSS.Engine
{
    public class ThRoom : ThModelElement
    {
        public override Dictionary<string, object> Properties { get; set; }
    }

    public class ThRoomRecognitionEngine : ThModeltRecognitionEngine
    {
        public override List<ThModelElement> Elements { get; set; }

        public override bool Acquire(Database database, Polyline polygon)
        {
            Elements = new List<ThModelElement>();

            //打开需要的图层
            List<string> wallLayers = null;
            List<string> arcDoorLayers = null;
            List<string> windLayers = null;
            List<string> validLayers = null;
            List<string> beamLayers = null;
            List<string> columnLayers = null;
            var allCurveLayers = Utils.ShowThLayers(out wallLayers, out arcDoorLayers, out windLayers, out validLayers, out beamLayers, out columnLayers);

            // 图元预处理
            var pickPoints = new List<Point3d>();
            var removeEntityLst = Utils.PreProcess2(validLayers);
            pickPoints = Utils.GetRoomPoints("AD-NAME-ROOM");

            foreach (var pt in pickPoints)
                Utils.DrawPreviewPoint(pt, "pick");

            // 获取相关图层中的数据
            var allCurves = Utils.GetAllCurvesFromLayerNames(allCurveLayers);// allCurves指所有能作为墙一部分的曲线
            var layerCurves = Utils.GetLayersFromCurves(allCurves);
            if (allCurves == null || allCurves.Count == 0)
            {
                return false;
            }

            allCurves = TopoUtils.TesslateCurve(allCurves);
            Utils.ExtendCurves(allCurves, 10);

            // wall 中的数据
            var wallAllCurves = Utils.GetAllCurvesFromLayerNames(wallLayers);
            if (wallAllCurves == null || wallAllCurves.Count == 0 || wallLayers.Count == 0)
            {
                Utils.PostProcess(removeEntityLst);
                return false;
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

            var progress = 61.0;
            var inc = 30.0 / pickPoints.Count;
            var hasPutPolys = new List<Tuple<Point3d, double>>();
            for (int i = 0; i < pickPoints.Count; i++)
            {
                try
                {
                    progress += inc;
                    var profile = TopoUtils.MakeProfileFromPoint2(allCurves, pickPoints[i]);
                    if (profile == null)
                        continue;

                    if (CommonUtils.HasPolylines(hasPutPolys, profile.profile))
                        continue;
                    
                    ThRoom thRoom = new ThRoom();
                    thRoom.Properties = new Dictionary<string, object>();
                    thRoom.Properties.Add("ThRoom" + i, profile.profile);
                    Elements.Add(thRoom);
                    
                    foreach (var plineDic in profile.InnerPolylineLayers)
                    {
                        if (plineDic.profileLayers.Where(x => x.ToUpper().Contains("S_COLU")).Count() > 0)
                        {
                            ThColumn thColumn = new ThColumn();
                            thColumn.Properties = new Dictionary<string, object>();
                            thColumn.Properties.Add("ThColumn" + i, plineDic.profile);
                            Elements.Add(thColumn);
                        }
                    }

                    Utils.DrawProfile(new List<Curve>() { profile.profile }, "outProfile");
                    Utils.DrawTextProfile(profile.profileCurves, profile.profileLayers);
                }
                catch
                { }
            }

            Utils.PostProcess(removeEntityLst);
            return true;
        }

        public override bool Acquire(ThModelElement element)
        {
            throw new NotImplementedException();
        }
    }
}
