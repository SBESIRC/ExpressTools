using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices.Filters;
using DotNetARX;

[assembly: CommandClass(typeof(ThXClip.ThXclipCommands))]
[assembly: ExtensionApplication(typeof(ThXClip.ThXClipApp))]
namespace ThXClip
{
    public class ThXClipApp : IExtensionApplication
    {
        public void Initialize()
        {
            //throw new NotImplementedException();
        }

        public void Terminate()
        {
            //throw new NotImplementedException();
        }
    }
    public delegate void ThXClipFinishHandler(ThXclipCommands sender,bool result);
    public class ThXclipCommands
    {
        public event ThXClipFinishHandler WorkFinished;
        [CommandMethod("TIANHUACAD", "THXLP", CommandFlags.Modal)]        
        public void ThXClip()
        {
            List<ObjectId> selObjIds = ThXClipCadOperation.GetSelectObjects(); //选择要处理的块
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            //判断所选块上的Wipeout 或模型中单独绘制了WipeOut影响了所选的块，都要处理
            //以下是获取附着在所选块上的WipeOut集合
            List<ObjectId> OnBlkWipeOutIds = new List<ObjectId>();
            using (Transaction trans = doc.Database.TransactionManager.StartTransaction())
            {
                //以读方式打开块表
                BlockTable bt = (BlockTable)trans.GetObject(doc.Database.BlockTableId, OpenMode.ForRead);
                //以写方式打开模型空间块表记录.
                BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                
                for (int i = 0; i < selObjIds.Count; i++)
                {
                    DBObject dbObj = trans.GetObject(selObjIds[i], OpenMode.ForRead);
                    if (dbObj is BlockReference)
                    {
                        BlockReference br = dbObj as BlockReference;
                        if(br.Bounds!=null && br.Bounds.HasValue) //获取所选块，是否有在模型空间绘制的WipeOut影响了它
                        {
                            Point3d minPt = br.Bounds.Value.MinPoint;
                            Point3d maxPt = br.Bounds.Value.MaxPoint;
                            if(minPt.GetVectorTo(maxPt).IsParallelTo(Vector3d.XAxis))
                            {
                                minPt = new Point3d(minPt.X,minPt.Y-5,minPt.Z);
                                maxPt = new Point3d(maxPt.X, maxPt.Y + 5, maxPt.Z);
                            }
                            else if (minPt.GetVectorTo(maxPt).IsParallelTo(Vector3d.YAxis))
                            {
                                minPt = new Point3d(minPt.X-5, minPt.Y, minPt.Z);
                                maxPt = new Point3d(maxPt.X+5, maxPt.Y, maxPt.Z);
                            }
                            PromptSelectionResult psr = ThXClipCadOperation.SelectByRectangle(ed, minPt,
                                maxPt, PolygonSelectionMode.Crossing);
                            if (psr.Status == PromptStatus.OK)
                            {
                                List<ObjectId> wipeOutIds = psr.Value.GetObjectIds().Where(j => trans.GetObject(j, OpenMode.ForRead) is Wipeout
                                  && OnBlkWipeOutIds.IndexOf(j) < 0).Select(j => j).ToList();
                                if (wipeOutIds != null && wipeOutIds.Count > 0)
                                {
                                    wipeOutIds=wipeOutIds.Where(j => OnBlkWipeOutIds.IndexOf(j) < 0).Select(j => j).ToList();
                                    OnBlkWipeOutIds.AddRange(wipeOutIds);
                                }
                            }
                        }
                    }
                }
                trans.Commit();
            }
            List<string> lockedLayerNames = new List<string>();
            if(selObjIds.Count>0)
            {
                lockedLayerNames=ThXClipCadOperation.UnlockedAllLayers(); //解锁所有的层
            }
            //ThXclipProgressBar progressBar = new ThXclipProgressBar();
            //progressBar.Register(this);
            //progressBar.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            //progressBar.Show();
            //分析传入的物体之间的 Draworder关系及对块进行炸开处理
            AnalyseRelation analyseRelation = new AnalyseRelation(selObjIds, OnBlkWipeOutIds);
            try
            {
                analyseRelation.Analyse();
                //修剪WipeOut和XClip
                WipeOutXClipTrim wipeOutXClipTrim = new WipeOutXClipTrim(analyseRelation);
                wipeOutXClipTrim.StartTrim();
                foreach (var blkItem in wipeOutXClipTrim.BlkNamePosDic)
                {
                    // 插入图块
                    ThXClipCadOperation.InsertBlockReference("0", blkItem.Key,
                        Point3d.Origin, new Scale3d(1.0, 1.0, 1.0), 0.0);
                }
            }
            catch(System.Exception ex)
            {
                ed.WriteLine(ex.Message);
            }
            finally
            {
                analyseRelation.EraseBlockAndItsExplodedObjs();
                ThXClipCadOperation.LockedLayers(lockedLayerNames);
                //WorkFinished(this, true);
            }
        }
    }
}
