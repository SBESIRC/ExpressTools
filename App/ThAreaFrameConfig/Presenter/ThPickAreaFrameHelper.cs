using System;
using System.Linq;
using AcHelper;
using DotNetARX;
using Linq2Acad;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using TianHua.AutoCAD.Utility.ExtensionTools;
using System.Collections.Generic;
using Autodesk.AutoCAD.Runtime;

namespace ThAreaFrameConfig.Presenter
{
    public static class ThPickAreaFrameHelper
    {
        public static bool PickAreaFrames(this IThAreaFramePresenterCallback presenterCallback, 
            string name,
            Func<string, ObjectId> layerCreator)
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
                    //  https://adndevblog.typepad.com/autocad/2012/06/editorselectall-with-entity-and-layer-selection-filter.html
                    TypedValue[] filterlist = new TypedValue[2];
                    // 支持的面积框线类型
                    filterlist[0] = new TypedValue(0, "CIRCLE,LWPOLYLINE");
                    // 过滤掉在锁定图层的面积框线
                    var layers = new List<string>();
                    acadDatabase.Layers
                        .Where(o => o.IsLocked == false)
                        .ForEach(o => layers.Add(o.Name));
                    filterlist[1] = new TypedValue(8, string.Join(",", layers));
                    var entSelected = Active.Editor.GetSelection(new SelectionFilter(filterlist));
                    if (entSelected.Status == PromptStatus.OK)
                    {
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
                            ObjectId layerId = layerCreator(name);
                            if (layerId.IsNull)
                            {
                                continue;
                            }

                            // 将复制的放置在指定图层上
                            ThResidentialRoomDbUtil.MoveToLayer(clonedObjId, layerId);
                        }
                    }

                    return entSelected.Status == PromptStatus.OK;
                }
            }
        }

        public static bool AdjustAreaFrames(this IThAreaFramePresenterCallback presenterCallback,
            Dictionary<string, string> parameters,
            Func<string, ObjectId> layerCreator)
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
                    //  https://adndevblog.typepad.com/autocad/2012/06/editorselectall-with-entity-and-layer-selection-filter.html
                    TypedValue[] filterlist = new TypedValue[2];
                    // 支持的面积框线类型
                    filterlist[0] = new TypedValue(0, "CIRCLE,LWPOLYLINE");
                    // 过滤掉不是"住宅构件"的面积框线
                    var layers = new List<string>();
                    acadDatabase.Layers
                        .Where(o => o.Name.StartsWith(@"住宅构件"))
                        .ForEach(o => layers.Add(o.Name));
                    filterlist[1] = new TypedValue(8, string.Join(",", layers));
                    var entSelected = Active.Editor.GetSelection(new SelectionFilter(filterlist));
                    if (entSelected.Status == PromptStatus.OK)
                    {
                        foreach (var objId in entSelected.Value.GetObjectIds())
                        {
                            // 规整面积框线图层名
                            string[] tokens = acadDatabase.ModelSpace.Element(objId).Layer.Split('_');
                            tokens[4] = parameters["room_name"];
                            tokens[5] = parameters["room_identifier"];
                            tokens[6] = parameters["storey_identifier"];
                            string name = string.Join("_", tokens);


                            // 图层管理
                            //  1. 如果指定图层不存在，创建图层
                            //  2. 如果指定图层存在，返回此图层
                            ObjectId layerId = layerCreator(name);
                            if (layerId.IsNull)
                            {
                                continue;
                            }

                            // 将复制的放置在指定图层上
                            ThResidentialRoomDbUtil.MoveToLayer(objId, layerId);
                        }
                    }

                    return entSelected.Status == PromptStatus.OK;
                }
            }
        }

        public static void DeleteAreaFrame(this IThAreaFramePresenterCallback presenterCallback, IntPtr areaFrame)
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

                    acadDatabase.ModelSpace.Element(new ObjectId(areaFrame), true).Erase();
                }
            }
        }

        public static void DeleteAreaFrames(this IThAreaFramePresenterCallback presenterCallback, IntPtr[] areaFrames)
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

                    foreach (var frame in areaFrames)
                    {
                        acadDatabase.ModelSpace.Element(new ObjectId(frame), true).Erase();
                    }
                }
            }
        }

        public static void HighlightAreaFrame(this IThAreaFramePresenterCallback presenterCallback, IntPtr areaFrame)
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

                    acadDatabase.ModelSpace.Element(new ObjectId(areaFrame)).Highlight();
                }
            }
        }

        public static void HighlightAreaFrames(this IThAreaFramePresenterCallback presenterCallback, IntPtr[] areaFrames)
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

                    foreach (var frame in areaFrames)
                    {
                        acadDatabase.ModelSpace.Element(new ObjectId(frame)).Highlight();
                    }
                }
            }
        }

        public static void UnhighlightAreaFrame(this IThAreaFramePresenterCallback presenterCallback, IntPtr areaFrame)
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

                    acadDatabase.ModelSpace.Element(new ObjectId(areaFrame)).Unhighlight();
                }
            }
        }

        public static void UnhighlightAreaFrames(this IThAreaFramePresenterCallback presenterCallback, IntPtr[] areaFrames)
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

                    foreach (var frame in areaFrames)
                    {
                        acadDatabase.ModelSpace.Element(new ObjectId(frame)).Unhighlight();
                    }
                }
            }
        }

        public static void RenameAreaFrameLayer(this IThAreaFramePresenterCallback presenterCallback, string newName, IntPtr areaFrame)
        {
            using (Active.Document.LockDocument())
            {
                using (AcadDatabase acadDatabase = AcadDatabase.Active())
                {
                    var name = acadDatabase.ModelSpace.Element(new ObjectId(areaFrame)).Layer;
                    ThResidentialRoomDbUtil.RenameLayer(name, newName);
                }
            }
        }

        public static void DeleteAreaFrameLayer(this IThAreaFramePresenterCallback presenterCallback, string name)
        {
            using (Active.Document.LockDocument())
            {
                using (AcadDatabase acadDatabase = AcadDatabase.Active())
                {
                    var areaFrames = acadDatabase.ModelSpace
                        .OfType<Curve>()
                        .Where(o => o.Layer == name);
                    if (!areaFrames.Any())
                    {
                        acadDatabase.Layers.Element(name, true).Erase();
                    }
                }
            }
        }

        public static void HandleAcadException(this IThAreaFramePresenterCallback presenterCallback, System.Exception e)
        {
            Active.Editor.Write(e.ToString());
        }
    }
}
