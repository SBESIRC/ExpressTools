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
                var blkRefs = acadDatabase.ModelSpace
                    .OfType<BlockReference>()
                    .Where(o => o.GetEffectiveName() == name);
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
        /// 将源块引用的属性“刷”到新的块引用
        /// </summary>
        /// <param name="blkRef"></param>
        /// <param name="source"></param>
        public static void MatchProperties(this ObjectId blkRef, ThBConvertBlockReference source)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                using (var objs = new ObjectIdCollection())
                {
                    // 打开块引用
                    var blockReference = acadDatabase.Element<BlockReference>(blkRef, true);

                    // 将块引用炸开，获取炸开后的文字对象
                    void handler(object s, ObjectEventArgs e)
                    {
                        if (e.DBObject is DBText text)
                        {
                            objs.Add(e.DBObject.ObjectId);
                        }
                    }
                    acadDatabase.Database.ObjectAppended += handler;
                    blockReference.ExplodeToOwnerSpace();
                    acadDatabase.Database.ObjectAppended -= handler;

                    // 将源块引用的属性填充在文字中
                    objs.FillProperties(source);

                    // 删除块引用
                    blockReference.Erase();
                }
            }
        }

        /// <summary>
        /// 将源块引用的属性“填充”到表格中
        /// </summary>
        /// <param name="objs"></param>
        /// <param name="source"></param>
        private static void FillProperties(this ObjectIdCollection objs, ThBConvertBlockReference source)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                foreach(ObjectId obj in objs)
                {
                    var text = acadDatabase.Element<DBText>(obj, true);
                    if (text.TextString == ThBConvertCommon.PROPERTY_LOAD_NUMBER)
                    {
                        // 负载编号：“设备符号"&"-"&"楼层-编号”
                        text.TextString = ThBConvertUtils.LoadSN(source);
                    }
                    else if (text.TextString == ThBConvertCommon.PROPERTY_POWER_QUANTITY)
                    {
                        // 电量：“电量”
                        text.TextString = ThBConvertUtils.PowerQuantity(source);
                    }
                    else if (text.TextString == ThBConvertCommon.PROPERTY_LOAD_USAGE)
                    {
                        // 负载用途：“负载用途”
                        text.TextString = ThBConvertUtils.LoadUsage(source);
                    }
                    else
                    {
                        // 未识别的文字，忽略
                    }
                }
            }
        }
    }
}