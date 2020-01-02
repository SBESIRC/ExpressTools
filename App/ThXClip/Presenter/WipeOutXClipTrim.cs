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
        Ray,
        WipeOut
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
            ThXclipCommands.pm.Start(@"正在裁剪...");
            int xClipNum = 0;
            List<int> numList=this._analyzeRelation.BlockXClips.Select(i=>i.Value.Count).ToList();
            xClipNum = numList.Sum(); 
            int limitLength= this._analyzeRelation.ModelWipeOutIds.Count+ this._analyzeRelation.BlkWipeOuts.Count+xClipNum;
            ThXclipCommands.pm.SetLimit(limitLength);
            //处理附加到块上的XClip，被其遮挡的物体
            foreach (var item in this._analyzeRelation.BlockXClips)
            {
                for (int i = 0; i < item.Value.Count; i++)
                {
                    Point2dCollection xClipEdgePts = TransWipeOutBoundaryPts(item.Value[i].Pts);
                    xClipEdgePts = ThXClipCadOperation.GetNoRepeatedPtCollection(xClipEdgePts);
                    List<DraworderInfo> needHandleDrawDois = new List<DraworderInfo>();
                    needHandleDrawDois.AddRange(_analyzeRelation.GetXClipDrawOrderInfs(item.Value[i]));
                    needHandleDrawDois.AddRange(_analyzeRelation.GetXClipAccessoryWipeOuts(item.Value[i]));
                    try
                    {
                        OperationXClip(xClipEdgePts, needHandleDrawDois, item.Value[i].KeepInternal);
                    }
                    catch (System.Exception ex)
                    {
                        ThXClipUtils.WriteException(ex, "处理XClip: 执行到第 " + i + " 条记录！");
                    }
                    finally
                    {
                        // 更新进度条
                        ThXclipCommands.pm.MeterProgress();
                        // 让CAD在长时间任务处理时任然能接收消息
                        System.Windows.Forms.Application.DoEvents();
                    }
                }
            }
            //处理ModelSpace中的WipeOut,被其遮挡的物体
            for (int i = 0; i < this._analyzeRelation.ModelWipeOutIds.Count; i++)
            {
                List<DraworderInfo> preDrawDoi = this._analyzeRelation.GetWipeOutPreDrawOrderInfs(this._analyzeRelation.ModelWipeOutIds[i]);
                Point2dCollection boundaryPts = ThXClipCadOperation.GetWipeOutBoundaryPts(this._analyzeRelation.ModelWipeOutIds[i]);
                boundaryPts = ThXClipCadOperation.GetNoRepeatedPtCollection(boundaryPts);

                //第一次过滤
                double minX = boundaryPts.ToArray().OrderBy(j => j.X).First().X;
                double minY = boundaryPts.ToArray().OrderBy(j => j.Y).First().Y;
                double maxX = boundaryPts.ToArray().OrderByDescending(j => j.X).First().X;
                double maxY = boundaryPts.ToArray().OrderByDescending(j => j.Y).First().Y;
                preDrawDoi=preDrawDoi.Where(j => IsInWipeOutRectangle(minX, minY, maxX, maxY, j.DbObjs)).Select(j => j).ToList();

                //第二次过滤
                Polyline wipeOutEdge = ThXClipCadOperation.CreatePolyline(boundaryPts, true);

                List<DraworderInfo> reservedDois = new List<DraworderInfo>();
                for (int j = 0; j < preDrawDoi.Count; j++)
                {
                    for (int k = 0; k < preDrawDoi[j].DbObjs.Count; k++)
                    {
                        if (WipeoutIntersectCurve(wipeOutEdge, preDrawDoi[j].DbObjs[k] as Curve))
                        {
                            reservedDois.Add(preDrawDoi[j]);
                            break;
                        }
                    }
                }
                //裁剪
                try
                {
                    OperationWipeOut(boundaryPts, reservedDois);
                }
                catch (System.Exception ex)
                {
                    ThXClipUtils.WriteException(ex, "处理ModelSpace中的WipeOut: 执行到第 " + i + " 条记录！");
                }
                finally
                {
                    wipeOutEdge.Dispose();
                    // 更新进度条
                    ThXclipCommands.pm.MeterProgress();
                    // 让CAD在长时间任务处理时任然能接收消息
                    System.Windows.Forms.Application.DoEvents();
                }
            }
            //处理块中的WipeOut,被其遮挡的物体
            for (int i = 0; i < this._analyzeRelation.BlkWipeOuts.Count; i++)
            {
                List<DraworderInfo> preDrawDoi = this._analyzeRelation.GetWipeOutPreDrawOrderInfs(this._analyzeRelation.BlkWipeOuts[i]);
                Point2dCollection boundaryPts = ThXClipCadOperation.GetWipeOutBoundaryPts(this._analyzeRelation.BlkWipeOuts[i].Id);
                boundaryPts = ThXClipCadOperation.GetNoRepeatedPtCollection(boundaryPts);
                
                //第一次过滤
                double minX = boundaryPts.ToArray().OrderBy(j => j.X).First().X;
                double minY = boundaryPts.ToArray().OrderBy(j => j.Y).First().Y;
                double maxX = boundaryPts.ToArray().OrderByDescending(j => j.X).First().X;
                double maxY = boundaryPts.ToArray().OrderByDescending(j => j.Y).First().Y;
                preDrawDoi = preDrawDoi.Where(j => IsInWipeOutRectangle(minX, minY, maxX, maxY, j.DbObjs)).Select(j => j).ToList();

                //第二次过滤
                Polyline wipeOutEdge = ThXClipCadOperation.CreatePolyline(boundaryPts, true);
                List<DraworderInfo> reservedDois = new List<DraworderInfo>();
                for (int j = 0; j < preDrawDoi.Count; j++)
                {
                    for (int k = 0; k < preDrawDoi[j].DbObjs.Count; k++)
                    {
                        if (WipeoutIntersectCurve(wipeOutEdge, preDrawDoi[j].DbObjs[k] as Curve))
                        {
                            reservedDois.Add(preDrawDoi[j]);
                            break;
                        }
                    }
                }
                try
                {
                    OperationWipeOut(boundaryPts, reservedDois);
                }
                catch (System.Exception ex)
                {
                    ThXClipUtils.WriteException(ex, "处理块中的WipeOut: 执行到第 " + i + " 条记录！");
                }
                finally
                {
                    wipeOutEdge.Dispose();
                    // 更新进度条
                    ThXclipCommands.pm.MeterProgress();
                    // 让CAD在长时间任务处理时任然能接收消息
                    System.Windows.Forms.Application.DoEvents();
                }
            }
            ThXclipCommands.pm.Stop();
        }
        private bool IsInWipeOutRectangle(double minX,double minY,double maxX,double maxY,DBObjectCollection dbObjs)
        {
            bool isIn = false;
            for(int i=0;i< dbObjs.Count;i++)
            {
                Entity ent = dbObjs[i] as Entity;
                if(ent==null || ent.GeometricExtents==null)
                {
                    continue;
                }
                Point3d minPt = ent.GeometricExtents.MinPoint;
                Point3d maxPt= ent.GeometricExtents.MaxPoint;
                if ( ((minPt.X>= minX && minPt.X<= maxX) && (minPt.Y >= minY && minPt.Y <= maxY)) ||
                     ((maxPt.X >= minX && maxPt.X <= maxX) && (maxPt.Y >= minY && maxPt.Y <= maxY))
                    )
                {
                    isIn = true;
                    break;
                }
            }
            return isIn;
        }
        private bool WipeoutIntersectCurve(Polyline wipeOutEdge, Curve curve)
        {
            bool isIntersect = false;
            Region curveRegion = null;
            Region wipeOutRegion = null;
            Point3d minPt, maxPt;
            Point2d pt1, pt2, pt3, pt4;
            try
            {
                //创建WipeOutRegion
                DBObjectCollection dbObjs = new DBObjectCollection();
                dbObjs.Add(wipeOutEdge);
                DBObjectCollection wipeOutRegions = Region.CreateFromCurves(dbObjs);
                if (wipeOutRegions.Count > 0)
                {
                    wipeOutRegion = wipeOutRegions[0] as Region;
                }
                else
                {
                    minPt = wipeOutEdge.GeometricExtents.MinPoint;
                    maxPt = wipeOutEdge.GeometricExtents.MaxPoint;
                    pt1 = new Point2d(minPt.X, minPt.Y);
                    pt2 = new Point2d(maxPt.X, minPt.Y);
                    pt3 = new Point2d(maxPt.X, maxPt.Y);
                    pt4 = new Point2d(minPt.X, maxPt.Y);
                    Point2dCollection wpPts = new Point2dCollection();
                    wpPts.Add(pt1);
                    wpPts.Add(pt2);
                    wpPts.Add(pt3);
                    wpPts.Add(pt4);
                    Polyline wpBoundpolyine =ThXClipCadOperation.CreatePolyline(wpPts, true);
                    dbObjs = new DBObjectCollection();
                    dbObjs.Add(wpBoundpolyine);
                    DBObjectCollection wipeOutRegions1 = Region.CreateFromCurves(dbObjs);
                    if (wipeOutRegions1.Count > 0)
                    {
                        wipeOutRegion = wipeOutRegions1[0] as Region;
                    }
                    wpBoundpolyine.Dispose();
                }
                if (wipeOutRegion == null || wipeOutRegion.Area == 0.0)
                {
                    return false;
                }
                //创建Curve Region
                minPt = curve.GeometricExtents.MinPoint;
                maxPt = curve.GeometricExtents.MaxPoint;
                if(minPt.GetVectorTo(maxPt).IsParallelTo(Vector3d.XAxis,new Tolerance(1e-4,1e-4)))                    
                {
                    minPt= minPt+new Vector3d(0,-1,0);
                    maxPt = maxPt + new Vector3d(0, 1, 0);
                }
                else if (minPt.GetVectorTo(maxPt).IsParallelTo(Vector3d.YAxis, new Tolerance(1e-4, 1e-4)))
                {
                    minPt = minPt + new Vector3d(-1, 0, 0);
                    maxPt = maxPt + new Vector3d(1, 0, 0);
                }
                else if(minPt.DistanceTo(maxPt)<=0.1)
                {
                    minPt = minPt + new Vector3d(-1, -1, 0);
                    maxPt = maxPt + new Vector3d(1, 1, 0);
                }
                pt1 = new Point2d(minPt.X, minPt.Y);
                pt2 = new Point2d(maxPt.X, minPt.Y);
                pt3 = new Point2d(maxPt.X, maxPt.Y);
                pt4 = new Point2d(minPt.X, maxPt.Y);
                Point2dCollection pts = new Point2dCollection();
                pts.Add(pt1);
                pts.Add(pt2);
                pts.Add(pt3);
                pts.Add(pt4);
                Polyline polyine = ThXClipCadOperation.CreatePolyline(pts, true);
                DBObjectCollection dbobjs = new DBObjectCollection();
                dbobjs.Add(polyine);
                DBObjectCollection curveRegionObjs = Region.CreateFromCurves(dbobjs);
                if (curveRegionObjs.Count > 0)
                {
                    curveRegion = curveRegionObjs[0] as Region;
                    wipeOutRegion.BooleanOperation(BooleanOperationType.BoolIntersect, curveRegion);
                    if (wipeOutRegion.Area > 0.0)
                    {
                        isIntersect = true;
                    }
                }
                polyine.Dispose();
            }
            catch (System.Exception ex)
            {
                ThXClipUtils.WriteException(ex, "TwoCurveRegionIntersect");
            }
            finally
            {
                if (wipeOutRegion != null)
                {
                    wipeOutRegion.Dispose();
                }
                if (curveRegion != null)
                {
                    curveRegion.Dispose();
                }
            }
            return isIntersect;
        }
        public void GenerateBlockThenInsert()
        {
            //以下是创建块
            string blockName = "";
            int limitNum = 0;
            foreach (var objId in this._analyzeRelation.ObjIds)
            {
                List<DraworderInfo> currentBlkObjs = this._analyzeRelation.DrawOrderinfs.
                    Where(i => i.BlockId == objId).Select(i => i).ToList();
                for (int i = 0; i < currentBlkObjs.Count; i++)
                {
                    limitNum += currentBlkObjs[i].DbObjs.Count;
                }
            }           
            using (Transaction trans = _document.Database.TransactionManager.StartTransaction())
            {
                BlockTable bt = trans.GetObject(_document.Database.BlockTableId, OpenMode.ForRead) as BlockTable;
                bt.UpgradeOpen();
                ThXclipCommands.pm.Start(@"正在组块...");
                ThXclipCommands.pm.SetLimit(limitNum);
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
                        if(!ent.Visible)
                        {
                            continue;
                        }
                        EntPropertyInfo entPropertyInf = new EntPropertyInfo() { Layer = ent.Layer, ColorIndex = ent.ColorIndex, Lw = ent.LineWeight };
                        for (int j = 0; j < currentBlkObjs[i].DbObjs.Count; j++)
                        {
                            DBObject dbObj = currentBlkObjs[i].DbObjs[j];
                            if (dbObj is Entity)
                            {
                                Entity entity = dbObj as Entity;   
                                ThXClipUtils.ChangeEntityProperty(entity, entPropertyInf);
                                if (entity is Dimension)
                                {
                                    entity.TransformBy(moveMt);
                                }
                                btr.AppendEntity(entity);

                                // 更新进度条
                                ThXclipCommands.pm.MeterProgress();
                                // 让CAD在长时间任务处理时任然能接收消息
                                System.Windows.Forms.Application.DoEvents();
                            }
                        }
                    }
                    bt.Add(btr);
                    trans.AddNewlyCreatedDBObject(btr, true);
                    this.BlkNamePosDic.Add(blockName, blkOriginPt);
                }
                ThXclipCommands.pm.Stop();
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
        private void OperationWipeOut(Point2dCollection wipeOutBoundaryPts, List<DraworderInfo> preDrawDois)
        {
            wipeOutBoundaryPts = TransWipeOutBoundaryPts(wipeOutBoundaryPts);
            CurveType curveType = CurveType.NotSupport;
            for (int i = 0; i < preDrawDois.Count; i++)
            {
                try
                {
                    curveType = GetDbObjType(preDrawDois[i].TypeName);
                    if (curveType == CurveType.NotSupport)
                    {
                        continue;
                    }
                    DBObjectCollection dbObjs = preDrawDois[i].DbObjs;
                    DBObjectCollection handledDbObjs = new DBObjectCollection();
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
                catch(System.Exception ex)
                {
                    ThXClipUtils.WriteException(ex);
                }
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
                try
                {
                    curveType = GetDbObjType(needHandleDrawDois[i].TypeName);
                    if (curveType == CurveType.NotSupport)
                    {
                        continue;
                    }
                    if(curveType == CurveType.WipeOut)
                    {
                        OperationXClipWithWipeOut(xClipEdgePts, needHandleDrawDois[i], keepInternal);
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
                catch (System.Exception ex)
                {
                    ThXClipUtils.WriteException(ex, "OperationXClip: 执行到第 " + i + " 条记录！");
                }
            }
        }
        private void OperationXClipWithWipeOut(Point2dCollection xClipEdgePts, DraworderInfo doi, bool keepInternal = false)
        {
            using (Transaction trans = this._document.TransactionManager.StartTransaction())
            {
                BlockTable bt = trans.GetObject(this._document.Database.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;
                Wipeout wp = trans.GetObject(doi.Id, OpenMode.ForRead) as Wipeout;
                DBObjectCollection dBObjects = wp.XClip(xClipEdgePts, keepInternal);
                List<DraworderInfo> currentWipeOuts = this._analyzeRelation.BlkWipeOuts.Where(j => j.Id ==
                   doi.Id).Select(j => j).ToList();
                if (dBObjects != null && dBObjects.Count > 0)
                {
                    currentWipeOuts.ForEach(j => this._analyzeRelation.BlkWipeOuts.Remove(j));
                    currentWipeOuts.ForEach(j => ThXClipCadOperation.EraseObjIds(j.Id));
                    btr.UpgradeOpen();
                    for (int j = 0; j < dBObjects.Count; j++)
                    {
                        ObjectId newWipeOutId = btr.AppendEntity(dBObjects[j] as Entity);
                        trans.AddNewlyCreatedDBObject(dBObjects[j], true);
                        DraworderInfo di = new DraworderInfo()
                        {
                            Id = newWipeOutId,
                            BlockId = doi.BlockId,
                            BlockName = doi.BlockName,
                            BlockPath = doi.BlockPath,
                            DrawIndex = doi.DrawIndex,
                            TypeName = doi.TypeName,
                        };
                        this._analyzeRelation.BlkWipeOuts.Add(di);
                    }
                    btr.DowngradeOpen();
                }
                else if (doi.Id.IsErased || !doi.Id.IsValid)
                {
                    currentWipeOuts.ForEach(j => this._analyzeRelation.BlkWipeOuts.Remove(j));
                }
                trans.Commit();
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
                case "WIPEOUT":
                    curveType = CurveType.WipeOut;
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
