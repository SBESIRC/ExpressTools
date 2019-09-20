using Linq2Acad;
using DotNetARX;
using AcHelper;
using System.Linq;
using Autodesk.AutoCAD.Runtime;
using ThRoomBoundary.topo;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace ThRoomBoundary
{
    public class ThRoomBoundaryApp : IExtensionApplication
    {
        public void Initialize()
        {
        }

        public void Terminate()
        {
        }

        [CommandMethod("TIANHUACAD", "THABC", CommandFlags.Modal)]
        static public void RoomBoundaryCalculate()
        {
            //打开需要的图层
            List<string> wallLayers = null;
            List<string> arcDoorLayers = null;
            List<string> windLayers = null;
            var allCurveLayers = ThRoomUtils.ShowThLayers(out wallLayers, out arcDoorLayers, out windLayers);
            if (allCurveLayers == null || allCurveLayers.Count == 0)
                return;

            if (wallLayers == null || wallLayers.Count == 0)
                return;

            // 选择框线生成
            var rectLines = ThRoomUtils.GetSelectRectLines();
            if (rectLines == null || rectLines.Count == 0)
                return;

            //// 是否插入面积值
            //Document doc = Application.DocumentManager.MdiActiveDocument;
            //Editor ed = doc.Editor;
            //PromptKeywordOptions getWhichOptions = new PromptKeywordOptions("是否在框线内插入面积值? [YES(Y)/NO(N)]", "YES NO");
            //getWhichOptions.Keywords.Add("Y");
            //getWhichOptions.Keywords.Add("N");
            //getWhichOptions.Keywords.Default = "Y";
            //PromptResult getWhichResult = ed.GetKeywords(getWhichOptions);
            //if ((getWhichResult.Status == PromptStatus.OK))
            //{
            //    if (getWhichResult.StringResult != "YES")
            //        return;
            //}

            // 获取相关图层中的数据
            // 所有数据
            var allCurves = ThRoomUtils.GetAllCurvesFromLayerNames(allCurveLayers);
            if (allCurves == null || allCurves.Count == 0)
                return;

            var layerCurves = ThRoomUtils.GetValidCurvesFromSelectArea(allCurves, rectLines);

            // wall 中的数据
            var wallAllCurves = ThRoomUtils.GetAllCurvesFromLayerNames(wallLayers);
            if (wallAllCurves == null || wallAllCurves.Count == 0 || wallLayers.Count == 0)
                return;

            var wallCurves = ThRoomUtils.GetValidCurvesFromSelectArea(wallAllCurves, rectLines);
            

            // door 内门中的数据
            if (arcDoorLayers != null && arcDoorLayers.Count != 0)
            {
                var doorBounds = ThRoomUtils.GetBoundsFromLayerBlocks(arcDoorLayers, rectLines);
                var doorInsertCurves = ThRoomUtils.InsertDoorRelatedCurveDatas(doorBounds, wallCurves, arcDoorLayers.First());

                if (doorInsertCurves != null && doorInsertCurves.Count != 0)
                {
                    //wallCurves.AddRange(doorInsertCurves);
                    layerCurves.AddRange(doorInsertCurves);
                }
            }

            // wind 中的数据
            if (windLayers != null && windLayers.Count != 0)
            {
                var windBounds = ThRoomUtils.GetBoundsFromLayerBlocks(windLayers, rectLines);
                var windInsertCurves = ThRoomUtils.InsertDoorRelatedCurveDatas(windBounds, wallCurves, windLayers.First());

                if (windInsertCurves != null && windInsertCurves.Count != 0)
                {
                    //wallCurves.AddRange(windInsertCurves);
                    layerCurves.AddRange(windInsertCurves);
                }
            }

            // 生成轮廓数据
            //var wallRoomDatas = TopoUtils.MakeSrcProfiles(wallCurves, rectLines);
            var roomDatas = TopoUtils.MakeSrcProfiles(layerCurves);
            if (roomDatas == null || roomDatas.Count == 0)
                return;

            //ThRoomUtils.MakeValidRoomDataPolylines(roomDatas, wallRoomDatas);
            // 界面显示数据
            ThRoomUtils.DisplayRoomProfile(roomDatas, "天华面积框线");
            Active.WriteMessage("框线生成完成");
        }
    }
}
