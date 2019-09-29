using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Autodesk.AutoCAD.DatabaseServices;
using TianHua.AutoCAD.Utility.ExtensionTools;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using AcHelper;
using DotNetARX;
using Linq2Acad;
using GeometryExtensions;
using System.Diagnostics;

namespace ThAreaFrameConfig.Model
{
    public static class ThFireCompartmentDbHelper
    {
        // 修改商业防火分区
        public static bool ModifyFireCompartment(ThFireCompartment compartment)
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

                    foreach (var frame in compartment.Frames)
                    {
                        ObjectId frameId = new ObjectId(frame.Frame);
                        TypedValueList valueList = frameId.GetXData(ThCADCommon.RegAppName_AreaFrame_FireCompartment);
                        if (valueList == null)
                        {
                            frameId.CreateFireCompartmentAreaFrame(compartment);
                        }
                        else
                        {
                            var handles = valueList.Where(o => o.TypeCode == (int)DxfCode.ExtendedDataHandle);
                            if (handles.Any())
                            {
                                // 修改防火分区标识文字
                                ObjectId objId = acadDatabase.Database.HandleToObjectId((string)handles.ElementAt(0).Value);
                                var text = acadDatabase.Element<MText>(objId, true);
                                text.Contents = compartment.CommerceTextContent();

                                // TODO:
                                //  修改防火分区文字框线
                            }

                            // 其他属性（是否自动灭火系统）
                            var properties = valueList.Where(o => o.TypeCode == (int)DxfCode.ExtendedDataInteger16);
                            if (properties.Any())
                            {
                                frameId.ModXData(
                                    ThCADCommon.RegAppName_AreaFrame_FireCompartment,
                                    DxfCode.ExtendedDataInteger16,
                                    properties.ElementAt(0).Value,
                                    compartment.SelfExtinguishingSystem);

                            }
                        }
                    }
                    return true;
                }
            }
        }

        public static bool ModifyFireCompartments(List<ThFireCompartment> compartments)
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

                    foreach(var compartment in compartments)
                    {
                        foreach (var frame in compartment.Frames)
                        {
                            ObjectId frameId = new ObjectId(frame.Frame);
                            TypedValueList valueList = frameId.GetXData(ThCADCommon.RegAppName_AreaFrame_FireCompartment);
                            if (valueList == null)
                            {
                                frameId.CreateFireCompartmentAreaFrame(compartment);
                            }
                            else
                            {
                                var handles = valueList.Where(o => o.TypeCode == (int)DxfCode.ExtendedDataHandle);
                                if (handles.Any())
                                {
                                    // 修改防火分区标识文字
                                    ObjectId objId = acadDatabase.Database.HandleToObjectId((string)handles.ElementAt(0).Value);
                                    var text = acadDatabase.Element<MText>(objId, true);
                                    text.Contents = compartment.CommerceTextContent();

                                    // TODO:
                                    //  修改防火分区文字框线
                                }

                                // 其他属性（是否自动灭火系统）
                                var properties = valueList.Where(o => o.TypeCode == (int)DxfCode.ExtendedDataInteger16);
                                if (properties.Any())
                                {
                                    frameId.ModXData(
                                        ThCADCommon.RegAppName_AreaFrame_FireCompartment,
                                        DxfCode.ExtendedDataInteger16,
                                        properties.ElementAt(0).Value,
                                        compartment.SelfExtinguishingSystem);

                                }
                            }
                        }
                    }

                    return true;
                }
            }
        }

        // 规整防火分区
        //  合并后防火分区的编号不再连续，规整后防火分区的编号保存连续
        public static void NormalizeFireCompartments(List<ThFireCompartment> compartments)
        {
            // 按照"子键"，“楼层”， “编号”排序
            compartments.Sort();

            // 按<子键，楼层>分组，在同一组内重新编号
            foreach (var group in compartments.GroupBy(o => new { o.Subkey, o.Storey }))
            {
                UInt16 index = 0;
                foreach(var compartment in group)
                {
                    compartment.Index = ++index;
                    ModifyFireCompartment(compartment);
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
                    // set focus to AutoCAD
                    //  https://adndevblog.typepad.com/autocad/2013/03/use-of-windowfocus-in-autocad-2014.html
#if ACAD2012
                    Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView();
#else
                    Active.Document.Window.Focus();
#endif

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
                            textObjIds.Add(acadDatabase.Database.HandleToObjectId((string)handles.ElementAt(0).Value));
                            bboxObjIds.Add(acadDatabase.Database.HandleToObjectId((string)handles.ElementAt(1).Value));
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

        public static bool DeleteFireCompartments(List<ThFireCompartment> compartments)
        {
            foreach(var compartment in compartments)
            {
                DeleteFireCompartment(compartment);
            }

            return true;
        }

        // 合并商业防火分区
        public static bool MergeFireCompartment(List<ThFireCompartment> compartments)
        {
            if (compartments.Count < 2)
            {
                return false;
            }

            // 排序
            compartments.Sort();

            // 往”最小“的防火分区合并，即往第一个防火分区合并
            for(int i = 1; i < compartments.Count; i++)
            {
                MergeFireCompartment(compartments[0], compartments[1]);
            }

            // 修改目标防火分区
            return ModifyFireCompartment(compartments[0]);
        }

        public static void MergeFireCompartment(ThFireCompartment compartment1, ThFireCompartment compartment2)
        {
            if (compartment1.CompareTo(compartment2) == 0)
            {
                return;
            }
            else if (compartment1.CompareTo(compartment2) < 0)
            {
                // compartment1 < compartment2，将compartment2合并到compartment1
                foreach (var frame in compartment2.Frames)
                {
                    compartment1.Frames.Add(frame);
                }

                // 清空compartment2
                compartment2.Frames.Clear();
            }
            else
            {
                // compartment1 > compartment2，将compartment1合并到compartment2
                foreach (var frame in compartment1.Frames)
                {
                    compartment2.Frames.Add(frame);
                }

                // 清空compartment1
                compartment1.Frames.Clear();
            }
        }

        public static bool CreateFireCompartmentAreaFrame(this ObjectId frame, ThFireCompartment compartment)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                // 创建防火分区文字
                //  https://www.keanw.com/2015/08/fitting-autocad-text-into-a-selected-space-using-net.html
                MText mText = new MText()
                {
                    Contents = compartment.CommerceTextContent(),
                    Location = frame.FireCompartmentAreaFrameCentroid()
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
                    { (int)DxfCode.ExtendedDataHandle, bboxId.Handle },
                    { (int)DxfCode.ExtendedDataInteger16, compartment.SelfExtinguishingSystem }
                };
                frame.AddXData(ThCADCommon.RegAppName_AreaFrame_FireCompartment, valueList);

                return true;
            }
        }

        public static bool IsFireCompartmentAreaFrame(this ObjectId frame)
        {
            return frame.GetXData(ThCADCommon.RegAppName_AreaFrame_FireCompartment) != null;
        }

        public static Point3d FireCompartmentAreaFrameCentroid(this ObjectId frameId)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(frameId.Database))
            {
                // 根据面积框线轮廓创建“区域”
                //  https://www.keanw.com/2015/08/getting-the-centroid-of-an-autocad-region-using-net.html
                DBObjectCollection curves = new DBObjectCollection()
                {
                    acadDatabase.Element<Curve>(frameId)
                };
                DBObjectCollection regions = Region.CreateFromCurves(curves);
                if (regions[0] is Region region)
                {
                    return region.Centroid();
                }

                return Point3d.Origin;
            }
        }

        public static ThFireCompartmentAreaFrame CreateFireCompartmentAreaFrame(this ObjectId frame, string islandLayer)
        {
            var obj = new ThFireCompartmentAreaFrame()
            {
                Frame = frame.OldIdPtr,
                IslandFrames = new List<IntPtr>()
            };

            using (Active.Document.LockDocument())
            {
                //
                using (AcadDatabase acadDatabase = AcadDatabase.Use(frame.Database))
                {
                    //
                    var curve = acadDatabase.Element<Polyline>(frame);

                    // SelectionFilter
                    //  https://adndevblog.typepad.com/autocad/2012/06/editorselectall-with-entity-and-layer-selection-filter.html
                    TypedValue[] filterlist = new TypedValue[2];
                    // 支持的面积框线类型
                    filterlist[0] = new TypedValue(0, "LWPOLYLINE");
                    // 过滤掉在锁定图层的面积框线
                    filterlist[1] = new TypedValue(8, islandLayer);
                    PromptSelectionResult psr = Active.Editor.SelectByPolyline(curve, PolygonSelectionMode.Window, filterlist);
                    if (psr.Status == PromptStatus.OK)
                    {
                        foreach (ObjectId objId in psr.Value.GetObjectIds())
                        {
                            obj.IslandFrames.Add(objId.OldIdPtr);
                        }
                    }

                    //
                    return obj;
                }
            }
        }

        public static ThFireCompartment CreateCommerceFireCompartment(this ObjectId frameId, string islandLayer)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(frameId.Database))
            {
                TypedValueList valueList = frameId.GetXData(ThCADCommon.RegAppName_AreaFrame_FireCompartment);
                if (valueList == null)
                {
                    return null;
                }
                var handles = valueList.Where(o => o.TypeCode == (int)DxfCode.ExtendedDataHandle);
                if (!handles.Any())
                {
                    return null;
                }

                // 获取防火分区编号
                Debug.Assert(handles.Count() == 2);
                var objId = acadDatabase.Database.HandleToObjectId((string)handles.ElementAt(0).Value);
                string[] tokens = Regex.Split(acadDatabase.Element<MText>(objId).Contents, @"\\P");
                ThFireCompartment compartment = new ThFireCompartment(tokens[0]);
                compartment.Frames.Add(CreateFireCompartmentAreaFrame(frameId, islandLayer));

                // 是否自动灭火系统
                var properties = valueList.Where(o => o.TypeCode == (int)DxfCode.ExtendedDataInteger16);
                if (properties.Any())
                {
                    Debug.Assert(properties.Count() == 1);
                    compartment.SelfExtinguishingSystem = Convert.ToBoolean(properties.ElementAt(0).Value);
                }

                return compartment;
            }
        }

        public static List<ThFireCompartment> CommerceFireCompartments(this Database database, string layer, string islandLayer)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var compartments = new List<ThFireCompartment>();
                var frames = acadDatabase.ModelSpace
                    .OfType<Curve>()
                    .Where(o => o.Layer == layer && o.ObjectId.IsFireCompartmentAreaFrame());
                foreach (var frame in frames)
                {
                    TypedValueList valueList = frame.ObjectId.GetXData(ThCADCommon.RegAppName_AreaFrame_FireCompartment);
                    if (valueList == null)
                        continue;

                    ThFireCompartment compartment = null;
                    var handles = valueList.Where(o => o.TypeCode == (int)DxfCode.ExtendedDataHandle);
                    if (handles.Any())
                    {
                        // 获取防火分区编号
                        var objId = acadDatabase.Database.HandleToObjectId((string)handles.ElementAt(0).Value);
                        string[] tokens = Regex.Split(acadDatabase.Element<MText>(objId).Contents, @"\\P");
                        compartment = new ThFireCompartment(tokens[0]);
                        // 考虑合并的情况
                        if (compartments.Contains(compartment))
                        {
                            foreach (var item in compartments.Where(o => o == compartment))
                            {
                                item.Frames.Add(CreateFireCompartmentAreaFrame(frame.ObjectId, islandLayer));
                            }
                        }
                        else
                        {
                            compartment.Number = compartments.Count() + 1;
                            compartment.Frames.Add(CreateFireCompartmentAreaFrame(frame.ObjectId, islandLayer));
                            compartments.Add(compartment);
                        }
                    }

                    // 是否自动灭火系统
                    var properties = valueList.Where(o => o.TypeCode == (int)DxfCode.ExtendedDataInteger16);
                    if (properties.Any())
                    {
                        compartment.SelfExtinguishingSystem = Convert.ToBoolean(properties.ElementAt(0).Value);
                    }
                }
                return compartments;
            }
        }

        public static List<ThFireCompartment> UnderGroundParkingFireCompartments(this Database database)
        {
            return null;
        }
    }
}
