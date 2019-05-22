using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TianHua.AutoCAD.Parking
{
    /// <summary>
    /// 管理编号显示信息的类
    /// </summary>
    public class ThNumberInfo
    {
        public int StartNumber { get; set; }//起始编号
        public string Prefix { get; set; }//前缀信息
        public string Suffix { get; set; }//后缀信息


        public ThNumberInfo(int startNumber, string prefix, string suffix)
        {
            this.StartNumber = startNumber;
            this.Prefix = prefix;
            this.Suffix = suffix;
        }

        /// <summary>
        /// 通过前缀、后缀和编号，确定真实的编号信息
        /// </summary>
        /// <returns></returns>
        public string NumberWithFix(int number)
        {
            return this.Prefix + number + this.Suffix;
        }
    }
}
