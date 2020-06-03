using AcHelper;
using Linq2Acad;
using System.Collections.Generic;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using NFox.Cad.Collections;
using GeometryExtensions;
using ThSitePlan.Configuration;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThSitePlan.Engine
{
    public class ThSitePlanBoundaryBuildingWorker : ThSitePlanBoundaryWorker
    {
        private PolygonSelectionMode SelectionMode { get; set; }
        public ThSitePlanBoundaryBuildingWorker(PolygonSelectionMode mode = PolygonSelectionMode.Crossing)
        {
            SelectionMode = mode;
        }

        public override bool DoProcess(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                // 根据建筑线稿，获取建筑面域
                using (var objs = Filter(database, configItem, options))
                {
                    if (objs.Count == 0)
                    {
                        return false;
                    }

                    ObjectId frame = (ObjectId)options.Options["Frame"];
                    var frameObj = acadDatabase.Element<Polyline>(frame);
                    var segments = new PolylineSegmentCollection(frameObj);
                    foreach (var segment in segments)
                    {
                        var fence = new Point3dCollection()
                        {
                            segment.StartPoint.toPoint3d(),
                            segment.EndPoint.toPoint3d()
                        };
                        var selObjs = FenceIntersectFilter(database, configItem, options, fence);
                        if (selObjs.Count != 0)
                        {
                            var objId = acadDatabase.ModelSpace.Add(new Line(segment.StartPoint.toPoint3d(), segment.EndPoint.toPoint3d()));
                            objs.Add(objId);
                        }
                    }

                    // 优化1：同心圆
                    // 在取面域的过程中，我们只需要取最大的一个圆的面域
                    var filterObjs = acadDatabase.Database.FilterConcentric(objs);
                    Active.Editor.SuperBoundaryCmd(filterObjs);
                }

                // 删除建筑线稿
                using (var objs = Filter(database, configItem, options))
                {
                    if (objs.Count == 0)
                    {
                        return false;
                    }

                    Active.Editor.EraseCmd(objs);
                }

                return true;
            }
        }

        /// <summary>
        /// 过滤线框内的建筑线稿
        /// </summary>
        /// <param name="database"></param>
        /// <param name="configItem"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public override ObjectIdCollection Filter(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
        {
            ObjectId frame = (ObjectId)options.Options["Frame"];
            var layers = configItem.Properties["CADLayer"] as List<string>;
            var filterlist = OpFilter.Bulid(o => 
                o.Dxf((int)DxfCode.Start) != RXClass.GetClass(typeof(Hatch)).DxfName &
                o.Dxf((int)DxfCode.LayerName) == string.Join(",", layers.ToArray())
                );
            PromptSelectionResult psr = Active.Editor.SelectByPolyline(
                frame,
                SelectionMode,
                filterlist);
            if (psr.Status == PromptStatus.OK)
            {
                return new ObjectIdCollection(psr.Value.GetObjectIds());
            }
            else
            {
                return new ObjectIdCollection();
            }
        }

        public ObjectIdCollection FenceIntersectFilter(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options, Point3dCollection fence)
        {
            ObjectId frame = (ObjectId)options.Options["Frame"];
            var layers = configItem.Properties["CADLayer"] as List<string>;
            var filterlist = OpFilter.Bulid(o =>
                o.Dxf((int)DxfCode.Start) != RXClass.GetClass(typeof(Hatch)).DxfName &
                o.Dxf((int)DxfCode.LayerName) == string.Join(",", layers.ToArray())
                );
            PromptSelectionResult psr = Active.Editor.SelectByFence(frame, fence, filterlist);

            //返回除图框自身以外的Fence选择集
            if (psr.Status == PromptStatus.OK)
            {
                ObjectIdCollection selobjectids = new ObjectIdCollection(psr.Value.GetObjectIds());
                selobjectids.Remove(frame);
                return selobjectids;
            }
            else
            {
                return new ObjectIdCollection();
            }
        }

    }
}
