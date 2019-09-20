using System.Linq;
using System.Collections.Generic;
using AcHelper;
using DotNetARX;
using Linq2Acad;
using GeometryExtensions;
using Autodesk.AutoCAD.DatabaseServices;
using TianHua.AutoCAD.Utility.ExtensionTools;
using ThAreaFrameConfig.Model;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using System;
using Autodesk.AutoCAD.Colors;

namespace ThAreaFrameConfig.Presenter
{
    public static class ThFireCompartmentHelper
    {
        // 修改商业防火分区
        public static bool ModifyFireCompartment(ThFireCompartment compartment)
        {
            using (Active.Document.LockDocument())
            {
                using (AcadDatabase acadDatabase = AcadDatabase.Active())
                {
                    var textObjIds = new List<ObjectId>();
                    var bboxObjIds = new List<ObjectId>();
                    foreach (var frame in compartment.Frames)
                    {
                        ObjectId frameId = new ObjectId(frame.Frame);
                        TypedValueList valueList = frameId.GetXData(ThCADCommon.RegAppName_AreaFrame_FireCompartment);
                        if (valueList == null)
                        {
                            frameId.CreateFireCompartmentAreaFrame(compartment.Subkey, compartment.Storey, compartment.Index);
                        }
                        else
                        {
                            var handles = valueList.Where(o => o.TypeCode == (int)DxfCode.ExtendedDataHandle);
                            if (handles.Any())
                            {
                                // 从Object Handle到ObjectId
                                //  https://through-the-interface.typepad.com/through_the_interface/2007/02/getting_access_.html
                                textObjIds.Add(acadDatabase.Database.GetObjectId(false, (Handle)handles.ElementAt(0).Value, 0));
                                bboxObjIds.Add(acadDatabase.Database.GetObjectId(false, (Handle)handles.ElementAt(1).Value, 0));
                            }
                        }
                    }

                    // 修改防火分区标识文字
                    foreach (var objId in textObjIds.Distinct())
                    {
                        var text = acadDatabase.Element<MText>(objId, true);
                        text.Contents = ThFireCompartmentUtil.CommerceSerialNumber(compartment.Subkey, compartment.Storey, compartment.Index);
                    }

                    // TODO:
                    //  修改防火分区文字框线

                    return true;
                }
            }
        }

        // 删除防火分区
        public static bool DeleteFireCompartment(ThFireCompartment compartment)
        {
            using (Active.Document.LockDocument())
            {
                using (AcadDatabase acadDatabase = AcadDatabase.Active())
                {
                    List<ObjectId> textObjIds = new List<ObjectId>();
                    List<ObjectId> bboxObjIds = new List<ObjectId>();
                    foreach (var frame in compartment.Frames)
                    {
                        ObjectId frameId = new ObjectId(frame.Frame);
                        TypedValueList valueList = frameId.GetXData(ThCADCommon.RegAppName_AreaFrame_FireCompartment);
                        if (valueList == null)
                            continue;

                        var handles = valueList.Where(o => o.TypeCode == (int)DxfCode.ExtendedDataHandle);
                        if (handles.Any())
                        {
                            // 从Object Handle到ObjectId
                            //  https://through-the-interface.typepad.com/through_the_interface/2007/02/getting_access_.html
                            textObjIds.Add(acadDatabase.Database.GetObjectId(false, (Handle)handles.ElementAt(0).Value, 0));
                            bboxObjIds.Add(acadDatabase.Database.GetObjectId(false, (Handle)handles.ElementAt(1).Value, 0));
                        }

                        // 删除面积框线XData
                        frameId.RemoveXData(ThCADCommon.RegAppName_AreaFrame_FireCompartment);
                    }

                    // 删除防火分区文字
                    foreach (var objId in textObjIds.Distinct())
                    {
                        acadDatabase.ModelSpace.Element(objId, true).Erase();
                    }

                    // 删除防火分区文字框线
                    foreach (var objId in bboxObjIds.Distinct())
                    {
                        acadDatabase.ModelSpace.Element(objId, true).Erase();
                    }

                    return true;
                }
            }
        }

