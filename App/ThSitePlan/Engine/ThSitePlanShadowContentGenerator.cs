using System;
using AcHelper;
using Linq2Acad;
using System.Collections.Generic;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using ThSitePlan.Configuration;
using Autodesk.AutoCAD.Runtime;
using NFox.Cad.Collections;
using Autodesk.AutoCAD.EditorInput;
using ThCADCore;

namespace ThSitePlan.Engine
{
    public class ThSitePlanShadowContentGenerator : ThSitePlanGenerator
    {
        public override ObjectId OriginFrame { get; set; }
        public override Tuple<ObjectId, Vector3d> Frame { get; set; }
        public ThSitePlanShadowContentGenerator()
        {

        }

        private ObjectIdCollection Filter(Database database, ThSitePlanConfigItem configItem, ObjectId frame, string filtertypedxf)
        {
            var filter = OpFilter.Bulid(o => o.Dxf((int)DxfCode.Start) == filtertypedxf);
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

        public override bool Generate(Database database, ThSitePlanConfigItem configItem)
        {
            var options = new ThSitePlanOptions()
            {
                Options = new Dictionary<string, object>() {
                            { "Frame", Frame.Item1 },
                            { "Offset", Frame.Item2 },
                            { "OriginFrame", OriginFrame },
                        }
            };

            //获取当前色块填充图框
            var currenthatchframe = ThSitePlanDbEngine.Instance.FrameByName(configItem.Properties["Name"].ToString());
            //查找当前item对应的分组
            ThSitePlanConfigService.Instance.Initialize();
            var currentgroup = ThSitePlanConfigService.Instance.FindGroupByItemName(configItem.Properties["Name"].ToString());

            var scriptId = configItem.Properties["CADScriptID"].ToString();
            if (scriptId == "3" || scriptId == "4")
            { 
                //获取该阴影图框中所有Hatch, 若已经存在用户hatch不再重新生成轮廓
                using (var objs = Filter(database, configItem, currenthatchframe, RXClass.GetClass(typeof(Hatch)).DxfName))
                {
                    if (objs.Count != 0)
                    {
                        return true;
                    }
                }

                // 从同组其他图框中复制填充到阴影图框
                using (AcadDatabase acadDatabase = AcadDatabase.Active())
                {
                    //遍历当前group，逐一获取group中各个item的图框，将图框中的所有色块图层的元素拷贝到当前色块图框
                    foreach (var item in currentgroup.Items)
                    {
                        if (item is ThSitePlanConfigItem it)
                        {
                            //获取item的图框
                            var groupitemframe = ThSitePlanDbEngine.Instance.FrameByName(it.Properties["Name"].ToString());

                            //计算该图框与当前色块图框的vector3d
                            Vector3d frameoffset = acadDatabase.Database.FrameOffset(groupitemframe, currenthatchframe);

                            //将item图框中的元素拷贝到当前色块图框
                            var newoptions = new ThSitePlanOptions()
                            {
                                Options = new Dictionary<string, object>() {
                                    { "Frame", currenthatchframe },
                                    { "Offset", frameoffset },
                                    { "OriginFrame", groupitemframe },
                                }
                            };
                            var itemworker = new ThSitePlanCopyWorkerEx(new string[] {
                                RXClass.GetClass(typeof(Hatch)).DxfName,
                            });
                            itemworker.DoProcess(database, configItem, newoptions);
                        }
                    }
                }
            }

            if (scriptId == "3")
            {
                // 合并建筑填充为一个整体填充，便于后面的阴影计算
                using (AcadDatabase acadDatabase = AcadDatabase.Active())
                {
                    using (var objs = Filter(database, configItem, currenthatchframe, RXClass.GetClass(typeof(Hatch)).DxfName))
                    {
                        if (objs.Count > 0)
                        {
                            var entityobjs = new DBObjectCollection();
                            foreach (ObjectId obj in objs)
                            {
                                entityobjs.Add(acadDatabase.Element<Hatch>(obj));
                            }

                            var hatches = new ObjectIdCollection();
                            foreach (Entity obj in entityobjs.MergeHatches())
                            {
                                hatches.Add(acadDatabase.ModelSpace.Add(obj));
                            }
                            Active.Editor.CreateHatchWithRegions(hatches);
                            foreach (ObjectId obj in objs)
                            {
                                acadDatabase.Element<Hatch>(obj, true).Erase();
                            }
                        }
                    }
                }
            }
            return true;
        }
    }
}
