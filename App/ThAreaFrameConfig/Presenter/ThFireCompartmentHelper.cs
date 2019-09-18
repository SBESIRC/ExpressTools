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
                    // 创建防火分区文字
                    DBText dbText = new DBText()
                    {
                        Position = pos,
                        TextString = ThFireCompartmentUtil.CommerceSerialNumber(compartment, subKey)
                    };
                    ObjectId textId = acadDatabase.ModelSpace.Add(dbText, true);

                    // 关联面积框线和防火分区文字
                    TypedValueList valueList = new TypedValueList
                    {
                        { (int)DxfCode.ExtendedDataHandle, textId.Handle }
                    };
                    textId.AddXData(ThCADCommon.RegAppName_AreaFrame_FireCompartment, valueList);

                    // TODO：创建防火分区面积文字
                    // TODO：创建防火分区包围框

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
                    List<ObjectId> textObjIds = new List<ObjectId>();
                    foreach (var frame in compartment.Frames)
                    {
                        ObjectId frameId = new ObjectId(frame.Frame);
                        TypedValueList valueList = frameId.GetXData(ThCADCommon.RegAppName_AreaFrame_FireCompartment);
                        if (valueList == null)
                            continue;

                        TypedValue value = valueList.Where(o => o.TypeCode == (int)DxfCode.ExtendedDataHandle).FirstOrDefault();
                        if (value == null)
                            continue;

                        // 从Object Handle到ObjectId
                        //  https://through-the-interface.typepad.com/through_the_interface/2007/02/getting_access_.html
                        textObjIds.Add(acadDatabase.Database.GetObjectId(false, (Handle)value.Value, 0));
                    }

                    foreach(var textObjId in textObjIds)
                    {
                        // 修改防火分区文字
                        if (acadDatabase.ModelSpace.Element(textObjId, true) is DBText dbText)
                        {
                            dbText.TextString = ThFireCompartmentUtil.CommerceSerialNumber(compartment, subKey);
                        }

                        // TODO：修改防火分区面积文字
                        // TODO：修改防火分区包围框
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
                    List<ObjectId> textObjIds = new List<ObjectId>();
                    foreach (var frame in compartment.Frames)
                    {
                        ObjectId frameId = new ObjectId(frame.Frame);
                        TypedValueList valueList = frameId.GetXData(ThCADCommon.RegAppName_AreaFrame_FireCompartment);
                        if (valueList == null)
                            continue;

                        TypedValue value = valueList.Where(o => o.TypeCode == (int)DxfCode.ExtendedDataHandle).FirstOrDefault();
                        if (value == null)
                            continue;

                        // 从Object Handle到ObjectId
                        //  https://through-the-interface.typepad.com/through_the_interface/2007/02/getting_access_.html
                        textObjIds.Add(acadDatabase.Database.GetObjectId(false, (Handle)value.Value, 0));

                        ObjectId textId = acadDatabase.Database.GetObjectId(false, (Handle)value.Value, 0);

                        // 删除防火分区文字
                        acadDatabase.ModelSpace.Element(textId, true).Erase();

                        // TODO：删除防火分区面积文字
                        // TODO：删除防火分区包围框

                        // 删除面积框线XData
                        frameId.RemoveXData(ThCADCommon.RegAppName_AreaFrame_FireCompartment);
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
