using Linq2Acad;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThStructure.BeamInfo.Utils
{
    public static class ThStructureDbExtension
    {
        public static bool IsBlockReferenceOnValidLayer(this BlockReference blockReference)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                if (acadDatabase.Layers.Where(o => o.ObjectId == blockReference.LayerId && o.IsLocked).Any())
                {
                    return false;
                }

                if (acadDatabase.Layers.Where(o => o.ObjectId == blockReference.LayerId && o.IsOff).Any())
                {
                    //return false;
                }

                if (acadDatabase.Layers.Where(o => o.ObjectId == blockReference.LayerId && o.IsFrozen).Any())
                {
                    return false;
                }

                return true;
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

                    // 暂时不支持动态块
                    if (blockTableRecord.IsDynamicBlock)
                    {
                        return false;
                    }

                    // 如果是外参，忽略unresolved块引用
                    if (blockTableRecord.IsFromExternalReference || blockTableRecord.IsFromOverlayReference)
                    {
                        if ((blockTableRecord.XrefStatus & XrefStatus.Resolved) != XrefStatus.Resolved)
                        {
                            return false;
                        }
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
    }
}
