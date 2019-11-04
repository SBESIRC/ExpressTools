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

        [CommandMethod("SPray", "SPray", CommandFlags.Modal)]
        static public void TestBoundary()
        {
            Active.WriteMessage("SPray");
            // 获取图层中的所有线
            var allcurves = Utils.GetAllCurves();
            //Utils.DrawProfile(allcurves, "test");
            // 拓扑 轮廓计算

            var noCalCurves = new List<Curve>();
            var curvesLst = TopoUtils.MakeSrcProfiles(allcurves, out noCalCurves);
            Utils.DrawProfile(noCalCurves, "test");
            // 界面显示
            Utils.ShowCurves(curvesLst);
        }
    }
}
