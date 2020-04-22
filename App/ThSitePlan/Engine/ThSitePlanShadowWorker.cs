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
                ThSitePlanDbEngine.Instance.Initialize(database);
                string frameName = (string)configItem.Properties["Name"];
                var frame = ThSitePlanDbEngine.Instance.FrameByName(frameName);
                var referenceFrame = ThSitePlanDbEngine.Instance.FrameByName("建筑物-场地内建筑-建筑色块");
                var offset = database.FrameOffset(referenceFrame, frame);

                // 复制建筑面域到阴影图框
                using (var objs = FilterByFrame(database, referenceFrame))
                {
                    database.CopyWithMove(objs, Matrix3d.Displacement(offset));
                }

                // 根据建筑面域生成阴影面域
                using (var objs = FilterByFrame(database, frame))
                {
                    foreach (ObjectId objId in objs)
                    {
                        using (var buildInfo = new ThSitePlanBuilding(database, objId, frameName))
                        {
                            UInt32 floor = buildInfo.Floor();
                            if (floor > 0)
                            {
                                // 创建建筑物阴影面域
                                var length = ThSitePlanCommon.shadow_length_scale * floor;
                                var angle = ThSitePlanCommon.shadow_angle * Math.PI / 180.0;
                                Matrix3d rotation = Matrix3d.Rotation(angle, Vector3d.ZAxis, Point3d.Origin);
                                var shadows = objId.CreateShadowRegion(Vector3d.XAxis.TransformBy(rotation).MultiplyBy(length));

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
            ObjectId frame = ThSitePlanDbEngine.Instance.FrameByName("建筑物-场地内建筑-建筑色块");
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

        private ObjectIdCollection FilterByFrame(Database database, ObjectId frame)
        {
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
