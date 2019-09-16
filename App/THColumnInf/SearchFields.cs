using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace THColumnInfo
{
    public class SearchFields
    {
        /// <summary>
        /// 柱子外轮廓线层名
        /// </summary>
        public string ColumnRangeLayerName { get; set; } = "";
        /// <summary>
        /// 柱集中标注层名
        /// </summary>
        public string ZhuJiZhongMarkLayerName { get; set; } = "";
        /// <summary>
        /// 柱箍筋层名
        /// </summary>
        public string ZhuGuJingLayerName { get; set; } = "";
        /// <summary>
        /// 柱标注引线层名
        /// </summary>
        public string ZhuMarkLeaderLayerName { get; set; } = "";
        /// <summary>
        /// 柱原味标注层名
        /// </summary>
        public string ZhuYuanWeiMarkLayerName { get; set; } = "";
        /// <summary>
        /// 柱尺寸标注
        /// </summary>
        public string ZhuSizeMark { get; set; } = "";
        /// <summary>
        /// 柱集中标注文字高度
        /// </summary>
        public double ZhuJiZhongMarkTextSize { get; set; }
        /// <summary>
        /// 柱原位标注文字高度
        /// </summary>
        public double ZhuYuanWeiMarkTextSize { get; set; }
    }
}
