using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ThColumnInfo.View
{
    class ProtectThicknessRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            double protectThickness;
            if (!double.TryParse(value.ToString(),out protectThickness))
            {
                return new ValidationResult(false, "无效的数据格式，只允许输入数字!");
            }
            if(protectThickness<0)
            {
                return new ValidationResult(false, "保护层厚度值要大于等于零!");
            }
            return ValidationResult.ValidResult;
        }
    }
    class FloorCountRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            int floorCount;
            if (!int.TryParse(value.ToString(), out floorCount))
            {
                return new ValidationResult(false, "无效的数据格式，只允许输入整数!");
            }
            if (floorCount < 0)
            {
                return new ValidationResult(false, "保护层厚度值要大于等于零!");
            }
            return ValidationResult.ValidResult;
        }
    }
    class EnlargeTimesRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            double enlargeTimes;
            if (!double.TryParse(value.ToString(), out enlargeTimes))
            {
                return new ValidationResult(false, "无效的数据格式，只允许输入正数!");
            }
            if (enlargeTimes < 0)
            {
                return new ValidationResult(false, "放大倍数要大于零!");
            }
            return ValidationResult.ValidResult;
        }
    }
    class NumberRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            double number=0.0;
            if (!double.TryParse(value.ToString(), out number))
            {
                return new ValidationResult(false, "无效的数据格式，只允许输入数字!");
            }
            return ValidationResult.ValidResult;
        }
    }
}
