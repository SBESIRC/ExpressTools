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
                using (var objs = Filter(database, configItem, options))
                {
                    ObjectId frame = (ObjectId)options.Options["Frame"];
                    Active.Editor.BoundaryCmd(objs, SeedPoint(database, frame));

                    // 删除原图元
                    foreach (ObjectId objId in objs)
                    {
                        acadDatabase.Element<DBObject>(objId, true).Erase();
                    }
                }

                using (var objs = Filter(database, configItem, options))
                {
                    if (objs.Count == 0)
                    {
                        return false;
                    }

                    Active.Editor.CreateRegions(objs);
                }

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

                    Active.Editor.CreateHatchWithRegions(regions);
                }

                return true;
            }
        }

        /// <summary>
        /// 过滤线框内所有对象
        /// </summary>
        /// <param name="database"></param>
        /// <param name="configItem"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public override ObjectIdCollection Filter(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
        {
            ObjectId frame = (ObjectId)options.Options["Frame"];
            PromptSelectionResult psr = Active.Editor.SelectByPolyline(
                frame,
                PolygonSelectionMode.Window,
                null);
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

        /// <summary>
        /// 在线框内选取Region对象
        /// </summary>
        /// <param name="database"></param>
        /// <param name="configItem"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        private ObjectIdCollection FilterRegion(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
        {
            ObjectId frame = (ObjectId)options.Options["Frame"];
            var filter = OpFilter.Bulid(o => o.Dxf((int)DxfCode.Start) == string.Join(",", new string[]
            {
                RXClass.GetClass(typeof(Region)).DxfName,
            }));
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
    }
}
