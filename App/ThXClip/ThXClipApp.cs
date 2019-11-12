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
using System.Threading;
using System.ComponentModel;

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
        [CommandMethod("TIANHUACAD", "THXLP", CommandFlags.Modal | CommandFlags.UsePickSet)]
        public void ThXClip()
        {
            List<ObjectId> selObjIds = ThXClipCadOperation.GetSelectObjects(); //选择要处理的块
            if (selObjIds.Count == 0)
            {
                return;
            }
            DateTime startTime = DateTime.Now.ToLocalTime();
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            ViewTableRecord view = ed.GetCurrentView();
            List<Point3d> boundPtList = ThXClipCadOperation.GetObjBoundingPoints(selObjIds);
            if (boundPtList.Count == 2)
            {
                Point3d minPt = boundPtList[0];
                Point3d maxPt = boundPtList[1];
                minPt = new Point3d(minPt.X - 5, minPt.Y - 5, minPt.Z);
                maxPt = new Point3d(maxPt.X + 5, maxPt.Y + 5, minPt.Z);
                ThXClipCadOperation.ZoomObject(ed, minPt, maxPt);
            }
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
                        if (br.Bounds != null && br.Bounds.HasValue) //获取所选块，是否有在模型空间绘制的WipeOut影响了它
                        {
                            PromptSelectionResult psr = ThXClipCadOperation.SelectByRectangle(ed, br.Bounds.Value.MinPoint,
                                br.Bounds.Value.MaxPoint, PolygonSelectionMode.Crossing);
                            if (psr.Status == PromptStatus.OK)
                            {
                                List<ObjectId> wipeOutIds = psr.Value.GetObjectIds().Where(j => trans.GetObject(j, OpenMode.ForRead) is Wipeout
                                  && OnBlkWipeOutIds.IndexOf(j) < 0).Select(j => j).ToList();
                                if (wipeOutIds != null && wipeOutIds.Count > 0)
                                {
                                    wipeOutIds = wipeOutIds.Where(j => OnBlkWipeOutIds.IndexOf(j) < 0).Select(j => j).ToList();
                                    OnBlkWipeOutIds.AddRange(wipeOutIds);
                                }
                            }
                        }
                    }
                }
                trans.Commit();
            }
            List<string> lockedLayerNames = new List<string>();
            if (selObjIds.Count > 0)
            {
                lockedLayerNames = ThXClipCadOperation.UnlockedAllLayers(); //解锁所有的层
            }            
            //分析传入的物体之间的 Draworder关系及对块进行炸开处理
            AnalyseRelation analyseRelation = new AnalyseRelation(selObjIds, OnBlkWipeOutIds);
            try
            {
                analyseRelation.Analyse();
                //修剪WipeOut和XClip
                WipeOutXClipTrim wipeOutXClipTrim = new WipeOutXClipTrim(analyseRelation);
                wipeOutXClipTrim.StartTrim();
                wipeOutXClipTrim.GenerateBlockThenInsert();
                System.Windows.MessageBox.Show("裁剪完毕！");
            }
            catch (System.Exception ex)
            {
                ThXClipUtils.WriteException(ex);
            }
            finally
            {
                ed.SetCurrentView(view);
                analyseRelation.EraseBlockAndItsExplodedObjs();
                ThXClipCadOperation.LockedLayers(lockedLayerNames);
                //WorkFinished(this, true);
            }
        }
        [CommandMethod("RetrieveXClipBoundary", CommandFlags.Modal | CommandFlags.UsePickSet)]
        public static void RetrieveXClipBoundary_Method()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            try
            {
                if (ed.SelectImplied().Status != PromptStatus.OK) throw new System.Exception("Nothing has been pre-selected!");

                RXClass BlockReferenceRXClass = RXClass.GetClass(typeof(BlockReference));
                using (Transaction tr = Application.DocumentManager.MdiActiveDocument.TransactionManager.StartTransaction())
                {
                    foreach (ObjectId id in ed.SelectImplied().Value.GetObjectIds())
                    {
                        if (id.ObjectClass == BlockReferenceRXClass)
                        {
                            BlockReference blkRef = (BlockReference)tr.GetObject(id, OpenMode.ForRead);
                            if (blkRef.ExtensionDictionary != ObjectId.Null)
                            {
                                DBDictionary extdict = (DBDictionary)tr.GetObject(blkRef.ExtensionDictionary, OpenMode.ForRead);
                                if (extdict.Contains("ACAD_FILTER"))
                                {
                                    DBDictionary dict = (DBDictionary)tr.GetObject(extdict.GetAt("ACAD_FILTER"), OpenMode.ForRead);
                                    if (dict.Contains("SPATIAL"))
                                    {
                                        SpatialFilter filter = (SpatialFilter)tr.GetObject(dict.GetAt("SPATIAL"), OpenMode.ForRead);
                                        DrawPolygon(blkRef.Database, filter.Definition.Normal, filter.ClipSpaceToWorldCoordinateSystemTransform, filter.Definition.GetPoints());
                                    }
                                }
                            }
                        }
                    }

                    tr.Commit();
                }
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage(Environment.NewLine + ex.Message);
            }
        }

        public static ObjectId DrawPolygon(Database db, Vector3d normal, Matrix3d mat, Point2dCollection vertices)
        {
            ObjectId ret = ObjectId.Null;

            Transaction tr = db.TransactionManager.TopTransaction;
            BlockTableRecord btr = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
            using (Polyline pl = new Polyline())
            {
                pl.SetDatabaseDefaults();
                pl.ColorIndex = 3;
                pl.Closed = true;
                for (int i = 0; i < vertices.Count; i++)
                {
                    pl.AddVertexAt(0, vertices[i], 0, 0, 0);
                }
                pl.TransformBy(mat);
                btr.AppendEntity(pl);
                tr.AddNewlyCreatedDBObject(pl, true);
                ret = pl.ObjectId;
            }

            return ret;
        }
    }
}
