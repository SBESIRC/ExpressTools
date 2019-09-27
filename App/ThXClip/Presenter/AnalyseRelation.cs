using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices.Filters;

namespace ThXClip
{
    public class AnalyseRelation
    {
        private List<ObjectId> _objIds = new List<ObjectId>();
        private List<XClipInfo> xclipInfs = new List<XClipInfo>();
        private List<WipeOutInfo> wipeOutInfs = new List<WipeOutInfo>();
        private Document _document;
        private Database _database;
        const string filterDictName = "ACAD_FILTER";
        const string spatialName = "SPATIAL";

        private ObjectId _modelSpaceId = ObjectId.Null;

        private List<DrawOrderInfo> drawOrderInfos = new List<DrawOrderInfo>();
        /// <summary>
        /// 获取所选物体的绘制顺序
        /// </summary>
        
        public List<DrawOrderInfo> DrawOrderInfos
        {
            get
            {
                return drawOrderInfos;
            }
        }
        /// <summary>
        /// 获取块上所用的XClip信息
        /// </summary>
        public List<XClipInfo> XclipInfs
        {
            get
            {
                return xclipInfs;
            }
        }
        /// <summary>
        /// 获取块上使用的WipeOut信息
        /// </summary>
        public List<WipeOutInfo> WipeOutInfs
        {
            get
            {
                return wipeOutInfs;
            }
        }
        public AnalyseRelation(List<ObjectId> objectIds)
        {
            this._objIds = objectIds;
            _document = CadOperation.GetMdiActiveDocument();
            _database = _document.Database;
        }
        public void Analyse()
        {
            using (Transaction trans = _database.TransactionManager.StartTransaction())
            {
                BlockTable bt = trans.GetObject(_database.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;
                _modelSpaceId = btr.Id;
                DrawOrderTable dot = trans.GetObject(btr.DrawOrderTableId, OpenMode.ForRead) as DrawOrderTable;
                ObjectIdCollection ocids = dot.GetFullDrawOrder(0);
                Dictionary<ObjectId, int> passObjIdSortDic = GetDrawOrderIndex(this._objIds, ocids); //用户所选的物体绘制顺序
                foreach (var item in passObjIdSortDic)
                {
                    DrawOrderInfo drawOrderInfo = new DrawOrderInfo();
                    drawOrderInfo.Id = item.Key;
                    drawOrderInfo.DrawIndex = item.Value;
                    drawOrderInfo.ParentBlkId = btr.Id;
                    drawOrderInfo.BlockName = btr.Name;
                    DBObject dBObject = trans.GetObject(item.Key, OpenMode.ForRead);
                    drawOrderInfo.TypeName = dBObject.GetType().Name;
                    this.drawOrderInfos.Add(drawOrderInfo);
                    if (dBObject is BlockReference)
                    {
                        BlockReference br = dBObject as BlockReference;                       
                        XClipInfo xClipInfo = RetrieveXClipBoundary(br);
                        if (xClipInfo.AttachBlockId != ObjectId.Null)
                        {
                            this.xclipInfs.Add(xClipInfo);
                        }
                        FindDrawOrder(br);
                    }
                    else if (dBObject is Wipeout)
                    {
                        WipeOutInfo wipeOutInfo = new WipeOutInfo();
                        wipeOutInfo.Id = item.Key;
                        Wipeout wipeout = dBObject as Wipeout;
                        wipeOutInfo.Pts = wipeout.GetClipBoundary();
                        wipeOutInfo.BlkId = btr.Id;
                        this.wipeOutInfs.Add(wipeOutInfo);
                    }
                }
                trans.Commit();
            }
            AnalyseWipeOutPreDrawInfs();
            AnalyseXclipPreDrawInfs();
        }
        private Dictionary<ObjectId, int> GetDrawOrderIndex(List<ObjectId> objIds,ObjectIdCollection ocids)
        {
            Dictionary<ObjectId, int> dic = new Dictionary<ObjectId, int>();
            foreach (ObjectId objId in objIds)
            {
                dic.Add(objId, ocids.IndexOf(objId));
            }
            return dic;
        }
        private void FindDrawOrder(BlockReference br)
        {
            Transaction _trans = _database.TransactionManager.TopTransaction;
            BlockTableRecord btr = _trans.GetObject(br.BlockTableRecord, OpenMode.ForRead) as BlockTableRecord;
            var dotSource =(DrawOrderTable)_trans.GetObject(btr.DrawOrderTableId, OpenMode.ForRead, true);
            ObjectIdCollection srcDotIds = new ObjectIdCollection();
            srcDotIds = dotSource.GetFullDrawOrder(0);
            foreach (ObjectId objectId in srcDotIds)
            {
                DBObject dBObject = _trans.GetObject(objectId, OpenMode.ForRead);
                DrawOrderInfo drawOrderInfo = new DrawOrderInfo();
                drawOrderInfo.Id = objectId;
                drawOrderInfo.DrawIndex = srcDotIds.IndexOf(objectId);
                drawOrderInfo.ParentBlkId = br.Id;
                drawOrderInfo.BlockName = btr.Name;
                drawOrderInfo.TypeName = dBObject.GetType().Name;
                this.drawOrderInfos.Add(drawOrderInfo);
                if (dBObject is BlockReference)
                {
                    BlockReference br1 = dBObject as BlockReference;
                    BlockTableRecord btr1 = _trans.GetObject(br1.BlockTableRecord, OpenMode.ForRead) as BlockTableRecord;
                    XClipInfo xClipInfo = RetrieveXClipBoundary(br1);
                    if (xClipInfo.AttachBlockId != ObjectId.Null)
                    {
                        this.xclipInfs.Add(xClipInfo);
                    }
                    FindDrawOrder(br1);
                }
                else if (dBObject is Wipeout)
                {
                    WipeOutInfo wipeOutInfo = new WipeOutInfo();
                    wipeOutInfo.Id = objectId;
                    Wipeout wipeout = dBObject as Wipeout;
                    wipeOutInfo.Pts = wipeout.GetClipBoundary();
                    wipeOutInfo.BlkId = br.Id;
                    this.wipeOutInfs.Add(wipeOutInfo);
                }
            }
        }
        /// <summary>
        /// 检测块中是否有XClip
        /// </summary>
        /// <param name="blockRef"></param>
        /// <returns></returns>
        private XClipInfo RetrieveXClipBoundary(BlockReference blockRef)
        {
            Transaction _trans = _database.TransactionManager.TopTransaction;
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
                            var fil = _trans.GetObject(fildict.GetAt(spatialName), OpenMode.ForRead) as SpatialFilter;
                            if (fil != null)
                            {
                                xClipInfo.AttachBlockId = blockRef.Id;
                                xClipInfo.Pts = fil.Definition.GetPoints();
                                xClipInfo.TrimInside = true;
                            }
                        }
                    }
                }
            }
            return xClipInfo;
        }

        #region---------分析每一个WipeOut前面绘制的物体----------
        private void AnalyseWipeOutPreDrawInfs()
        {
            foreach(WipeOutInfo wipeOutInfo in this.wipeOutInfs)
            {
                List<ObjectId> blockIds = new List<ObjectId>() { };
                TraverseDrawOrderInfo(wipeOutInfo.BlkId,ref blockIds);
                blockIds.Remove(this._modelSpaceId);
                wipeOutInfo.NestedBlockIds = blockIds;
            }
        }
        private void AnalyseXclipPreDrawInfs()
        {
            foreach (XClipInfo xClipInfo in this.xclipInfs)
            {
                List<ObjectId> blockIds = new List<ObjectId>() { };
                TraverseDrawOrderInfo(xClipInfo.AttachBlockId, ref blockIds);
                blockIds.Remove(this._modelSpaceId);
                xClipInfo.NestedBlockIds = blockIds;
            }
        }
        /// <summary>
        /// 遍历分析当前的DrawOrderInfo列表
        /// </summary>
        /// <param name="wipeOutInfo"></param>
        private void TraverseDrawOrderInfo(ObjectId blkId,ref List<ObjectId> objectIds)
        {
           objectIds.Add(blkId);
            //List<DrawOrderInfo> parentDrawOrderInfs = this.drawOrderInfos.Where(i => i.ParentBlkId == blkId).Select(i => i).ToList(); //获取在同一块中所有的物体
            List<DrawOrderInfo> drawOrderInfs =this.drawOrderInfos.Where(i => i.Id == blkId).Select(i => i).ToList(); 
           if (drawOrderInfs != null && drawOrderInfs.Count>0)
           {
                TraverseDrawOrderInfo(drawOrderInfs[0].ParentBlkId,ref objectIds);
           }
        }
        #endregion
    }
}
