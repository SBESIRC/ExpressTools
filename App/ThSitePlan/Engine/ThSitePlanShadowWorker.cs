using System;
using AcHelper;
using Linq2Acad;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using NFox.Cad.Collections;
using ThSitePlan.Configuration;

namespace ThSitePlan.Engine
{
    public class ThSitePlanShadowWorker : ThSitePlanWorker
    {
        public override bool DoProcess(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                string frameName = (string)configItem.Properties["Name"];
                using (var objs = Filter(database, configItem, options))
                {
                    foreach (ObjectId objId in objs)
                    {
                        using (var buildInfo = new ThSitePlanBuilding(database, objId, frameName))
                        {
                            UInt32 floor = buildInfo.Floor();
                            if (floor > 0)
                            {
                                // 创建建筑物阴影面域
                                var shadows = objId.CreateShadowRegion(new Vector3d(-5, 5, 0));

                                // 设置建筑物阴影面域图层
                                acadDatabase.Database.MoveToLayer(shadows, ThSitePlanCommon.LAYER_GLOBAL_SHADOW);

                                //var shadowRegion = acadDatabase.Element<Region>(shadows[0]);
                                //var buildingRegion = acadDatabase.Element<Region>(objId, true);
                                //// 将阴影Region和建筑物阴影做Union
                                //shadowRegion.BooleanOperation(BooleanOperationType.BoolUnite, buildingRegion);
                                //// 在阴影Region中寻找可能存在的遮挡的建筑
                                //var filter = OpFilter.Bulid(o => o.Dxf((int)DxfCode.Start) == string.Join(",", new string[]
                                //{
                                //    RXClass.GetClass(typeof(Region)).DxfName,
                                //}));
                                //PromptSelectionResult psr = Active.Editor.SelectByRegion(
                                //    shadowRegion.ObjectId,
                                //    PolygonSelectionMode.Crossing,
                                //    filter);
                                //if (psr.Status == PromptStatus.OK)
                                //{
                                //    foreach(ObjectId objId2 in psr.Value.GetObjectIds())
                                //    {
                                //        //using (var buildInfo2 = new ThSitePlanBuilding(database, objId2, frameName))
                                //        //{
                                //        //    if (buildInfo2.Floor() < floor * 0.5)
                                //        //    {
                                //        //        var buildingRegion2 = acadDatabase.Element<Region>(objId2);
                                //        //        shadowRegion.BooleanOperation(BooleanOperationType.BoolSubtract, buildingRegion2);
                                //        //    }
                                //        //}
                                //    }
                                //}
                            }
                        }
                    }
                }

                return true;
            }
        }

        public override ObjectIdCollection Filter(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
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
