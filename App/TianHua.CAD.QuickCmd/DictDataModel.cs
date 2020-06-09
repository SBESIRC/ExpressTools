using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TianHua.CAD.QuickCmd
{
    public class DictDataModel
    {
        /// <summary>
        /// ID
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// 左手键
        /// </summary>
        public string LefthandKeys { get; set; }

        /// <summary>
        /// 命令
        /// </summary>
        public string Cmd { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remarks { get; set; }

    }
}
