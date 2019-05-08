using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TianHua.AutoCAD.Utility.ExtensionTools
{
    public static class ZiFu
    {
        /// <summary>
        /// 提取某个字符前所有的字符
        /// </summary>
        /// <param name="text"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public static string Right(this string text, string a)
        {
            return text.Substring(0, text.IndexOf(a));
        }
    }
}
