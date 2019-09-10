using Linq2Acad;
using DotNetARX;
using AcHelper;
using System.Linq;
using Autodesk.AutoCAD.Runtime;
using ThRoomBoundary.topo;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

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
            //关闭所有图层
            using (var db = AcadDatabase.Active())
            {
                var layers = db.Layers;
                foreach (var layer in layers)
                {
                    //layerNames.Add(layer.Name);
                }
            }

            //test
            string layerName = "0";
            var curves = ThRoomData.GetAllCurvesFromLayerName(layerName);
            //ThRoomData.DrawCurves(curves);

            //打开需要图层
            var texts = ThRoomData.GetAllTextFromLayerName(layerName);
            foreach (var text in texts)
            {
                var loopLst = TopoUtils.MakeStructureMinLoop(curves, text.Position);
                if (loopLst == null)
                    continue;
                var loopCurveLst = new List<List<Curve>>();
                foreach (var lines in loopLst)
                {
                    loopCurveLst.Add(lines.ToList<Curve>());
                }
                foreach (var loop in loopCurveLst)
                {
                     ThRoomData.DrawCurvesAdd(loop);
                }
            }
            // 矩形框裁剪


        }
    }
}
