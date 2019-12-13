using AcHelper;
using Linq2Acad;
using DotNetARX;
using NFox.Cad.Collections;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThMirror
{
    public class ThMirrorApp : IExtensionApplication
    {
        public void Initialize()
        {
            ThMirrorDocumentCollectionReactor.Instance.Register();
        }

        public void Terminate()
        {
            ThMirrorDocumentCollectionReactor.Instance.UnRegister();
        }

        // 天华镜像
        //  基于下面一个事实：
        //  CAD自动的"MIRROR"命令，在MIRRTEXT=0情况下，对于文字实体，镜像后的结果正是我们需要的。
        //  实现原理：
        //  通过各种技术手段（Events，Overrule），“监听”镜像命令整个过程。
        //  对于其中包含有文字的块引用，通过将块引用“炸成”基本图元，并对这个基本图元完成相同的镜像操作。
        //  在镜像命令结束后，对这些基本图元再根据原先的块结构还原成新的块，并用新创建的块作为镜像后的结果。
        //  复杂情况：
        //      1. 多层嵌套块
        //      2. 动态块
        //      3. 外部参照(Xref）
        [CommandMethod("TIANHUACAD", "THMIR", CommandFlags.Transparent)]
        public void ThMirror()
        {
            ThMirrorEngine.Instance.Start();
            Active.Document.SendStringToExecute("_.MIRROR ", true, false, true);
        }

        // 天华Burst
        //  模拟CAD自带的效率工具中提供的Burst命令
        //  具体参考了下面这些文档：
        //      https://adndevblog.typepad.com/autocad/2015/06/programmatically-mimic-the-burst-command.html
        [CommandMethod("TIANHUACAD", "THBURST", CommandFlags.Modal)]
        public void ThBurst()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                PromptSelectionOptions options = new PromptSelectionOptions()
                {
                    AllowDuplicates = false,
                    RejectObjectsOnLockedLayers = true,
                };
                var filterlist = OpFilter.Bulid(o => o.Dxf((int)DxfCode.Start) == ThCADCommon.DxfName_Insert);
                var entSelected = Active.Editor.GetSelection(options, filterlist);
                if (entSelected.Status == PromptStatus.OK)
                {
                    foreach (var objId in entSelected.Value.GetObjectIds())
                    {
                        var blockEntities = new DBObjectCollection();
                        var blockReference = acadDatabase.Element<BlockReference>(objId);
                        blockReference.Burst(blockEntities);
                        foreach(DBObject dbObj in blockEntities)
                        {
                            if (dbObj is Entity entObj)
                            {
                                acadDatabase.ModelSpace.Add(entObj, true);
                            }
                        }
                        blockReference.UpgradeOpen();
                        blockReference.Erase();
                    }
                }
            }
        }
    }
}
