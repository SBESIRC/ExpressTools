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

            // 是否插入面积值
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            PromptKeywordOptions getWhichOptions = new PromptKeywordOptions("\n是否在框线内插入面积值? [YES(Y)/NO(N)]", "YES NO");
            getWhichOptions.Keywords.Default = "YES";
            PromptResult getWhichResult = ed.GetKeywords(getWhichOptions);
            if ((getWhichResult.Status == PromptStatus.OK))
            {
                if (getWhichResult.StringResult != "YES")
                    return;
            }

            ThProgressDialog.ShowProgress();

            // 图元预处理
            var removeEntityLst = ThRoomUtils.PreProcess(rectLines);
            ThProgressDialog.SetValue(30);
            // 获取相关图层中的数据
            // 所有数据
            var allCurves = ThRoomUtils.GetAllCurvesFromLayerNames(allCurveLayers);
            if (allCurves == null || allCurves.Count == 0)
            {
                ThProgressDialog.HideProgress();
                return;
            }
            ThProgressDialog.SetValue(31);
            var layerCurves = ThRoomUtils.GetValidCurvesFromSelectArea(allCurves, rectLines);
            ThProgressDialog.SetValue(32);
            // wall 中的数据
            var wallAllCurves = ThRoomUtils.GetAllCurvesFromLayerNames(wallLayers);
            if (wallAllCurves == null || wallAllCurves.Count == 0 || wallLayers.Count == 0)
            {
                ThProgressDialog.HideProgress();
                return;
            }

            var wallCurves = ThRoomUtils.GetValidCurvesFromSelectArea(wallAllCurves, rectLines);
            ThProgressDialog.SetValue(35);
            // door 内门中的数据
            if (arcDoorLayers != null && arcDoorLayers.Count != 0)
            {
                var doorBounds = ThRoomUtils.GetBoundsFromLayerBlocksAndCurves(arcDoorLayers, rectLines);
                var doorInsertCurves = ThRoomUtils.InsertDoorRelatedCurveDatas(doorBounds, wallCurves, arcDoorLayers.First());

                if (doorInsertCurves != null && doorInsertCurves.Count != 0)
                {
                    layerCurves.AddRange(doorInsertCurves);
                }
            }

            ThProgressDialog.SetValue(39);

            // wind 中的数据
            if (windLayers != null && windLayers.Count != 0)
            {
                var windBounds = ThRoomUtils.GetBoundsFromLayerBlocksAndCurves(windLayers, rectLines);
                var windInsertCurves = ThRoomUtils.InsertDoorRelatedCurveDatas(windBounds, wallCurves, windLayers.First());
                if (windInsertCurves != null && windInsertCurves.Count != 0)
                {
                    layerCurves.AddRange(windInsertCurves);
                }
            }

            ThProgressDialog.SetValue(45);
            // 生成轮廓数据
            var roomDatas = TopoUtils.MakeSrcProfiles(layerCurves);
            if (roomDatas == null || roomDatas.Count == 0)
            {
                ThProgressDialog.HideProgress();
                return;
            }

            ThRoomUtils.PostProcess(removeEntityLst);
            ThProgressDialog.HideProgress();
            // 界面显示数据
            ThRoomUtils.DisplayRoomProfile(roomDatas, ThRoomUtils.ROOMBOUNDARY, ThRoomUtils.ROOMAREAVALUE);
            Active.WriteMessage("框线生成完成");
        }
    }
}
