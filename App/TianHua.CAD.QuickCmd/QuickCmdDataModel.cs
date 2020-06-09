using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TianHua.CAD.QuickCmd
{
    public class QuickCmdDataModel
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


        /// <summary>
        /// 替换快捷键
        /// </summary>
        public string ReplaceKye { get; set; }

        /// <summary>
        /// 替换命令
        /// </summary>
        public string ReplaceCmd { get; set; }

        /// <summary>
        /// 原文本
        /// </summary>
        public string OldText { get; set; }

        /// <summary>
        /// 0:默认 1:新增 2:修改 
        /// </summary>
        public int Statu { get; set; }

    }
}
