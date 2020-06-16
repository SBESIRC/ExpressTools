using AcHelper;
using Linq2Acad;
using System.Linq;
using System.Collections.Generic;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using NFox.Cad.Collections;
using GeometryExtensions;
using ThSitePlan.Configuration;
using Dreambuild.AutoCAD;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThSitePlan.Engine
{
    public class ThSitePlanBoundaryPathWorker : ThSitePlanBoundaryWorker
    {
        public override bool DoProcess(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                using (var objs = Filter(database, configItem, options))
                {
                    if (objs.Count == 0)
                    {
                        return false;
                    }

                    //获取Frame顶点，作为Fence选择点
                    ObjectId frame = (ObjectId)options.Options["Frame"];
                    var pline = acadDatabase.Element<Polyline>(frame);
                    Point3dCollection points = pline.Vertices();
                    Point3dCollection fence = new Point3dCollection();
                    for (int i = 0; i < points.Count; i++)
                    {
                        var pt = points[i];
                        if (!fence.Contains(pt, ThCADCommon.Global_Tolerance))
                        {
                            fence.Add(pt);
                        }
                    }

                    //通过frame顶点创建Fence选择集
                    var selObjs = FenceIntersectFilter(database, configItem, options, fence);

                    //若Fence选择集不为空将frame自身加到最终superboundary集合中
                    if (selObjs.Count != 0)
                    {
                        objs.Add(frame);
                    }

                    //执行superboundary
                    Active.Editor.SuperBoundaryCmd(objs);
                }

                using (var objs = HatchFilter(database, configItem, options))
                {
                    foreach (ObjectId oid in objs)
                    {
                        Hatch roadhatch = acadDatabase.Element<Hatch>(oid,true);
                        DeleteHeavyArea(database, configItem, options, roadhatch);
                    }
                }
            }
            return true;
        }

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
                PolygonSelectionMode.Crossing,
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

        public ObjectIdCollection HatchFilter(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
        {
            ObjectId frame = (ObjectId)options.Options["Frame"];
            var layers = configItem.Properties["CADLayer"] as List<string>;
            var filterlist = OpFilter.Bulid(o =>
                o.Dxf((int)DxfCode.Start) == RXClass.GetClass(typeof(Hatch)).DxfName
                );
            PromptSelectionResult psr = Active.Editor.SelectByPolyline(
                frame,
                PolygonSelectionMode.Crossing,
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

        private void DeleteHeavyArea(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options, Hatch roadhatch)
        {
            try
            {
                double hatcharea = roadhatch.Area;
                Extents3d hatchextents = roadhatch.GeometricExtents;
                double hatchoutsidearea = hatchextents.ToExtents2d().GetArea();
                double hatchratio = hatcharea / hatchoutsidearea;
                // 根据区域的密度和面积，判断是否需要剔除
                if (hatchratio > ThSitePlanCommon.hatch_density_ratio && hatcharea > ThSitePlanCommon.hatch_area_threshold)
                {
                    roadhatch.Erase();
                }
            }
            catch
            {
                //在某些情况下，获取Hatch的面积会抛出异常
                //这里我们直接返回
            }
        }
    }
}