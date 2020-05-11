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
                              .First(o =>o.Cells[0, 0].Text() == ThBConvertCommon.BLOCK_MAP_RULES_TABLE_TITLE_STRONG);
                return table.Rules();
            }
        }

        private static string Text(this Cell cell)
        {
            if (cell.Value == null)
            {
                return string.Empty;
            }
            using (var mText = new MText()
            {
                Contents = cell.Value.ToString(),
            })
            {
                return mText.Text;
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

                // 暖通块名
                int column = 0;
                source.Attributes[ThBConvertCommon.BLOCK_MAP_ATTRIBUTES_BLOCK] = table.Cells[row, column].Text();

                // 暖通设备块
                column++;

                var target = new ThBlockConvertBlock()
                {
                    Attributes = new Dictionary<string, object>(),
                };

                // 电气块名
                column++;
                target.Attributes[ThBConvertCommon.BLOCK_MAP_ATTRIBUTES_BLOCK] = table.Cells[row, column].Text();

                // 电气样例
                column++;

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
