using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMirror
{
    public class ThMirrorEngine
    {
        private Dictionary<ObjectId, ThMirrorData> sources;
        private Dictionary<ObjectId, ThMirrorData> targets;
        public Dictionary<ObjectId, ThMirrorData> Sources { get => sources; set => sources = value; }
        public Dictionary<ObjectId, ThMirrorData> Targets { get => targets; set => targets = value; }


        //==============SINGLETON============
        //fourth version from:
        //http://csharpindepth.com/Articles/General/Singleton.aspx
        private static readonly ThMirrorEngine instance = new ThMirrorEngine();
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit    
        static ThMirrorEngine() { }
        internal ThMirrorEngine()
        {
            Sources = new Dictionary<ObjectId, ThMirrorData>();
            Targets = new Dictionary<ObjectId, ThMirrorData>();
        }
        public static ThMirrorEngine Instance { get { return instance; } }

        //-------------SINGLETON-----------------

        // 启动引擎
        public void Start()
        {
            ThMirrorOverruleManager.Instance.Register();
        }

        // 停止引擎
        public void Stop()
        {
            ThMirrorOverruleManager.Instance.UnRegister();
        }
    }
}
