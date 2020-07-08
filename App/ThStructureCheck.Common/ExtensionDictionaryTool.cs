using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThStructureCheck.Common
{
    public static class ExtensionDictionaryTool
    {
        public static void AddExtensionDictionary(ObjectId entityId, string dicKey, List<TypedValue> tvs)
        {
            Document doc = CadTool.GetMdiActiveDocument();
            Database db = doc.Database;
            if (entityId == ObjectId.Null || string.IsNullOrEmpty(dicKey) || tvs.Count == 0)
                return;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                DBObject dbObj = tr.GetObject(entityId, OpenMode.ForRead);
                ObjectId extId = dbObj.ExtensionDictionary;
                if (extId == ObjectId.Null)
                {
                    dbObj.UpgradeOpen();
                    dbObj.CreateExtensionDictionary();
                    extId = dbObj.ExtensionDictionary;
                }
                //now we will have extId...
                DBDictionary dbExt =
                        (DBDictionary)tr.GetObject(extId, OpenMode.ForRead);
                Xrecord xRec = new Xrecord();
                ResultBuffer rb = new ResultBuffer();
                tvs.ForEach(i => rb.Add(i));
                //set the data
                xRec.Data = rb;
                //if not present add the data
                if (!dbExt.Contains(dicKey))
                {
                    dbExt.UpgradeOpen();
                    dbExt.SetAt(dicKey, xRec);
                    tr.AddNewlyCreatedDBObject(xRec, true);
                }
                else
                {
                    ObjectId xrecId = dbExt.GetAt(dicKey);
                    tr.GetObject(xrecId, OpenMode.ForWrite).Erase();
                    dbExt[dicKey] = xRec;
                    tr.AddNewlyCreatedDBObject(xRec, true);
                }
                tr.Commit();
            }
        }
        public static ObjectId AddXrecord(ObjectId objId, string searchKey, List<TypedValue> tvs)
        {
            ObjectId idXrec = ObjectId.Null;
            Document doc = CadTool.GetMdiActiveDocument();
            Database db = doc.Database;
            if (objId == ObjectId.Null || string.IsNullOrEmpty(searchKey) || tvs.Count == 0)
                return idXrec;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                DBObject dbObj = trans.GetObject(objId, OpenMode.ForRead);
                if (dbObj.ExtensionDictionary == ObjectId.Null)
                {
                    dbObj.UpgradeOpen(); //切换对象为写的状态
                    dbObj.CreateExtensionDictionary(); //为对象创建扩展字典
                    dbObj.DowngradeOpen(); //为了安全，将对象切换成读的状态
                }
                //打开对象的扩展字典
                DBDictionary dict = (DBDictionary)trans.GetObject(dbObj.ExtensionDictionary, OpenMode.ForRead);
                Xrecord xrec = new Xrecord(); //
                ResultBuffer rb = new ResultBuffer();
                tvs.ForEach(i => rb.Add(i));
                xrec.Data = rb;
                dict.UpgradeOpen();
                if (!dict.Contains(searchKey))
                {
                    dict.SetAt(searchKey, xrec);
                    trans.AddNewlyCreatedDBObject(xrec, true);
                }
                else
                {
                    ObjectId oldXRecId = dict.GetAt(searchKey);
                    trans.GetObject(oldXRecId, OpenMode.ForWrite).Erase();
                    dict[searchKey] = xrec;
                    trans.AddNewlyCreatedDBObject(xrec, true);
                }
                idXrec = dict.GetAt(searchKey);
                dict.DowngradeOpen();
                trans.Commit();
            }
            return idXrec;
        }
        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="id"></param>
        /// <param name="searchKey"></param>
        /// <returns></returns>
        public static List<TypedValue> GetXRecord(ObjectId id, string searchKey, Database db = null)
        {
            List<TypedValue> tvs = new List<TypedValue>();
            Document doc= CadTool.GetMdiActiveDocument();
            try
            {
                if (db == null)
                {
                    db = doc.Database;
                }
                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    DBObject obj = trans.GetObject(id, OpenMode.ForRead);
                    ObjectId dictId = obj.ExtensionDictionary;
                    if (dictId == ObjectId.Null)
                    {
                        return tvs;
                    }
                    DBDictionary dict = trans.GetObject(dictId, OpenMode.ForRead) as DBDictionary;
                    if (!dict.Contains(searchKey))
                    {
                        return tvs;
                    }
                    ObjectId xrecordId = dict.GetAt(searchKey);
                    if (xrecordId == ObjectId.Null)
                    {
                        return tvs;
                    }
                    Xrecord xrecord = trans.GetObject(xrecordId, OpenMode.ForRead) as Xrecord;
                    ResultBuffer rb = xrecord.Data;
                    tvs = rb.AsArray().ToList();
                    trans.Commit();
                }
            }
            catch (System.Exception ex)
            {
                Utils.WriteException(ex, "GetXRecord");
            }
            return tvs;
        }
        /// <summary>
        /// 添加有名字典
        /// </summary>
        /// <param name="db"></param>
        /// <param name="searchKey"></param>
        /// <returns></returns>
        public static ObjectId AddNamedDictionary(Database db, string searchKey)
        {
            ObjectId dictId = ObjectId.Null;
            if (string.IsNullOrEmpty(searchKey))
            {
                return dictId;
            }
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                DBDictionary dicts = trans.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForRead) as DBDictionary;
                if (!dicts.Contains(searchKey))
                {
                    DBDictionary dict = new DBDictionary();
                    dicts.UpgradeOpen();
                    dictId = dicts.SetAt(searchKey, dict);
                    dicts.DowngradeOpen();
                    trans.AddNewlyCreatedDBObject(dict, true);
                }
                else
                {
                    dictId = dicts.GetAt(searchKey);
                }
                trans.Commit();
            }
            return dictId;
        }
    }
}
