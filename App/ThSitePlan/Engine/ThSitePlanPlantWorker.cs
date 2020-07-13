using System.Collections.Generic;
using AcHelper;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using ThSitePlan.Configuration;
using NFox.Cad.Collections;
using Linq2Acad;
using DotNetARX;

namespace ThSitePlan.Engine
{
    public class ThSitePlanPlantWorker : ThSitePlanWorker
    {
        public override bool DoProcess(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                using (var objs = FilterByPolyline(database, configItem, options))
                {
                    if (objs.Count == 0)
                    {
                        return false;
                    }

                    // 执行EXPLODE命令将图元“分解成”线段和圆弧
                    Active.Editor.ExplodeCmd(objs);
                }

                using (var objs = Filter(database, configItem, options))
                {
                    if (objs.Count == 0)
                    {
                        return false;
                    }

                    // 执行OVERKILL命令将图元
                    Active.Editor.OverkillCmd(objs);
                }

                using (var objs = Filter(database, configItem, options))
                {
                    if (objs.Count == 0)
                    {
                        return false;
                    }

                    // 执行PEDIT命令将图元合并
                    Active.Editor.PeditCmd(objs);
                }

                using (var objs = Filter(database, configItem, options))
                {
                    if (objs.Count == 0)
                    {
                        return false;
                    }

                    // 执行MEASURE命令将图元分段
                    foreach (ObjectId obj in objs)
                    {
                        Active.Editor.MeasureCmd(obj);
                    }
                }

                using (var objs = FilterPoints(database, configItem, options))
                {
                    if (objs.Count == 0)
                    {
                        return false;
                    }

                    // 执行CIRCLE命令种树
                    int i = 0;
                    ObjectIdCollection plantids = new ObjectIdCollection();
                    foreach (ObjectId pt in objs)
                    {
                        if (++i % Properties.Settings.Default.PlantDensity != 0)
                        {
                            var center = acadDatabase.Element<DBPoint>(pt).Position;

                            Circle plantcircle = new Circle()
                            {
                                Center = acadDatabase.Element<DBPoint>(pt).Position,
                                Radius = Properties.Settings.Default.PlantRadius
                            };
                            plantids.Add(acadDatabase.ModelSpace.Add(plantcircle));
                            plantcircle.LayerId = LayerTools.AddLayer(acadDatabase.Database, ThSitePlanCommon.LAYER_TREE);
                        }
                    }
                    Active.Editor.SuperBoundaryCmd(plantids);
                }

                return true;
            }
        }

        private  ObjectIdCollection FilterByPolyline(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
        {
            ObjectId frame = (ObjectId)options.Options["Frame"];
            var layers = configItem.Properties["CADLayer"] as List<string>;
            var filter = OpFilter.Bulid(o => o.Dxf((int)DxfCode.Start) == string.Join(",", new string[] {
                RXClass.GetClass(typeof(Polyline)).DxfName}
            ) &
            o.Dxf((int)DxfCode.LayerName) == string.Join(",", layers.ToArray())
            );
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

        public override ObjectIdCollection Filter(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
        {
            ObjectId frame = (ObjectId)options.Options["Frame"];
            var layers = configItem.Properties["CADLayer"] as List<string>;
            var filter = OpFilter.Bulid(o => o.Dxf((int)DxfCode.Start) == string.Join(",", new string[] {
                RXClass.GetClass(typeof(Line)).DxfName,
                RXClass.GetClass(typeof(Polyline)).DxfName,
                RXClass.GetClass(typeof(Arc)).DxfName}
            ) &
            o.Dxf((int)DxfCode.LayerName) == string.Join(",", layers.ToArray())
            );
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

        private ObjectIdCollection FilterPoints(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
        {
            ObjectId frame = (ObjectId)options.Options["Frame"];
            var filter = OpFilter.Bulid(o => o.Dxf((int)DxfCode.Start) == string.Join(",", new string[] {
                RXClass.GetClass(typeof(DBPoint)).DxfName,
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
