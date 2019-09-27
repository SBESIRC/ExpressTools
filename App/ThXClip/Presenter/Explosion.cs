using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThXClip
{
    public class Explosion
    {
        public Explosion()
        {
            _document = CadOperation.GetMdiActiveDocument();
        }
        private List<DBObject> _blockRemoveObjs = new List<DBObject>();

        private List<BlockRefObjSort> _blockRefObjSorts  = new List<BlockRefObjSort>();
        private List<DBObjectSort> _dbObjSorts = new List<DBObjectSort>();

        private Document _document;
        public List<DBObject> BlockRemoveObjs
        {
            get
            {
                return _blockRemoveObjs;
            }
        }
        /// <summary>
        /// 记录块参照
        /// </summary>
        public List<BlockRefObjSort> BlockExpodeMapping
        {
            get
            {
                return _blockRefObjSorts;
            }
        }
        /// <summary>
        /// 获取BlkRefence中实体Id对应炸开的内存图元对象
        /// </summary>
        /// <param name="objId"></param>
        /// <returns></returns>
        public DBObject GetBlockReferenceMapping(ObjectId objId)
        {
            DBObject dbObj = null;
            BlockRefObjSort blkRefObjSort= this._blockRefObjSorts.Where(i => i.Id.Equals(objId)).Select(i => i).FirstOrDefault() ;
            if(blkRefObjSort==null)
            {
                return dbObj;
            }
            DBObjectSort dbObjSort= this._dbObjSorts.Where(i=>i.BlockName==blkRefObjSort.BlockName && 
            i.index==blkRefObjSort.index && i.TypeName==blkRefObjSort.BlockName).Select(i=>i).FirstOrDefault();
            if(dbObjSort==null)
            {
                return dbObj;
            }
            dbObj = dbObjSort.DbObj;
            return dbObj;
        }
        public List<Curve> GetCurvesFromBlock(BlockReference block)
        {
            DBObjectCollection collection = new DBObjectCollection();
            block.Explode(collection);
            var blockCurves = new List<Curve>();
            foreach(var obj in collection)
            {
                if(obj is Curve)
                {
                    var curve = obj as Curve;
                    curve.Layer = block.Layer;
                    blockCurves.Add(curve);
                }
                else if(obj is BlockReference)
                {
                    var blockRef = obj as BlockReference;
                    blockRef.Layer = block.Layer;
                    var childCurves = GetCurvesFromBlock(blockRef);
                    if(childCurves!=null && childCurves.Count>0)
                    {
                        blockCurves.AddRange(childCurves);
                    }
                }
                else
                {
                    DBObject dbObj = obj as DBObject;
                    if(dbObj!=null)
                    {
                        _blockRemoveObjs.Add(dbObj);
                    }
                }
            }
            return blockCurves;
        }
        public List<Entity> GetEntitiesFromBlock(BlockReference block)
        {
            DBObjectCollection collection = new DBObjectCollection();
            block.Explode(collection);
            var ents = new List<Entity>();
            foreach (var obj in collection)
            {
                if (obj is Entity)
                {
                    var ent = obj as Entity;
                    ent.Layer = block.Layer;
                    collection.Add(ent);
                }
                else if (obj is BlockReference)
                {
                    var blockRef = obj as BlockReference;
                    blockRef.Layer = block.Layer;
                    var childEnts = GetCurvesFromBlock(blockRef);
                    if (childEnts != null && childEnts.Count > 0)
                    {
                        ents.AddRange(childEnts);
                    }
                }
                else
                {
                    DBObject dbObj = obj as DBObject;
                    if (dbObj != null)
                    {
                        _blockRemoveObjs.Add(dbObj);
                    }
                }
            }
            return ents;
        }        
        private List<DBObjectSort> GetDbObjectSortFromBlock(BlockReference block)
        {
            DBObjectCollection collection = new DBObjectCollection();
            block.Explode(collection);
            var ents = new List<DBObjectSort>();
            int index = 0;
            foreach (DBObject obj in collection)
            {
                DBObjectSort dbObjSort = new DBObjectSort();
                dbObjSort.DbObj = obj;
                dbObjSort.index = index;
                dbObjSort.BlockName = block.Name;
                dbObjSort.TypeName = obj.GetType().Name;
                ents.Add(dbObjSort);
                if (obj is BlockReference)
                {
                    var br = obj as BlockReference;
                    var childDbObjs = GetDbObjectSortFromBlock(br);
                    if(childDbObjs!=null)
                    {
                        ents.AddRange(childDbObjs);
                    }
                }
            }
            return ents;
        }
        /// <summary>
        /// 用于创建BlockReference炸开后与源对象的映射关系
        /// </summary>
        /// <param name="block"></param>
        public void CreateBlockRefExplodeMappingInfo(ObjectId blkRefId)
        {
            using (Transaction trans=_document.Database.TransactionManager.StartTransaction())
            {
                BlockReference br = trans.GetObject(blkRefId,OpenMode.ForRead) as BlockReference;
                this._dbObjSorts = GetDbObjectSortFromBlock(br);
                this._blockRefObjSorts = GetBlockRefObjSort(br);
                trans.Commit();
            }
            for(int i=0;i<this._blockRefObjSorts.Count;i++)
            {
                DBObjectSort dbObjSort = this._dbObjSorts.Where(j => j.BlockName == this._blockRefObjSorts[i].BlockName &&
                j.index == this._blockRefObjSorts[i].index &&
                this._blockRefObjSorts[i].TypeName == this._blockRefObjSorts[i].TypeName).Select(j => j).FirstOrDefault();
                if(dbObjSort==null)
                {
                    continue;
                }
                DBObjectCollection newDbObjs = new DBObjectCollection();
                newDbObjs.Add(dbObjSort.DbObj);
                this._blockRefObjSorts[i].DbObjs = newDbObjs;
            }
        }
        /// <summary>
        /// 获取块本身的结构序列
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        private List<BlockRefObjSort> GetBlockRefObjSort(BlockReference block)
        {
            Transaction trans = _document.Database.TransactionManager.TopTransaction;
            BlockTableRecord btr = trans.GetObject(block.BlockTableRecord, OpenMode.ForRead) as BlockTableRecord;
            var bfSorts = new List<BlockRefObjSort>();
            int index = 0;
            foreach (var objId in btr)
            {
                DBObject dbObj = trans.GetObject(objId, OpenMode.ForRead);
                BlockRefObjSort dbObjSort = new BlockRefObjSort();
                dbObjSort.Id = objId;
                dbObjSort.index = index;
                dbObjSort.BlockName = block.Name;
                dbObjSort.TypeName = dbObj.GetType().Name;
                bfSorts.Add(dbObjSort);
                if (dbObj is BlockReference)
                {
                    var br = dbObj as BlockReference;
                    var childDbObjs = GetBlockRefObjSort(br);
                    if (childDbObjs != null)
                    {
                        bfSorts.AddRange(childDbObjs);
                    }
                }
            }
            return bfSorts;
        }
    }
    /// <summary>
    /// 炸开后的物体顺序
    /// </summary>
    public class DBObjectSort
    {
        public DBObject DbObj
        {
            get;
            set;
        }

        public string BlockName { get; set; } = "";

        public int index { get; set; }
        public string TypeName { get; set; } = "";
    }
    /// <summary>
    /// 块中的物体结构顺序
    /// </summary>
    public class BlockRefObjSort
    {
        public ObjectId Id { get; set; } = ObjectId.Null;

        public string BlockName { get; set; } = "";

        public int index { get; set; }
        public string TypeName { get; set; } = "";
        /// <summary>
        /// 从块炸开后的表格中来获取
        /// </summary>
        public DBObjectCollection DbObjs { get; set; } = new DBObjectCollection();
    }

}
