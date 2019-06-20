using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TianHua.AutoCAD.Utility.ExtensionTools
{
    public static class TextTool
    {
        public static string GetRealTextString(this Cell cell)
        {
            if (cell.Value == null)
            {
                return "";
            }
            return cell.TextString.GetRealCADTextstring();
        }


        private static string GetRealCADTextstring(this string content)
        {
            //将多行文本按“\\”进行分割
            string[] strs = content.Split(new string[] { @"\\" }, StringSplitOptions.None);
            //指定不区分大小写
            RegexOptions ignoreCase = RegexOptions.IgnoreCase;
            for (int i = 0; i < strs.Length; i++)
            {
                //删除段落缩进格式
                strs[i] = Regex.Replace(strs[i], @"\\pi(.[^;]*);", "", ignoreCase);
                //删除制表符格式
                strs[i] = Regex.Replace(strs[i], @"\\pt(.[^;]*);", "", ignoreCase);
                //删除堆迭格式
                strs[i] = Regex.Replace(strs[i], @"\\S(.[^;]*)(\^|#|\\)(.[^;]*);", @"$1$3", ignoreCase);
                strs[i] = Regex.Replace(strs[i], @"\\S(.[^;]*)(\^|#|\\);", "$1", ignoreCase);
                //删除字体、颜色、字高、字距、倾斜、字宽、对齐格式
                strs[i] = Regex.Replace(strs[i], @"(\\F|\\C|\\H|\\T|\\Q|\\W|\\A)(.[^;]*);", "", ignoreCase);
                //删除下划线、删除线格式
                strs[i] = Regex.Replace(strs[i], @"(\\L|\\O|\\l|\\o)", "", ignoreCase);
                //删除不间断空格格式
                strs[i] = Regex.Replace(strs[i], @"\\~", "", ignoreCase);
                //删除换行符格式
                strs[i] = Regex.Replace(strs[i], @"\\P", "\n", ignoreCase);
                //删除换行符格式(针对Shift+Enter格式)
                //strs[i] = Regex.Replace(strs[i], "\n", "", ignoreCase);
                //删除{}
                strs[i] = Regex.Replace(strs[i], @"({|})", "", ignoreCase);
                //替换回\\,\{,\}字符
                //strs[i] = Regex.Replace(strs[i], @"\x01", @"\", ignoreCase);
                //strs[i] = Regex.Replace(strs[i], @"\x02", @"{", ignoreCase);
                //strs[i] = Regex.Replace(strs[i], @"\x03", @"}", ignoreCase);
            }
            return string.Join("\\", strs);//将文本中的特殊字符去掉后重新连接成一个字符串

        }



    }
}
