using System;
using AcHelper;
using System.Linq;
using Linq2Acad;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;

namespace ThMirror
{
    public static class ThMirrorDbUtils
    {
        public static bool IsBlockReferenceContainText(this ObjectId objId)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                try
                {
                    var blockReference = acadDatabase.Element<BlockReference>(objId);
                    var blockTableRecord = acadDatabase.Element<BlockTableRecord>(blockReference.BlockTableRecord);

                    if (blockReference == null || blockTableRecord == null)
                    {
                        return false;
                    }

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

                    // 搜寻块中是否有文字图元
                    foreach (ObjectId id in blockTableRecord)
                    {
                        if (id.ObjectClass == RXClass.GetClass(typeof(DBText)))
                        {
                            return true;
                        }

                        if (id.ObjectClass == RXClass.GetClass(typeof(MText)))
                        {
                            return true;
                        }

                        if (id.ObjectClass == RXClass.GetClass(typeof(MLeader)))
                        {
                            return true;
                        }

                        if (id.ObjectClass == RXClass.GetClass(typeof(Dimension)))
                        {
                            return true;
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

        public static string NextMirroredBlockName(this ObjectId objId)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                try
                {
                    var blockReference = acadDatabase.Element<BlockReference>(objId);
                    var blockName = blockReference.GetEffectiveName();
                    var blockCount = acadDatabase.Blocks.Where(o => o.Name.StartsWith(blockName + "_mirror_")).Count();
                    return string.Format("{0}_mirror_{1}", blockName, blockCount + 1);
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}
