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
            string dtlCalcPath = @"D:\梁校核\梁图与模型\10地块4#号楼\施工图\dtlCalc.ydb";
            string dtlModelPath = @"D:\梁校核\梁图与模型\10地块4#号楼\施工图\dtlmodel.ydb";
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
