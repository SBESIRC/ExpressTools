using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using ThSitePlan.Configuration;
using Linq2Acad;
using AcHelper;
using Autodesk.AutoCAD.EditorInput;

namespace ThSitePlan.Engine
{
    public class ThSitePlanTrimGenerator : ThSitePlanGenerator
    {
        public override ObjectId OriginFrame { get; set; }
        public override Tuple<ObjectId, Vector3d> Frame { get; set; }
        private Dictionary<string, ThSitePlanWorker> Workers { get; set; }

        public ThSitePlanTrimGenerator()
        {
            //
        }

        public override bool Generate(Database database, ThSitePlanConfigItem configItem)
        {
            var options = new ThSitePlanOptions()
            {
                Options = new Dictionary<string, object>()
                {
                    {"Frame", Frame.Item1},
                    {"Offset",  Frame.Item2},
                    {"OriginFrame", OriginFrame},
                }
            };
            
            //获取当前需要处理的图框
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                  ThSitePlanDbEngine.Instance.Initialize(Active.Database);
                  var currenthatchframe = ThSitePlanDbEngine.Instance.FrameByName(configItem.Properties["Name"].ToString());

                  //分别获取当前处理图框的Window选择集和Crossing选择集
                  PromptSelectionResult windowprs = Active.Editor.SelectByPolyline(
                        currenthatchframe,
                        PolygonSelectionMode.Window,
                        null);
                  PromptSelectionResult crossingprs = Active.Editor.SelectByPolyline(
                        currenthatchframe,
                        PolygonSelectionMode.Crossing,
                        null);

                  //若两个选择集之差为空或仅有一个图框元素，不执行trim
                  var difselection = crossingprs.Value.GetObjectIds().Except(windowprs.Value.GetObjectIds()).ToList();
                  if (difselection.Count == 0 || (difselection.Count == 1 && difselection.First().Equals(currenthatchframe)))
                  {
                        return true;
                  }

                  var worker = new ThSitePlanTrimWorker();
                  worker.DoProcess(database, configItem, options);
            }
            
            return true;
        }

    }
}
