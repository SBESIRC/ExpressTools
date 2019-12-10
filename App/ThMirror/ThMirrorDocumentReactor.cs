using System;
using System.Collections.Generic;
using AcHelper;
using Linq2Acad;
using DotNetARX;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThMirror
{
    public class ThMirrorDocumentReactor
    {
        //==============SINGLETON============
        //fourth version from:
        //http://csharpindepth.com/Articles/General/Singleton.aspx
        private static readonly ThMirrorDocumentReactor instance = new ThMirrorDocumentReactor();
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit    
        static ThMirrorDocumentReactor() { }
        internal ThMirrorDocumentReactor() { }
        public static ThMirrorDocumentReactor Instance { get { return instance; } }
        //-------------SINGLETON-----------------

        public void SubscribeToDoc(Document d)
        {
            d.CommandWillStart += Document_CommandWillStart;
            d.CommandEnded += Document_CommandEnded;
            d.CommandCancelled += Document_CommandCancelled;
            d.CommandFailed += Documet_CommandFailed;
        }

        public void UnsubscribeToDoc(Document d)
        {
            d.CommandWillStart -= Document_CommandWillStart;
            d.CommandEnded -= Document_CommandEnded;
            d.CommandCancelled -= Document_CommandCancelled;
        }

        private void Document_CommandWillStart(object sender, CommandEventArgs e)
        {
            if (e.GlobalCommandName == "MIRROR")
            {
                ThMirrorEngine.Instance.Start();
            }
        }

        private void Document_CommandEnded(object sender, CommandEventArgs e)
        {
            if (e.GlobalCommandName == "MIRROR")
            {
                using (AcadDatabase acadDatabase = AcadDatabase.Active())
                {
                    foreach(var item in ThMirrorEngine.Instance.Targets)
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
                }

                // 镜像结束后，“清零”所有数据
                ThMirrorEngine.Instance.Sources.Clear();
                ThMirrorEngine.Instance.Targets.Clear();

                // 停止镜像处理
                ThMirrorEngine.Instance.Stop();
                Instance.UnsubscribeToDoc(Active.Document);
            }
        }

        private void Document_CommandCancelled(object sender, CommandEventArgs e)
        {
            if (e.GlobalCommandName == "MIRROR")
            {
                ThMirrorEngine.Instance.Stop();
                Instance.UnsubscribeToDoc(Active.Document);
            }
        }

        private void Documet_CommandFailed(object sender, CommandEventArgs e)
        {
            if (e.GlobalCommandName == "MIRROR")
            {
                ThMirrorEngine.Instance.Stop();
                Instance.UnsubscribeToDoc(Active.Document);
            }
        }
    }
}
