using System;
using System.Collections.Generic;

namespace ThEssential.BlockConvert
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
    public class ThBlockConvertRule
    {
        public Tuple<ThBlockConvertBlock, ThBlockConvertBlock> Transformation { get; set; }
    }
}
