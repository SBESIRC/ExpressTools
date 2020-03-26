using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Linq2Acad;
using AcHelper;
using ThSitePlan.Configuration;

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
                    foreach (DBObject dbObj in boundaries)
                    {
                        if (dbObj is Entity boundary)
                        {
                            acadDatabase.ModelSpace.Add(boundary);
                        }

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
