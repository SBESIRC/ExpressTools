using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TianHua.AutoCAD.Parking
{
    /// <summary>
    /// 管理编号显示信息的类
    /// </summary>
    public class ThNumberInfo : ThNotifyObject
    {
        private string _startNumber;
        public string StartNumber
        {
            get
            {
                return _startNumber;
            }

            set
            {
                _startNumber = value;
                RaisePropertyChanged("StartNumber");
            }
        }
        //public string ForMat { get; set; }//数据格式
        //public string ErrorContent { get; set; }//错误提示信息
        public string Prefix { get; set; }//前缀信息
        public string Suffix { get; set; }//后缀信息


        public ThNumberInfo(string startNumber, string prefix, string suffix)
        {
            this.StartNumber = startNumber;
            //this.ForMat = forMat;
            //this.ErrorContent = errorContent;
            this.Prefix = prefix;
            this.Suffix = suffix;
        }


        /// <summary>
        /// 通过前缀、后缀和编号，确定真实的编号信息
        /// </summary>
        /// <returns></returns>
        public string NumberWithFix(string number)
        {
            return this.Prefix + number + this.Suffix;
        }

        /// <summary>
        /// 按补位的情况加指定数值
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public string NumberWithComplementaryAdd(int i)
        {
            //去除多余的0，或者只有0
            var match = Regex.Match(this.StartNumber, @"[^0]\d*|^0$");
            //成功了再继续下去
            if (match.Success)
            {
                //在真实值基础上加上对应的值，再根据最小位数，转回编号的格式，完成加法运算
                var realNumber = Convert.ToInt32(match.Value) + i;
                //根据起始数值的长度进行补0操作
                return String.Format("{0:D" + this.StartNumber.Length + "}", realNumber);
            }

            //否则返回-1表示匹配错误
            return "-1";

        }
    }
}
