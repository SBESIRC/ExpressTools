using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TianHua.AutoCAD.Utility.ExtensionTools
{
    public class EnumHelper
    {
        ///// <summary>
        ///// 通过枚举类型得到集合
        ///// </summary>
        ///// <param name="type">集合类型</param>
        ///// <param name="hasAll">是否包含请选择</param>
        ///// <returns></returns>
        //public static List<ListItem> GetListItemByEnum(Type type, bool hasAll = true)
        //{
        //    List<ListItem> list = new List<ListItem>();
        //    FieldInfo[] fields = type.GetFields();
        //    if (hasAll)
        //    {
        //        list.Add(new ListItem() { Value = "-1", Text = "请选择" });
        //    }

        //    for (int i = 1, count = fields.Length; i < count; i++)
        //    {
        //        list.Add(new ListItem() { Value = ((int)Enum.Parse(type, fields[i].Name)).ToString(), Text = fields[i].Name });
        //    }
        //    return list;
        //}

        #region 枚举,值,串的相互转化
        /// <summary>
        /// 枚举转字符串
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="t">枚举对象</param>
        /// <returns></returns>
        private static string Enum2Text<T>(T t)
        {
            //string enumStringOne = color.ToString(); //效率低，不推荐  
            //string enumStringTwo = Enum.GetName(typeof(Color), color);//推荐  
            return Enum.GetName(typeof(T), t);
        }

        /// <summary>
        /// 枚举转值
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="t">枚举对象</param>
        /// <returns></returns>
        private static int Enum2Value<T>(T t)
        {
            //int enumValueOne = t.GetHashCode();
            //int enumValueTwo = (int)color;
            //int enumValueThree = Convert.ToInt32(color);
            return t.GetHashCode();
        }

        /// <summary>
        /// 字符串转枚举
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="text">字符串</param>
        /// <returns></returns>
        private static T String2Enum<T>(string text)
        {
            //Color enumOne = (Color)Enum.Parse(typeof(Color), colorString);
            return (T)Enum.Parse(typeof(T), text);
        }

        /// <summary>
        /// 字符串转值
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="text">字符串</param>
        /// <returns></returns>
        public static int String2Value<T>(string text)
        {
            //int enumValueFour = (int)Enum.Parse(typeof(Color), colorString);
            return (int)Enum.Parse(typeof(T), text);
        }

        /// <summary>
        /// 值转枚举
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="value">值</param>
        /// <returns></returns>
        public static T Value2Enum<T>(int value)
        {
            //Color enumTwo = (Color)colorValue;
            //Color enumThree = (Color)Enum.ToObject(typeof(Color), colorValue);
            return (T)Enum.ToObject(typeof(T), value);
        }

        /// <summary>
        /// 值转字符串
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="value">值</param>
        /// <returns></returns>
        public static string Value2Text<T>(int value)
        {
            //string enumStringThree = Enum.GetName(typeof(Color), colorValue);
            return Enum.GetName(typeof(T), value);
        }
        #endregion
    }

    //public class ListItem
    //{
    //    /// <summary>
    //    /// 显示值
    //    /// </summary>
    //    public string Text { get; set; }
    //    /// <summary>
    //    /// 实际值
    //    /// </summary>
    //    public string Value { get; set; }
    //    /// <summary>
    //    /// 是否选中
    //    /// </summary>
    //    public bool Selected { get; set; }
    //}
}
