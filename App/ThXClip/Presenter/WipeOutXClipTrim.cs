using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Geometry;
using TianHua.AutoCAD.Utility.ExtensionTools;

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
            _document = CadOperation.GetMdiActiveDocument();
        }
        public void StartTrim()
        {
            for (int i = 0; i < this._analyzeRelation.ModelWipeOutIds.Count; i++)
            {
                List<DraworderInf> preDrawDoi = this._analyzeRelation.GetWipeOutPreDrawOrderInfs(this._analyzeRelation.ModelWipeOutIds[i]);
                Point2dCollection boundaryPts = GetWipeOutBoundaryPts(this._analyzeRelation.ModelWipeOutIds[i]);
                OperationWipeOut(boundaryPts, preDrawDoi);

            }
            for (int i = 0; i < this._analyzeRelation.BlkWipeOuts.Count; i++)
            {
                List<DraworderInf> preDrawDoi = this._analyzeRelation.GetWipeOutPreDrawOrderInfs(this._analyzeRelation.BlkWipeOuts[i]);
                Point2dCollection boundaryPts = GetWipeOutBoundaryPts(this._analyzeRelation.BlkWipeOuts[i].Id);
                OperationWipeOut(boundaryPts, preDrawDoi);
            }
            for (int i = 0; i < this._analyzeRelation.XclipInfs.Count; i++)
            {
                OperationXClip(this._analyzeRelation.XclipInfs[i]);
            }
            string blockName = "";
            //List<Point3d> cornerPts=GetHandleObjsCornerPts();
            //if(cornerPts.Count==2)
            //{
            //    this._blkOriginPt = CadOperation.GetMidPt(cornerPts[0], cornerPts[1]);
            //}
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
                    List<DraworderInf> currentBlkObjs = this._analyzeRelation.DrawOrderinfs.
                        Where(i => i.BlockId == objId).Select(i => i).ToList();
                    for (int i = 0; i < currentBlkObjs.Count; i++)
                    {
                        Entity ent = _document.TransactionManager.TopTransaction.GetObject(currentBlkObjs[i].Id, OpenMode.ForRead) as Entity;
                        EntPropertyInf entPropertyInf = new EntPropertyInf() { Layer = ent.Layer, ColorIndex = ent.ColorIndex, Lw = ent.LineWeight };
                        for (int j = 0; j < currentBlkObjs[i].DbObjs.Count; j++)
                        {
                            DBObject dbObj = currentBlkObjs[i].DbObjs[j];
                            if (dbObj is Entity)
                            {
                                Entity entity = dbObj as Entity;
                                if(!entity.Visible)
                                {
                                    entity.Visible = true;
                                }
                                PubilcFunction.ChangeEntityProperty(entity, entPropertyInf);
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
        private void OperationWipeOut(Point2dCollection wipeOutBoundaryPts, List<DraworderInf> preDrawDois)
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
                    #region----------暂时不处理----------
                    //for (int j = 0; j < dbObjs.Count; j++)
                    //{
                    //    Entity ent = dbObjs[j] as Entity;
                    //    if (ent != null)
                    //    {
                    //        Point3dCollection intersectPts = new Point3dCollection();
                    //        Polyline polyline = CadOperation.CreatePolyline(wipeOutBoundaryPts);
                    //        polyline.BoundingBoxIntersectWith(ent, Intersect.OnBothOperands, intersectPts, IntPtr.Zero, IntPtr.Zero);
                    //        bool keepEnt = true;
                    //        if (intersectPts == null || intersectPts.Count == 0) //没有交点
                    //        {
                    //            if (ent.Bounds != null && ent.Bounds.HasValue)
                    //            {
                    //                Point3d midPt = CadOperation.GetMidPt(ent.Bounds.Value.MinPoint, ent.Bounds.Value.MaxPoint);
                    //                if (CadOperation.IsPointInPolyline(wipeOutBoundaryPts, new Point2d(midPt.X, midPt.Y)))
                    //                {
                    //                    keepEnt = false;
                    //                }
                    //            }
                    //            else
                    //            {
                    //                if(ent is Dimension)
                    //                {
                    //                   Entity cloneEnt= ent.Clone() as Entity;
                    //                   List<ObjectId> entIds= CadOperation.AddToBlockTable(cloneEnt);
                    //                    polyline.BoundingBoxIntersectWith(ent, Intersect.OnBothOperands, intersectPts, IntPtr.Zero, IntPtr.Zero);
                    //                }
                    //            }
                    //        }
                    //        else if (intersectPts != null && intersectPts.Count > 0)
                    //        {
                    //            keepEnt = false;
                    //        }
                    //        if (keepEnt)
                    //        {                                    
                    //            handledDbObjs.Add(dbObjs[j]);
                    //        }
                    //    }
                    //}
                    #endregion
                }
                else
                {
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
                            case CurveType.Xline:
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
        private void OperationXClip(XClipInfo xClipInfo)
        {
            Point2dCollection xClipEdgePts = TransWipeOutBoundaryPts(xClipInfo.Pts);
            List<DraworderInf> needHandleDrawDois = _analyzeRelation.GetXClipDrawOrderInfs(xClipInfo);
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
                            subDbObjs = line.XClip(xClipEdgePts, xClipInfo.KeepInternal);
                            break;
                        case CurveType.Circle:
                            Circle circle = dbObjs[j] as Circle;
                            subDbObjs = circle.XClip(xClipEdgePts, xClipInfo.KeepInternal);
                            break;
                        case CurveType.Arc:
                            Arc arc = dbObjs[j] as Arc;
                            subDbObjs = arc.XClip(xClipEdgePts, xClipInfo.KeepInternal);
                            break;
                        case CurveType.Ellipse:
                            Ellipse ellipse = dbObjs[j] as Ellipse;
                            subDbObjs = ellipse.XClip(xClipEdgePts, xClipInfo.KeepInternal);
                            break;
                        case CurveType.Xline:
                            Xline xline = dbObjs[j] as Xline;
                            subDbObjs = xline.XClip(xClipEdgePts, xClipInfo.KeepInternal);
                            break;
                        case CurveType.Spline:
                            Spline spline = dbObjs[j] as Spline;
                            subDbObjs = spline.XClip(xClipEdgePts, xClipInfo.KeepInternal);
                            break;
                        case CurveType.Polyline:
                            Polyline polyline = dbObjs[j] as Polyline;
                            subDbObjs = polyline.XClip(xClipEdgePts, xClipInfo.KeepInternal);
                            break;
                        case CurveType.Polyline2d:
                            Polyline2d polyline2d = dbObjs[j] as Polyline2d;
                            subDbObjs = polyline2d.XClip(xClipEdgePts, xClipInfo.KeepInternal);
                            break;
                        case CurveType.Polyline3d:
                            Polyline3d polyline3d = dbObjs[j] as Polyline3d;
                            subDbObjs = polyline3d.XClip(xClipEdgePts, xClipInfo.KeepInternal);
                            break;
                        case CurveType.Ray:
                            Ray ray = dbObjs[j] as Ray;
                            subDbObjs = ray.XClip(xClipEdgePts, xClipInfo.KeepInternal);
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
