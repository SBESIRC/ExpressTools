using System;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using ThSitePlan.Configuration;

namespace ThSitePlan.Engine {
      /// <summary>
      /// 解构图集生成器
      /// </summary>
      public class ThSitePlanContentGenerator : ThSitePlanGenerator {
            public override ObjectId OriginFrame { get; set; }
            public override Tuple<ObjectId, Vector3d> Frame { get; set; }
            public ThSitePlanContentGenerator () {
                  //
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
            var worker = new ThSitePlanCopyWorker();
            worker.DoProcess(database, configItem, options);

            return true;
        }
      }
}