using System.Linq;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Linq2Acad;
using AcHelper;
using NFox.Cad.Collections;
using ThSitePlan.Configuration;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThSitePlan.Engine
{
    public class ThSitePlanBoundaryWorker : ThSitePlanWorker
    {
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
                    Active.Editor.BoundaryCmd(objs, SeedPoint(database, frame));
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

                // 获取建筑外部面域
                using (var regions = FilterRegion(database, configItem, options))
                {
                    if (regions.Count == 0)
                    {
                        return false;
                    }

                    Active.Editor.UnionRegions(regions);
                }
                using (var regions = FilterRegion(database, configItem, options))
                {
                    if (regions.Count == 0)
                    {
                        return false;
                    }

                    Active.Editor.ExplodeCmd(regions);
                }

                // 将建筑外部面域移到指定的图层
                using (var regions = FilterRegion(database, configItem, options))
                {
                    if (regions.Count == 0)
                    {
                        return false;
                    }

                    acadDatabase.Database.MoveToLayer(regions, ThSitePlanCommon.LAYER_BUILD_HATCH);
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
            var filter = OpFilter.Bulid(o => o.Dxf((int)DxfCode.Start) != RXClass.GetClass(typeof(Region)).DxfName);
            PromptSelectionResult psr = Active.Editor.SelectByPolyline(
                frame,
                PolygonSelectionMode.Window,
                filter);
            if (psr.Status == PromptStatus.OK)
            {
                return new ObjectIdCollection(psr.Value.GetObjectIds());
            }
            else
            {
                return new ObjectIdCollection();
            }
        }

        /// <summary>
        /// 过滤线框内的建筑面域
        /// </summary>
        /// <param name="database"></param>
        /// <param name="configItem"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        private ObjectIdCollection FilterRegion(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
        {
            ObjectId frame = (ObjectId)options.Options["Frame"];
            var filter = OpFilter.Bulid(o => o.Dxf((int)DxfCode.Start) == RXClass.GetClass(typeof(Region)).DxfName);
            PromptSelectionResult psr = Active.Editor.SelectByPolyline(
                frame,
                PolygonSelectionMode.Window,
                filter);
            if (psr.Status == PromptStatus.OK)
            {
                return new ObjectIdCollection(psr.Value.GetObjectIds());
            }
            else
            {
                return new ObjectIdCollection();
            }
        }

        /// <summary>
        /// 在线框内选取一个种子点
        /// </summary>
        /// <param name="database"></param>
        /// <param name="frame"></param>
        /// <returns></returns>
        private Point3d SeedPoint(Database database, ObjectId frame)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var polygon = acadDatabase.Element<Polyline>(frame);
                var offset = polygon.Offset(ThSitePlanCommon.seed_point_offset, 
                    ThPolylineExtension.OffsetSide.In);
                return offset.First().StartPoint;
            }
        }
    }
}