        // 合并商业防火分区
        public static bool MergeFireCompartment(ThFireCompartment targetCompartment, ThFireCompartment sourceCompartment, string subKey)
        {
            foreach (var frame in sourceCompartment.Frames)
            {
                targetCompartment.Frames.Add(frame);
            }

            // 删除源防火分区
            DeleteFireCompartment(sourceCompartment);

            // 修改目标防火分区
            ModifyFireCompartment(targetCompartment);

            return true;
        }

        private static bool CreateFireCompartmentAreaFrame(this ObjectId frame, UInt16 subKey, UInt16 storey, UInt16 index)
        {
            using (Active.Document.LockDocument())
            {
                using (AcadDatabase acadDatabase = AcadDatabase.Active())
                {
                    // 根据面积框线轮廓创建“区域”
                    //  https://www.keanw.com/2015/08/getting-the-centroid-of-an-autocad-region-using-net.html
                    DBObjectCollection curves = new DBObjectCollection()
                    {
                        acadDatabase.Element<Curve>(frame)
                    };
                    DBObjectCollection regions = Region.CreateFromCurves(curves);
                    Region region = regions[0] as Region;

                    // 创建防火分区文字
                    //  https://www.keanw.com/2015/08/fitting-autocad-text-into-a-selected-space-using-net.html
                    MText mText = new MText()
                    {
                        Location = region.Centroid(),
                        Contents = frame.OldIdPtr.CommerceTextContent(subKey, storey, index)
                    };
                    ObjectId textId = acadDatabase.ModelSpace.Add(mText, true);

                    // 创建防火分区文字框线
                    Polyline bbox = new Polyline()
                    {
                        Closed = true,
                    };
                    // 变换顶点顺序
                    //  (0)-----(1)     (0)-----(1)
                    //   |       |       |       |
                    //   |       |  ==>  |       |
                    //   |       |       |       |
                    //  (2)-----(3)     (3)-----(2)
                    Point3dCollection points = acadDatabase.Element<MText>(textId).GetBoundingPoints();
                    points.Swap(2, 3);
                    bbox.CreatePolyline(points);
                    ObjectId bboxId = acadDatabase.ModelSpace.Add(bbox, true);

                    // 关联面积框线和防火分区
                    TypedValueList valueList = new TypedValueList
                    {
                        { (int)DxfCode.ExtendedDataHandle, textId.Handle },
                        { (int)DxfCode.ExtendedDataHandle, bboxId.Handle }
                    };
                    frame.AddXData(ThCADCommon.RegAppName_AreaFrame_FireCompartment, valueList);

                    // 填充面积框线
                    //  https://www.keanw.com/2010/06/creating-transparent-hatches-in-autocad-using-net.html
                    Hatch hatch = new Hatch()
                    {
                        // Set our transparency to 50% (=127)
                        // Alpha value is Truncate(255 * (100-n)/100)
                        Transparency = new Transparency(127)
                    };
                    hatch.SetHatchPattern(HatchPatternType.PreDefined, "ANSI31");
                    ObjectId objId = acadDatabase.ModelSpace.Add(hatch);
                    var hat = acadDatabase.Element<Hatch>(objId, true);
                    hat.Associative = true;
                    hat.AppendLoop(HatchLoopTypes.Default, new ObjectIdCollection
                    {
                        frame
                    });
                    hat.EvaluateHatch(true);

                    return true;
                }
            }
        }

        // 拾取防火分区框线
        public static bool PickFireCompartmentFrames(UInt16 subKey, UInt16 storey, UInt16 index)
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
                    // 支持的框线类型
                    filterlist[0] = new TypedValue(0, "CIRCLE,LWPOLYLINE");
                    // 只拾取指定图层上的框线
                    filterlist[1] = new TypedValue(8, "AD-AREA-DIVD");
                    var entSelected = Active.Editor.GetSelection(new SelectionFilter(filterlist));
                    if (entSelected.Status == PromptStatus.OK)
                    {
                        foreach (var objId in entSelected.Value.GetObjectIds())
                        {
                            // 创建防火分区
                            objId.CreateFireCompartmentAreaFrame(subKey, storey, index++);
                        }

                        return true;
                    }

                    return false;
                }
            }
        }
    }
}
