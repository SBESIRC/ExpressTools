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

            // 拓扑 轮廓计算
            var profileCurves = new List<Curve>();
            var polylines = TopoUtils.MakeSrcProfiles(allcurves);
        }
    }
}
