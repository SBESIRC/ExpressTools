using Autodesk.AutoCAD.ApplicationServices;
using System.Collections.Generic;
using ThStructure.BeamInfo.Command;
using ThStructure.BeamInfo.Model;
using ThStructureCheck.Common;
using ThStructureCheck.ThBeamInfo.Model;
using ThStructureCheck.ThBeamInfo.Service;
using ThStructureCheck.ThBeamInfo.View;
using ThWSS.Beam;
using System.IO;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using System;
using ThStructureCheck.YJK.Model;
using Autodesk.AutoCAD.Geometry;

namespace ThStructureCheck
{
    public class ThBeamInfoApp
    {
        public static void Run(int flrNo)
        {
            TbBeamLayerManager.CreateLayer();            
            string dtlCalcPath = "";
            string dtlModelPath = "";
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "请选择计算书文件路径";
            if(dialog.ShowDialog()==DialogResult.OK)
            {
                dtlCalcPath = dialog.SelectedPath + "\\施工图\\dtlCalc.ydb";
                dtlModelPath = dialog.SelectedPath + "\\施工图\\dtlmodel.ydb";
            }
            else
            {
                return;
            }
            if(string.IsNullOrEmpty(dtlCalcPath) || !File.Exists(dtlCalcPath))
            {
                MessageBox.Show("dtlCalc.ydb 文件不存在!");
            }
            if (string.IsNullOrEmpty(dtlModelPath) || !File.Exists(dtlModelPath))
            {
                MessageBox.Show("dtlmodel.ydb 文件不存在!");
            }
            //从Yjk导入柱、梁集合信息，在
            ThDrawBeam thDrawBeam = new ThDrawBeam(dtlModelPath, dtlCalcPath, flrNo);
            thDrawBeam.Draw();
            if(!thDrawBeam.IsGoOn)
            {
                return;
            }
            //打印配筋信息
            List<Entity> textEnts = new List<Entity>();
            thDrawBeam.GenerateBeamCalculateInfo();
            thDrawBeam.BeamSegIndicator.ForEach(o =>
            {
                textEnts.AddRange(new BeamLinkExtension().DrawTexts(o.Item2, o.Item3));
            });
            textEnts.ForEach(o => o.Layer = TbBeamLayerManager.ThBeamTextLayer);
            
            //组块
            Document doc = CadTool.GetMdiActiveDocument();
            List<Entity> blockEnts = new List<Entity>();
            blockEnts.AddRange(thDrawBeam.TotalDrawEnts);
            blockEnts.AddRange(textEnts);

            string blkName = "ThBeam"+ DateTime.Now.ToString("yyyyMMddHHmmss");
            ObjectId objId = doc.Database.CreateBlock(blkName, blockEnts);
            Scale3d scale = new Scale3d(1.0, 1.0, 1.0);
            doc.Database.InsertBlock(Point3d.Origin, blkName, scale, 0.0);

            //识别图纸
            //Document document = CadTool.GetMdiActiveDocument();
            //var promptRes = document.Editor.GetEntity("\nPlease select a polyline");
            //Autodesk.AutoCAD.DatabaseServices.Polyline polyline = null;
            //if (promptRes.Status == Autodesk.AutoCAD.EditorInput.PromptStatus.OK)
            //{
            //    using (var trans = document.TransactionManager.StartTransaction())
            //    {
            //        polyline = trans.GetObject(promptRes.ObjectId, Autodesk.AutoCAD.DatabaseServices.OpenMode.ForRead) as
            //            Autodesk.AutoCAD.DatabaseServices.Polyline;
            //        trans.Commit();
            //    }
            //}
            //if (polyline == null)
            //{
            //    return;
            //}
            //List<Beam> dwgBeams = new List<Beam>();
            //using (ThBeamDbManager beamManager = new ThBeamDbManager(document.Database))
            //{
            //    ThDisBeamCommand thDisBeamCommand = new ThDisBeamCommand();
            //    dwgBeams=thDisBeamCommand.CalBeamStrucWithInfo(ThBeamGeometryService.Instance.BeamCurves(document.Database, polyline));
            //}

            ////关联Dwg识别的梁断和Yjk导入的梁断
            //ThBeamMatch thBeamMatch = new ThBeamMatch(thDrawBeam.BeamEnts, dwgBeams);
            //thBeamMatch.Match();

            ////对图纸中梁段进行校核 
        }
        private void CreateTestBeams()
        {
            #region----------Create test beams-----------
            List<TestBeamInfo> testBeamInfos = new List<TestBeamInfo>();
            testBeamInfos.Add(new TestBeamInfo
            {
                BeamNum = "WKL1",
                SpanNum = "1",
                SectionSize = "300x800",
                Hooping = "C10@100/200(4)",
                UpErectingBar = "4C20",
                DownErectingBar = "5C16",
                UpLeftSteel = "4C20",
                UpRightSteel = "4C20",
                TwistedSteel = "N6C12",
                Handle = "19BA"
            });
            testBeamInfos.Add(new TestBeamInfo
            {
                BeamNum = "WKL1",
                SpanNum = "2",
                SectionSize = "300x800",
                Hooping = "C10@100/200(4)",
                UpErectingBar = "4C20",
                DownErectingBar = "5C16",
                UpLeftSteel = "4C20",
                UpRightSteel = "4C20",
                TwistedSteel = "N6C12",
                Handle = "19B9"
            });
            testBeamInfos.Add(new TestBeamInfo
            {
                BeamNum = "WKL1",
                SpanNum = "悬挑1",
                SectionSize = "300x800",
                Hooping = "C8@100(2)",
                UpErectingBar = "5C20",
                DownErectingBar = "2C18",
                UpLeftSteel = "5C20",
                UpRightSteel = "5C20",
                TwistedSteel = "G6C12",
                Handle = "19BD"
            });
            testBeamInfos.Add(new TestBeamInfo
            {
                BeamNum = "WKL2",
                SpanNum = "1",
                SectionSize = "300x800",
                Hooping = "C10@100/200(4)",
                UpErectingBar = "4C25",
                DownErectingBar = "4C25",
                UpLeftSteel = "4C25",
                UpRightSteel = "5C25 3/2",
                TwistedSteel = "N6C12",
                Handle = "19C0"
            });
            testBeamInfos.Add(new TestBeamInfo
            {
                BeamNum = "WKL2",
                SpanNum = "2",
                SectionSize = "300x800",
                Hooping = "C10@100/200(4)",
                UpErectingBar = "4C25",
                DownErectingBar = "4C25+1C20",
                UpLeftSteel = "5C25 3/2",
                UpRightSteel = "4C25",
                TwistedSteel = "G6C12",
                Handle = "19BF"
            });
            #endregion
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
    class TestBeamInfo : ThOriginMarkingcs
    {
        /// <summary>
        /// 梁编号
        /// </summary>
        public string BeamNum { get; set; } = "";
        /// <summary>
        /// 第几跨
        /// </summary>
        public string SpanNum { get; set; } = "";
        public string Handle { get; set; } = "";
    }

}
