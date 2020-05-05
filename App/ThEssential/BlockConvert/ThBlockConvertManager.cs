using System;
using System.Linq;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThEssential.BlockConvert
{
    public class ThBlockConvertManager
    {
        //==============SINGLETON============
        //fourth version from:
        //http://csharpindepth.com/Articles/General/Singleton.aspx
        private static readonly ThBlockConvertManager instance = new ThBlockConvertManager();
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit    
        static ThBlockConvertManager() { }
        internal ThBlockConvertManager() { }
        public static ThBlockConvertManager Instance { get { return instance; } }
        //-------------SINGLETON-----------------

        /// <summary>
        /// 保存有块转换映射规则的图纸
        /// </summary>
        public Database Database { get; set; }

        /// <summary>
        /// 块转换的映射规则
        /// </summary>
        public List<ThBlockConvertRule> Rules { get; set; }

        /// <summary>
        /// 获取图纸中的映射表，并提取去映射信息
        /// </summary>
        /// <param name="database"></param>
        public void Initialize(Database database)
        {
            Database = database;
            Rules = Database.Rules();
        }

        /// <summary>
        /// 根据源块引用，获取转换后的块信息
        /// </summary>
        /// <param name="blkRef"></param>
        /// <returns></returns>
        public ThBlockConvertBlock TransformRule(string block)
        {
            var rule = Rules.First(o =>
                (string)o.Transformation.Item1.Attributes[ThBConvertCommon.BLOCK_MAP_ATTRIBUTES_BLOCK] == block);
            return rule?.Transformation.Item2;
        }
    }
}
