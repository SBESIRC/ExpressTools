using Autodesk.AutoCAD.ApplicationServices;
using ThStructure.BeamInfo.Command;
using ThStructureCheck.Common;
using ThStructureCheck.ThBeamInfo.Service;
using ThWSS.Beam;

namespace ThStructureCheck
{
    public class ThBeamInfoApp
    {
        public static void Run()
        {
            string dtlCalcPath = @"D:\柱校核\实例 - Send 1023\A3#楼 - 伪原位\计算模型\施工图\dtlCalc.ydb";
            string dtlModelPath = @"D:\柱校核\实例 - Send 1023\A3#楼 - 伪原位\计算模型\施工图\dtlmodel.ydb";
            ThDrawBeam thDrawBeam = new ThDrawBeam(dtlModelPath, dtlCalcPath, 3);
            thDrawBeam.Draw();
            Document document = CadTool.GetMdiActiveDocument();
            using (ThBeamDbManager beamManager = new ThBeamDbManager(document.Database))
            {
                ThDisBeamCommand thDisBeamCommand = new ThDisBeamCommand();
                thDisBeamCommand.CalBeamStruc(ThBeamGeometryService.Instance.BeamCurves(beamManager));
            }
        }
    }
}
