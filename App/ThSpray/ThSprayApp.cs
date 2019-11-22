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
            // 获取图层中的所有线
            var allcurves = Utils.GetAllCurves();

            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            PromptPointOptions ppo = new PromptPointOptions("\n请点击");
            ppo.AllowNone = false;
            PromptPointResult ppr = ed.GetPoint(ppo);
            if (ppr.Status != PromptStatus.OK)
                return;

            var pickPoint = ppr.Value;
            var polylines = TopoUtils.MakeProfileFromPoint(allcurves, pickPoint);
            Utils.DrawProfile(polylines, "图论拓扑");
            //// 拓扑 轮廓计算
            //var polylines = TopoUtils.MakeSrcProfiles(allcurves);
            //Utils.DrawProfile(polylines, "re");
        }
    }
}
