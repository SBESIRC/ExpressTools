using System;
using AcHelper.Commands;
using ThEssential.MatchProps;
using Linq2Acad;
using AcHelper;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using Autodesk.AutoCAD.Colors;
using ThEssential.QSelect;

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
                MarchProperty marchProperty = new MarchProperty(marchPropertyVM);
                marchPropertyVM.Owner = marchProperty;
                marchProperty.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                marchProperty.ShowDialog();

                if(!marchPropertyVM.Executed)
                {
                    return;
                }
                Dictionary<ObjectId, Color> entColorDic = new Dictionary<ObjectId, Color>();
                foreach(ObjectId objId in entModified.Value.GetObjectIds())
                {
                    Entity currentEnt = acadDatabase.Element<Entity>(objId);
                    Color entColor = currentEnt.Color;
                    if (entColor.ColorMethod == Autodesk.AutoCAD.Colors.ColorMethod.ByLayer)
                    {
                        entColor = ThQuickSelect.GetByLayerColor(acadDatabase.Database, currentEnt);
                    }
                    entColorDic.Add(objId, entColor);
                }
                // 执行操作
                foreach (var objId in entModified.Value.GetObjectIds())
                {
                    var destEntity = acadDatabase.Element<Entity>(objId, true);
                    ThMatchPropsEntityExtension.MatchProps(sourceEntity, destEntity, marchPropertyVM.MarchPropSet);
                    if(marchPropertyVM.MarchPropSet.ColorOp==false)
                    {
                        if(marchPropertyVM.MarchPropSet.LayerOp)
                        {
                            destEntity.ColorIndex = entColorDic[objId].ColorIndex;
                        }
                    }
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
