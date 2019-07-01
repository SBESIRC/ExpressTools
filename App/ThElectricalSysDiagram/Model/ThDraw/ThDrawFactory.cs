using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThElectricalSysDiagram
{
    public class ThDrawFactory
    {
        public static ThDraw CreateThDraw(string ruleName, List<ThRelationInfo> infos)
        {
            switch (ruleName)
            {
                case "按图层转换":
                    return new ThFanDraw(infos.Cast<ThRelationFanInfo>().ToList());
                case "按图块转换":
                    return new ThBlockDraw(infos.Cast<ThRelationBlockInfo>().ToList());
                default:
                    return null;
            }
        }

    }
}
