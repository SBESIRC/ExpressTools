using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThEssential.Distribute
{
    /// <summary>
    /// 分布方式
    /// </summary>
    public enum DistributeMode
    {
        /// <summary>
        /// 水平分布
        /// </summary>
        XGap = 0x01,
        /// <summary>
        /// 垂直分布
        /// </summary>
        YGap = 0x02
    }
}
