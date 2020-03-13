using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThEssential.Copy
{
    /// <summary>
    /// 复制选项
    /// </summary>
    public enum ThCopyArrayOptions
    {
        /// <summary>
        /// 阵列
        /// </summary>
        Array = 0x1,
        /// <summary>
        /// 复制
        /// </summary>
        Copy = 0x2,
        /// <summary>
        /// 均分
        /// </summary>
        Divide = 0x4,
        /// <summary>
        /// 全部
        /// </summary>
        All = Array | Copy | Divide,
    }
}
