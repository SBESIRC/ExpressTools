using System;
using System.Linq;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThEssential.BlockConvert
{
    public class ThBlockConvertManager :IDisposable
    {
        /// <summary>
        /// 块转换的映射规则
        /// </summary>
        public List<ThBlockConvertRule> Rules { get; set; }

        /// <summary>
        /// 从数据库中读取数据创建对象
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        public static ThBlockConvertManager CreateManager(Database database)
        {
            return new ThBlockConvertManager()
            {
                Rules = database.Rules(),
            };
        }

        public void Dispose()
        {
            //
        }

        /// <summary>
        /// 根据源块引用，获取转换后的块信息
        /// </summary>
        /// <param name="blkRef"></param>
        /// <returns></returns>
        public ThBlockConvertBlock TransformRule(string block)
        {
            var rule = Rules.First(o =>
                (string)o.Transformation.Item1.Attributes[ThBConvertCommon.BLOCK_MAP_ATTRIBUTES_BLOCK] == block);
            return rule?.Transformation.Item2;
        }
    }
}
