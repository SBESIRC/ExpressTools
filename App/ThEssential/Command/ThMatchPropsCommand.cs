using System;
using AcHelper.Commands;
using ThEssential.MatchProps;
using Linq2Acad;
using AcHelper;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThEssential.Command
{
    public class ThMatchPropsCommand : IAcadCommand, IDisposable
    {
        public void Dispose()
        {
            //
        }

        public void Execute()
        {
            //
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                PromptSelectionOptions options = new PromptSelectionOptions()
                {
                    AllowDuplicates = false,
                    RejectObjectsOnLockedLayers = true,
                    MessageForAdding = "\n请选择要修改的对象"
                };
                var entModified = Active.Editor.GetSelection(options);
                if (entModified.Status != PromptStatus.OK)
                {
                    return;
                };

                PromptEntityResult result = Active.Editor.GetEntity("\n请选择源对象");
                if (result.Status != PromptStatus.OK)
                {
                    return;
                }
                Entity sourceEntity = acadDatabase.Element<Entity>(result.ObjectId);

                MarchPropertyVM marchPropertyVM = new MarchPropertyVM();
                MarchProperty marchProperty = new MarchProperty();
                marchPropertyVM.Owner = marchProperty;
                marchProperty.DataContext = marchPropertyVM;
                marchProperty.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                marchProperty.ShowDialog();

                // 执行操作
                foreach (var objId in entModified.Value.GetObjectIds())
                {
                    var destEntity = acadDatabase.Element<Entity>(objId, true);
                    ThMatchPropsEntityExtension.MatchProps(sourceEntity, destEntity, marchPropertyVM.MarchPropSet);
                    if (marchPropertyVM.MarchPropSet.TextContentOp)
                    {
                        ThMatchPropsEntityExtension.MarchTextContentProperty(sourceEntity, destEntity);
                    }
                    if (marchPropertyVM.MarchPropSet.TextSizeOp)
                    {
                        ThMatchPropsEntityExtension.MarchTextSizeProperty(sourceEntity, destEntity);
                    }
                    if (marchPropertyVM.MarchPropSet.TextDirectionOp)
                    {
                        ThMatchPropsEntityExtension.MarchTextDirectionProperty(sourceEntity, destEntity);
                    }
                }
            }
        }
    }
}
