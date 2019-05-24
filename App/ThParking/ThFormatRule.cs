using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace TianHua.AutoCAD.Parking
{
    /// <summary>
    /// 仅输入数字的验证规则
    /// </summary>
    public class ThFormatRule : ValidationRule
    {
        public string ForMat { get; set; }//数据格式
        public string ErrorContent { get; set; }//错误提示信息
        public ThFormatRule() { }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            
            //首先检查是否全是数字，如果不是，则返回错误
            if (Regex.Match(((string)value), this.ForMat).Success)
            {
                return new ValidationResult(true, null);
            }
            else
            {
                return new ValidationResult(false, this.ErrorContent);
            }

        }
    }
}
