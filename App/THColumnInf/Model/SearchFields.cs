using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace THColumnInfo
{
    public class SearchFields: CNotifyPropertyChange
    {
        private string _columnRangeLayerName = "";
        /// <summary>
        /// 柱子外轮廓线层名
        /// </summary>
        public string ColumnRangeLayerName
        {
            get
            {
                return _columnRangeLayerName;
            }
            set
            {
                _columnRangeLayerName = value;
                this.NotifyPropertyChange("ColumnRangeLayerName");
            }
        }
        private string _zhuJiZhongMarkLayerName = "";
        /// <summary>
        /// 柱集中标注层名
        /// </summary>
        public string ZhuJiZhongMarkLayerName
        {
            get
            {
                return _zhuJiZhongMarkLayerName;
            }
            set
            {
                _zhuJiZhongMarkLayerName = value;
                this.NotifyPropertyChange("ZhuJiZhongMarkLayerName");
            }
        }
        private string _zhuGuJingLayerName = "";
        /// <summary>
        /// 柱箍筋层名
        /// </summary>
        public string ZhuGuJingLayerName
        {
            get
            {
                return _zhuGuJingLayerName;
            }
            set
            {
                _zhuGuJingLayerName = value;
                this.NotifyPropertyChange("ZhuGuJingLayerName");
            }
        }
        private string _zhuMarkLeaderLayerName = "";
        /// <summary>
        /// 柱标注引线层名
        /// </summary>
        public string ZhuMarkLeaderLayerName
        {
            get
            {
                return _zhuMarkLeaderLayerName;
            }
            set
            {
                _zhuMarkLeaderLayerName = value;
                this.NotifyPropertyChange("ZhuMarkLeaderLayerName");
            }
        }
        private string _zhuYuanWeiMarkLayerName = "";
        /// <summary>
        /// 柱原位标注层名
        /// </summary>
        public string ZhuYuanWeiMarkLayerName
        {
            get
            {
                return _zhuYuanWeiMarkLayerName;
            }
            set
            {
                _zhuYuanWeiMarkLayerName = value;
                this.NotifyPropertyChange("ZhuYuanWeiMarkLayerName");
            }
        }
        private string _zhuSizeMark = "";
        /// <summary>
        /// 柱尺寸标注
        /// </summary>
        public string ZhuSizeMark
        {
            get
            {
                return _zhuSizeMark;
            }
            set
            {
                _zhuSizeMark = value;
                this.NotifyPropertyChange("ZhuSizeMark");
            }
        }

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
