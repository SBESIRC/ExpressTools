using System;
using System.Linq;
using AcHelper;
using DotNetARX;
using Linq2Acad;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using TianHua.AutoCAD.Utility.ExtensionTools;
using System.Collections.Generic;
using ThAreaFrameConfig.Model;

namespace ThAreaFrameConfig.Presenter
{
    public static class ThFireCompartmentHelper
    {
        // 创建防火分区
        public static bool CreateFireCompartment(ThFireCompartment compartment, string subKey, ObjectId frame, Point3d pos)
        {
            using (Active.Document.LockDocument())
            {
                using (AcadDatabase acadDatabase = AcadDatabase.Active())
                {
                    // 创建防火分区标识文字
                    DBText dbText = new DBText()
                    {
                        Position = pos,
                        TextString = ThFireCompartmentUtil.CommerceSerialNumber(compartment, subKey)
                    };
                    ObjectId textId = acadDatabase.ModelSpace.Add(dbText, true);

                    // 创建防火分区面积文字
                    MText mText = new MText()
                    {
                        Location = pos,
                        Contents = ThFireCompartmentUtil.AreaTextContent(compartment)
                    };
                    ObjectId areaId = acadDatabase.ModelSpace.Add(mText, true);

                    // 创建防火分区包围框线
                    Polyline bbox = new Polyline();
                    ObjectId bboxId = acadDatabase.ModelSpace.Add(bbox, true);

                    // 关联面积框线和防火分区
                    TypedValueList valueList = new TypedValueList
                    {
                        { (int)DxfCode.ExtendedDataHandle, textId.Handle },
                        { (int)DxfCode.ExtendedDataHandle, areaId.Handle },
                        { (int)DxfCode.ExtendedDataHandle, bboxId.Handle }
                    };
                    frame.AddXData(ThCADCommon.RegAppName_AreaFrame_FireCompartment, valueList);

                    return true;
                }
            }
        }

        // 修改防火分区
        public static bool ModifyFireCompartment(ThFireCompartment compartment, string subKey)
        {
            using (Active.Document.LockDocument())
            {
                using (AcadDatabase acadDatabase = AcadDatabase.Active())
                {
                    List<ObjectId> snTextObjIds = new List<ObjectId>();
                    List<ObjectId> areaTextObjIds = new List<ObjectId>();
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
                            snTextObjIds.Add(acadDatabase.Database.GetObjectId(false, (Handle)handles.ElementAt(0).Value, 0));
                            areaTextObjIds.Add(acadDatabase.Database.GetObjectId(false, (Handle)handles.ElementAt(1).Value, 0));
                        }
                    }

                    // 修改防火分区标识文字
                    foreach (var objId in snTextObjIds)
                    {
                        if (acadDatabase.ModelSpace.Element(objId, true) is DBText dbText)
                        {
                            dbText.TextString = ThFireCompartmentUtil.CommerceSerialNumber(compartment, subKey);
                        }
                    }

                    // 修改防火分区面积文字
                    foreach(var objId in areaTextObjIds)
                    {
                        if (acadDatabase.ModelSpace.Element(objId, true) is MText mText)
                        {
                            mText.Contents = ThFireCompartmentUtil.AreaTextContent(compartment);
                        }
                    }

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
                    List<ObjectId> bboxObjIds = new List<ObjectId>();
                    List<ObjectId> snTextObjIds = new List<ObjectId>();
                    List<ObjectId> areaTextObjIds = new List<ObjectId>();
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
                            snTextObjIds.Add(acadDatabase.Database.GetObjectId(false, (Handle)handles.ElementAt(0).Value, 0));
                            areaTextObjIds.Add(acadDatabase.Database.GetObjectId(false, (Handle)handles.ElementAt(1).Value, 0));
                            bboxObjIds.Add(acadDatabase.Database.GetObjectId(false, (Handle)handles.ElementAt(2).Value, 0));
                        }

                        // 删除面积框线XData
                        frameId.RemoveXData(ThCADCommon.RegAppName_AreaFrame_FireCompartment);
                    }

                    // 删除防火分区标识文字
                    foreach (var objId in snTextObjIds)
                    {
                        acadDatabase.ModelSpace.Element(objId, true).Erase();
                    }

                    // 删除防火分区面积文字
                    foreach (var objId in areaTextObjIds)
                    {
                        acadDatabase.ModelSpace.Element(objId, true).Erase();
                    }

                    // 删除防火分区包围框
                    foreach(var objId in bboxObjIds)
                    {
                        acadDatabase.ModelSpace.Element(objId, true).Erase();
                    }

                    return true;
                }
            }
        }

        // 合并防火分区
        public static bool MergeFireCompartment(ThFireCompartment targetCompartment, ThFireCompartment sourceCompartment, string subKey)
        {
            foreach (var frame in sourceCompartment.Frames)
            {
                targetCompartment.Frames.Add(frame);
            }

            // 删除源防火分区
            DeleteFireCompartment(sourceCompartment);

            // 修改目标防火分区
            ModifyFireCompartment(targetCompartment, subKey);

            return true;
        }
    }
}
