using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TianHua.AutoCAD.Utility.ExtensionTools
{
    public static class EnumTool
    {
        public static string EnumToString<T>(this T ck)
        {
            // get the field informaiton
            FieldInfo fieldInfo = ck.GetType().GetField(ck.ToString());

            // get the attributes for the enum field
            object[] attribArray = fieldInfo.GetCustomAttributes(false);

            // cast the one and only attribute to EnumDescriptionAttribute
            EnumDescriptionAttribute attrib = (EnumDescriptionAttribute)attribArray[0];

            return attrib.Description;

        }

        /// <summary>
        /// 根据Description获取枚举值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T GetValueByDescription<T>(this string description) where T : struct
        {
            Type type = typeof(T);
            foreach (var field in type.GetFields())
            {
                if (field.Name == description)
                {
                    return (T)field.GetValue(null);
                }

                var attributes = (EnumDescriptionAttribute[])field.GetCustomAttributes(typeof(EnumDescriptionAttribute), true);
                if (attributes != null && attributes.FirstOrDefault() != null)
                {
                    if (attributes.First().Description == description)
                    {
                        return (T)field.GetValue(null);
                    }
                }
            }

            throw new ArgumentException(string.Format("{0} 未能找到对应的枚举.", description), "Description");
        }

    }
}
