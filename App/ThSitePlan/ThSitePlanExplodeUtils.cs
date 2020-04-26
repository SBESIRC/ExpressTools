using AcHelper;
using Linq2Acad;
using System.Linq;
using System.Collections.Generic;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using NFox.Cad.Collections;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThSitePlan
{
    public static class ThSitePlanExplodeUtils
    {
        public static void ExplodeToOwnerSpace(this Database database, ObjectId frame)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                while (true)
                {
                    var filter = OpFilter.Bulid(o => o.Dxf((int)DxfCode.Start) == ThCADCommon.DxfName_Insert);
                    PromptSelectionResult psr = Active.Editor.SelectByPolyline(
                        frame,
                        PolygonSelectionMode.Crossing,
                        filter);
                    if (psr.Status != PromptStatus.OK)
                    {
                        break;
                    }

                    var blockReferences = new List<BlockReference>();
                    foreach(var obj in psr.Value.GetObjectIds())
                    {
                        var item = acadDatabase.Element<BlockReference>(obj, true);
                        if (item.IsBlockReferenceExplodable())
                        {
                            blockReferences.Add(item);
                        }
                    }
                    if (!blockReferences.Any())
                    {
                        break;
                    }

                    foreach(BlockReference blockReference in blockReferences)
                    {
                        using (var objs = new ObjectIdCollection())
                        {
                            ObjectEventHandler handler = (s, e) =>
                            {
                                 if (e.DBObject is Entity entity)
                                 {
                                    if (entity.Layer == ThSitePlanCommon.LAYER_ZERO)
                                    {
                                        objs.Add(e.DBObject.ObjectId);
                                    }
                                 }

                            };

                            // 将块引用炸开
                            acadDatabase.Database.ObjectAppended += handler;
                            blockReference.ExplodeToOwnerSpace();
                            acadDatabase.Database.ObjectAppended -= handler;

                            // 将块引用中图层为“0”的图元调整到块引用所在的图层
                            foreach(ObjectId obj in objs)
                            {
                                acadDatabase.Element<Entity>(obj, true).Layer = blockReference.Layer;
                            }

                            // 删除块引用
                            blockReference.Erase();
                        }
                    }
                }
            }
        }
    }
}
