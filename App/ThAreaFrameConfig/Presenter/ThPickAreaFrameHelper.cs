using System;
using System.Linq;
using System.Diagnostics;
using AcHelper;
using DotNetARX;
using Linq2Acad;
using NFox.Cad.Collections;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using TianHua.AutoCAD.Utility.ExtensionTools;
using System.Collections.Generic;
using ThAreaFrameConfig.Model;
using Autodesk.AutoCAD.Geometry;

namespace ThAreaFrameConfig.Presenter
{
    public static class ThPickAreaFrameHelper
    {
        static readonly List<Tuple<string, HatchPatternType, int>> Hathes = new List<Tuple<string, HatchPatternType, int>>()
        {
            new Tuple<string, HatchPatternType, int>("ANSI31",   HatchPatternType.PreDefined,    500),
            new Tuple<string, HatchPatternType, int>("STARS",    HatchPatternType.PreDefined,    500),
            new Tuple<string, HatchPatternType, int>("CROSS",    HatchPatternType.PreDefined,    500),
            new Tuple<string, HatchPatternType, int>("ANSI37",   HatchPatternType.PreDefined,    800)
        };

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

                    // Cancel active selection session
                    //  https://through-the-interface.typepad.com/through_the_interface/2006/08/cancelling_an_a.html
                    Active.Editor.PostCommand("CANCELCMD");

                    // SelectionFilter
                    PromptSelectionOptions options = new PromptSelectionOptions()
                    {
                        AllowDuplicates = false,
                        RejectObjectsOnLockedLayers = true,
                    };
                    var filterlist = OpFilter.Bulid(o => o.Dxf((int)DxfCode.Start) == "CIRCLE,LWPOLYLINE");
                    var entSelected = Active.Editor.GetSelection(filterlist);
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

        public static bool PickAreaFrames(this IThFireCompartmentPresenterCallback presenterCallback,
            ThFireCompartment compartment,
            string layer, 
            string islandLayer)
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

                    // Cancel active selection session
                    //  https://through-the-interface.typepad.com/through_the_interface/2006/08/cancelling_an_a.html
                    Active.Editor.PostCommand("CANCELCMD");

                    // SelectionFilter
                    PromptSelectionOptions options = new PromptSelectionOptions()
                    {
                        AllowDuplicates = false,
                        RejectObjectsOnLockedLayers = true,
                    };
                    var filterlist = OpFilter.Bulid(
                        o => o.Dxf((int)DxfCode.Start) == "CIRCLE,LWPOLYLINE" &
                        o.Dxf((int)DxfCode.LayerName) == layer);
                    var entSelected = Active.Editor.GetSelection(filterlist);
                    if (entSelected.Status == PromptStatus.OK)
                    {
                        // 根据面积框线的质心，按照顺时针方向排序
                        Point3dCollection centroids = new Point3dCollection();
                        Dictionary<Point3d, ObjectId> frameDict = new Dictionary<Point3d, ObjectId>();
                        foreach (var objId in entSelected.Value.GetObjectIds())
                        {
                            Point3d? centroid = objId.FireCompartmentAreaFrameCentroid();
                            if (centroid != null)
                            {
                                centroids.Add(centroid.Value);
                                frameDict[centroid.Value] = objId;
                            }
                        }
                        ThPoint3dComparer comparer = new ThPoint3dComparer(centroids.CenterPoint());
                        var sortedFrameDicts = new SortedDictionary<Point3d, ObjectId>(frameDict,comparer);
                        foreach(var item in sortedFrameDicts)
                        {
                            // 创建防火分区
                            item.Value.CreateFireCompartmentAreaFrame(compartment);

                            // 下一个防火分区序号
                            ++compartment.Index;
                        }

                        return true;
                    }

