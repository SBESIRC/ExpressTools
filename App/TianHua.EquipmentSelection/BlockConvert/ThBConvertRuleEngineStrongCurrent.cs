using System;
using Linq2Acad;
using System.Linq;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace TianHua.AutoCAD.BlockConvert
{
    public class ThBConvertRuleEngineStrongCurrent : ThBConvertRuleEngine
    {
        public override List<ThBConvertRule> Acquire(Database database)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var table = acadDatabase.ModelSpace
                              .OfType<Table>()
                              .First(o => o.Cells[0, 0].Text() == ThBConvertCommon.BLOCK_MAP_RULES_TABLE_TITLE_STRONG);
                return Rules(table);
            }
        }

        public override List<ThBConvertRule> Rules(Table table)
        {
            var rules = new List<ThBConvertRule>();
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
                rules.Add(new ThBConvertRule()
                {
                    Transformation = new Tuple<ThBlockConvertBlock, ThBlockConvertBlock>(source, target),
                });
            }
            return rules;
        }
    }
}
