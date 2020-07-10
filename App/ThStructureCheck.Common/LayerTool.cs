using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThStructureCheck.Common
{
    public static class LayerTool
    {
        /// <summary>
        /// 创建图层
        /// </summary>
        /// <param name="layerName"></param>
        /// <returns></returns>
        public static ObjectId CreateLayer(string layerName)
        {
            ObjectId layerId = ObjectId.Null;
            if (string.IsNullOrEmpty(layerName))
            {
                return layerId;
            }
            Document doc =  CadTool.GetMdiActiveDocument();
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                LayerTable lt = trans.GetObject(doc.Database.LayerTableId, OpenMode.ForRead) as LayerTable;
                if (!lt.Has(layerName))
                {
                    lt.UpgradeOpen();
                    LayerTableRecord ltr = new LayerTableRecord();
                    ltr.Name = layerName;
                    layerId = lt.Add(ltr);
                    trans.AddNewlyCreatedDBObject(ltr, true);
                    lt.DowngradeOpen();
                }
                else
                {
                    layerId = lt[layerName];
                }
                trans.Commit();
            }
            return layerId;
        }
        /// <summary>
        /// 设置图层关闭
        /// </summary>
        /// <param name="layerName"></param>
        /// <param name="isOff"></param>
        public static void SetLayerOff(string layerName,bool isOff)
        {
            Document doc = CadTool.GetMdiActiveDocument();
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                LayerTable lt = trans.GetObject(doc.Database.LayerTableId, OpenMode.ForRead) as LayerTable;
                if (lt.Has(layerName))
                {
                    LayerTableRecord ltr =trans.GetObject(lt[layerName],OpenMode.ForRead) as LayerTableRecord;
                    ltr.UpgradeOpen();
                    ltr.IsOff = isOff;
                    ltr.DowngradeOpen();
                }
                trans.Commit();
            }
        }
        /// <summary>
        /// 设置图层冻结
        /// </summary>
        /// <param name="layerName"></param>
        /// <param name="isOff"></param>
        public static void SetLayerFrozen(string layerName, bool isFrozen)
        {
            Document doc = CadTool.GetMdiActiveDocument();
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                LayerTable lt = trans.GetObject(doc.Database.LayerTableId, OpenMode.ForRead) as LayerTable;
                if (lt.Has(layerName))
                {
                    LayerTableRecord ltr = trans.GetObject(lt[layerName], OpenMode.ForRead) as LayerTableRecord;
                    ltr.UpgradeOpen();
                    ltr.IsFrozen = isFrozen;
                    ltr.DowngradeOpen();
                }
                trans.Commit();
            }
        }
        /// <summary>
        /// 设置图层隐藏
        /// </summary>
        /// <param name="layerName"></param>
        /// <param name="isHidden"></param>
        public static void SetLayerHidden(string layerName, bool isHidden)
        {
            Document doc = CadTool.GetMdiActiveDocument();
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                LayerTable lt = trans.GetObject(doc.Database.LayerTableId, OpenMode.ForRead) as LayerTable;
                if (lt.Has(layerName))
                {
                    LayerTableRecord ltr = trans.GetObject(lt[layerName], OpenMode.ForRead) as LayerTableRecord;
                    ltr.UpgradeOpen();
                    ltr.IsHidden = isHidden;
                    ltr.DowngradeOpen();
                }
                trans.Commit();
            }
        }
        /// <summary>
        /// 设置图层锁定
        /// </summary>
        /// <param name="layerName"></param>
        /// <param name="isLocked"></param>
        public static void SetLayerLocked(string layerName, bool isLocked)
        {
            Document doc = CadTool.GetMdiActiveDocument();
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                LayerTable lt = trans.GetObject(doc.Database.LayerTableId, OpenMode.ForRead) as LayerTable;
                if (lt.Has(layerName))
                {
                    LayerTableRecord ltr = trans.GetObject(lt[layerName], OpenMode.ForRead) as LayerTableRecord;
                    ltr.UpgradeOpen();
                    ltr.IsLocked = isLocked;
                    ltr.DowngradeOpen();
                }
                trans.Commit();
            }
        }
        /// <summary>
        /// 设置图层可打印
        /// </summary>
        /// <param name="layerName"></param>
        /// <param name="isPlottable"></param>
        public static void SetLayerPlottable(string layerName, bool isPlottable)
        {
            Document doc = CadTool.GetMdiActiveDocument();
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                LayerTable lt = trans.GetObject(doc.Database.LayerTableId, OpenMode.ForRead) as LayerTable;
                if (lt.Has(layerName))
                {
                    LayerTableRecord ltr = trans.GetObject(lt[layerName], OpenMode.ForRead) as LayerTableRecord;
                    ltr.UpgradeOpen();
                    ltr.IsPlottable = isPlottable;
                    ltr.DowngradeOpen();
                }
                trans.Commit();
            }
        }
        /// <summary>
        /// 设置图层颜色
        /// </summary>
        /// <param name="layerName"></param>
        /// <param name="colorIndex"></param>
        public static void SetLayerColorIndex(string layerName,short colorIndex)
        {
            Document doc = CadTool.GetMdiActiveDocument();
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                LayerTable lt = trans.GetObject(doc.Database.LayerTableId, OpenMode.ForRead) as LayerTable;
                if (lt.Has(layerName))
                {
                    LayerTableRecord ltr = trans.GetObject(lt[layerName], OpenMode.ForRead) as LayerTableRecord;
                    ltr.UpgradeOpen();
                    ltr.Color = Autodesk.AutoCAD.Colors.Color.FromColorIndex(
                        Autodesk.AutoCAD.Colors.ColorMethod.ByAci,colorIndex);
                    ltr.DowngradeOpen();
                }
                trans.Commit();
            }
        }
        /// <summary>
        /// 设置实体图层
        /// </summary>
        /// <param name="ent"></param>
        /// <param name="layer"></param>
        public static void SetEntityLayer(this Entity ent ,string layer)
        {
            if(ent==null || string.IsNullOrEmpty(layer))
            {
                return;
            }
            ent.UpgradeOpen();
            ent.Layer = layer;
            ent.DowngradeOpen();
        }
        /// <summary>
        /// 设置实体图层
        /// </summary>
        /// <param name="objId"></param>
        /// <param name="layer"></param>
        public static void SetEntityLayer(this ObjectId objId, string layer)
        {
            Document doc = CadTool.GetMdiActiveDocument();
            using (Transaction trans=doc.TransactionManager.StartTransaction())
            {
                DBObject dbObj = trans.GetObject(objId,OpenMode.ForRead);
                if(dbObj is Entity ent)
                {
                    SetEntityLayer(ent, layer);
                }
                trans.Commit();
            }
        }
    }
}
