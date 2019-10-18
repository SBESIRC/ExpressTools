using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMirror
{
    public class ThMirrorOverruleManager
    {
        //==============SINGLETON============
        //fourth version from:
        //http://csharpindepth.com/Articles/General/Singleton.aspx
        private static readonly ThMirrorOverruleManager instance = new ThMirrorOverruleManager();
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit    
        static ThMirrorOverruleManager() { }
        internal ThMirrorOverruleManager() { }
        public static ThMirrorOverruleManager Instance { get { return instance; } }

        //-------------SINGLETON-----------------

        public void Register()
        {
            Overrule.AddOverrule(RXClass.GetClass(typeof(BlockReference)), ThMirrorObjectOverrule.Instance, true);
            Overrule.AddOverrule(RXClass.GetClass(typeof(BlockReference)), ThMirrorTransformOverrule.Instance, true);
        }

        public void UnRegister()
        {
            Overrule.RemoveOverrule(RXClass.GetClass(typeof(BlockReference)), ThMirrorObjectOverrule.Instance);
            Overrule.RemoveOverrule(RXClass.GetClass(typeof(BlockReference)), ThMirrorTransformOverrule.Instance);
        }
    }
}
