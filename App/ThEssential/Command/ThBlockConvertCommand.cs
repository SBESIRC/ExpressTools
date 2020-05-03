using System;
using AcHelper;
using Linq2Acad;
using AcHelper.Commands;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThEssential.Command
{
    public class ThBlockConvertCommand : IAcadCommand, IDisposable
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Execute()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                // 在当前图纸中选择一个区域
                PromptSelectionOptions options = new PromptSelectionOptions()
                {
                    AllowDuplicates = false,
                    RejectObjectsOnLockedLayers = true,
                };
                var entSelected = Active.Editor.GetSelection(options);
                if (entSelected.Status != PromptStatus.OK)
                {
                    return;
                };
                var objs = new ObjectIdCollection(entSelected.Value.GetObjectIds());

                // 过滤所选的对象
                // 需要获取这些对象：
                //      1. 在Xref中的块定义
               
                // 遍历每一个XRef的Database，在其中寻找在选择框线内特定的块引用
                // 需要获取块引用的这些信息：
                //      1. 转换矩阵
                //      2. 动态块属性
                //      3. 块属性

                // 通过查找映射表，获取映射后的块信息

                // 根据获取后的块信息，在当前图纸中创建新的块应用
            }
        }
    }
}
