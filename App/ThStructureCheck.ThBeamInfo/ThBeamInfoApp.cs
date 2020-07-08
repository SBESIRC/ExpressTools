using Autodesk.AutoCAD.ApplicationServices;
using System.Collections.Generic;
using ThStructure.BeamInfo.Command;
using ThStructure.BeamInfo.Model;
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
            //从Yjk导入柱、梁集合信息，在
            ThDrawBeam thDrawBeam = new ThDrawBeam(dtlModelPath, dtlCalcPath, 3);
            thDrawBeam.Draw();

            //识别图纸
            Document document = CadTool.GetMdiActiveDocument();
            var promptRes = document.Editor.GetEntity("\nPlease select a polyline");
            Autodesk.AutoCAD.DatabaseServices.Polyline polyline = null;
            if (promptRes.Status == Autodesk.AutoCAD.EditorInput.PromptStatus.OK)
            {
                using (var trans = document.TransactionManager.StartTransaction())
                {
                    polyline = trans.GetObject(promptRes.ObjectId, Autodesk.AutoCAD.DatabaseServices.OpenMode.ForRead) as
                        Autodesk.AutoCAD.DatabaseServices.Polyline;
                    trans.Commit();
                }
            }
            if (polyline == null)
            {
                return;
            }
            List<Beam> dwgBeams = new List<Beam>();
            using (ThBeamDbManager beamManager = new ThBeamDbManager(document.Database))
            {
                ThDisBeamCommand thDisBeamCommand = new ThDisBeamCommand();
                dwgBeams=thDisBeamCommand.CalBeamStrucWithInfo(ThBeamGeometryService.Instance.BeamCurves(document.Database, polyline));
            }

            //关联Dwg识别的梁断和Yjk导入的梁断
            ThBeamMatch thBeamMatch = new ThBeamMatch(thDrawBeam.BeamEnts, dwgBeams);
            thBeamMatch.Match();

            //对图纸中梁段进行校核 
        }
        public static void TestModelBeamLink()
        {
            Document document = CadTool.GetMdiActiveDocument();
            var promptRes = document.Editor.GetEntity("\nPlease select a polyline");
            Autodesk.AutoCAD.DatabaseServices.Polyline polyline=null;
            if (promptRes.Status==Autodesk.AutoCAD.EditorInput.PromptStatus.OK)
            {
                using (var trans=document.TransactionManager.StartTransaction())
                {
                    polyline = trans.GetObject(promptRes.ObjectId,Autodesk.AutoCAD.DatabaseServices.OpenMode.ForRead) as
                        Autodesk.AutoCAD.DatabaseServices.Polyline;
                    trans.Commit();
                }
            }
            if(polyline == null)
            {
                return;
            }
            using (ThBeamDbManager beamManager = new ThBeamDbManager(document.Database))
            {
                ThDisBeamCommand thDisBeamCommand = new ThDisBeamCommand();
                var beams = thDisBeamCommand.CalBeamStruc(ThBeamGeometryService.Instance.BeamCurves(document.Database, polyline));

                List<BeamDistinguishInfo> beamInfos = new List<BeamDistinguishInfo>();
                beams.ForEach(i => beamInfos.Add(new BeamDistinguishInfo(i)));
                DataPalette.Instance.Show(beamInfos);
            }
        }
    }
}
