using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo
{
    public class ColorTextInfo:CNotifyPropertyChange
    {
        private System.Windows.Media.Brush backGroundBrush;
        private string text = "";
        private object content;
        private short colorIndex = -1;

        /// <summary>
        /// 背景色
        /// </summary>
        public System.Windows.Media.Brush BackGroundBrush
        {
            get
            {
                return backGroundBrush;
            }
            set
            {
                backGroundBrush = value;
                NotifyPropertyChange("BackGroundBrush");
            }
        }
        /// <summary>
        /// 值
        /// </summary>
        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                text = value;
                NotifyPropertyChange("Text");
            }
        }
        public object Content
        {
            get
            {
                return content;
            }
            set
            {
                content = value;
                NotifyPropertyChange("Content");
            }
        }
        /// <summary>
        /// 颜色值
        /// </summary>
        public short ColorIndex
        {
            get
            {
                return colorIndex; 
            }
            set
            {
                colorIndex = value;
            }
        }
    }
}
