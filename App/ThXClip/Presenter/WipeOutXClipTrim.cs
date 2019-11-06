using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Geometry;
using TianHua.AutoCAD.Utility.ExtensionTools;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;

namespace ThXClip
{
    /// <summary>
    /// 目前可支持的用于修剪的Curve类型
    /// </summary>
    public enum CurveType
    {
        NotSupport,       
        Circle,
        Arc,
        Ellipse,
        Polyline,
        Polyline2d,
        Polyline3d,
        Xline,
        Spline,
        Line,
        Ray
    }
    /// <summary>
    /// 修剪WipeOut
    /// </summary>
    public class WipeOutXClipTrim
    {
        private AnalyseRelation _analyzeRelation;
        private Document _document;
        //用于记录模型空间中Curve被修剪后，对应的关系
        private Dictionary<string, Point3d> _blkNamePosDic = new Dictionary<string, Point3d>();
        private string _blockNamePrefix = "WipeoutXClip-";
        private int layerIndex = 2;
        private int padTotalWidth = 4;
        public Dictionary<string, Point3d> BlkNamePosDic
        {
            get { return _blkNamePosDic;}
        }
        public WipeOutXClipTrim(AnalyseRelation analyseRelation)
        {
            _analyzeRelation= analyseRelation;
            _document = ThXClipCadOperation.GetMdiActiveDocument();
        }
        public void StartTrim()
        {
            TypedValue[] tvs = new TypedValue[] { new TypedValue((int)DxfCode.Start, "Line,Circle,Arc,Ellipse,LWPolyline,Polyline,Spline") };
            SelectionFilter sf = new SelectionFilter(tvs);
            ProgressMeter pm = new ProgressMeter();
            pm.Start(@"正在裁剪...");
            int limitLength= this._analyzeRelation.ModelWipeOutIds.Count+ this._analyzeRelation.BlkWipeOuts.Count+
                 this._analyzeRelation.XclipInfs.Count;
            pm.SetLimit(limitLength);
            for (int i = 0; i < this._analyzeRelation.ModelWipeOutIds.Count; i++)
            {
                Point2dCollection boundaryPts = GetWipeOutBoundaryPts(this._analyzeRelation.ModelWipeOutIds[i]);
                boundaryPts = ThXClipCadOperation.GetNoRepeatedPtCollection(boundaryPts);
                PromptSelectionResult psr = ThXClipCadOperation.SelectByPolyline(_document.Editor, boundaryPts,
                    Autodesk.AutoCAD.EditorInput.PolygonSelectionMode.Crossing, sf);
                List<IntPtr> intPtrs = new List<IntPtr>();
                if (psr.Status == PromptStatus.OK)
                {
                    intPtrs = psr.Value.GetObjectIds().Select(j => j.OldIdPtr).ToList();//获取当前WipeOut查找的物体
                }
                List<DraworderInfo> preDrawDoi = this._analyzeRelation.GetWipeOutPreDrawOrderInfs(this._analyzeRelation.ModelWipeOutIds[i]);
                preDrawDoi = preDrawDoi.Where(j => intPtrs.IndexOf(j.Id.OldIdPtr) >= 0).Select(j => j).ToList();                
                try
                {
                    OperationWipeOut(boundaryPts, preDrawDoi);
                }
                catch (System.Exception ex)
                {
                    ThXClipUtils.WriteException(ex, "ModelWipeOut: 执行到第 " + i + " 条记录！");
                }
                finally
                {
                    // 更新进度条
                    pm.MeterProgress();

                    // 让CAD在长时间任务处理时任然能接收消息
                    System.Windows.Forms.Application.DoEvents();
                }
            }            
            for (int i = 0; i < this._analyzeRelation.BlkWipeOuts.Count; i++)
            {
                List<DraworderInfo> preDrawDoi = this._analyzeRelation.GetWipeOutPreDrawOrderInfs(this._analyzeRelation.BlkWipeOuts[i]);
                Point2dCollection boundaryPts = GetWipeOutBoundaryPts(this._analyzeRelation.BlkWipeOuts[i].Id);
                boundaryPts = ThXClipCadOperation.GetNoRepeatedPtCollection(boundaryPts);
                PromptSelectionResult psr = ThXClipCadOperation.SelectByPolyline(_document.Editor, boundaryPts,
                    Autodesk.AutoCAD.EditorInput.PolygonSelectionMode.Crossing);
                List<IntPtr> intPtrs = new List<IntPtr>();
                if (psr.Status == PromptStatus.OK)
                {
                    intPtrs = psr.Value.GetObjectIds().Select(j => j.OldIdPtr).ToList();//获取当前WipeOut查找的物体
                }
                preDrawDoi = preDrawDoi.Where(j => intPtrs.IndexOf(j.Id.OldIdPtr) >= 0).Select(j => j).ToList();
                try
                {
                    OperationWipeOut(boundaryPts, preDrawDoi);
                }
                catch (System.Exception ex)
                {
                    ThXClipUtils.WriteException(ex, "BlockWipeOut: 执行到第 " + i + " 条记录！");
                }
                finally
                {
                    // 更新进度条
                    pm.MeterProgress();

                    // 让CAD在长时间任务处理时任然能接收消息
                    System.Windows.Forms.Application.DoEvents();
                }
            }
            for (int i = 0; i < this._analyzeRelation.XclipInfs.Count; i++)
            {
                Point2dCollection xClipEdgePts = TransWipeOutBoundaryPts(this._analyzeRelation.XclipInfs[i].Pts);
                xClipEdgePts = ThXClipCadOperation.GetNoRepeatedPtCollection(xClipEdgePts);
                PromptSelectionResult psr = ThXClipCadOperation.SelectByPolyline(_document.Editor, xClipEdgePts,
                     Autodesk.AutoCAD.EditorInput.PolygonSelectionMode.Crossing, sf);
                List<IntPtr> intPtrs = new List<IntPtr>();
                if (psr.Status == PromptStatus.OK)
                {
                    intPtrs = psr.Value.GetObjectIds().Select(j => j.OldIdPtr).ToList();//获取当前WipeOut查找的物体
                }
                List<DraworderInfo> needHandleDrawDois = _analyzeRelation.GetXClipDrawOrderInfs(this._analyzeRelation.XclipInfs[i]);
                //needHandleDrawDois = needHandleDrawDois.Where(j => intPtrs.IndexOf(j.Id.OldIdPtr) >= 0).Select(j => j).ToList();
                try
                {
                    OperationXClip(xClipEdgePts, needHandleDrawDois, this._analyzeRelation.XclipInfs[i].KeepInternal);
                }
                catch (System.Exception ex)
                {
                    ThXClipUtils.WriteException(ex, "XClip: 执行到第 " + i+ " 条记录！");
                }
                finally
                {
                    // 更新进度条
                    pm.MeterProgress();

                    // 让CAD在长时间任务处理时任然能接收消息
                    System.Windows.Forms.Application.DoEvents();
                }
            }
            pm.Stop();
        }
        public void GenerateBlockThenInsert()
        {
            //以下是创建块
            string blockName = "";
            using (Transaction trans = _document.Database.TransactionManager.StartTransaction())
            {
                BlockTable bt = trans.GetObject(_document.Database.BlockTableId, OpenMode.ForRead) as BlockTable;
                bt.UpgradeOpen();
                foreach (var objId in this._analyzeRelation.ObjIds)
                {
                    BlockTableRecord btr = new BlockTableRecord();
                    BlockReference br = trans.GetObject(objId, OpenMode.ForRead) as BlockReference;
                    Point3d blkOriginPt = br.Position;
                    blockName = _blockNamePrefix + "0001";
                    Vector3d vec = blkOriginPt - Point3d.Origin;
                    Matrix3d moveMt = Matrix3d.Displacement(vec);
                    while (bt.Has(blockName))
                    {
                        if (layerIndex >= 1 && layerIndex <= 9999)
                        {
                            padTotalWidth = 4;
                        }
                        else if (layerIndex >= 10000 && layerIndex <= 99999)
                        {
                            padTotalWidth = 5;
                        }
                        else if (layerIndex >= 100000 && layerIndex <= 999999)
                        {
                            padTotalWidth = 6;
                        }
                        else if (layerIndex >= 1000000 && layerIndex <= 9999999)
                        {
                            padTotalWidth = 7;
                        }
                        else if (layerIndex >= 10000000 && layerIndex <= 99999999)
                        {
                            padTotalWidth = 8;
                        }
                        blockName = _blockNamePrefix + layerIndex.ToString().PadLeft(padTotalWidth, '0');
                        layerIndex++;
                    }
                    btr.Name = blockName;
                    List<DraworderInfo> currentBlkObjs = this._analyzeRelation.DrawOrderinfs.
                        Where(i => i.BlockId == objId).Select(i => i).ToList();
                    for (int i = 0; i < currentBlkObjs.Count; i++)
                    {
                        Entity ent = _document.TransactionManager.TopTransaction.GetObject(currentBlkObjs[i].Id, OpenMode.ForRead) as Entity;
                        EntPropertyInfo entPropertyInf = new EntPropertyInfo() { Layer = ent.Layer, ColorIndex = ent.ColorIndex, Lw = ent.LineWeight };
                        for (int j = 0; j < currentBlkObjs[i].DbObjs.Count; j++)
                        {
                            DBObject dbObj = currentBlkObjs[i].DbObjs[j];
                            if (dbObj is Entity)
                            {
                                Entity entity = dbObj as Entity;
                                if (!entity.Visible)
                                {
                                    entity.Visible = true;
                                }
                                ThXClipUtils.ChangeEntityProperty(entity, entPropertyInf);
                                if (entity is Dimension)
                                {
                                    entity.TransformBy(moveMt);
                                }
                                btr.AppendEntity(entity);
                            }
                        }
                    }
                    bt.Add(btr);
                    trans.AddNewlyCreatedDBObject(btr, true);
                    this.BlkNamePosDic.Add(blockName, blkOriginPt);
                }
                bt.DowngradeOpen();
                trans.Commit();
            }
            foreach (var blkItem in this.BlkNamePosDic)
            {
                // 插入图块
                ThXClipCadOperation.InsertBlockReference("0", blkItem.Key,
                    Point3d.Origin, new Scale3d(1.0, 1.0, 1.0), 0.0);
            }
        }
        private Point2dCollection GetWipeOutBoundaryPts(ObjectId wpId,bool needTransform=true)
        {
            Point2dCollection newPts = new Point2dCollection();
            using (Transaction trans = this._document.Database.TransactionManager.StartTransaction())
            {
                Wipeout wp = trans.GetObject(wpId, OpenMode.ForRead) as Wipeout;
                Point2dCollection pts = wp.GetClipBoundary();
                Matrix3d mt = wp.PixelToModelTransform;
                if (pts.Count == 2)
                {
                    double minX = Math.Min(pts[0].X, pts[1].X);
                    double maxX = Math.Max(pts[0].X, pts[1].X);
                    double minY = Math.Min(pts[0].Y, pts[1].Y);
                    double maxY = Math.Max(pts[0].Y, pts[1].Y);
                    newPts.Add(new Point2d(minX, minY));
                    newPts.Add(new Point2d(maxX, minY));
                    newPts.Add(new Point2d(maxX, maxY));
                    newPts.Add(new Point2d(minX, maxY));
                }
                else
                {
                    newPts = pts;
                }
                if(true)
                {
                    Point2dCollection tempPts = new Point2dCollection();
                    foreach(Point2d pt in newPts)
                    {
                        Point3d newPt = new Point3d(pt.X,pt.Y,0.0);
                        newPt= newPt.TransformBy(mt);
                        tempPts.Add(new Point2d(newPt.X, newPt.Y));
                    }
                    newPts = tempPts;
                }
                trans.Commit();
            }
            return newPts;
        }
        private void OperationWipeOut(Point2dCollection wipeOutBoundaryPts, List<DraworderInfo> preDrawDois)
        {
            wipeOutBoundaryPts = TransWipeOutBoundaryPts(wipeOutBoundaryPts);
            CurveType curveType = CurveType.NotSupport;
            for (int i = 0; i < preDrawDois.Count; i++)
            {
                DBObjectCollection handledDbObjs = new DBObjectCollection();
                DBObjectCollection dbObjs = preDrawDois[i].DbObjs;
                curveType = GetDbObjType(preDrawDois[i].TypeName);
                if (curveType == CurveType.NotSupport)
                {
                    continue;
                }
                if (curveType == CurveType.NotSupport)
                {
                    continue;
                }
                for (int j = 0; j < dbObjs.Count; j++)
                {
                    curveType = GetDbObjType(dbObjs[j]);
                    DBObjectCollection subDbObjs = new DBObjectCollection();
                    switch (curveType)
                    {
                        case CurveType.Line:
                            Line line = dbObjs[j] as Line;
                            subDbObjs = line.XClip(wipeOutBoundaryPts);
                            break;
                        case CurveType.Circle:
                            Circle circle = dbObjs[j] as Circle;
                            subDbObjs = circle.XClip(wipeOutBoundaryPts);
                            break;
                        case CurveType.Arc:
                            Arc arc = dbObjs[j] as Arc;
                            subDbObjs = arc.XClip(wipeOutBoundaryPts);
                            break;
                        case CurveType.Ellipse:
                            Ellipse ellipse = dbObjs[j] as Ellipse;
                            subDbObjs = ellipse.XClip(wipeOutBoundaryPts);
                            break;
                        case CurveType.Xline: //暂不支持
                            Xline xline = dbObjs[j] as Xline;
                            subDbObjs = xline.XClip(wipeOutBoundaryPts);
                            break;
                        case CurveType.Spline:
                            Spline spline = dbObjs[j] as Spline;
                            subDbObjs = spline.XClip(wipeOutBoundaryPts);
                            break;
                        case CurveType.Polyline:
                            Polyline polyline = dbObjs[j] as Polyline;
                            subDbObjs = polyline.XClip(wipeOutBoundaryPts);
                            break;
                        case CurveType.Polyline2d:
                            Polyline2d polyline2d = dbObjs[j] as Polyline2d;
                            subDbObjs = polyline2d.XClip(wipeOutBoundaryPts);
                            break;
                        case CurveType.Polyline3d:
                            Polyline3d polyline3d = dbObjs[j] as Polyline3d;
                            subDbObjs = polyline3d.XClip(wipeOutBoundaryPts);
                            break;
                    }
                    foreach (DBObject newDbObj in subDbObjs)
                    {
                        handledDbObjs.Add(newDbObj);
                    }
                }
                _analyzeRelation.DrawOrderinfs.Where
                    (j => j.Id == preDrawDois[i].Id).Select(j => j).FirstOrDefault().DbObjs = handledDbObjs;
            }
        }
        private List<string> GetBlockNames(ObjectId blkId,string blkName,bool isCompared=true)
        {
            List<string> blkNames = new List<string>();
            Transaction trans = _document.TransactionManager.TopTransaction;
            BlockReference br = trans.GetObject(blkId,OpenMode.ForRead) as BlockReference;
            if(isCompared)
            {
                if(br.Name == blkName)
                {
                    blkNames.Add(br.Name);
                    isCompared = false;
                }
            }
            else
            {
                blkNames.Add(br.Name);
            }            
            BlockTableRecord btr = trans.GetObject(br.BlockTableRecord, OpenMode.ForRead) as BlockTableRecord;
            foreach (var id in btr)
            {
                BlockReference newBr = trans.GetObject(id,OpenMode.ForRead) as BlockReference;
                if (newBr == null)
                {
                    continue;
                }
                List<string> subNames = new List<string>();
                if (isCompared)
                {
                    if (newBr.Name == blkName)
                    {
                        isCompared = false;
                        subNames = GetBlockNames(newBr.Id, blkName, isCompared);
                        if (subNames != null && subNames.Count > 0)
                        {
                            blkNames.AddRange(subNames);
                        }
                        break;
                    }
                }
                else
                {
                    subNames = GetBlockNames(newBr.Id, blkName, isCompared);
                    if (subNames != null && subNames.Count > 0)
                    {
                        blkNames.AddRange(subNames);
                    }
                }
            }
            if(blkNames.Count>0)
            {
                blkNames = blkNames.Distinct().ToList();
            }            
            return blkNames;
        }
        private void OperationXClip(Point2dCollection xClipEdgePts, List<DraworderInfo> needHandleDrawDois,bool keepInternal=false)
        {
            CurveType curveType = CurveType.NotSupport;
            for (int i = 0; i < needHandleDrawDois.Count; i++)
            {
                curveType = GetDbObjType(needHandleDrawDois[i].TypeName);
                if (curveType == CurveType.NotSupport)
                {
                    continue;
                }
                DBObjectCollection handledDbObjs = new DBObjectCollection();
                DBObjectCollection dbObjs = needHandleDrawDois[i].DbObjs;
                for (int j = 0; j < dbObjs.Count; j++)
                {
                    curveType = GetDbObjType(dbObjs[j]);
                    DBObjectCollection subDbObjs = new DBObjectCollection();
                    switch (curveType)
                    {
                        case CurveType.Line:
                            Line line = dbObjs[j] as Line;
                            subDbObjs = line.XClip(xClipEdgePts, keepInternal);
                            break;
                        case CurveType.Circle:
                            Circle circle = dbObjs[j] as Circle;
                            subDbObjs = circle.XClip(xClipEdgePts, keepInternal);
                            break;
                        case CurveType.Arc:
                            Arc arc = dbObjs[j] as Arc;
                            subDbObjs = arc.XClip(xClipEdgePts, keepInternal);
                            break;
                        case CurveType.Ellipse:
                            Ellipse ellipse = dbObjs[j] as Ellipse;
                            subDbObjs = ellipse.XClip(xClipEdgePts, keepInternal);
                            break;
                        case CurveType.Xline:
                            Xline xline = dbObjs[j] as Xline;
                            subDbObjs = xline.XClip(xClipEdgePts, keepInternal);
                            break;
                        case CurveType.Spline:
                            Spline spline = dbObjs[j] as Spline;
                            subDbObjs = spline.XClip(xClipEdgePts, keepInternal);
                            break;
                        case CurveType.Polyline:
                            Polyline polyline = dbObjs[j] as Polyline;
                            subDbObjs = polyline.XClip(xClipEdgePts, keepInternal);
                            break;
                        case CurveType.Polyline2d:
                            Polyline2d polyline2d = dbObjs[j] as Polyline2d;
                            subDbObjs = polyline2d.XClip(xClipEdgePts, keepInternal);
                            break;
                        case CurveType.Polyline3d:
                            Polyline3d polyline3d = dbObjs[j] as Polyline3d;
                            subDbObjs = polyline3d.XClip(xClipEdgePts, keepInternal);
                            break;
                        case CurveType.Ray:
                            Ray ray = dbObjs[j] as Ray;
                            subDbObjs = ray.XClip(xClipEdgePts, keepInternal);
                            break;
                    }
                    foreach (DBObject newDbObj in subDbObjs)
                    {
                        handledDbObjs.Add(newDbObj);
                    }
                }
                _analyzeRelation.DrawOrderinfs.Where
                        (j => j.Id == needHandleDrawDois[i].Id).Select(j => j).FirstOrDefault().DbObjs = handledDbObjs;
            }
        }
        private Point2dCollection TransWipeOutBoundaryPts(Point2dCollection pts)
        {
            Point2dCollection newBoundaryPts = new Point2dCollection();
            if(pts.Count==2)
            {
                double minX = Math.Min(pts[0].X, pts[1].X);
                double minY = Math.Min(pts[0].Y, pts[1].Y);
                double maxX = Math.Max(pts[0].X, pts[1].X);
                double maxY = Math.Max(pts[0].Y, pts[1].Y);
                newBoundaryPts.AddRange(new Point2d[]{new Point2d(minX, minY), new Point2d(maxX, minY),
                    new Point2d(maxX, maxY), new Point2d(minX, maxY)});
            }
            else
            {
                newBoundaryPts = pts;
            }
            return newBoundaryPts;
        }
        private CurveType GetDbObjType(string typeName)
        {
            CurveType curveType = CurveType.NotSupport;
            typeName = typeName.ToUpper();
            switch(typeName)
            {
                case "LINE":
                    curveType = CurveType.Line;
                    break;
                case "CIRCLE":
                    curveType = CurveType.Circle;
                    break;
                case "ARC":
                    curveType = CurveType.Arc;
                    break;
                case "ELLIPSE":
                    curveType = CurveType.Ellipse;
                    break;
                case "XLINE":
                    curveType = CurveType.Xline;
                    break;
                case "SPLINE":
                    curveType = CurveType.Spline;
                    break;
                case "POLYLINE":
                    curveType = CurveType.Polyline;
                    break;
                case "POLYLINE2d":
                    curveType = CurveType.Polyline2d;
                    break;
                case "POLYLINE3d":
                    curveType = CurveType.Polyline3d;
                    break;
            }
            return curveType;
        }
        private CurveType GetDbObjType(DBObject dbObj)
        {
            CurveType curveType = CurveType.NotSupport;
            if (dbObj.GetType()==typeof(Circle))
            {
                curveType = CurveType.Circle;
            }
            else if(dbObj.GetType() == typeof(Arc))
            {
                curveType = CurveType.Arc;
            }
            else if (dbObj.GetType() == typeof(Ellipse))
            {
                curveType = CurveType.Ellipse;
            }
            else if(dbObj.GetType() == typeof(Xline))
            {
                curveType = CurveType.Xline;
            }
            else if (dbObj.GetType() == typeof(Line))
            {
                curveType = CurveType.Line;
            }
            else if (dbObj.GetType() == typeof(Spline))
            {
                curveType = CurveType.Spline;
            }
            else if(dbObj.GetType() == typeof(Polyline))
            {
                curveType = CurveType.Polyline;
            }
            else if (dbObj.GetType() == typeof(Polyline2d))
            {
                curveType = CurveType.Polyline2d;
            }
            else if (dbObj.GetType() == typeof(Polyline3d))
            {
                curveType = CurveType.Polyline3d;
            }
            return curveType;
        }
    }
}
