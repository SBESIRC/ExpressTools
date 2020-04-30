﻿using AcHelper;
using Linq2Acad;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using NFox.Cad.Collections;
using ThSitePlan.Configuration;

namespace ThSitePlan.Engine
{
    public class ThSitePlanBoundaryOuterBuildingWorker : ThSitePlanBoundaryWorker
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

                    Active.Editor.BoundaryCmdEx(objs);
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

                //// 获取建筑外部面域
                //using (var regions = FilterRegion(database, configItem, options))
                //{
                //    if (regions.Count == 0)
                //    {
                //        return false;
                //    }

                //    Active.Editor.UnionRegions(regions);
                //}
                //using (var regions = FilterRegion(database, configItem, options))
                //{
                //    if (regions.Count == 0)
                //    {
                //        return false;
                //    }

                //    Active.Editor.ExplodeCmd(regions);
                //}

                //// 将建筑外部面域移到指定的图层
                //using (var regions = FilterRegion(database, configItem, options))
                //{
                //    if (regions.Count == 0)
                //    {
                //        return false;
                //    }

                //    acadDatabase.Database.MoveToLayer(regions, ThSitePlanCommon.LAYER_BUILD_HATCH);
                //}

                return true;
            }
        }

        public override ObjectIdCollection Filter(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
        {
            ObjectId frame = (ObjectId)options.Options["Frame"];
            var filter = OpFilter.Bulid(o => o.Dxf((int)DxfCode.LayerName) == ThSitePlanCommon.LAYER_BUILD_OUT);
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