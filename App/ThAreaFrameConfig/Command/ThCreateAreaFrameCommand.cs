using System;
using Linq2Acad;
using AcHelper;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using NFox.Cad.Collections;
using TianHua.AutoCAD.Utility.ExtensionTools;
using ThAreaFrameConfig.Presenter;

namespace ThAreaFrameConfig.Command
{
    public class ThCreateAreaFrameCommand : IAreaFrameCommand
    {
        private static PromptStatus Status { get; set; }
        public Func<string, ObjectId> LayerCreator { get; set; }

        public bool Success
        {
            get
            {
                return Status == PromptStatus.OK;
            }
        }

        public void Execute()
        {
            using (Active.Document.LockDocument())
            {
                using (AcadDatabase acadDatabase = AcadDatabase.Active())
                {
                    // set focus to AutoCAD
                    //  https://adndevblog.typepad.com/autocad/2013/03/use-of-windowfocus-in-autocad-2014.html
#if ACAD2012
                    Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView();
#else
                    Active.Document.Window.Focus();
#endif

                    // SelectionFilter
                    PromptSelectionOptions options = new PromptSelectionOptions()
                    {
                        AllowDuplicates = false,
                        RejectObjectsOnLockedLayers = true,
                    };
                    var filterlist = OpFilter.Bulid(o => o.Dxf((int)DxfCode.Start) == "CIRCLE,LWPOLYLINE");
                    var entSelected = Active.Editor.GetSelection(options, filterlist);
                    if ((Status = entSelected.Status) == PromptStatus.OK)
                    {
                        var name = (string)Active.Document.UserData["LayerName"];
                        foreach (var objId in entSelected.Value.GetObjectIds())
                        {
                            // 复制面积框线
                            ObjectId clonedObjId = ThEntTool.DeepClone(objId);
                            if (clonedObjId.IsNull)
                            {
                                continue;
                            }

                            // 图层管理
                            //  1. 如果指定图层不存在，创建图层
                            //  2. 如果指定图层存在，返回此图层
                            ObjectId layerId = LayerCreator(name);
                            if (layerId.IsNull)
                            {
                                continue;
                            }

                            // 将复制的放置在指定图层上
                            ThResidentialRoomDbUtil.MoveToLayer(clonedObjId, layerId);
                        }
                    }
                }
            }
        }
    }
}
