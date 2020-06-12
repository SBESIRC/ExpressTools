using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThStructureCheck.Common;
using ThStructureCheck.ThBeamInfo.Service;

namespace ThStructureCheck.UI
{
    public class ThStructureCheckUiApp : IExtensionApplication
    {
        public void Initialize()
        {
        }

        public void Terminate()
        {
        }
    }
    public class Commands
    {
        [CommandMethod("ThBeamCheck")]
        public void TestBeamRelate()
        {
            ThBeamInfoApp.Run();
        }
        [CommandMethod("ThTestApi")]
        public void ThTestApi()
        {
            var doc = CadTool.GetMdiActiveDocument();
            string message = "\n选择一条直线"; //"\n选择一个三维多段线"
            var res = doc.Editor.GetEntity(message);
            var ptRes = doc.Editor.GetPoint("\n选择一点");
            if (res.Status == Autodesk.AutoCAD.EditorInput.PromptStatus.OK)
            {
                //Curve curve = CadTool.GetEntity<Curve>(res.ObjectId);
                //CadTool.GetPolylinePts(curve);
                Line line = CadTool.GetEntity<Line>(res.ObjectId);

                //Vector3d vec= ThBeamUtils.GetBeamOffsetDirection(line.StartPoint, line.EndPoint);
                //Point3d pt = line.EndPoint + vec.GetNormal().MultiplyBy(line.Length);
                //Line newLine = new Line(line.EndPoint, pt);
                //CadTool.AddToBlockTable(newLine);
            }
        }
    }
}
