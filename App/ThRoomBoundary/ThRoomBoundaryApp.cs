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
            ThRoomData.ShowLayers();

            // 选择框线生成
            var rectLines = ThRoomData.GetSelectRectLines();
            if (rectLines == null || rectLines.Count == 0)
                return;

            // 是否插入面积值
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            PromptKeywordOptions getWhichOptions = new PromptKeywordOptions("是否在框线内插入面积值? [YES(Y)/NO(N)]", "YES NO");
            getWhichOptions.Keywords.Add("Y");
            getWhichOptions.Keywords.Add("N");
            getWhichOptions.Keywords.Default = "Y";
            PromptResult getWhichResult = ed.GetKeywords(getWhichOptions);
            if ((getWhichResult.Status == PromptStatus.OK))
            {
                if (getWhichResult.StringResult != "YES")
                    return;
            }


            string layerName = "WINDOW";
            var curves = ThRoomData.GetAllCurvesFromLayerName(layerName);
            if (curves == null || curves.Count == 0)
                return;

            // 生成轮廓数据
            var roomDatas = TopoUtils.MakeSrcProfiles(curves, rectLines);

            // 界面显示数据
            ThRoomData.DisplayRoomProfile(roomDatas, "天华面积框线");
            Active.WriteMessage("框线生成完成");
        }
    }
}
