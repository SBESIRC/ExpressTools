using System;
using Linq2Acad;
using System.Linq;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThEssential.BlockConvert
{
    public static class ThBConvertTableUtils
    {
        /// <summary>
        ///获取图纸中的块转换映射表
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        public static List<ThBlockConvertRule> Rules(this Database database)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var table = acadDatabase.ModelSpace
                              .OfType<Table>()
                              .First(o => 
                              o.Cells[0, 0].Value != null &&
                              o.Cells[0, 0].Value.ToString() == ThBConvertCommon.BLOCK_MAP_TABLE_TITLE_NAME
                              );
                return table.Rules();
            }
        }

        /// <summary>
        /// 获取表中的块转换映射表
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        private static List<ThBlockConvertRule> Rules(this Table table)
        {
            var rules = new List<ThBlockConvertRule>();
            for (int row = 2; row < table.Rows.Count; row++)
            {
                var source = new ThBlockConvertBlock()
                {
                    Attributes = new Dictionary<string, object>(),
                };

                // 序号
                int column = 0;

                // 源块名
                column++;
                source.Attributes[ThBConvertCommon.BLOCK_MAP_ATTRIBUTES_BLOCK] = table.Cells[row, column].Value.ToString();

                // 源块
                column++;

                // 源块图层
                column++;
                source.Attributes[ThBConvertCommon.BLOCK_MAP_ATTRIBUTES_LAYER] = table.Cells[row, column].Value.ToString();

                var target = new ThBlockConvertBlock()
                {
                    Attributes = new Dictionary<string, object>(),
                };

                // 目标块名
                column++;
                target.Attributes[ThBConvertCommon.BLOCK_MAP_ATTRIBUTES_BLOCK] = table.Cells[row, column].Value.ToString();

                // 目标块
                column++;

                // 目标快图层
                column++;
                target.Attributes[ThBConvertCommon.BLOCK_MAP_ATTRIBUTES_LAYER] = table.Cells[row, column].Value.ToString();

                // 创建映射规则
                rules.Add(new ThBlockConvertRule()
                {
                    Transformation = new Tuple<ThBlockConvertBlock, ThBlockConvertBlock>(source, target),
                });
            }
            return rules;
        }
    }
}
