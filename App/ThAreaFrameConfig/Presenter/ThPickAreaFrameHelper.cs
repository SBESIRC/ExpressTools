using System;
using System.Linq;
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
                            continue;

                        // 图层管理
                        //  1. 如果指定图层不存在，创建图层
                        //  2. 如果指定图层存在，返回此图层
                        ObjectId layerId = ThResidentialRoomDbUtil.ConfigRoofLayer(name);
                        if (layerId.IsNull)
                            continue;

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

        public static void DeleteAreaFrame(this IThAreaFramePresenterCallback presenterCallback, IntPtr areaFrame)
        {
            using (Active.Document.LockDocument())
            {
                using (AcadDatabase acadDatabase = AcadDatabase.Active())
                {
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
                    foreach(var frame in areaFrames)
                    {
                        acadDatabase.ModelSpace.Element(new ObjectId(frame), true).Erase();
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
                    var areaFrames = acadDatabase.ModelSpace
                        .OfType<Polyline>()
                        .Where(o => o.Layer == name);
                    if (!areaFrames.Any())
                    {
                        acadDatabase.Layers.Element(name, true).Erase();
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
                    foreach(var frame in areaFrames)
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
                    foreach(var frame in areaFrames)
                    {
                        acadDatabase.ModelSpace.Element(new ObjectId(frame)).Unhighlight();
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
