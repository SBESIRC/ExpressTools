using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Geometry;

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
        Line
    }
    /// <summary>
    /// 修剪WipeOut
    /// </summary>
    public class WipeOutXClipTrim
    {
        private AnalyseRelation _analyzeRelation;
        private Dictionary<ObjectId, Explosion> _blockRefExplodeDic = new Dictionary<ObjectId, Explosion>();
        private Document _document;
        //用于记录模型空间中Curve被修剪后，对应的关系
        private List<ModelObjInfo> modelObjInfos = new List<ModelObjInfo>();

        public WipeOutXClipTrim(AnalyseRelation analyseRelation, Dictionary<ObjectId, Explosion> blockRefExplodeDic)
        {
            _analyzeRelation= analyseRelation;
            _blockRefExplodeDic = blockRefExplodeDic;
            _document = CadOperation.GetMdiActiveDocument();
        }
        public void StartTrim()
        {
            Init();
            for (int i = 0; i < this._analyzeRelation.WipeOutInfs.Count; i++)
            {
                WipeOutInfo wipeOutInfo = this._analyzeRelation.WipeOutInfs[i];
                OperationWipeOut(wipeOutInfo);
            }
            for (int i = 0; i < this._analyzeRelation.XclipInfs.Count; i++)
            {
                XClipInfo xClipInfo = this._analyzeRelation.XclipInfs[i];
                OperationXClip(xClipInfo);
            }
        }
        private void Init()
        {
           List<ObjectId> objIds= _analyzeRelation.DrawOrderInfos.Where(i=>i.BlockName.ToUpper()=="MODELSPACE").Select(i=>i.Id).ToList();
           for(int i=0;i< objIds.Count;i++)
            {
                modelObjInfos.Add(new ModelObjInfo() {Id= objIds[i], DbObjs= new DBObjectCollection()});
            }
        }
        private void OperationWipeOut(WipeOutInfo wipeOutInfo)
        {
            List<DrawOrderInfo> drawOrderInfos = new List<DrawOrderInfo>();
            TraverseDrawOrderInfo(wipeOutInfo.BlkId, ref drawOrderInfos); //获取WipeOut之前的DrawOrders
            ObjectId outermostBlkRefId = wipeOutInfo.NestedBlockIds[wipeOutInfo.NestedBlockIds.Count - 1]; //获取Wipeout所在块的BlkRefId
            if (this._blockRefExplodeDic.ContainsKey(outermostBlkRefId))
            {
                Explosion explosion = this._blockRefExplodeDic[outermostBlkRefId]; //获取
                TrimWipeOutWithCurve(wipeOutInfo, drawOrderInfos, explosion);
            }
        }
        /// <summary>
        /// WipeOut
        /// </summary>
        /// <param name="wipeOutInfo">WipeOut 对象信息</param>
        /// <param name="drawOrderInfos"> wipeout之前的DrawOrder Object 信息</param>
        /// <param name="explosion"></param>
        private void TrimWipeOutWithCurve(WipeOutInfo wipeOutInfo, List<DrawOrderInfo> drawOrderInfos, Explosion explosion)
        {
            using (Transaction trans=_document.Database.TransactionManager.StartTransaction())
            {
                Wipeout wipeout = trans.GetObject(wipeOutInfo.Id,OpenMode.ForRead) as Wipeout; //
                CurveType curveType = CurveType.NotSupport;
                for(int i=0;i<drawOrderInfos.Count; i++)
                {
                    DBObject dbObj = trans.GetObject(drawOrderInfos[i].Id,OpenMode.ForRead);
                    if(!(dbObj is Curve))
                    {
                        continue;
                    }
                    curveType = GetDbObjType(dbObj);
                    if(curveType == CurveType.NotSupport)
                    {
                        continue;
                    }
                    DBObjectCollection dbObjs = new DBObjectCollection();
                    DBObjectCollection handledDbObjs = new DBObjectCollection();
                    if (drawOrderInfos[i].BlockName.ToUpper()=="MODELSPACE")
                    {
                        dbObjs = this.modelObjInfos.Where(j => j.Id == drawOrderInfos[i].Id).Select(j => j.DbObjs).FirstOrDefault();
                        if(dbObjs.Count==0)
                        {
                            dbObjs.Add(dbObj); //对于模型空间中的物体，初始用其本身去处理
                        }                        
                    }
                    else
                    {
                        BlockRefObjSort blkRefObjSort = explosion.BlockExpodeMapping.Where
                        (j => j.Id == drawOrderInfos[i].Id).Select(j => j).FirstOrDefault();
                        dbObjs = blkRefObjSort.DbObjs;
                    }
                    for(int j=0;j< dbObjs.Count;j++)
                    {
                        curveType= GetDbObjType(dbObjs[i]);
                        DBObjectCollection subDbObjs = new DBObjectCollection();
                        switch (curveType)
                        {
                            case CurveType.Line:
                                Line line = dbObjs[i] as Line;
                                subDbObjs = line.XClip(TransWipeOutBoundaryPts(wipeOutInfo.Pts));
                                break;
                            case CurveType.Circle:
                                Circle circle = dbObjs[i] as Circle;
                                subDbObjs = circle.XClip(TransWipeOutBoundaryPts(wipeOutInfo.Pts));
                                break;
                            case CurveType.Arc:
                                Arc arc = dbObjs[i] as Arc;
                                subDbObjs = arc.XClip(TransWipeOutBoundaryPts(wipeOutInfo.Pts)); 
                                break;
                            case CurveType.Ellipse:
                                Ellipse ellipse = dbObjs[i] as Ellipse;
                                subDbObjs = ellipse.XClip(TransWipeOutBoundaryPts(wipeOutInfo.Pts));
                                break;
                            case CurveType.Xline:
                                Xline xline = dbObjs[i] as Xline;
                                subDbObjs = xline.XClip(TransWipeOutBoundaryPts(wipeOutInfo.Pts));
                                break;
                            case CurveType.Spline:
                                Spline spline = dbObjs[i] as Spline;
                                subDbObjs = spline.XClip(TransWipeOutBoundaryPts(wipeOutInfo.Pts));
                                break;
                            case CurveType.Polyline:
                                Polyline polyline = dbObjs[i] as Polyline;
                                subDbObjs = polyline.XClip(TransWipeOutBoundaryPts(wipeOutInfo.Pts));
                                break;
                            case CurveType.Polyline2d:
                                Polyline2d polyline2d = dbObjs[i] as Polyline2d;
                                subDbObjs = polyline2d.XClip(TransWipeOutBoundaryPts(wipeOutInfo.Pts));
                                break;
                            case CurveType.Polyline3d:
                                Polyline3d polyline3d = dbObjs[i] as Polyline3d;
                                subDbObjs = polyline3d.XClip(TransWipeOutBoundaryPts(wipeOutInfo.Pts));
                                break;
                        }
                        foreach(DBObject newDbObj in subDbObjs)
                        {
                            handledDbObjs.Add(newDbObj);
                        }
                    }
                    if (drawOrderInfos[i].BlockName.ToUpper() == "MODELSPACE")
                    {
                        this.modelObjInfos.Where(j => j.Id == drawOrderInfos[i].Id).Select(j => j).FirstOrDefault().DbObjs= handledDbObjs;
                    }
                    else
                    {
                        explosion.BlockExpodeMapping.Where
                        (j => j.Id == drawOrderInfos[i].Id).Select(j => j).FirstOrDefault().DbObjs= handledDbObjs;
                    }
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
        private void OperationXClip(XClipInfo xClipInfo)
        {
           
        }
        private void TraverseDrawOrderInfo(ObjectId id, ref List<DrawOrderInfo> drawOrderInfos)
        {
            DrawOrderInfo currentDrawOrderInf = _analyzeRelation.DrawOrderInfos.Where(i => i.Id == id).Select(i => i).FirstOrDefault();
            List<DrawOrderInfo> newDrawOrderInfs = _analyzeRelation.DrawOrderInfos.Where(i => i.ParentBlkId == currentDrawOrderInf.ParentBlkId
            && i.DrawIndex< currentDrawOrderInf.DrawIndex).Select(i => i).ToList();
            if (newDrawOrderInfs != null && newDrawOrderInfs.Count > 0)
            {
                drawOrderInfos.AddRange(newDrawOrderInfs);
                TraverseDrawOrderInfo(currentDrawOrderInf.ParentBlkId, ref drawOrderInfos);
            }
        }
    }
}
