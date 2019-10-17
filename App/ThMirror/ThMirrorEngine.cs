using System;
using System.Collections.Generic;
using AcHelper;

namespace ThMirror
{
    public class ThMirrorEngine
    {
        private List<ThMirrorData> sources;
        private List<ThMirrorData> targets;
        public List<ThMirrorData> Sources { get => sources; set => sources = value; }
        public List<ThMirrorData> Targets { get => targets; set => targets = value; }

        //==============SINGLETON============
        //fourth version from:
        //http://csharpindepth.com/Articles/General/Singleton.aspx
        private static readonly ThMirrorEngine instance = new ThMirrorEngine();
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit    
        static ThMirrorEngine() { }
        internal ThMirrorEngine()
        {
            Sources = new List<ThMirrorData>();
            Targets = new List<ThMirrorData>();
        }
        public static ThMirrorEngine Instance { get { return instance; } }

        //-------------SINGLETON-----------------

        // 启动引擎
        public void Start()
        {
            ThMirrorOverruleManager.Instance.Register();
            ThMirrorEditorReactor.Instance.SubscribeToEditor();
        }

        // 停止引擎
        public void Stop()
        {
            ThMirrorOverruleManager.Instance.UnRegister();
            ThMirrorEditorReactor.Instance.UnsubscribeToEditor();
        }
    }
}
