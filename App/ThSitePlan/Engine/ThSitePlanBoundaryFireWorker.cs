using AcHelper;
using Linq2Acad;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using ThSitePlan.Configuration;
using NFox.Cad.Collections;

namespace ThSitePlan.Engine
{
    public class ThSitePlanBoundaryFireWorker : ThSitePlanBoundaryWorker
    {
        public override bool DoProcess(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                // 将场地线稿“分解成”线段和圆弧
                using (var objs = Filter(database, configItem, options))
                {
                    if (objs.Count == 0)
                    {
                        return false;
                    }

                    Active.Editor.ExplodeCmd(objs);
                }

                // 获取场地外部面域
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

                    Active.Editor.ExplodeCmd(regions);
                }

                return true;
            }
        }

        /// <summary>
        /// 过滤线框内的场地线稿
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
    }
}