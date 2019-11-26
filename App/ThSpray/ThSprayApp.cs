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

        [CommandMethod("TIANHUACAD", "TopoSearch", CommandFlags.Modal)]
        static public void TestBoundary()
        {
            //打开需要的图层
            List<string> wallLayers = null;
            List<string> arcDoorLayers = null;
            List<string> windLayers = null;
            List<string> validLayers = null;
            var allCurveLayers = Utils.ShowThLayers(out wallLayers, out arcDoorLayers, out windLayers, out validLayers);

            // 获取相关图层中的数据
            // 所有数据
            var allCurves = Utils.GetAllCurvesFromLayerNames(allCurveLayers);
            if (allCurves == null || allCurves.Count == 0)
            {
                return;
            }

            // wall 中的数据
            var wallAllCurves = Utils.GetAllCurvesFromLayerNames(wallLayers);
            if (wallAllCurves == null || wallAllCurves.Count == 0 || wallLayers.Count == 0)
            {
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

            allCurves = TopoUtils.TesslateCurve(allCurves);
            allCurves = CommonUtils.RemoveCollinearLines(allCurves);

            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            while (true)
            {
                PromptPointOptions ppo = new PromptPointOptions("\n请点击");
                ppo.AllowNone = false;
                PromptPointResult ppr = ed.GetPoint(ppo);
                if (ppr.Status != PromptStatus.OK)
                    return;

                var pickPoint = ppr.Value;
                var polylines = TopoUtils.MakeProfileFromPoint(allCurves, pickPoint);
                Utils.DrawProfile(polylines, "sd");
            }
        }
    }
}