                    return false;
                }
            }
        }

        public static bool PickAreaFrameLayer(this IThFireCompartmentPresenterCallback presenterCallback,
            ThFCCommerceSettings settings, 
            string key)
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

                    // Cancel active selection session
                    //  https://through-the-interface.typepad.com/through_the_interface/2006/08/cancelling_an_a.html
                    Active.Editor.PostCommand("CANCELCMD");

                    // SelectionFilter
                    PromptSelectionOptions options = new PromptSelectionOptions()
                    {
                        SingleOnly = true,
                        SinglePickInSpace = true,
                        RejectObjectsOnLockedLayers = true
                    };
                    var filterlist = OpFilter.Bulid(o => o.Dxf((int)DxfCode.Start) == "CIRCLE,LWPOLYLINE");
                    var entSelected = Active.Editor.GetSelection(options, filterlist);
                    if (entSelected.Status == PromptStatus.OK)
                    {
                        Debug.Assert(entSelected.Value.GetObjectIds().Count() == 1);
                        ObjectId objId = entSelected.Value.GetObjectIds().ElementAt(0);
                        settings.Layers[key] = acadDatabase.Element<Entity>(objId).Layer;
                        return true;
                    }

                    return false;
                }
            }
        }

        public static bool PickedFireCompartments(this IThFireCompartmentPresenterCallback presenterCallback,
            ThFCCommerceSettings settings,
            ref List<ThFireCompartment> compartments)
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

                    // Cancel active selection session
                    //  https://through-the-interface.typepad.com/through_the_interface/2006/08/cancelling_an_a.html
                    Active.Editor.PostCommand("CANCELCMD");

                    // SelectionFilter
                    var filterlist = OpFilter.Bulid(
                        o => o.Dxf((int)DxfCode.Start) == "CIRCLE,LWPOLYLINE" &
                        o.Dxf((int)DxfCode.LayerName) == settings.Layers["OUTERFRAME"] &
                        o.Dxf((int)DxfCode.ExtendedDataRegAppName) == ThCADCommon.RegAppName_AreaFrame_FireCompartment);
                    var entSelected = Active.Editor.GetSelection(filterlist);
                    if (entSelected.Status == PromptStatus.OK)
                    {
                        foreach(var objId in entSelected.Value.GetObjectIds())
                        {
                            compartments.Add(objId.CreateCommerceFireCompartment(settings.Layers["INNERFRAME"]));
                        }

                        return true;
                    }

                    return false;
                }
            }
        }

        public static bool CreateFCCommerceFills(this IThFireCompartmentPresenterCallback presenterCallback,
            List<ThFireCompartment> compartments)
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

                    // 按照"子键"，“楼层”， “编号”排序
                    compartments.Sort();

                    // 按<子键，楼层>分组，在同一组内填充
                    foreach (var group in compartments.GroupBy(o => new { o.Subkey, o.Storey }))
                    {
                        int index = 0;
                        foreach (var compartment in group)
                        {
                            foreach (var frame in compartment.Frames)
                            {
                                try
                                {
                                    // 填充面积框线
                                    Hatch hatch = new Hatch();
                                    ObjectId objId = acadDatabase.ModelSpace.Add(hatch);
                                    hatch.SetHatchPattern(Hathes[index % 4].Item2, Hathes[index % 4].Item1);
                                    hatch.PatternScale = Hathes[index % 4].Item3;
                                    hatch.Associative = true;

                                    // 图层
                                    string layer = "AE-PATN-MATE";
                                    LayerTools.AddLayer(acadDatabase.Database, layer);
                                    LayerTools.SetLayerColor(acadDatabase.Database, layer, 8);
                                    hatch.Layer = layer;

                                    // 外圈轮廓
                                    ObjectIdCollection objIdColl = new ObjectIdCollection();
                                    objIdColl.Add(new ObjectId(frame.Frame));
                                    hatch.AppendLoop(HatchLoopTypes.Outermost, objIdColl);

                                    // 孤岛
                                    objIdColl.Clear();
                                    foreach (var item in frame.IslandFrames)
                                    {
                                        objIdColl.Add(new ObjectId(item));
                                    }
                                    hatch.AppendLoop(HatchLoopTypes.Default, objIdColl);

                                    // 重新生成Hatch纹理
                                    hatch.EvaluateHatch(true);
                                }
                                catch
                                {
                                    // 由于绘图精度或者绘图不规范，面积框线处于“假闭合”的状态。
                                    // 在放大很多倍的情况下，多段线和起点和终点并不完全重合。
                                    // 在这样的情况下，对于有些孤岛，AppendLoop()会抛"InvalidInput"异常。
                                    // 这里通过捕捉异常，忽略孤岛，保证Hatch仍然可以正确创建。
                                }
                                finally
                                {
                                    index++;
                                }
                            }
                        }
                    }

                    return true;
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

        public static void MoveAreaFrameToLayer(this IThAreaFramePresenterCallback presenterCallback, 
            string newName, 
            IntPtr areaFrame,
            Func<string, ObjectId> layerCreator)
        {
            using (Active.Document.LockDocument())
            {
                using (AcadDatabase acadDatabase = AcadDatabase.Active())
                {
                    // 面积框线
                    ObjectId objId = new ObjectId(areaFrame);
                    if (objId.IsNull)
                    {
                        return;
                    }

                    // 图层管理
                    //  1. 如果指定图层不存在，创建图层
                    //  2. 如果指定图层存在，返回此图层
                    ObjectId layerId = layerCreator(newName);
                    if (layerId.IsNull)
                    {
                        return;
                    }

                    // 面积框线原图层
                    string oldName = acadDatabase.ModelSpace.Element(objId).Layer;

                    // 将面积框线放置在指定图层上
                    ThResidentialRoomDbUtil.MoveToLayer(objId, layerId);

                    // 删除原面积框线原图层
                    LayerTools.DeleteLayer(acadDatabase.Database, oldName);
                }
            }
        }

        public static void RenameAreaFrameLayer(this IThAreaFramePresenterCallback presenterCallback,
            string newName, 
            IntPtr[] areaFrames)
        {
            using (Active.Document.LockDocument())
            {
                using (AcadDatabase acadDatabase = AcadDatabase.Active())
                {
                    foreach(var areaFrame in areaFrames)
                    {
                        // 面积框线
                        ObjectId objId = new ObjectId(areaFrame);
                        if (objId.IsNull)
                        {
                            continue;
                        }

                        // 面积框线图层
                        string oldName = acadDatabase.ModelSpace.Element(objId).Layer;

                        // 重命名图层名
                        acadDatabase.Layers.Element(oldName, true).Name = newName;
                    }
                }
            }
        }

        public static void DeleteAreaFrameLayer(this IThAreaFramePresenterCallback presenterCallback, string name)
        {
            using (Active.Document.LockDocument())
            {
                using (AcadDatabase acadDatabase = AcadDatabase.Active())
                {
                    LayerTools.DeleteLayer(acadDatabase.Database, name);
                }
            }
        }

        public static void HandleAcadException(this IThAreaFramePresenterCallback presenterCallback, System.Exception e)
        {
            Active.Editor.Write(e.ToString());
        }
    }
}
