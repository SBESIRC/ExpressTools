using System;
using AcHelper;
using Linq2Acad;
using System.Collections.Generic;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using ThSitePlan.Configuration;
using Autodesk.AutoCAD.Runtime;

namespace ThSitePlan.Engine
{
    public class ThSitePlanShadowContentGenerator : ThSitePlanGenerator
    {
        public override ObjectId OriginFrame { get; set; }
        public override Tuple<ObjectId, Vector3d> Frame { get; set; }
        public ThSitePlanShadowContentGenerator()
        {

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

            var scriptId = configItem.Properties["CADScriptID"].ToString();
            if (scriptId == "3")
            {
                //
                using (AcadDatabase acadDatabase = AcadDatabase.Active())
                {
                    //查找当前item对应的分组
                    ThSitePlanConfigService.Instance.Initialize();
                    var currentgroup = ThSitePlanConfigService.Instance.FindGroupByItemName(configItem.Properties["Name"].ToString());

                    //获取当前色块填充图框
                    ThSitePlanDbEngine.Instance.Initialize(Active.Database);
                    var currenthatchframe = ThSitePlanDbEngine.Instance.FrameByName(configItem.Properties["Name"].ToString());

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
            return true;
        }
    }
}
