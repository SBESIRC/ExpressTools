using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Dreambuild.AutoCAD;
using Linq2Acad;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ThStructure.BeamInfo.Utils
{
    public static class ThStructureUtils
    {
        public static ObjectIdCollection AddToDatabase(List<Entity> ents)
        {
            using (var db = AcadDatabase.Active())
            {
                var objs = new ObjectIdCollection();
                ents.ForEach(o => objs.Add(db.ModelSpace.Add(o)));           
                return objs;
            }
        }
        public static List<Entity> Explode(ExplodeType explodeType, bool keepUnvisible = false)
        {
            using (var db = AcadDatabase.Active())
            {
                var resEntityLst = new List<Entity>();
                var blockRefs = db.ModelSpace
                    .OfType<BlockReference>()
                    .Where(p => p.Visible)
                    .Where(p => p.IsBlockReferenceExplodable())
                    .Where(p => p.IsBlockReferenceOnValidLayer());
                blockRefs.ForEach(i => resEntityLst.AddRange(Explode(db, i, keepUnvisible)));
                return resEntityLst;
            }
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
            if (!br.Visible)
            {
                return entities;
            }
            if (!br.IsBlockReferenceExplodable())
            {
                return entities;
            }
            if (!br.IsBlockReferenceOnValidLayer())
            {
                return entities;
            }

            try
            {
                DBObjectCollection collection = new DBObjectCollection();
                br.Explode(collection);
                foreach (Entity ent in collection)
                {
                    if (!keepUnVisible && ent.Visible == false)
                    {
                        continue;
                    }
                    if (ent is BlockReference newBr)
                    {
                        entities.AddRange(Explode(db, newBr, keepUnVisible));
                    }
                    else if (ent is Mline mline)
                    {
                        var sunEntities = new DBObjectCollection();
                        mline.Explode(sunEntities);
                        foreach (Entity subEntity in sunEntities)
                        {
                            if (subEntity is Line line)
                            {
                                Line lineEnt = new Line(line.StartPoint, line.EndPoint)
                                {
                                    Layer = mline.Layer
                                };
                                entities.Add(lineEnt);
                            }
                        }
                        sunEntities.Dispose();
                    }
                    else if (ent is DBPoint)
                    {
                        continue;
                    }else
                    {
                        entities.Add(ent);
                    }
                }
            }
            catch
            {
                // 
            }

            return entities;
        }
        private static DBObjectCollection dbObjs;
        public static DBObjectCollection ExplodeToOwnerSpace3(this BlockReference br)
        {
            dbObjs = new DBObjectCollection();
            LoopThroughInsertAndAddEntity2n3(br.BlockTransform, br);
            return dbObjs;
        }

        public static void LoopThroughInsertAndAddEntity2n3(Matrix3d mat, BlockReference br)
        {
            Transaction tr = br.Database.TransactionManager.TopTransaction;
            BlockTableRecord btr = tr.GetObject(br.BlockTableRecord, OpenMode.ForRead) as BlockTableRecord;

            foreach (ObjectId id in btr)
            {
                DBObject obj = tr.GetObject(id, OpenMode.ForRead);
                Entity ent = obj.Clone() as Entity;
                if (ent is BlockReference)
                {
                    BlockReference br1 = (BlockReference)ent;
                    LoopThroughInsertAndAddEntity2n3(br1.BlockTransform.PreMultiplyBy(mat), br1);
                }
                else
                {
                    ent.TransformBy(mat);
                    dbObjs.Add(ent);
                }
            }
        }
        public static List<Entity> FilterCurveByLayers(List<Entity> ents, List<string> layerNames, bool fullMatch = false)
        {
            List<Entity> filterEnts = new List<Entity>();
            layerNames = layerNames.Select(i => i.ToUpper()).ToList();
            if (fullMatch)
            {
                filterEnts = ents.Where(i => layerNames.IndexOf(i.Layer.ToUpper()) >= 0 && i is Curve).Select(i => i).ToList();
            }
            else
            {
                filterEnts = ents.Where(i =>
                {
                    bool containsLayer = false;
                    containsLayer = ((Func<Entity, bool>)((ent) =>
                    {
                        bool contains = false;
                        foreach (string layerName in layerNames)
                        {
                            int index = ent.Layer.ToUpper().LastIndexOf(layerName);
                            if (index >= 0 && (index + layerName.Length) == i.Layer.Length)
                            {
                                contains = true;
                                break;
                            }
                        }
                        return contains;
                    }))(i);
                    if (containsLayer && i is Curve)
                    {
                        return true;
                    }
                    return false;
                }).Select(i => i).ToList();
            }
            return filterEnts;
        }
        public static List<Entity> FilterAnnotationByLayers(List<Entity> ents, List<string> layerNames, bool fullMatch = false)
        {
            List<Entity> filterEnts = new List<Entity>();
            layerNames = layerNames.Select(i => i.ToUpper()).ToList();
            if (fullMatch)
            {
                filterEnts = ents.Where(i => layerNames.IndexOf(i.Layer.ToUpper()) >= 0 &&
                (i is DBText || i is MText || i is Dimension)).Select(i => i).ToList();
            }
            else
            {
                filterEnts = ents.Where(i =>
                {
                    bool containsLayer = ((Func<Entity, bool>)((ent) =>
                    {
                        bool contains = false;
                        foreach (string layerName in layerNames)
                        {
                            int index = ent.Layer.ToUpper().LastIndexOf(layerName);
                            if (index >= 0 && (index + layerName.Length) == i.Layer.Length)
                            {
                                contains = true;
                                break;
                            }
                        }
                        return contains;
                    }))(i);
                    if (containsLayer && (i is DBText || i is MText || i is Dimension))
                    {
                        return true;
                    }
                    return false;
                }).Select(i => i).ToList();
            }
            return filterEnts;
        }
    }
    public enum ExplodeType
    {
        All,
        /// <summary>
        /// 外部参照
        /// </summary>
        XRef,
        /// <summary>
        /// 本地块
        /// </summary>
        Local
    }
}
