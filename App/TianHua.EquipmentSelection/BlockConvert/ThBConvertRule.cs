using System;
using System.Collections.Generic;

namespace TianHua.AutoCAD.BlockConvert
{
    /// <summary>
    /// 图纸中块的信息
    /// </summary>
    public class ThBlockConvertBlock
    { 
        public Dictionary<string, object> Attributes { get; set; }
    }

    /// <summary>
    /// 图纸中块的映射规则
    /// </summary>
    public class ThBConvertRule
    {
        public Tuple<ThBlockConvertBlock, ThBlockConvertBlock> Transformation { get; set; }
    }
}
