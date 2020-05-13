using System.Linq;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Linq2Acad;
using AcHelper;
using ThSitePlan.Configuration;
using Dreambuild.AutoCAD;

namespace ThSitePlan.Engine
{
    public class ThSitePlanOuterWorker : ThSitePlanWorker
    {
        public override bool DoProcess(Database database, 
            ThSitePlanConfigItem configItem, 
            ThSitePlanOptions options)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                using (var objs = Filter(database, configItem, options))
                {
                    // 获取边界
                    ObjectId frame = (ObjectId)options.Options["Frame"];
                    var polygon = acadDatabase.Element<Polyline>(frame);
                    var boundaries = Active.Editor.TraceBoundaryEx(polygon);
                    // 删除outermost boundary（最后一个） 
                    boundaries.RemoveAt(boundaries.Count - 1);
                    // 转化成Region loops
                    var loops = acadDatabase.Database.CreateRegionLoops(boundaries);
                    // 合并loops，消除掉"Holes"和"Islands"
                    foreach (ObjectId objId in RegionLoopService.MergeBoundary(loops.Cast<ObjectId>()))
                    {
                        //var shadows = objId.CreateRegionShadow(new Vector3d(5, -5, 0));
                        //var shadowloops = acadDatabase.Database.CreateRegionLoops(shadows);
                        //foreach (ObjectId shadowloop in RegionLoopService.MergeBoundary(shadowloops.Cast<ObjectId>()))
                        //{
                        //    shadowloop.CreateHatchWithPolygon();
                        //}
                        // 创建Hatch
                        //objId.CreateHatchWithPolygon();
                    }

                    // 删除拷贝的图元
                    foreach (ObjectId objId in objs)
                    {
                        acadDatabase.Element<DBObject>(objId, true).Erase();
                    }

                    return true;
                }
            }
        }

        public override ObjectIdCollection Filter(Database database,
            ThSitePlanConfigItem configItem,
            ThSitePlanOptions options)
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
    }
}
