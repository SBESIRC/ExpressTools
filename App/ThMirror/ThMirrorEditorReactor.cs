using System;
using AcHelper;
using Linq2Acad;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMirror
{
    public class ThMirrorEditorReactor
    {
        //==============SINGLETON============
        //fourth version from:
        //http://csharpindepth.com/Articles/General/Singleton.aspx
        private static readonly ThMirrorEditorReactor instance = new ThMirrorEditorReactor();
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit    
        static ThMirrorEditorReactor() { }
        internal ThMirrorEditorReactor() { }
        public static ThMirrorEditorReactor Instance { get { return instance; } }
        //-------------SINGLETON-----------------

        public void SubscribeToEditor()
        {
            Active.Editor.SelectionAdded += new SelectionAddedEventHandler(Editor_SelectionAdded);
            Active.Editor.SelectionRemoved += new SelectionRemovedEventHandler(Editor_SelectionRemoved);
        }

        public void UnsubscribeToEditor()
        {
            Active.Editor.SelectionAdded -= new SelectionAddedEventHandler(Editor_SelectionAdded);
            Active.Editor.SelectionRemoved -= new SelectionRemovedEventHandler(Editor_SelectionRemoved);
        }

        private static void Editor_SelectionRemoved(object sender, SelectionRemovedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void Editor_SelectionAdded(object sender, SelectionAddedEventArgs e)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                foreach (var objId in e.AddedObjects.GetObjectIds())
                {
                    // 仅处理带有文字的块
                    if (!objId.IsBlockReferenceContainText())
                    {
                        continue;
                    }
                   
                    var blockReference = acadDatabase.Element<BlockReference>(objId);
                    ThMirrorEngine.Instance.Sources.Add(new ThMirrorData(blockReference));
                }
            }
        }
    }
}
