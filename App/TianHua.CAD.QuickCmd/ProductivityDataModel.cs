using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TianHua.CAD.QuickCmd
{
    public class ProductivityDataModel
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// 快捷键
        /// </summary>
        public string ShortcutKeys { get; set; }


        /// <summary>
        /// 指令
        /// </summary>
        public string Cmd { get; set; }


        /// <summary>
        /// 备注
        /// </summary>
        public string Remarks { get; set; }


 

    }
}
