using AcHelper;
using Linq2Acad;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
using System;

namespace ThMirror
{
    public class ThMirrorDocumentCollectionReactor
    {
        //==============SINGLETON============
        //fourth version from:
        //http://csharpindepth.com/Articles/General/Singleton.aspx
        private static readonly ThMirrorDocumentCollectionReactor instance = new ThMirrorDocumentCollectionReactor();
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit    
        static ThMirrorDocumentCollectionReactor() { }
        internal ThMirrorDocumentCollectionReactor() { }
        public static ThMirrorDocumentCollectionReactor Instance { get { return instance; } }
        //-------------SINGLETON-----------------

        // 注册事件
        public void Register()
        {
            AcadApp.DocumentManager.DocumentLockModeChanged += DocCollEvent_DocumentLockModeChanged_Handler;
            AcadApp.DocumentManager.DocumentLockModeWillChange += DocCollEvent_DocumentLockModeWillChange_Handler;
            AcadApp.DocumentManager.DocumentLockModeChangeVetoed += DocCollEvent_DocumentLockModeChangeVetoed_Handler;
        }

        // 反注册事件
        public void UnRegister()
        {
            AcadApp.DocumentManager.DocumentLockModeChanged -= DocCollEvent_DocumentLockModeChanged_Handler;
            AcadApp.DocumentManager.DocumentLockModeWillChange -= DocCollEvent_DocumentLockModeWillChange_Handler;
            AcadApp.DocumentManager.DocumentLockModeChangeVetoed -= DocCollEvent_DocumentLockModeChangeVetoed_Handler;
        }

        private void DocCollEvent_DocumentLockModeChanged_Handler(object sender, DocumentLockModeChangedEventArgs e)
        {
            if (e.GlobalCommandName == "MIRROR")
            {
                // 在镜像命令开始前，初始化
                ThMirrorEngine.Instance.Targets.Clear();
            }
            else if (e.GlobalCommandName == "#MIRROR")
            {
                AcadApp.Idle += Application_OnIdle;
            }
        }

        private void Application_OnIdle(object sender, EventArgs e)
        {
            AcadApp.Idle -= Application_OnIdle;

            using (Active.Document.LockDocument())
            {
                using (AcadDatabase acadDatabase = AcadDatabase.Active())
                {
                    foreach (var item in ThMirrorEngine.Instance.Targets)
                    {
                        var mirrorData = item.Value;
                        var mirrorBlockReference = item.Key;

                        try
                        {
                            // 处理镜像对象
                            ThMirrorDbUtils.ReplaceBlockReferenceWithMirrorData(mirrorData);

                            // 创建新的块引用
                            ThMirrorDbUtils.InsertBlockReferenceWithMirrorData(mirrorData);

                            // 删除旧的块引用
                            acadDatabase.Element<BlockReference>(mirrorBlockReference, true).Erase();
                        }
                        catch
                        {
                            // 捕捉处理过程中的所有异常
                        }
                    };

                    // 镜像结束后，“清零”所有数据
                    ThMirrorEngine.Instance.Targets.Clear();

                    // 停止镜像处理
                    ThMirrorEngine.Instance.Stop();
                }
            }
        }

        private void DocCollEvent_DocumentLockModeWillChange_Handler(object sender, DocumentLockModeWillChangeEventArgs e)
        {
        }

        private void DocCollEvent_DocumentLockModeChangeVetoed_Handler(object sender, DocumentLockModeChangeVetoedEventArgs e)
        {
        }
    }
}
