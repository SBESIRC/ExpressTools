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
    public class ThXclipCommands
    {
        [CommandMethod("TIANHUACAD", "ThXClip", CommandFlags.Modal)]
        public void ThXClip()
        {
            List<ObjectId> selObjIds = CadOperation.GetSelectObjects(); //选择物体
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            List<Point2dCollection> boundPts = new List<Point2dCollection>();
            List<Polyline> polylines = new List<Polyline>(); //用于保存所选块中WipeOut边界点绘制的Polyline实体集合
            using (Transaction trans = doc.Database.TransactionManager.StartTransaction())
            {
                //以读方式打开块表
                BlockTable bt = (BlockTable)trans.GetObject(doc.Database.BlockTableId, OpenMode.ForRead);
                //以写方式打开模型空间块表记录.
                BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                //以下是获取WipeOut boundary 
                for (int i = 0; i < selObjIds.Count; i++)
                {
                    DBObject dbObj = trans.GetObject(selObjIds[i], OpenMode.ForRead);
                    if (dbObj is BlockReference)
                    {
                        BlockReference br = dbObj as BlockReference;
                        List<Point2dCollection> tempPts = CadOperation.RetrieveWipeOutBoundaryFromBlkRef(br, trans);
                        if (tempPts != null && tempPts.Count > 0)
                        {
                            boundPts.AddRange(tempPts);
                        }
                    }
                }
                //通过 boundary 边界点来生成Polyline
                foreach (var pts in boundPts)
                {
                    Polyline polyline = new Polyline();
                    polyline.CreatePolyline(pts);
                    btr.AppendEntity(polyline);//将图形对象的信息添加到块表记录中                    
                    trans.AddNewlyCreatedDBObject(polyline, true);//把对象添加到事务处理中
                    polylines.Add(polyline);
                }
                btr.DowngradeOpen();
                trans.Commit();
            }
            //获取Wipeout边界所有的物体
            using (Transaction trans = doc.Database.TransactionManager.StartTransaction())
            {
                List<ObjectId> objIds = new List<ObjectId>();
                for (int i = 0; i < polylines.Count; i++)
                {
                    PromptSelectionResult psr = CadOperation.SelectByPolyline(ed, polylines[i], PolygonSelectionMode.Crossing);
                    if (psr.Status == PromptStatus.OK)
                    {
                        objIds.AddRange(psr.Value.GetObjectIds());
                    }
                }
                for (int i = 0; i < objIds.Count; i++)
                {
                    DBObject dbObj = trans.GetObject(objIds[1], OpenMode.ForRead);
                    if (dbObj is Curve || dbObj is BlockReference)
                    {
                        if (selObjIds.IndexOf(objIds[i]) < 0)
                        {
                            var findRes = polylines.Where(j => j.ObjectId == objIds[i]).Select(j => j).ToList();
                            if (findRes == null || findRes.Count == 0)
                            {
                                selObjIds.Add(objIds[i]);
                            }
                        }
                    }
                }
                for (int i = 0; i < polylines.Count; i++)
                {
                    polylines[i].Erase();
                }
                trans.Commit();
            }
            //分析传入的物体之间的 Draworder关系
            AnalyseRelation analyseRelation = new AnalyseRelation(selObjIds);
            analyseRelation.Analyse();
            //创建块炸开后的映射表
            Dictionary<ObjectId, Explosion> blockRefExplosionInf = new Dictionary<ObjectId, Explosion>();
            using (Transaction trans = doc.Database.TransactionManager.StartTransaction())
            {
                Explosion explosion = new Explosion();
                for (int i = 0; i < selObjIds.Count; i++)
                {
                    DBObject dbObj = trans.GetObject(selObjIds[i], OpenMode.ForRead);
                    if (dbObj is BlockReference)
                    {
                        explosion.CreateBlockRefExplodeMappingInfo(selObjIds[i]);
                        blockRefExplosionInf.Add(selObjIds[i], explosion);
                    }
                }
                trans.Commit();
            }
            //修剪WipeOut和XClip
            WipeOutXClipTrim wipeOutXClipTrim = new WipeOutXClipTrim(analyseRelation, blockRefExplosionInf);
            wipeOutXClipTrim.StartTrim();
        }
        [CommandMethod("TIANHUACAD", "Test", CommandFlags.Modal)]
        public void Test()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            PromptEntityResult per = ed.GetEntity(" \n选择一个多段线");
            ObjectId polylineId = ObjectId.Null;
            if (per.Status == PromptStatus.OK)
            {
                polylineId = per.ObjectId;
            }
            //PromptPointResult ppr1=ed.GetPoint("\n选择椭圆上的第一点");
            //PromptPointResult ppr2 = ed.GetPoint("\n选择椭圆上的第二点");
            PromptPointResult ppr3 = ed.GetPoint("\n选择椭圆内的点");
            //Point3d pt1 = Point3d.Origin;
            //Point3d pt2 = Point3d.Origin;
            Point3d pt3 = Point3d.Origin;
            //if (ppr1.Status==PromptStatus.OK)
            //{
            //    pt1 = ppr1.Value;
            //}
            //if (ppr2.Status == PromptStatus.OK)
            //{
            //    pt2 = ppr2.Value;
            //}
            pt3 = ppr3.Value;
            using (Transaction trans = doc.Database.TransactionManager.StartTransaction())
            {
                //以读方式打开块表
                BlockTable bt = (BlockTable)trans.GetObject(doc.Database.BlockTableId, OpenMode.ForRead);
                //以写方式打开模型空间块表记录.
                BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                //Ellipse ellipse = trans.GetObject(ellipseId,OpenMode.ForRead) as Ellipse;
                DBObject dbObj = trans.GetObject(polylineId, OpenMode.ForRead);
                if(dbObj is Polyline)
                {
                    Polyline polyline = dbObj as Polyline;
                    int vertexCount = polyline.NumberOfVertices;
                    for (int i = 0; i < vertexCount; i++)
                    {
                        SegmentType st = polyline.GetSegmentType(i);
                        if (st == SegmentType.Arc)
                        {
                            CircularArc2d circularArc2d = polyline.GetArcSegment2dAt(i);
                            CircularArc3d circularArc3d = polyline.GetArcSegmentAt(i);
                        }
                    }
                }
                else if(dbObj is Arc)
                {
                    Arc arc = dbObj as Arc;
                    double bulge=CadOperation.GetBulge(arc.Center, arc.StartPoint, arc.EndPoint);
                }

                //double para1= ellipse.GetParameterAtPoint(pt1);
                //double para2 = ellipse.GetParameterAtPoint(pt2);

                //double angle1=ellipse.GetAngleAtParameter(para1);
                //double angle2=ellipse.GetAngleAtParameter(para2);

                //Ellipse ellipse1 = new Ellipse(ellipse.Center, ellipse.Normal, 
                //    ellipse.MajorAxis, ellipse.RadiusRatio, angle1, angle2);
                //double jiaJiao = (ellipse.Center - pt3).GetAngleTo(Vector3d.XAxis);
                //double para3=ellipse.GetParameterAtAngle(jiaJiao);
                //Point3d closestPt= ellipse.GetPointAtParameter(para3);

                //Point3d closestPt = CadOperation.GetPtOnEllipse(ellipse, pt3);
                //Circle circle = new Circle(closestPt, ellipse.Normal, 100);
                //btr.AppendEntity(ellipse1);
                //trans.AddNewlyCreatedDBObject(ellipse1, true);


                //btr.AppendEntity(circle);                
                //trans.AddNewlyCreatedDBObject(circle, true);
                btr.DowngradeOpen();
                trans.Commit();
            }
        }
        #region---------参考命令---------
        [CommandMethod("ViewOrder")]
        public static void ViewOrder()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var db = doc.Database;

            try
            {
                var peo =
                  new PromptEntityOptions("\nSelect block to fix <all>");
                peo.SetRejectMessage("Must be a block.");
                peo.AddAllowedClass(typeof(BlockReference), false);
                peo.AllowNone = true;
                var per = ed.GetEntity(peo);
                if (
                  per.Status != PromptStatus.OK &&
                  per.Status != PromptStatus.None
                )
                    return;

                // If the user hit enter, run on all blocks in the drawing

                bool allBlocks = per.Status == PromptStatus.None;

                using (var tr = db.TransactionManager.StartTransaction())
                {
                    var toProcess = new ObjectIdCollection();
                    if (allBlocks)
                    {
                        var bt =
                          (BlockTable)tr.GetObject(
                            db.BlockTableId, OpenMode.ForRead
                          );

                        // Collect all the non-layout blocks in the drawing

                        var modelSpace = (BlockTableRecord)tr.GetObject(
                            bt[BlockTableRecord.ModelSpace], OpenMode.ForRead
                          );
                        var dotSource =
   (DrawOrderTable)tr.GetObject(modelSpace.DrawOrderTableId,
                               OpenMode.ForRead, true);
                        ObjectIdCollection ocids = dotSource.GetFullDrawOrder(0);

                        foreach (ObjectId id in ocids)
                        {
                            DBObject dbObj = tr.GetObject(id, OpenMode.ForRead);
                        }

                        foreach (ObjectId btrId in bt)
                        {
                            var btr =
                              (BlockTableRecord)tr.GetObject(
                                btrId, OpenMode.ForRead
                              );
                            if (!btr.IsLayout)
                            {
                                toProcess.Add(btrId);
                            }
                        }
                    }
                    else
                    {
                        var brId = per.ObjectId;
                        BlockReference br =
                          (BlockReference)tr.GetObject(brId, OpenMode.ForRead);
                        BlockTableRecord btr = tr.GetObject(br.BlockTableRecord, OpenMode.ForRead) as BlockTableRecord;
                        var dotSource =
    (DrawOrderTable)tr.GetObject(btr.DrawOrderTableId,
                                OpenMode.ForRead, true);
                        ObjectIdCollection srcDotIds = new ObjectIdCollection();
                        srcDotIds = dotSource.GetFullDrawOrder(0);
                    }
                    tr.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception e)
            {
                doc.Editor.WriteMessage(
                "\nException: {0}", e.Message
                );
            }
        }

        [CommandMethod("WTB")]
        public static void WipeoutsToBottom()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var db = doc.Database;

            try
            {
                // Ask the user to select a block or None for "all"

                var peo =
                  new PromptEntityOptions("\nSelect block to fix <all>");
                peo.SetRejectMessage("Must be a block.");
                peo.AddAllowedClass(typeof(BlockReference), false);
                peo.AllowNone = true;

                var per = ed.GetEntity(peo);

                if (
                  per.Status != PromptStatus.OK &&
                  per.Status != PromptStatus.None
                )
                    return;

                // If the user hit enter, run on all blocks in the drawing

                bool allBlocks = per.Status == PromptStatus.None;

                using (var tr = db.TransactionManager.StartTransaction())
                {
                    var toProcess = new ObjectIdCollection();
                    if (allBlocks)
                    {
                        var bt =
                          (BlockTable)tr.GetObject(
                            db.BlockTableId, OpenMode.ForRead
                          );

                        // Collect all the non-layout blocks in the drawing

                        foreach (ObjectId btrId in bt)
                        {
                            var btr =
                              (BlockTableRecord)tr.GetObject(
                                btrId, OpenMode.ForRead
                              );
                            if (!btr.IsLayout)
                            {
                                toProcess.Add(btrId);
                            }
                        }
                    }
                    else
                    {
                        // A specific block was selected, let's open it

                        var brId = per.ObjectId;
                        var br =
                          (BlockReference)tr.GetObject(brId, OpenMode.ForRead);

                        // Collect the ID of its underlying block definition

                        toProcess.Add(br.BlockTableRecord);
                    }

                    var brIds = CadOperation.MoveWipeoutsToBottom(tr, toProcess);
                    var count = toProcess.Count;

                    // Open each of the returned block references and
                    // request that they be redrawn

                    foreach (ObjectId brId in brIds)
                    {
                        var br =
                          (BlockReference)tr.GetObject(brId, OpenMode.ForWrite);

                        // We want to redraw a specific block, so let's modify a
                        // property on the selected block reference

                        // We might also have called this method:
                        // br.RecordGraphicsModified(true);
                        // but setting a property works better with undo

                        br.Visible = br.Visible;
                    }

                    // Report the number of blocks modified (after
                    // being filtered by MoveWipeoutsToBottom())

                    ed.WriteMessage(
                      "\nModified {0} block definition{1}.",
                      count, count == 1 ? "" : "s"
                    );

                    // Commit the transaction

                    tr.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception e)
            {
                doc.Editor.WriteMessage(
                "\nException: {0}", e.Message
                );
            }
        }
        [CommandMethod("MoveAbove")]
        static public void MoveAbove()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;


            string message = "\nSelect a entity to move above";
            PromptEntityOptions optEnt = new PromptEntityOptions(message);

            PromptEntityResult acEnt = ed.GetEntity(optEnt);

            if (acEnt.Status != PromptStatus.OK)
                return;

            message = "\nSelect target entity";
            PromptEntityOptions optTarget = new PromptEntityOptions(message);

            PromptEntityResult acTarget = ed.GetEntity(optTarget);

            if (acTarget.Status != PromptStatus.OK)
                return;


            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                Entity ent = tr.GetObject(acEnt.ObjectId,
                                                OpenMode.ForRead) as Entity;

                //get the block
                BlockTableRecord block = tr.GetObject(ent.BlockId,
                                       OpenMode.ForRead) as BlockTableRecord;
                //get the draw oder table of the block
                DrawOrderTable drawOrder =
                                         tr.GetObject(block.DrawOrderTableId,
                                        OpenMode.ForWrite) as DrawOrderTable;

                ObjectIdCollection ids = new ObjectIdCollection();
                ids.Add(acEnt.ObjectId);

                //moves entities above the target entity.
                drawOrder.MoveAbove(ids, acTarget.ObjectId);

                tr.Commit();

            }
        }
        #endregion
    }
}
