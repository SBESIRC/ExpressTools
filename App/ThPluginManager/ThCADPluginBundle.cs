using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThPluginManager
{
    /// <summary>
    /// Autoloader 现在支持这些类型：
    ///     Bundle ARX Lisp CompiledLisp Dbx .NET Cui CuiX Mnu and Dependency
    /// </summary>
    public enum Content
    {
        Lisp,
        CompiledLisp
    };

    /// <summary>
    /// CAD Autoloader bundle
    /// 这个是对位于"%APPDATA%/Autodesk/ApplicationPlugins/ThPartialCADPlugin.bundle"的一个封装
    /// </summary>
    public class ThCADPluginBundle
    {
        //==============SINGLETON============
        //fourth version from:
        //http://csharpindepth.com/Articles/General/Singleton.aspx
        private static readonly ThCADPluginBundle instance = new ThCADPluginBundle();
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit    
        static ThCADPluginBundle() { }
        internal ThCADPluginBundle() { }
        public static ThCADPluginBundle Instance { get { return instance; } }
        //-------------SINGLETON-----------------

        // bucket structure for all different plugin types
        private readonly Dictionary<Content, List<ThCADPlugin>> buckets;

        // 构造函数
        ThCADPluginBundle(string path)
        {
            buckets = new Dictionary<Content, List<ThCADPlugin>>();
        }

        public List<ThCADPlugin> Plugins(Content content)
        {
            return buckets[content];
        }

        // 安装插件
        public void InstallPlugin(ThCADPlugin plugin)
        {
            throw new NotImplementedException();
        }

        // 卸载插件
        public void UninstallPlugin(ThCADPlugin plugin)
        {
            throw new NotImplementedException();
        }

        // 禁用/启用插件
        public void EnablePlugin(ThCADPlugin plugin, bool bEnable)
        {
            throw new NotImplementedException();
        }
    }
}
