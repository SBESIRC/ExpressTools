using System;
using System.Collections.Generic;

namespace ThStructure.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class ThSComponentDbStyle : IThSComponentRenderStyle
    {
        /// <summary>
        /// 属性
        /// </summary>
        public Guid Guid { get; }
        public string Name { get; }
        public Dictionary<string, object> Values { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name"></param>
        public ThSComponentDbStyle(string name)
        {
            this.Name = name;
            this.Guid = new Guid();
        }

        public object Value(string key)
        {
            return Values[key];
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ThSComponentDbStyleManager
    {
        //==============SINGLETON============
        //fourth version from:
        //http://csharpindepth.com/Articles/General/Singleton.aspx
        private static readonly ThSComponentDbStyleManager instance = new ThSComponentDbStyleManager();
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit    
        static ThSComponentDbStyleManager() { }
        internal ThSComponentDbStyleManager() { }
        public static ThSComponentDbStyleManager Instance { get { return instance; } }
        //-------------SINGLETON-----------------

        public Dictionary<string, ThSComponentDbStyle> Styles { get; set; }
    }
}