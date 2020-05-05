using System;
using Linq2Acad;
using DotNetARX;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using GeometryExtensions;

namespace ThEssential.BlockConvert
{
    public static class ThBlockConvertDbExtension
    {
        /// <summary>
        /// 提取图纸中的块引用
        /// </summary>
        /// <param name="database"></param>
        /// <param name="blkRef"></param>
        /// <returns></returns>
        public static ThBConvertBlockReference GetBlockReference(this Database database, ObjectId blkRef)
        {
            return new ThBConvertBlockReference(blkRef);
        }

        /// <summary>
        /// 提取图纸中某个范围内所有的特点块的引用
        /// </summary>
        /// <param name="database"></param>
        /// <param name="block"></param>
        /// <param name="extents"></param>
        /// <returns></returns>
        public static ObjectIdCollection GetBlockReferences(this Database database, 
            ThBlockConvertBlock block, Extents3d extents)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var objs = new ObjectIdCollection();
                var name = (string)block.Attributes[ThBConvertCommon.BLOCK_MAP_ATTRIBUTES_BLOCK];
                var layer = (string)block.Attributes[ThBConvertCommon.BLOCK_MAP_ATTRIBUTES_LAYER];
                var blkRefs = acadDatabase.ModelSpace
                    .OfType<BlockReference>()
                    .Where(o => o.GetEffectiveName() == name && o.Layer == layer);
                foreach(var blkRef in blkRefs)
                {
                    // 这里用一个“简单粗暴”的方法判断块引用是否在一个范围内：
                    //  计算块引用外包框的中心点是否落在这个范围内
                    if (blkRef.GeometricExtents.CenterPoint().IsInside(extents))
                    {
                        objs.Add(blkRef.ObjectId);
                    }
                }
                return objs;
            }
        }

        /// <summary>
        /// 根据块应用，在当前图纸中创建转换后的块引用
        /// </summary>
        /// <param name="database"></param>
        /// <param name="blk"></param>
        /// <returns></returns>
        public static void CreateTransformedCopy(this ObjectId blkRef)
        {
            // 根据块引用的“块名”，匹配转换后的块定义的信息
            var blockReference = blkRef.Database.GetBlockReference(blkRef);
            var transformedBlock = ThBlockConvertManager.Instance.TransformRule(blockReference.EffectiveName);
            if (transformedBlock == null)
            {
                return;
            }

            try
            {
                // 在源块引用位置插入新的块引用
                using (AcadDatabase acadDatabase = AcadDatabase.Active())
                {
                    // 在当前图纸中查找是否存在新的块定义
                    // 若不存在，则插入新的块定义；
                    // 若存在，则保持现有的块定义
                    var name = (string)transformedBlock.Attributes[ThBConvertCommon.BLOCK_MAP_ATTRIBUTES_BLOCK];
                    var layer = (string)transformedBlock.Attributes[ThBConvertCommon.BLOCK_MAP_ATTRIBUTES_LAYER];
                    using (var sourceDb = AcadDatabase.Use(ThBlockConvertManager.Instance.Database))
                    {
                        var block = sourceDb.Blocks.ElementOrDefault(name);
                        var result = acadDatabase.Blocks.Import(block, false);
                    }

                    // 插入新的块引用
                    var objId = acadDatabase.ModelSpace.ObjectId.InsertBlockReference(
                        layer,
                        name,
                        Point3d.Origin,
                        new Scale3d(1.0),
                        0.0);

                    // 将新插入的块引用调整到源块引用所在的位置
                    acadDatabase.Element<BlockReference>(objId, true).TransformBy(blockReference.BlockTransform);

                    // 将源块引用的属性“刷”到新的块引用
                    objId.MatchProperties(blockReference);
                }
            }
            catch
            {
                return;
            }
        }

        /// <summary>
        /// 将源块引用的属性“刷”到新的块引用
        /// </summary>
        /// <param name="blkRef"></param>
        /// <param name="source"></param>
        private static void MatchProperties(this ObjectId blkRef, ThBConvertBlockReference source)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(blkRef.Database))
            {
                using (var objs = new ObjectIdCollection())
                {
                    // 打开块引用
                    var blockReference = acadDatabase.Element<BlockReference>(blkRef, true);

                    // 将块引用炸开，获取炸开后的表格对象
                    ObjectEventHandler handler = (s, e) =>
                    {
                        if (e.DBObject is Table table)
                        {
                            objs.Add(e.DBObject.ObjectId);
                        }
                    };
                    acadDatabase.Database.ObjectAppended += handler;
                    blockReference.ExplodeToOwnerSpace();
                    acadDatabase.Database.ObjectAppended -= handler;

                    // 将源块引用的属性填充在表格中
                    if (objs.Count == 1)
                    {
                        objs[0].FillProperties(source);
                    }

                    // 删除块引用
                    blockReference.Erase();
                }
            }
        }

        /// <summary>
        /// 将源块引用的属性“填充”到表格中
        /// </summary>
        /// <param name="tableId"></param>
        /// <param name="source"></param>
        private static void FillProperties(this ObjectId tableId, ThBConvertBlockReference source)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(tableId.Database))
            {
                int row = 0;
                int column = 0;
                var table = acadDatabase.Element<Table>(tableId, true);

                // 负载编号：“设备符号"&"-"&"楼层-编号”
                table.Cells[row++, column].Value = ThBConvertUtils.LoadSN(source);

                // 电量：“电量”
                table.Cells[row++, column].Value = ThBConvertUtils.PowerQuantity(source);

                // 负载用途：“负载用途”
                table.Cells[row++, column].Value = ThBConvertUtils.LoadUsage(source);
            }
        }
    }
}