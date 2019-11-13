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
            //var point = new Point3d(25204.6072597596, 27744.9386472017, 0);
            //foreach (var curve in allcurves)
            //{
            //    var arc = curve as Arc;
            //    CommonUtils.IsPointOnArc(point, arc);
            //}
            //return;

            // 拓扑 轮廓计算
            var profileCurves = new List<Curve>();
            var polylines = TopoUtils.MakeSrcProfiles(allcurves);
            Utils.DrawProfile(polylines, "re");
        }
    }
}
