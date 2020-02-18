using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThEssential.Align
{
    /// <summary>
    /// 对齐方式
    /// </summary>
    public enum AlignMode
    {
        /// <summary>
        /// 向下对齐
        /// </summary>
        XFont = 0x1,
        /// <summary>
        /// 水平居中
        /// </summary>
        XCenter = 0x2,
        /// <summary>
        /// 向上对齐
        /// </summary>
        XBack = 0x3,
        /// <summary>
        /// 向左对齐
        /// </summary>
        YLeft = 0x4,
        /// <summary>
        /// 垂直居中
        /// </summary>
        YCenter = 0x5,
        /// <summary>
        /// 向右对齐
        /// </summary>
        YRight = 0x6
    }
}
