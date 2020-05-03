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
        /// 获取图纸中的映射表，并提取去映射信息
        /// </summary>
        /// <param name="database"></param>
        public void Initialize(Database database)
        {
            //
        }
    }
}
