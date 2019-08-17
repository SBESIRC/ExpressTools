using System;
using AcHelper;
using DotNetARX;
using Linq2Acad;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThAreaFrameConfig.Presenter
{
    public static class ThPickAreaFrameHelper
    {
        public static void PickAreaFrames(this IThAreaFramePresenterCallback presenterCallback, string name)
        {
            using (Active.Document.LockDocument())
            {
                using (AcadDatabase acadDatabase = AcadDatabase.Active())
                {
                    foreach (var pline in Active.Database.GetSelection<Polyline>())
                    {
                        // 复制面积框线
                        ObjectId clonedObjId = ThEntTool.DeepClone(pline.ObjectId);
                        if (clonedObjId.IsNull)
                            return;

                        // 图层管理
                        //  1. 如果指定图层不存在，创建图层
                        //  2. 如果指定图层存在，返回此图层
                        ObjectId layerId = ThResidentialRoomDbUtil.ConfigLayer(name);
                        if (layerId.IsNull)
                            return;

                        // 将复制的放置在指定图层上
                        ThResidentialRoomDbUtil.MoveToLayer(clonedObjId, layerId);
                    }
                }
            }
        }

        public static void PickRoofAreaFrames(this IThAreaFramePresenterCallback presenterCallback, string name)
        {
            using (Active.Document.LockDocument())
            {
                using (AcadDatabase acadDatabase = AcadDatabase.Active())
                {
                    foreach (var pline in Active.Database.GetSelection<Polyline>())
                    {
                        // 复制面积框线
                        ObjectId clonedObjId = ThEntTool.DeepClone(pline.ObjectId);
                        if (clonedObjId.IsNull)
                            return;

                        // 图层管理
                        //  1. 如果指定图层不存在，创建图层
                        //  2. 如果指定图层存在，返回此图层
                        ObjectId layerId = ThResidentialRoomDbUtil.ConfigRoofLayer(name);
                        if (layerId.IsNull)
                            return;

                        // 将复制的放置在指定图层上
                        ThResidentialRoomDbUtil.MoveToLayer(clonedObjId, layerId);
                    }
                }
            }
        }

        public static void PickBuildingAreaFrames(this IThAreaFramePresenterCallback presenterCallback, string name)
        {
            using (Active.Document.LockDocument())
            {
                using (AcadDatabase acadDatabase = AcadDatabase.Active())
                {
                    foreach (var pline in Active.Database.GetSelection<Polyline>())
                    {
                        // 复制面积框线
                        ObjectId clonedObjId = ThEntTool.DeepClone(pline.ObjectId);
                        if (clonedObjId.IsNull)
                            return;

                        // 图层管理
                        //  1. 如果指定图层不存在，创建图层
                        //  2. 如果指定图层存在，返回此图层
                        ObjectId layerId = ThResidentialRoomDbUtil.ConfigBuildingLayer(name);
                        if (layerId.IsNull)
                            return;

                        // 将复制的放置在指定图层上
                        ThResidentialRoomDbUtil.MoveToLayer(clonedObjId, layerId);
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

        public static void HandleAcadException(this IThAreaFramePresenterCallback presenterCallback, System.Exception e)
        {
            Active.Editor.Write(e.ToString());
        }
    }
}
