using Autodesk.AutoCAD.DatabaseServices;
using Linq2Acad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThStructure.BeamInfo.Utils
{
    public class ThStructureUtils
    {
        public static void AddToDatabase(List<Entity> ents)
        {
            using (var db = AcadDatabase.Active())
            {
                ents.ForEach(i=> db.ModelSpace.Add(i));
            }
        }
        public static List<Entity> ExplodeAllXRef(bool keepUnvisible=false)
        {
            var resEntityLst = new List<Entity>();
            // 本图纸数据块处理
            using (var db = AcadDatabase.Active())
            {
                var blockRefs = db.CurrentSpace.OfType<BlockReference>().Where(p => p.Visible).ToList();
                if (blockRefs.Count == 0)
                {
                    return resEntityLst;
                }
                for(int i=0;i<blockRefs.Count;i++)
                {
                    var layerId = blockRefs[i].LayerId;
                    if (layerId == null || !layerId.IsValid)
                    {
                        blockRefs.RemoveAt(i);
                        i = i - 1;
                    }
                    LayerTableRecord layerTableRecord = db.Element<LayerTableRecord>(layerId);
                    if (layerTableRecord.IsOff && layerTableRecord.IsFrozen)
                    {
                        blockRefs.RemoveAt(i);
                        i = i - 1;
                    }
                    BlockTableRecord btr = db.Element<BlockTableRecord>(blockRefs[i].Id);
                    if(!btr.IsFromExternalReference)
                    {
                        blockRefs.RemoveAt(i);
                        i = i - 1;
                    }
                }
                blockRefs.ForEach(i => resEntityLst.AddRange(Explode(db, i, keepUnvisible)));
            }
            return resEntityLst;
        }
        /// <summary>
        /// 炸块
        /// </summary>
        /// <param name="br">块</param>
        /// <param name="keepUnvisibleEnts">保留隐藏的物体</param>
        /// <returns></returns>
        public static List<Entity> Explode(AcadDatabase db, BlockReference br, bool keepUnVisible = true)
        {
            List<Entity> entities = new List<Entity>();
            if(br==null || br.IsErased)
            {
                return entities;
            }
            var btr = db.Element<BlockTableRecord>(br.BlockTableRecord, true) as BlockTableRecord;
            bool go = true;
            if (btr.IsFromExternalReference && btr.IsFromOverlayReference)
            {
                // 暂时不考虑unresolved的情况
                var xrefDatatbase = btr.GetXrefDatabase(false);
                if (xrefDatatbase == null)
                {
                    go = false;
                }
            }
            if (go)
            {
                DBObjectCollection collection = new DBObjectCollection();
                br.Explode(collection);
                foreach (DBObject obj in collection)
                {
                    if (obj is BlockReference)
                    {
                        var newBr = obj as BlockReference;
                        if (!keepUnVisible && newBr.Visible == false)
                        {
                            continue;
                        }
                        var childEnts = Explode(db,newBr, keepUnVisible);
                        if (childEnts != null)
                        {
                            entities.AddRange(childEnts);
                        }
                    }
                    else if (obj is Entity)
                    {
                        Entity ent = obj as Entity;
                        if (!keepUnVisible && ent.Visible == false)
                        {
                            continue;
                        }
                        entities.Add(obj as Entity);
                    }
                }
            }
            return entities;
        }
        public static List<Entity> FilterByLayers(List<Entity> ents,List<string> layerNames,bool fullMatch=false)
        {
            List<Entity> filterEnts = new List<Entity>();
            layerNames = layerNames.Select(i => i.ToUpper()).ToList();
            if (fullMatch)
            {
                filterEnts = ents.Where(i => layerNames.IndexOf(i.Layer.ToUpper()) >= 0).Select(i=>i).ToList();
            }
            else
            {
                filterEnts =ents.Where(i =>
                {
                   return ((Func<Entity, bool>)((ent) =>
                     {
                         bool contains = false;
                         foreach(string layerName in layerNames)
                         {
                             int index = ent.Layer.LastIndexOf(layerName);
                             if (index >= 0 && (index + layerName.Length) == i.Layer.Length)
                             {
                                 contains = true;
                                 break;
                             }
                         }
                         return contains;
                     }))(i);  
                }).Select(i => i).ToList();
            }
            return filterEnts;
        }
    }
}
