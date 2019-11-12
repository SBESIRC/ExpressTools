using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices.Filters;

namespace ThXClip
{
    public class AnalyseRelation
    {
        private List<ObjectId> _objIds = new List<ObjectId>();
        private List<ObjectId> _modelWipeOutIds = new List<ObjectId>();

        private Dictionary<ObjectId, List<XClipInfo>> blockXClips = new Dictionary<ObjectId, List<XClipInfo>>();
        private List<DraworderInfo> _blkWipeOuts = new List<DraworderInfo>(); //
        private List<DraworderInfo> _drawOrderinfs = new List<DraworderInfo>();
        private Document _document;
        const string filterDictName = "ACAD_FILTER";
        const string spatialName = "SPATIAL";
        /// <summary>
        /// 获取块上所用的XClip信息
        /// </summary>
        public Dictionary<ObjectId, List<XClipInfo>> BlockXClips
        {
            get
            {
                return blockXClips;
            }
        }
        public List<ObjectId> ObjIds
        {
            get { return _objIds; }
        }
        /// <summary>
        /// ModelSpace中的WipeOut
        /// </summary>
        public List<ObjectId> ModelWipeOutIds
        {
            get{ return _modelWipeOutIds; } 
        }
        public List<DraworderInfo> BlkWipeOuts
        {
            get { return _blkWipeOuts; }
        }
        public List<DraworderInfo> DrawOrderinfs
        {
            get { return _drawOrderinfs; }
        }
        public AnalyseRelation(List<ObjectId> objectIds, List<ObjectId> modelWipeOutIds)
        {
            this._objIds = objectIds;
            this._modelWipeOutIds = modelWipeOutIds;
            _document = ThXClipCadOperation.GetMdiActiveDocument();
        }
        /// <summary>
        /// 传入的块与Wipeout在ModelSpace的先后绘制顺序
        /// </summary>
        public Dictionary<ObjectId, int> ModelSpaceDrawOrderDic { get; set; } = new Dictionary<ObjectId, int>();
        public void Analyse()
        {
            using (Transaction trans = _document.TransactionManager.StartTransaction())
            {
                BlockTable bt = trans.GetObject(_document.Database.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;
                DrawOrderTable dot = trans.GetObject(btr.DrawOrderTableId, OpenMode.ForRead) as DrawOrderTable;
                ObjectIdCollection ocids = dot.GetFullDrawOrder(0);
                this._objIds.ForEach(i => this.ModelSpaceDrawOrderDic.Add(i, ocids.IndexOf(i)));
                this._modelWipeOutIds.ForEach(i => this.ModelSpaceDrawOrderDic.Add(i, ocids.IndexOf(i)));
                btr.UpgradeOpen();
                for (int i = 0; i < this._objIds.Count; i++)
                {
                    BlockReference br = trans.GetObject(this._objIds[i], OpenMode.ForRead) as BlockReference;
                    if (br == null)
                    {
                        continue;
                    }
                    List<EntInf> entities = Explode(br,br.Name);
                    entities=entities.Where(j => j.Ent.Visible && JudgeEntityInsPointedType(j.Ent)).Select(j => j).ToList();
                    entities.ForEach(j => j.BlockPath.Reverse());
                    List<XClipInfo> currentXClipInfos = RetrieveXClipBoundary(br, br.Name);
                    if (currentXClipInfos != null && currentXClipInfos.Count>0)
                    {
                        currentXClipInfos.ForEach(j => j.BlockPath.Reverse());
                        currentXClipInfos.ForEach(j => j.AttachBlockId = br.Id);
                        currentXClipInfos=currentXClipInfos.OrderBy(j => j.BlockPath.Count).ToList(); //把层级从小到大排序
                        this.blockXClips.Add(this._objIds[i], currentXClipInfos);                
                    }
                    entities.ForEach(j => btr.AppendEntity(j.Ent));
                    entities.ForEach(j => trans.AddNewlyCreatedDBObject(j.Ent, true));
                    //entities.ForEach(j => j.Ent.Visible = false);
                    this._drawOrderinfs.AddRange(entities.Select(j => GenerateDoi(j,this._objIds[i])).ToList());
                }
                btr.DowngradeOpen();
                ObjectIdCollection ocids1 = dot.GetFullDrawOrder(0);
                _drawOrderinfs.ForEach(i => i.DrawIndex = ocids1.IndexOf(i.Id));
                trans.Commit();
            }
           this._blkWipeOuts=this._drawOrderinfs.Where(i => i.TypeName.ToUpper() == "WIPEOUT").Select(i => i).ToList(); //把WipeOut给收集下来
           this._drawOrderinfs = this._drawOrderinfs.Where(i => i.TypeName.ToUpper() != "WIPEOUT").Select(i => i).ToList(); //把WipeOut从DraworderInf中移除         
        }
        /// <summary>
        /// 判断实体是否是指定类型
        /// </summary>
        /// <param name="ent"></param>
        /// <returns></returns>
        private bool JudgeEntityInsPointedType(Entity ent)
        {
            bool res = false;
            if(ent.GetType()==typeof(Line))
            {
                res = true;
            }
            else if(ent.GetType() == typeof(Circle))
            {
                res = true;
            }
            else if (ent.GetType() == typeof(Arc))
            {
                res = true;
            }
            else if (ent.GetType() == typeof(Ellipse))
            {
                res = true;
            }
            else if (ent.GetType() == typeof(Spline))
            {
                res = true;
            }
            else if (ent.GetType() == typeof(Polyline))
            {
                res = true;
            }
            else if(ent.GetType() == typeof(Wipeout))
            {
                res = true;
            }
            return res;
        }
        private List<EntInf> Explode(BlockReference br,string preBlkName)
        {
            List<EntInf> entities = new List<EntInf>();
            DBObjectCollection collection = new DBObjectCollection();
            br.Explode(collection);
            List<string> currentLayBlkNames = new List<string>();
            string blkName = "";
            foreach (DBObject obj in collection)
            {
                if (obj is BlockReference)
                {
                    var newBr = obj as BlockReference;
                    if(currentLayBlkNames.IndexOf(newBr.Name)<0)
                    {
                        blkName = newBr.Name;
                    }
                    else
                    {
                        blkName = SetBlkName(currentLayBlkNames, newBr.Name);
                    }
                    currentLayBlkNames.Add(newBr.Name);
                    var childEnts = Explode(newBr, blkName);
                    if (childEnts != null)
                    {
                        entities.AddRange(childEnts);
                    }
                }
                else if (obj is Entity)
                {
                    entities.Add(new EntInf() { Ent = obj as Entity, BlockName = br.Name });
                }
            }
            entities.ForEach(i => i.BlockPath.Add(preBlkName));
            return entities;
        }
        private string SetBlkName(List<string> blkNameList,string blkName)
        {
            string newBlkName = blkName;
            int i = 1;
            while(blkNameList.IndexOf(newBlkName)>=0) //有此块名
            {
                newBlkName = blkName + i.ToString().PadLeft(3, '0');
            }            
            return newBlkName;
        }
        private DraworderInfo GenerateDoi(EntInf entInf,ObjectId blkId)
        {
            DraworderInfo draworderInf = new DraworderInfo();
            draworderInf.Id = entInf.Ent.Id;
            draworderInf.BlockId = blkId;
            draworderInf.TypeName = entInf.Ent.GetType().Name;
            draworderInf.BlockName = entInf.BlockName;
            draworderInf.BlockPath = entInf.BlockPath;
            DBObject dbObj = entInf.Ent.Clone() as DBObject;
            draworderInf.DbObjs.Add(dbObj);
            return draworderInf;
        }
        /// <summary>
        /// 检测块中是否有XClip
        /// </summary>
        /// <param name="blockRef"></param>
        /// <returns></returns>
        private XClipInfo RetrieveXClipBoundary(BlockReference blockRef)
        {
            Transaction _trans = _document.Database.TransactionManager.TopTransaction;
            XClipInfo xClipInfo = new XClipInfo();
            if (blockRef != null && blockRef.ExtensionDictionary != ObjectId.Null)
            {
                // The extension dictionary needs to contain a nested
                // dictionary called ACAD_FILTER
                var extdict = _trans.GetObject(blockRef.ExtensionDictionary, OpenMode.ForRead) as DBDictionary;
                if (extdict != null && extdict.Contains(filterDictName))
                {
                    var fildict = _trans.GetObject(extdict.GetAt(filterDictName), OpenMode.ForRead) as DBDictionary;
                    if (fildict != null)
                    {
                        // The nested dictionary should contain a
                        // SpatialFilter object called SPATIAL
                        if (fildict.Contains(spatialName))
                        {
                            SpatialFilter fil = _trans.GetObject(fildict.GetAt(spatialName), OpenMode.ForRead) as SpatialFilter;
                            if (fil != null)
                            {

                                bool isInverted = true;
#if ACAD2012 || ACAD2014
                                isInverted = true;

#else
                                isInverted = fil.Inverted;
#endif
                                xClipInfo.BlockName = blockRef.Name;
                                xClipInfo.AttachBlockId = blockRef.Id;
                                xClipInfo.Pts = fil.Definition.GetPoints();
                                Point2dCollection pts = new Point2dCollection();
                                foreach (Point2d pt in fil.Definition.GetPoints())
                                {
                                    Point3d tempPt = new Point3d(pt.X, pt.Y, 0.0);
                                    tempPt = tempPt.TransformBy(fil.ClipSpaceToWorldCoordinateSystemTransform); //从Ucs到Wcs
                                    tempPt = tempPt.TransformBy(fil.OriginalInverseBlockTransform); //转到当前块定义中的坐标点
                                    pts.Add(new Point2d(tempPt.X, tempPt.Y));
                                }
                                xClipInfo.Pts = pts;
                                xClipInfo.KeepInternal = false; //等找到设置内、外部的属性后，再修改此值
                                if(isInverted)
                                {
                                    xClipInfo.KeepInternal = false; 
                                }
                                else
                                {
                                    xClipInfo.KeepInternal = true;
                                }
                            }
                        }
                    }
                }
            }
            return xClipInfo;
        }
        private List<XClipInfo> RetrieveXClipBoundary(BlockReference br,string preBlkName)
        {
            List<XClipInfo> xClipInfos = new List<XClipInfo>();           
            Transaction trans = _document.Database.TransactionManager.TopTransaction;
            XClipInfo xClipInf = RetrieveXClipBoundary(br); //得到当前块下如果有BlockReference的物体信息
            if(xClipInf.Pts.Count>0)
            {
                xClipInfos.Add(xClipInf);
            }
            List<string> currentLayBlkNames = new List<string>();
            string blkName = "";
            BlockTableRecord btr = trans.GetObject(br.BlockTableRecord,OpenMode.ForRead) as BlockTableRecord;
            foreach (var id in btr)
            {
                DBObject dbObj=  trans.GetObject(id, OpenMode.ForRead); 
                if(dbObj is BlockReference)
                {
                    BlockReference currentBr = dbObj as BlockReference;                  
                    if (currentLayBlkNames.IndexOf(currentBr.Name) < 0)
                    {
                        blkName = currentBr.Name;
                    }
                    else
                    {
                        blkName = SetBlkName(currentLayBlkNames, currentBr.Name);
                    }
                    currentLayBlkNames.Add(currentBr.Name);                   
                    List<XClipInfo> subXClipInfs = RetrieveXClipBoundary(currentBr, blkName);
                    if(subXClipInfs!=null && subXClipInfs.Count>0)
                    {
                        xClipInfos.AddRange(subXClipInfs);
                    }
                }
            }
            if (xClipInfos != null && xClipInfos.Count > 0)
            {
                for (int i = 0; i < xClipInfos.Count; i++)
                {
                    if (xClipInfos[i].Pts.Count == 0)
                    {
                        continue;
                    }
                    Point2dCollection pts = new Point2dCollection();
                    foreach (Point2d pt in xClipInfos[i].Pts)
                    {
                        Point3d tempPt = new Point3d(pt.X, pt.Y, 0.0);
                        tempPt = tempPt.TransformBy(br.BlockTransform);
                        pts.Add(new Point2d(tempPt.X, tempPt.Y));
                    }
                    xClipInfos[i].Pts = pts;
                }
            }
            xClipInfos.ForEach(i => i.BlockPath.Add(preBlkName));
            return xClipInfos;
        }
        public List<DraworderInfo> GetWipeOutPreDrawOrderInfs(ObjectId wpId)
        {
            List<DraworderInfo> preDrawInfs = new List<DraworderInfo>();
            if(this.ModelSpaceDrawOrderDic.ContainsKey(wpId))
            {
                int drawOrderIndex = this.ModelSpaceDrawOrderDic[wpId];
                List<ObjectId> preDrawBlks= this.ModelSpaceDrawOrderDic.Where(i => i.Value < drawOrderIndex).Select(i => i.Key).ToList();
                preDrawInfs= this._drawOrderinfs.Where(i => preDrawBlks.IndexOf(i.BlockId) >= 0).Select(i => i).ToList();
            }
            return preDrawInfs;
        }
        public List<DraworderInfo> GetWipeOutPreDrawOrderInfs(DraworderInfo wpDoi)
        {
            List<DraworderInfo> preDrawInfs = new List<DraworderInfo>();
            preDrawInfs = this._drawOrderinfs.Where(i => i.DrawIndex< wpDoi.DrawIndex).Select(i => i).ToList();
            return preDrawInfs;
        }
        /// <summary>
        /// 删除传入的块，以及从块中炸开的并提交到ModelSpace中的实体
        /// </summary>
        public void EraseBlockAndItsExplodedObjs()
        {
            using (Transaction trans=this._document.Database.TransactionManager.StartTransaction())
            {
                for(int i=0;i<this._drawOrderinfs.Count;i++)
                {
                   Entity ent= trans.GetObject(this._drawOrderinfs[i].Id, OpenMode.ForWrite) as Entity;
                   ent.Erase();
                }
                for (int i = 0; i < this._blkWipeOuts.Count; i++)
                {
                    Entity ent = trans.GetObject(this._blkWipeOuts[i].Id, OpenMode.ForWrite) as Entity;
                    ent.Erase();
                }
                for(int i=0;i<this._objIds.Count;i++)
                {
                    BlockReference br= trans.GetObject(this._objIds[i], OpenMode.ForWrite) as BlockReference;
                    br.Erase();
                }
                trans.Commit();
            }
        }
        public List<DraworderInfo> GetXClipDrawOrderInfs(XClipInfo xClipInfo)
        {
            List<DraworderInfo> draworderInfs = new List<DraworderInfo>();
            //把所属同一个父块的
            List<DraworderInfo> firstFilterDraworderInfs =this.DrawOrderinfs.Where(i=>i.BlockId==xClipInfo.AttachBlockId).Select(i=>i).ToList();
            draworderInfs=firstFilterDraworderInfs.Where(i => CompareListIsSubOfOther(xClipInfo.BlockPath, i.BlockPath)).Select(i => i).ToList();
            return draworderInfs;
        }
        /// <summary>
        /// 获取XClip所在块的WipeOut及嵌套块下的所有WipeOut
        /// </summary>
        /// <param name="xClipInfo"></param>
        /// <returns></returns>
        public List<DraworderInfo> GetXClipAccessoryWipeOuts(XClipInfo xClipInfo)
        {
            List<DraworderInfo> wipeOutDis = new List<DraworderInfo>();
            //把所属同一个父块的
            List<DraworderInfo> firstFilterDraworderInfs = this.BlkWipeOuts.Where(i => i.BlockId == xClipInfo.AttachBlockId).Select(i => i).ToList();
            wipeOutDis = firstFilterDraworderInfs.Where(i => CompareListIsSubOfOther(xClipInfo.BlockPath, i.BlockPath)).Select(i => i).ToList();
            return wipeOutDis;
        }
        private bool CompareListIsSubOfOther(List<string> firstList,List<string> secondList)
        {            
            if(secondList.Count< firstList.Count)
            {
                return false;
            }
            bool res = true;
            for (int i=0;i<firstList.Count;i++)
            {
                if(firstList[i]!= secondList[i])
                {
                    res = false;
                    break;
                }
            }
            return res;
        }
    }
}
