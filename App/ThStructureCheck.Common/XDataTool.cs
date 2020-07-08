using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThStructureCheck.Common
{
    public static class XDataTool
    {
        public static void AddXData(ObjectId objectId, string regAppName, List<TypedValue> tvs)
        {
            if (objectId == ObjectId.Null || string.IsNullOrEmpty(regAppName) || tvs.Count == 0)
            {
                return;
            }
            Document doc = CadTool.GetMdiActiveDocument();
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                RegAppTable regAppTable = trans.GetObject(doc.Database.RegAppTableId, OpenMode.ForRead) as RegAppTable;
                if (!regAppTable.Has(regAppName))
                {
                    regAppTable.UpgradeOpen();
                    RegAppTableRecord regAppTableRecord = new RegAppTableRecord();
                    regAppTableRecord.Name = regAppName;
                    regAppTable.Add(regAppTableRecord);
                    trans.AddNewlyCreatedDBObject(regAppTableRecord, true);
                    regAppTable.DowngradeOpen();
                }
                DBObject dbObj = trans.GetObject(objectId, OpenMode.ForWrite);
                tvs.Insert(0, new TypedValue((int)DxfCode.ExtendedDataRegAppName, regAppName));
                ResultBuffer rb = new ResultBuffer();
                tvs.ForEach(i => rb.Add(i));
                dbObj.XData = rb;
                dbObj.DowngradeOpen();
                trans.Commit();
            }
        }
        public static List<object> GetXData(ObjectId objId, string regAppName)
        {
            List<object> values = new List<object>();
            if (objId == ObjectId.Null || string.IsNullOrEmpty(regAppName))
            {
                return values;
            }
            Document doc =CadTool.GetMdiActiveDocument();
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                DBObject dbObj = trans.GetObject(objId, OpenMode.ForRead);
                ResultBuffer rbs = dbObj.GetXDataForApplication(regAppName);
                if (rbs != null)
                {
                    foreach (var rb in rbs)
                    {
                        if (rb.TypeCode == (int)DxfCode.ExtendedDataAsciiString)
                        {
                            values.Add((string)rb.Value);
                        }
                        else if (rb.TypeCode == (int)DxfCode.ExtendedDataInteger32)
                        {
                            values.Add((int)rb.Value);
                        }
                        else if (rb.TypeCode == (int)DxfCode.ExtendedDataReal)
                        {
                            values.Add((double)rb.Value);
                        }
                        else if (rb.TypeCode == (int)DxfCode.ExtendedDataRegAppName)
                        {
                            values.Add((string)rb.Value);
                        }
                    }
                }
                trans.Commit();
            }
            return values;
        }
        public static void RemoveXData(ObjectId objectId, string regAppName)
        {
            if (objectId == ObjectId.Null || string.IsNullOrEmpty(regAppName))
            {
                return;
            }
            List<object> values = GetXData(objectId, regAppName);
            if (values.Count == 0)
            {
                return;
            }
            Document doc = CadTool.GetMdiActiveDocument();
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                DBObject dbObj = trans.GetObject(objectId, OpenMode.ForWrite);
                ResultBuffer rb = new ResultBuffer(new TypedValue(1001, regAppName));
                dbObj.XData = rb;
                dbObj.DowngradeOpen();
                trans.Commit();
            }
        }
    }
}
