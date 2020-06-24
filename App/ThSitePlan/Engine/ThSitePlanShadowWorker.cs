using AcHelper;
using Linq2Acad;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using NFox.Cad.Collections;
using ThSitePlan.Configuration;
using System.Windows.Forms;

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

                // 分解复杂的填充为简单填充
                using (var objs = FilterHatch(database, configItem, options))
                {
                    if (objs.Count == 0)
                    {
                        return false;
                    }

                    Active.Editor.HatchDecomposeCmd(objs);
                }

                // 获得其轮廓线
                using (var objs = FilterHatch(database, configItem, options))
                {
                    if (objs.Count == 0)
                    {
                        return false;
                    }

                    Active.Editor.HatchBoundaryCmd(objs);
                }

                // 删除Hatch，只保留其轮廓线
                using (var objs = FilterHatch(database, configItem, options))
                {
                    if (objs.Count == 0)
                    {
                        return false;
                    }

                    Active.Editor.EraseCmd(objs);
                }

                //// 剔除所有内部的面域
                //using (var objs = FilterRegion(database, configItem, options))
                //{
                //    if (objs.Count == 0)
                //    {
                //        return false;
                //    }

                //    Active.Editor.UnionRegions(objs);
                //}
                //using (var objs = FilterRegion(database, configItem, options))
                //{
                //    if (objs.Count == 0)
                //    {
                //        return false;
                //    }

                //    Active.Editor.ExplodeCmd(objs);
                //}

                // 设置建筑物面域图层
                using (var objs = FilterRegion(database, configItem, options))
                {
                    if (objs.Count == 0)
                    {
                        return false;
                    }

                    acadDatabase.Database.MoveToLayer(objs, ThSitePlanCommon.LAYER_BUILD_HATCH);
                }

                // 根据建筑物面域生成阴影面域
                using (var objs = FilterRegion(database, configItem, options))
                {
                    // 启动进度条
                    using (ProgressMeter pm = new ProgressMeter())
                    {
                        pm.SetLimit(objs.Count);
                        pm.Start("正在生成建筑阴影");

                        foreach (ObjectId objId in objs)
                        {
                            using (var buildInfo = new ThSitePlanBuilding(database, objId, frameName))
                            {
                                var shadow = ThSitePlanBuildingShadow.CreateShadow(buildInfo);
                                if (shadow != null)
                                {
                                    // 计算阴影和其他建筑物的遮挡
                                    shadow.ProjectShadow(buildInfo);
                                }
                                else
                                {
                                    // 创建简易的阴影面域
                                    shadow = ThSitePlanBuildingShadow.CreateSimpleShadow(buildInfo, 3);
                                    shadow.Regions[0].CreateHatchWithPolygon();
                                }
                                // 更新进度条
                                pm.MeterProgress();
                                // 让CAD在长时间任务处理时任然能接收消息
                                Application.DoEvents();
                            }
                        }

                        // 停止进度条
                        pm.Stop();
                    }
                }

                // 删除建筑物面域
                using (var objs = FilterRegion(database, configItem, options))
                {
                    foreach (ObjectId obj in objs)
                    {
                        acadDatabase.Element<Entity>(obj, true).Erase();
                    }
                }

                return true;
            }
        }

        public override ObjectIdCollection Filter(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
        {
            ObjectId frame = (ObjectId)options.Options["Frame"];
            var filter = OpFilter.Bulid(o => o.Dxf((int)DxfCode.Start) != RXClass.GetClass(typeof(Hatch)).DxfName);
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

        private ObjectIdCollection FilterRegion(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
        {
            ObjectId frame = (ObjectId)options.Options["Frame"];
            var filter = OpFilter.Bulid(o => o.Dxf((int)DxfCode.Start) == RXClass.GetClass(typeof(Region)).DxfName);
            PromptSelectionResult psr = Active.Editor.SelectByPolyline(
                frame,
                PolygonSelectionMode.Crossing,
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

        private ObjectIdCollection FilterRegion(ObjectId frame)
        {
            var filter = OpFilter.Bulid(o => o.Dxf((int)DxfCode.Start) == RXClass.GetClass(typeof(Region)).DxfName);
            PromptSelectionResult psr = Active.Editor.SelectByRegion(
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

        private ObjectIdCollection FilterHatch(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
        {
            ObjectId frame = (ObjectId)options.Options["Frame"];
            var filter = OpFilter.Bulid(o => o.Dxf((int)DxfCode.Start) == RXClass.GetClass(typeof(Hatch)).DxfName);
            PromptSelectionResult psr = Active.Editor.SelectByPolyline(
                frame,
                PolygonSelectionMode.Crossing,
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
