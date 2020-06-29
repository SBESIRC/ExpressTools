using Autodesk.AutoCAD.ApplicationServices;
using System.Collections.Generic;
using ThStructure.BeamInfo.Command;
using ThStructureCheck.Common;
using ThStructureCheck.ThBeamInfo.Model;
using ThStructureCheck.ThBeamInfo.Service;
using ThStructureCheck.ThBeamInfo.View;
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
            //Document document = CadTool.GetMdiActiveDocument();
            //using (ThBeamDbManager beamManager = new ThBeamDbManager(document.Database))
            //{
            //    ThDisBeamCommand thDisBeamCommand = new ThDisBeamCommand();
            //    thDisBeamCommand.CalBeamStruc(ThBeamGeometryService.Instance.BeamCurves(document.Database));
            //}
        }
        public static void TestModelBeamLink()
        {
            Document document = CadTool.GetMdiActiveDocument();
            using (ThBeamDbManager beamManager = new ThBeamDbManager(document.Database))
            {
                ThDisBeamCommand thDisBeamCommand = new ThDisBeamCommand();
                var beams = thDisBeamCommand.CalBeamStruc(ThBeamGeometryService.Instance.BeamCurves(document.Database));

                List<BeamDistinguishInfo> beamInfos = new List<BeamDistinguishInfo>();
                beams.ForEach(i => beamInfos.Add(new BeamDistinguishInfo(i)));
                DataPalette.Instance.Show(beamInfos);
            }
        }
    }
}
