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
    public class ThSitePlanSimpleShadowWorker : ThSitePlanWorker
    {
        public override bool DoProcess(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                ThSitePlanDbEngine.Instance.Initialize(database);
                string frameName = (string)configItem.Properties["Name"];

                // 根据目标填充生成阴影填充
                using (var objs = Filter(database, configItem, options))
                {
                    if (objs.Count == 0)
                    {
                        return false;
                    }

                    using (ProgressMeter pm = new ProgressMeter())
                    {
                        // 启动进度条
                        pm.SetLimit(objs.Count);
                        pm.Start("正在生成树木阴影");

                        foreach (ObjectId objId in objs)
                        {
                            using (var buildInfo = new ThSitePlanBuilding(database, objId, frameName))
                            {
                                // 创建简易的阴影填充
                                var shadow = ThSitePlanBuildingShadow.CreateSimpleShadow(buildInfo, 2);

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

                // 删除原目标填充
                // 这里利用了CAD的一个Bug：
                //  用代码新创建的对象，不能立即被Editor.SelectXX()选中；用命令新创建的对象却可以
                // 所以这里用Editor.SelectXX()选中的填充都是原目标填充，正好是我们需要的
                using (var objs = Filter(database, configItem, options))
                {
                    foreach(ObjectId obj in objs)
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
            var filter = OpFilter.Bulid(o => o.Dxf((int)DxfCode.Start) == RXClass.GetClass(typeof(Hatch)).DxfName);
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
