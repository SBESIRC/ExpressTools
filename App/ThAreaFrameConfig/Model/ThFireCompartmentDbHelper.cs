using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using AcHelper;
using DotNetARX;
using Linq2Acad;
using GeometryExtensions;
using Autodesk.AutoCAD.DatabaseServices;
using TianHua.AutoCAD.Utility.ExtensionTools;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using System;
using Autodesk.AutoCAD.Colors;

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
                                // 修改防火分区标识文字
                                ObjectId objId = acadDatabase.Database.HandleToObjectId((string)handles.ElementAt(0).Value);
                                var text = acadDatabase.Element<MText>(objId, true);
                                text.Contents = text.Contents.UpdateCommerceSerialNumber(
                                    compartment.Subkey,
                                    compartment.Storey,
                                    compartment.Index);

                                // TODO:
                                //  修改防火分区文字框线
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

        // 合并商业防火分区
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
                foreach(var frame in compartment1.Frames)
                {
                    compartment2.Frames.Add(frame);
                }

                // 清空compartment1
                compartment1.Frames.Clear();
            }

            // 修改防火分区
            ModifyFireCompartment(compartment1);
            ModifyFireCompartment(compartment2);
        }

        public static bool CreateFireCompartmentAreaFrame(this ObjectId frame, UInt16 subKey, UInt16 storey, UInt16 index)
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

                //// 填充面积框线
                ////  https://www.keanw.com/2010/06/creating-transparent-hatches-in-autocad-using-net.html
                //Hatch hatch = new Hatch()
                //{
                //    // Set our transparency to 50% (=127)
                //    // Alpha value is Truncate(255 * (100-n)/100)
                //    Transparency = new Transparency(127)
                //};
                //hatch.SetHatchPattern(HatchPatternType.PreDefined, "ANSI31");
                //ObjectId objId = acadDatabase.ModelSpace.Add(hatch);
                //var hat = acadDatabase.Element<Hatch>(objId, true);
                //hat.Associative = true;
                //hat.AppendLoop(HatchLoopTypes.Default, new ObjectIdCollection
                //    {
                //        frame
                //    });
                //hat.EvaluateHatch(true);

                return true;
            }
        }

        public static bool IsFireCompartmentAreaFrame(this ObjectId frame)
        {
            return frame.GetXData(ThCADCommon.RegAppName_AreaFrame_FireCompartment) != null;
        }


        public static List<ThFireCompartment> CommerceFireCompartments(this Database database)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var compartments = new List<ThFireCompartment>();
                var frames = acadDatabase.ModelSpace
                    .OfType<Curve>()
                    .Where(o => o.Layer == "AD-AREA-DIVD" && o.ObjectId.IsFireCompartmentAreaFrame());
                foreach (var frame in frames)
                {
                    TypedValueList valueList = frame.ObjectId.GetXData(ThCADCommon.RegAppName_AreaFrame_FireCompartment);
                    if (valueList == null)
                        continue;

                    var handles = valueList.Where(o => o.TypeCode == (int)DxfCode.ExtendedDataHandle);
                    if (handles.Any())
                    {
                        // 获取防火分区编号
                        var objId = acadDatabase.Database.HandleToObjectId((string)handles.ElementAt(0).Value);
                        string[] tokens = Regex.Split(acadDatabase.Element<MText>(objId).Contents, @"\\P");
                        var compartment = new ThFireCompartment(tokens[0]);
                        // 考虑合并的情况
                        if (compartments.Contains(compartment))
                        {
                            foreach(var item in compartments.Where(o => o == compartment))
                            {
                                item.Frames.Add(new ThFireCompartmentAreaFrame()
                                {
                                    Frame = frame.ObjectId.OldIdPtr
                                });
                            }
                        }
                        else
                        {
                            compartment.Frames.Add(new ThFireCompartmentAreaFrame()
                            {
                                Frame = frame.ObjectId.OldIdPtr
                            });
                            compartments.Add(compartment);
                        }
                    }
                }
                return compartments;
            }
        }

        public static List<ThFireCompartment> UnderGroundParkingFireCompartments(this Database database)
        {
            return null;
        }

        // 拾取防火分区框线
        public static bool PickFireCompartmentFrames(UInt16 subKey, UInt16 storey, UInt16 index)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
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
