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
        /// 重复
        /// </summary>
        Copy = 0x2,
        /// <summary>
        /// 等分
        /// </summary>
        Divide = 0x4,
        /// <summary>
        /// 全部
        /// </summary>
        All = Array | Copy | Divide,
    }
}
