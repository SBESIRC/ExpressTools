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
using NFox.Cad.Collections;

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
                        TypedValueList valueList = frameId.GetXData(ThCADCommon.RegAppName_AreaFrame_FireCompartment_Commerce);
                        if (valueList != null)
                        {
                            var handles = valueList.Where(o => o.TypeCode == (int)DxfCode.ExtendedDataHandle);
                            if (handles.Any())
                            {
                                // 修改防火分区标识文字
                                ObjectId objId = acadDatabase.Database.HandleToObjectId((string)handles.ElementAt(0).Value);
                                var text = acadDatabase.Element<MText>(objId, true);
                                text.Contents = compartment.CommerceTextContent();
                            }

                            // 其他属性（是否自动灭火系统）
                            var properties = valueList.Where(o => o.TypeCode == (int)DxfCode.ExtendedDataInteger16);
                            if (properties.Any())
                            {
                                frameId.ModXData(
                                    ThCADCommon.RegAppName_AreaFrame_FireCompartment_Commerce,
                                    DxfCode.ExtendedDataInteger16,
                                    properties.ElementAt(0).Value,
                                    compartment.SelfExtinguishingSystem);

                            }
                        }

                        valueList = frameId.GetXData(ThCADCommon.RegAppName_AreaFrame_FireCompartment_Parking);
                        if (valueList != null)
                        {
                            var handles = valueList.Where(o => o.TypeCode == (int)DxfCode.ExtendedDataHandle);
                            if (handles.Any())
                            {
                                // 修改防火分区标识文字
                                ObjectId objId = acadDatabase.Database.HandleToObjectId((string)handles.ElementAt(0).Value);
                                var text = acadDatabase.Element<MText>(objId, true);
                                text.Contents = compartment.CommerceTextContent();
                            }

                            // 其他属性（楼层）
                            var properties = valueList.Where(o => o.TypeCode == (int)DxfCode.ExtendedDataInteger16);
                            if (properties.Any())
                            {
                                frameId.ModXData(
                                    ThCADCommon.RegAppName_AreaFrame_FireCompartment_Parking,
                                    DxfCode.ExtendedDataInteger16,
                                    properties.ElementAt(0).Value,
                                    compartment.Storey);

                            }
                        }
                    }
                    return true;
                }
            }
        }
        public static bool ModifyFireCompartments(List<ThFireCompartment> compartments)
        {
            foreach (var compartment in compartments)
            {
                ModifyFireCompartment(compartment);
            }

            return true;
        }

        // 规整商业防火分区
        //  合并后防火分区的编号不再连续，规整后防火分区的编号保存连续
        public static void NormalizeFireCompartments(List<ThFireCompartment> compartments)
        {
            // 防火分区必须是同一类型
            var enumerator = compartments.Select(o => o.Type).Distinct();
            if (enumerator.Count() != 1)
            {
                return;
            }

            if (enumerator.First() == ThFireCompartment.FCType.FCCommerce)
            {
                // 按照"子键"，“楼层”， “编号”排序
                compartments.Sort();

                // 按<子键，楼层>分组，在同一组内重新编号
                foreach (var group in compartments.GroupBy(o => new { o.Subkey, o.Storey }))
                {
                    UInt16 index = 0;
                    foreach (var compartment in group)
                    {
                        compartment.Index = ++index;
                        ModifyFireCompartment(compartment);
                    }
                }

                // 按照"子键"，“楼层”， “编号”排序
                compartments.Sort();
            }
            else if (enumerator.First() == ThFireCompartment.FCType.FCUnderGroundParking)
            {
                // 按照"子键"，“楼层”， “编号”排序
                compartments.Sort();

                // 按子键分组，在同一组内重新编号
                foreach (var group in compartments.GroupBy(o => new { o.Subkey }))
                {
                    UInt16 index = 0;
                    foreach (var compartment in group)
                    {
                        compartment.Index = ++index;
                        ModifyFireCompartment(compartment);
                    }
                }

                // 按照"子键"，“楼层”， “编号”排序
                compartments.Sort();
            }
            else 
            {
                throw new NotSupportedException();
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
                        TypedValueList valueList = frameId.GetXData(ThCADCommon.RegAppName_AreaFrame_FireCompartment_Commerce);
                        if (valueList != null)
                        {
                            var handles = valueList.Where(o => o.TypeCode == (int)DxfCode.ExtendedDataHandle);
                            if (handles.Any())
                            {
                                // 从Object Handle到ObjectId
                                //  https://through-the-interface.typepad.com/through_the_interface/2007/02/getting_access_.html
                                textObjIds.Add(acadDatabase.Database.HandleToObjectId((string)handles.ElementAt(0).Value));
                                bboxObjIds.Add(acadDatabase.Database.HandleToObjectId((string)handles.ElementAt(1).Value));
                            }

                            // 删除面积框线XData
                            frameId.RemoveXData(ThCADCommon.RegAppName_AreaFrame_FireCompartment_Commerce);
                        }

                        valueList = frameId.GetXData(ThCADCommon.RegAppName_AreaFrame_FireCompartment_Parking);
                        if (valueList != null)
                        {
                            var handles = valueList.Where(o => o.TypeCode == (int)DxfCode.ExtendedDataHandle);
                            if (handles.Any())
                            {
                                // 从Object Handle到ObjectId
                                //  https://through-the-interface.typepad.com/through_the_interface/2007/02/getting_access_.html
                                textObjIds.Add(acadDatabase.Database.HandleToObjectId((string)handles.ElementAt(0).Value));
                                bboxObjIds.Add(acadDatabase.Database.HandleToObjectId((string)handles.ElementAt(1).Value));
                            }

                            // 删除面积框线XData
                            frameId.RemoveXData(ThCADCommon.RegAppName_AreaFrame_FireCompartment_Parking);
                        }
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

        // 创建防火分区
        public static bool CreateFireCompartment(ThFireCompartment compartment)
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

                        // 创建一个基于面积框线“质心”的UCS，
                        // 并创建一个从UCS到此UCS之间的创建一个坐标系统变换矩阵，
                        // 将位于WCS中的图元转换到另外一个坐标系统
                        // 在坐标系统转换过程中图元的位置相对于坐标系统保持不变
                        var insertPt = frameId.FireCompartmentAreaFrameCentroid().Value;
                        var coordinate = Active.Editor.CurrentUserCoordinateSystem.CoordinateSystem3d;
                        var transform = Matrix3d.AlignCoordinateSystem(
                            Point3d.Origin,
                            Vector3d.XAxis,
                            Vector3d.YAxis,
                            Vector3d.ZAxis,
                            insertPt,
                            coordinate.Xaxis,
                            coordinate.Yaxis,
                            coordinate.Zaxis);

                        if (compartment.Type == ThFireCompartment.FCType.FCCommerce)
                        {
                            // 创建防火分区文字
                            MText mText = new MText()
                            {
                                TextHeight = 1100,
                                LineSpaceDistance = 1800,
                                Attachment = AttachmentPoint.MiddleCenter,
                                Contents = compartment.CommerceTextContent(),
                                Location = new Point3d(0, 0, 0)
                            };
                            ObjectId textId = acadDatabase.ModelSpace.Add(mText, true);
                            mText.TransformBy(transform);

                            // 设置文字样式
                            mText.TextStyleId = acadDatabase.Database.CreateFCNoteTextStyle();

                            // 设置文字图层
                            mText.LayerId = acadDatabase.Database.CreateFCNoteTextLayer();

                            // 创建防火分区文字框线
                            Polyline bbox = new Polyline()
                            {
                                Closed = true
                            };

                            // 通过建立ECS来方便计算文字框线的位置
                            // 顶点顺序
                            //  (0)-----(1)
                            //   |       |
                            //   |  (c)  |
                            //   |       |
                            //  (3)-----(2)
                            Point3dCollection points = new Point3dCollection()
                            {
                                // 左上角点
                                new Point3d(-5000, 2000, 1),
                                // 右上角点
                                new Point3d(5000, 2000, 1),
                                // 右下角点
                                new Point3d(5000, -2000, 1),
                                // 左下角点
                                new Point3d(-5000, -2000, 1)
                            };
                            bbox.CreatePolyline(points);
                            ObjectId bboxId = acadDatabase.ModelSpace.Add(bbox, true);
                            bbox.TransformBy(transform);

                            // 设置全局宽度
                            bbox.ConstantWidth = 150;

                            // 设置图层
                            bbox.LayerId = acadDatabase.Database.CreateFCNoteTextLayer();

                            // 关联面积框线和防火分区
                            TypedValueList valueList = new TypedValueList
                            {
                                { (int)DxfCode.ExtendedDataHandle, textId.Handle },
                                { (int)DxfCode.ExtendedDataHandle, bboxId.Handle },
                                { (int)DxfCode.ExtendedDataInteger16, compartment.SelfExtinguishingSystem }
                            };
                            frameId.AddXData(ThCADCommon.RegAppName_AreaFrame_FireCompartment_Commerce, valueList);
                        }
                        else if (compartment.Type == ThFireCompartment.FCType.FCUnderGroundParking)
                        {
                            // 创建防火分区文字
                            MText mText = new MText()
                            {
                                TextHeight = 750,
                                LineSpaceDistance = 1200,
                                Attachment = AttachmentPoint.MiddleCenter,
                                Contents = compartment.CommerceTextContent(),
                                Location = new Point3d(0, 0, 0)
                            };
                            ObjectId textId = acadDatabase.ModelSpace.Add(mText, true);
                            mText.TransformBy(transform);

                            // 设置文字样式
                            mText.TextStyleId = acadDatabase.Database.CreateFCNoteTextStyle();

                            // 设置文字图层
                            mText.LayerId = acadDatabase.Database.CreateFCNoteTextLayer();

                            // 创建防火分区文字框线
                            Polyline bbox = new Polyline()
                            {
                                Closed = true
                            };

                            // 通过建立ECS来方便计算文字框线的位置
                            // 顶点顺序
                            //  (0)-----(1)
                            //   |       |
                            //   |  (c)  |
                            //   |       |
                            //  (3)-----(2)
                            Point3dCollection points = new Point3dCollection()
                            {
                                // 左上角点
                                new Point3d(-3500, 1400, 1),
                                // 右上角点
                                new Point3d(3500, 1400, 1),
                                // 右下角点
                                new Point3d(3500, -1400, 1),
                                // 左下角点
                                new Point3d(-3500, -1400, 1)
                            };
                            bbox.CreatePolyline(points);
                            ObjectId bboxId = acadDatabase.ModelSpace.Add(bbox, true);
                            bbox.TransformBy(transform);

                            // 设置全局宽度
                            bbox.ConstantWidth = 150;

                            // 设置图层
                            bbox.LayerId = acadDatabase.Database.CreateFCNoteTextLayer();

                            // 关联面积框线和防火分区
                            TypedValueList valueList = new TypedValueList
                            {
                                { (int)DxfCode.ExtendedDataHandle, textId.Handle },
                                { (int)DxfCode.ExtendedDataHandle, bboxId.Handle },
                                { (int)DxfCode.ExtendedDataInteger16, compartment.Storey }
                            };
                            frameId.AddXData(ThCADCommon.RegAppName_AreaFrame_FireCompartment_Parking, valueList);
                        }
                        else
                        {
                            throw new NotSupportedException();
                        }
                    }

                    return true;
                }
            }
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
                MergeFireCompartment(compartments[0], compartments[i]);
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

        public static bool IsFireCompartmentAreaFrame(this ObjectId frame)
        {
            return (frame.GetXData(ThCADCommon.RegAppName_AreaFrame_FireCompartment_Commerce) != null) ||
                (frame.GetXData(ThCADCommon.RegAppName_AreaFrame_FireCompartment_Parking) != null);
        }

        public static Point3d? FireCompartmentAreaFrameCentroid(this ObjectId frameId)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(frameId.Database))
            {
                try
                {
                    // 根据面积框线轮廓创建“区域”
                    //  https://www.keanw.com/2015/08/getting-the-centroid-of-an-autocad-region-using-net.html
                    DBObjectCollection curves = new DBObjectCollection()
                    {
                        acadDatabase.Element<Curve>(frameId)
                    };
                    DBObjectCollection regions = Region.CreateFromCurves(curves);
                    Region region = regions[0] as Region;
                    return region.Centroid();
                }
                catch
                {
                    // 由于绘图精度或者绘图不规范，面积框线处于“假闭合”的状态。
                    // 在放大很多倍的情况下，多段线和起点和终点并不完全重合。
                    // 在这样的情况下，CreateFromCurves()会抛出异常。
                    // 这里通过捕捉异常，返回null表示“失败”。
                    return null;
                }
            }
        }

        public static ThFireCompartmentAreaFrame CreateFireCompartmentAreaFrame(this ObjectId frame, string islandLayer)
        {
            using (Active.Document.LockDocument())
            {
                using (AcadDatabase acadDatabase = AcadDatabase.Use(frame.Database))
                {
                    var obj = new ThFireCompartmentAreaFrame()
                    {
                        Frame = frame.OldIdPtr,
                        IslandFrames = new List<IntPtr>(),
                        EmergencyExitNotes = new List<IntPtr>(),
                        EvacuationWidthNotes = new List<IntPtr>(),
                        EvacuationDistanceNotes = new List<IntPtr>()
                    };

                    // 拾取内部孤岛轮廓
                    var filterlist = OpFilter.Bulid(
                        o => o.Dxf((int)DxfCode.Start) == "LWPOLYLINE" &
                        o.Dxf((int)DxfCode.LayerName) == islandLayer);
                    PromptSelectionResult psr = Active.Editor.SelectByPolyline(
                        frame,
                        PolygonSelectionMode.Window, 
                        filterlist);
                    if (psr.Status == PromptStatus.OK)
                    {
                        foreach (ObjectId objId in psr.Value.GetObjectIds())
                        {
                            obj.IslandFrames.Add(objId.OldIdPtr);
                        }
                    }

                    // 拾取有效疏散宽度
                    filterlist = OpFilter.Bulid(
                        o => o.Dxf((int)DxfCode.Start) == "TEXT" &
                        o.Dxf((int)DxfCode.Text) == "有效疏散宽度*");
                    psr = Active.Editor.SelectByPolyline(
                        frame,
                        PolygonSelectionMode.Window,
                        filterlist);
                    if (psr.Status == PromptStatus.OK)
                    {
                        foreach (ObjectId objId in psr.Value.GetObjectIds())
                        {
                            obj.EvacuationWidthNotes.Add(objId.OldIdPtr);
                        }
                    }

                    // 拾取最远疏散距离
                    filterlist = OpFilter.Bulid(
                        o => o.Dxf((int)DxfCode.Start) == "TEXT" &
                        o.Dxf((int)DxfCode.Text) == "最远疏散距离*");
                    psr = Active.Editor.SelectByPolyline(
                        frame,
                        PolygonSelectionMode.Window,
                        filterlist);
                    if (psr.Status == PromptStatus.OK)
                    {
                        foreach (ObjectId objId in psr.Value.GetObjectIds())
                        {
                            obj.EvacuationDistanceNotes.Add(objId.OldIdPtr);
                        }
                    }

                    // 拾取安全出口
                    filterlist = OpFilter.Bulid(
                        o => o.Dxf((int)DxfCode.Start) == "TEXT" &
                        o.Dxf((int)DxfCode.Text) == "安全出口*");
                    psr = Active.Editor.SelectByPolyline(
                        frame,
                        PolygonSelectionMode.Window,
                        filterlist);
                    if (psr.Status == PromptStatus.OK)
                    {
                        foreach (ObjectId objId in psr.Value.GetObjectIds())
                        {
                            obj.EmergencyExitNotes.Add(objId.OldIdPtr);
                        }
                    }

                    //
                    return obj;
                }
            }
        }

        public static ThFireCompartment LoadCommerceFireCompartment(this ObjectId frameId, string islandLayer)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(frameId.Database))
            {
                TypedValueList valueList = frameId.GetXData(ThCADCommon.RegAppName_AreaFrame_FireCompartment_Commerce);
                if (valueList != null)
                {
                    var handles = valueList.Where(o => o.TypeCode == (int)DxfCode.ExtendedDataHandle);
                    if (!handles.Any())
                    {
                        return null;
                    }

                    // 获取防火分区编号
                    Debug.Assert(handles.Count() == 2);
                    var objId = acadDatabase.Database.HandleToObjectId((string)handles.ElementAt(0).Value);
                    string[] tokens = Regex.Split(acadDatabase.Element<MText>(objId).Contents, @"\\P");
                    ThFireCompartment compartment = ThFireCompartment.Commerce(tokens[0]);
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

                valueList = frameId.GetXData(ThCADCommon.RegAppName_AreaFrame_FireCompartment_Parking);
                if (valueList != null)
                {
                    var handles = valueList.Where(o => o.TypeCode == (int)DxfCode.ExtendedDataHandle);
                    if (!handles.Any())
                    {
                        return null;
                    }

                    // 获取防火分区编号
                    Debug.Assert(handles.Count() == 2);
                    var objId = acadDatabase.Database.HandleToObjectId((string)handles.ElementAt(0).Value);
                    string[] tokens = Regex.Split(acadDatabase.Element<MText>(objId).Contents, @"\\P");
                    ThFireCompartment compartment = ThFireCompartment.UnderGroundParking(tokens[0]);
                    compartment.Frames.Add(CreateFireCompartmentAreaFrame(frameId, islandLayer));

                    //地下停车库楼层
                    var properties = valueList.Where(o => o.TypeCode == (int)DxfCode.ExtendedDataInteger16);
                    if (properties.Any())
                    {
                        Debug.Assert(properties.Count() == 1);
                        compartment.Storey = Convert.ToInt16(properties.ElementAt(0).Value);
                    }

                    return compartment;
                }

                return null;
            }
        }

        public static List<ThFireCompartment> LoadCommerceFireCompartments(this Database database, string layer, string islandLayer)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var compartments = new List<ThFireCompartment>();
                var frames = acadDatabase.ModelSpace
                    .OfType<Curve>()
                    .Where(o => o.Layer == layer && o.ObjectId.IsFireCompartmentAreaFrame());
                foreach (var frame in frames)
                {
                    TypedValueList valueList = frame.ObjectId.GetXData(ThCADCommon.RegAppName_AreaFrame_FireCompartment_Commerce);
                    if (valueList != null)
                    {
                        ThFireCompartment compartment = null;
                        var handles = valueList.Where(o => o.TypeCode == (int)DxfCode.ExtendedDataHandle);
                        if (handles.Any())
                        {
                            // 获取防火分区编号
                            var objId = acadDatabase.Database.HandleToObjectId((string)handles.ElementAt(0).Value);
                            if (objId.IsNull || objId.IsErased)
                            {
                                // 若防火分区编号出错，则断开“关联”。
                                // 断开“关联”后，此框线将不再被标识为防火分区。
                                frame.ObjectId.RemoveXData(ThCADCommon.RegAppName_AreaFrame_FireCompartment_Commerce);

                                continue;
                            }
                            string[] tokens = Regex.Split(acadDatabase.Element<MText>(objId).Contents, @"\\P");
                            compartment = ThFireCompartment.Commerce(tokens[0]);
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

                    valueList = frame.ObjectId.GetXData(ThCADCommon.RegAppName_AreaFrame_FireCompartment_Parking);
                    if (valueList != null)
                    {
                        ThFireCompartment compartment = null;
                        var handles = valueList.Where(o => o.TypeCode == (int)DxfCode.ExtendedDataHandle);
                        if (handles.Any())
                        {
                            // 获取防火分区编号
                            var objId = acadDatabase.Database.HandleToObjectId((string)handles.ElementAt(0).Value);
                            if (objId.IsNull || objId.IsErased)
                            {
                                // 若防火分区编号出错，则断开“关联”。
                                // 断开“关联”后，此框线将不再被标识为防火分区。
                                frame.ObjectId.RemoveXData(ThCADCommon.RegAppName_AreaFrame_FireCompartment_Parking);

                                continue;
                            }

                            string[] tokens = Regex.Split(acadDatabase.Element<MText>(objId).Contents, @"\\P");
                            compartment = ThFireCompartment.UnderGroundParking(tokens[0]);
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

                        //地下停车库楼层
                        var properties = valueList.Where(o => o.TypeCode == (int)DxfCode.ExtendedDataInteger16);
                        if (properties.Any())
                        {
                            compartment.Storey = Convert.ToInt16(properties.ElementAt(0).Value);
                        }
                    }
                }
                return compartments;
            }
        }
    }
}
