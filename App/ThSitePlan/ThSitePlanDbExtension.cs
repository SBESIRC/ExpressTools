using System.Linq;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using AcHelper;
using Linq2Acad;

namespace ThSitePlan
{
    public static class ThSitePlanDbExtension
    {
        public static ObjectIdCollection SelectAll(this Database database, string layer)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var objs = new ObjectIdCollection();
                acadDatabase.ModelSpace
                    .OfType<Entity>()
                    .Where(o => o.Layer == layer)
                    .ForEachDbObject(o => objs.Add(o.ObjectId));
                return objs;
            }
        }

        public static void CopyWithMove(this Database database,
            ObjectIdCollection objs, 
            Matrix3d displacement,
            bool bErase = false)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                using (IdMapping idMap = new IdMapping())
                {
                    acadDatabase.Database.DeepCloneObjects(objs,
                            acadDatabase.Database.CurrentSpaceId, idMap, false);
                    foreach (IdPair pair in idMap)
                    {
                        if (pair.IsPrimary && pair.IsCloned)
                        {
                            var entity = acadDatabase.Element<Entity>(pair.Value, true);
                            entity.TransformBy(displacement);
                            if (bErase)
                            {
                                acadDatabase.Element<Entity>(pair.Key, true).Erase();
                            }
                        }
                    }
                }
            }
        }

        public static void CopyWithMove(this Database database, ObjectId frame, Vector3d offset)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                PromptSelectionResult psr = Active.Editor.SelectByPolyline(
                    frame,
                    PolygonSelectionMode.Crossing,
                    null);
                if (psr.Status == PromptStatus.OK)
                {
                    var objs = new ObjectIdCollection(psr.Value.GetObjectIds());
                    acadDatabase.Database.CopyWithMove(objs, Matrix3d.Displacement(offset));

                }
            }
        }

        // 这里实现了一个特殊的"Move"操作。
        // 简单的"Move"操作，即直接通过调用Entity.TransformBy()，
        // 在这样的应用场景下，简单的"Move"操作有问题：
        //  1. 在一个框线的区域内用Editor.SelectXXX()选择对象
        //  2. 将选择的对象移动到另外一个区域
        //  3. 继续在框线内用Editor.SelectXXX()选择对象
        //  4. 将选择的对象移动到另外一个区域
        // 这里实现的特殊的"Move"操作的流程是这样的：
        //  1. 在一个区域内用Editor.SelectXXX()选择对象
        //  2. 将选择对象复制到另外一个区域
        //  3. 将原区域内的原选择对象删除
        //  4. 继续在框线内用Editor.SelectXXX()选择对象
        //  5. 将原区域内的原选择对象删除
        public static void Move(this Database database, ObjectIdCollection objs, Matrix3d displacement)
        {
            CopyWithMove(database, objs, displacement, true);
        }

        public static ObjectId Frame(this Database database, string layer)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var frame = acadDatabase.ModelSpace
                    .OfType<Polyline>()
                    .Where(o => o.Layer == layer)
                    .FirstOrDefault();
                if (frame != null)
                {
                    return frame.ObjectId;
                }

                return ObjectId.Null;
            }
        }

        public static bool IsBlockReferenceExplodable(this ObjectId blockReferenceId)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                try
                {
                    var blockReference = acadDatabase.Element<BlockReference>(blockReferenceId);
                    return blockReference.IsBlockReferenceExplodable();
                }
                catch
                {
                    // 图元不是块引用
                    return false;
                }
            }
        }

        public static bool IsBlockReferenceExplodable(this BlockReference blockReference)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                try
                {
                    // 对于动态块，BlockReference.Name返回的可能是一个匿名块的名字（*Uxxx）
                    // 对于这样的动态块，我们并不需要访问到它的“原始”动态块定义，我们只关心它“真实”的块定义
                    var blockTableRecord = acadDatabase.Blocks.Element(blockReference.Name);

                    // 暂时不支持动态块，外部参照，覆盖
                    if (blockTableRecord.IsDynamicBlock ||
                        blockTableRecord.IsFromExternalReference ||
                        blockTableRecord.IsFromOverlayReference)
                    {
                        return false;
                    }

                    // 忽略图纸空间和匿名块
                    if (blockTableRecord.IsLayout || blockTableRecord.IsAnonymous)
                    {
                        return false;
                    }

                    // 忽略不可“炸开”的块
                    if (!blockTableRecord.Explodable)
                    {
                        return false;
                    }

                    // 可以“炸”的块
                    return true;
                }
                catch
                {
                    // 图元不是块引用
                    return false;
                }
            }
        }

        public static void CreateHatchWithPolygon(this ObjectId polygon)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(polygon.Database))
            {
                try
                {
                    // Hatch外轮廓
                    var dbObjIds = new ObjectIdCollection()
                    {
                        polygon
                    };

                    // 填充面积框线
                    Hatch hatch = new Hatch();
                    ObjectId hatchId = ObjectId.Null;

                    hatchId = acadDatabase.ModelSpace.Add(hatch);
                    hatch.SetHatchPattern(HatchPatternType.PreDefined, ThSitePlanCommon.hatch_pattern);
                    hatch.ColorIndex = ThSitePlanCommon.hatch_color_index;

                    // 外圈轮廓
                    hatch.Associative = true;
                    hatch.AppendLoop(HatchLoopTypes.Default, dbObjIds);

                    // 重新生成Hatch纹理
                    hatch.EvaluateHatch(true);
                }
                catch
                {
                    // 未知错误
                }
            }
        }

        public static ObjectIdCollection CreateRegionLoops(this Database database, DBObjectCollection polygons)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var loops = new ObjectIdCollection();
                foreach (DBObject polygon in polygons)
                {
                    try
                    {
                        var loop = polygon as Polyline;
                        var items = new DBObjectCollection()
                        {
                            loop
                        };
                        // 若可以创建Region，即为一个正确的loop
                        using (var regions = Region.CreateFromCurves(items))
                        {
                            loops.Add(acadDatabase.ModelSpace.Add(loop));
                        }
                    }
                    catch
                    {
                        // 未知错误
                    }
                }
                return loops;
            }
        }

        public static ObjectIdCollection CreateRegionsWithPolygons(this Database database, DBObjectCollection polygons)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var regions = new ObjectIdCollection();
                foreach (DBObject obj in polygons)
                {
                    try
                    {
                        var items = new DBObjectCollection()
                        {
                            obj
                        };
                        foreach (Region region in Region.CreateFromCurves(items))
                        {
                            regions.Add(acadDatabase.ModelSpace.Add(region));
                        }
                    }
                    catch
                    {
                        // 未知错误
                    }
                }
                return regions;
            }
        }
    }
}
