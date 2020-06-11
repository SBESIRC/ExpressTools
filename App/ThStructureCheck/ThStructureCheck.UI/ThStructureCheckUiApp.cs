using Autodesk.AutoCAD.DatabaseServices;
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
            string dtlCalcPath = @"D:\柱校核\实例 - Send 1023\A3#楼 - 伪原位\计算模型\施工图\dtlCalc.ydb";
            string dtlModelPath = @"D:\柱校核\实例 - Send 1023\A3#楼 - 伪原位\计算模型\施工图\dtlmodel.ydb";
            ThDrawBeam thDrawBeam = new ThDrawBeam(dtlModelPath, dtlCalcPath, 3);
            thDrawBeam.Draw();
        }
        [CommandMethod("ThTestApi")]
        public void ThTestApi()
        {
            var doc = CadTool.GetMdiActiveDocument();
            var res = doc.Editor.GetEntity("\n选择一个三维多段线");
            if (res.Status == Autodesk.AutoCAD.EditorInput.PromptStatus.OK)
            {
                Curve curve = CadTool.GetEntity<Curve>(res.ObjectId);
                CadTool.GetPolylinePts(curve);
            }
        }
    }
}
