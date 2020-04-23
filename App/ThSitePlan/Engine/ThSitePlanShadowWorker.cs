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

                // 复制建筑物面域到阴影图框
                using (var objs = FilterByFrame(database, referenceFrame))
                {
                    database.CopyWithMove(objs, Matrix3d.Displacement(offset));
                }

                // 根据建筑物面域生成阴影面域
                using (var objs = FilterByFrame(database, frame))
                {
                    foreach (ObjectId objId in objs)
                    {
                        using (var buildInfo = new ThSitePlanBuilding(database, objId, frameName))
                        {
                            var shadow = ThSitePlanBuildingShadow.CreateShadow(buildInfo);
                            if (shadow != null)
                            {
                                // 计算阴影和建筑物的遮挡
                                shadow.ProjectShadow();
                            }
                        }
                    }
                }

                // 删除建筑物面域
                using (var objs = FilterByFrame(database, frame))
                {
                    foreach (ObjectId obj in objs)
                    {
                        acadDatabase.Element<Region>(obj, true).Erase();
                    }
                }

                return true;
            }
        }

        public override ObjectIdCollection Filter(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
        {
            throw new NotSupportedException();
        }

        private ObjectIdCollection FilterByFrame(Database database, ObjectId frame)
        {
            var filterlist = OpFilter.Bulid(o => 
                o.Dxf((int)DxfCode.Start) == RXClass.GetClass(typeof(Region)).DxfName &
                o.Dxf((int)DxfCode.LayerName) == ThSitePlanCommon.LAYER_BUILD_HATCH);
            PromptSelectionResult psr = Active.Editor.SelectByPolyline(
                frame,
                PolygonSelectionMode.Window,
                filterlist);
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
