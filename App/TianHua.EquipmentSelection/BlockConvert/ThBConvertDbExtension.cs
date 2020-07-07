using System;
using Linq2Acad;
using System.Linq;
using GeometryExtensions;
using Dreambuild.AutoCAD;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace TianHua.AutoCAD.BlockConvert
{
    public static class ThBConvertDbExtension
    {
        /// <summary>
        ///获取图纸中的块转换映射表
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        public static List<ThBConvertRule> Rules(this Database database, ConvertMode mode)
        {
            switch (mode)
            {
                case ConvertMode.STRONGCURRENT:
                    {
                        var engine = new ThBConvertRuleEngineStrongCurrent();
                        return engine.Acquire(database);
                    }
                case ConvertMode.WEAKCURRENT:
                    {
                        var engine = new ThBConvertRuleEngineWeakCurrent();
                        return engine.Acquire(database);
                    }
                default:
                    throw new NotSupportedException();
            }
        }

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
                    // GeometricExtents会得到非常非常小的-Z值，例如-1E-24
                    // 这个精度已经远远超过CAD的默认全局公差(点有1E-10的公差，向量有1E-12的公差）
                    // 但是C#中double的精度更高，这样会导致在double比较时由于精度过高，出现错误结果
                    // 为了规避由于精度不同导致的错误，这里首先对Point3d的Z值用CAD的全局公差处理
                    // 若Z值已经小于全局公差，则认为其为0
                    var center = blkRef.GetCenter();
                    var center2d = new Point3d(center.X, center.Y, 0.0);
                    if (center2d.IsEqualTo(center))
                    {
                        center = center2d;
                    }
                    if (center.IsInside(extents))
                    {
                        objs.Add(blkRef.ObjectId);
                    }
                }
                return objs;
            }
        }
    }
}