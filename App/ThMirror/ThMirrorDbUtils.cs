using System.Linq;
using System.Collections.Generic;
using Linq2Acad;
using DotNetARX;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThMirror
{
    public static class ThMirrorDbUtils
    {
        public static bool IsBlockReferenceContainText(this ObjectId blockReferenceId)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                try
                {
                    var blockReference = acadDatabase.Element<BlockReference>(blockReferenceId);
                    return blockReference.IsBlockReferenceContainText();
                }
                catch
                {
                    // 图元不是块引用
                    return false;
                }
            }
        }

        public static bool IsBlockReferenceContainText(this BlockReference blockReference)
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

                    // 带有属性的块
                    if (blockTableRecord.HasAttributeDefinitions)
                    {
                        // 如果块引用有“非常量”属性，在Explode()的过程中会被转换成AttributeDefinition
                        // 由于暂时我们无法很好的处理AttributeDefinition，这里就不支持这种块引用
                        // CAD自带的效率工具中的“Burst”可以很好的解决这个问题，但是我们不能保证它的安装
                        //  https://adndevblog.typepad.com/autocad/2012/07/constant-and-non-constant-block-attributes.html
                        if (blockReference.AttributeCollection.Count != 0)
                        {
                            return false;
                        }

                        // 对于“常量”属性，在后面重组块定义的过程中会丢失掉
                        // 如果需要保持“常量”属性，需要先保存常量属性定义，在后面重组块定义的过程中重建他们
                    }

                    // 忽略不可“炸开”的块
                    if (!blockTableRecord.Explodable)
                    {
                        return false;
                    }

                    // 搜寻块中是否有文字图元
                    foreach (ObjectId id in blockTableRecord)
                    {
                        // 使用DxfName去获取对象类型有很好的效率优势
                        //  https://spiderinnet1.typepad.com/blog/2012/04/various-ways-to-check-object-types-in-autocad-net.html
                        if (id.ObjectClass.DxfName == ThCADCommon.DxfName_Text)
                        {
                            return true;
                        }

                        if (id.ObjectClass.DxfName == ThCADCommon.DxfName_MText)
                        {
                            return true;
                        }

                        if (id.ObjectClass.DxfName == ThCADCommon.DxfName_Leader)
                        {
                            return true;
                        }

                        if (id.ObjectClass.DxfName == ThCADCommon.DxfName_Dimension)
                        {
                            return true;
                        }

                        if (id.ObjectClass.DxfName == ThCADCommon.DxfName_Insert)
                        {
                            if (IsBlockReferenceContainText(id))
                            {
                                return true;
                            }
                        }
                    }

                    // 没有文字图元
                    return false;
                }
                catch
                {
                    // 图元不是块引用
                    return false;
                }
            }
        }

        public static string NextMirroredBlockName(this ObjectId blockId)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                try
                {
                    var blockName = acadDatabase.Element<BlockTableRecord>(blockId).Name;
                    var blockCount = acadDatabase.Blocks.Where(o => o.Name.StartsWith(blockName + "_mirror_")).Count();
                    return string.Format("{0}_mirror_{1}", blockName, blockCount + 1);
                }
                catch
                {
                    return null;
                }
            }
        }

        public static void ReplaceBlockReferenceWithMirrorData(ThMirrorData mirrorData)
        {
            // 将块引用“炸开”后经过变换后的图元按照原来的块结构重新组合成新的块。
            // 使用“Post Order Depth First Traversal”算法:
            //  https://blogs.msdn.microsoft.com/daveremy/2010/03/16/non-recursive-post-order-depth-first-traversal-in-c/
            //       a
            //   b    c    d
            // e  f  g    h i
            // dfs: efbgchida
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                foreach (var item in mirrorData.nestedBlockReferences)
                {
                    ReplaceBlockReferenceWithMirrorData(item);
                }

                // 若mirrorData是叶子节点（没有嵌套块的块引用）
                if (mirrorData.nestedBlockReferences.Count == 0)
                {
                    // 创建一个新的块定义
                    CreateBlockWithMirrorData(mirrorData);
                }
                else
                {
                    // 若mirrorData不是是叶子节点（有嵌套块的块引用）
                    //  用新的块定义替换原来的块引用
                    try
                    {
                        foreach (var item in mirrorData.nestedBlockReferences)
                        {
                            var layer = acadDatabase.Layers.Element(item.layerId).Name;
                            var blockName = acadDatabase.Element<BlockTableRecord>(item.mirroredBlockId).Name;
                            var blockReference = new BlockReference(new Point3d(0, 0, 0), item.mirroredBlockId)
                            {
                                Layer = layer
                            };
                            blockReference.TransformBy(item.nestedBlockTransform);
                            mirrorData.blockEntities.Add(blockReference);
                        }
                        mirrorData.nestedBlockReferences.Clear();
                    }
                    catch
                    {
                        //
                    }
                }
            }
        }

        public static void InsertBlockReferenceWithMirrorData(ThMirrorData mirrorData)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                if (mirrorData.mirroredBlockId.IsNull)
                {
                    CreateBlockWithMirrorData(mirrorData);
                }

                var layer = acadDatabase.Layers.Element(mirrorData.layerId).Name;
                var blockName = acadDatabase.Element<BlockTableRecord>(mirrorData.mirroredBlockId).Name;
                mirrorData.blockTransform.DecomposeBlockTransform(out Point3d insertPt, out double rotation, out Scale3d scale);
                acadDatabase.ModelSpace.ObjectId.InsertBlockReference(layer, blockName, insertPt, scale, rotation);
            }
        }

        public static void CreateBlockWithMirrorData(ThMirrorData mirrorData)
        {
            // 若ThMirrorData代表一个叶子节点（没有嵌套块的块引用）
            //  根据这些图元创建一个新的块定义
            //      1：这些图元处于WCS中
            //      2.新的块名为“原块名_mirror_<index>”，其中“index”为下一个可用的索引
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var blockName = mirrorData.blockId.NextMirroredBlockName();
                var blockEntities = new List<Entity>();

                // 图元从WCS到MCS转换
                //  https://spiderinnet1.typepad.com/blog/2014/02/autocad-net-add-entity-in-model-space-to-block.html
                var transform = mirrorData.blockTransform.Inverse();
                foreach (DBObject dbObj in mirrorData.blockEntities)
                {
                    var entity = dbObj as Entity;
                    entity.TransformBy(transform);
                    if (entity is Dimension dim)
                    {
                        dim.RecomputeDimensionBlock(true);
                    }
                    blockEntities.Add(entity);
                }

                // 创建块定义
                mirrorData.mirroredBlockId = acadDatabase.Database.AddBlockTableRecord(blockName, blockEntities);
            }
        }
    }
}
