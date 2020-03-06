using AcHelper;
using DotNetARX;
using Linq2Acad;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using ThSitePlan.Engine;
using ThSitePlan.Configuration;
using System.Collections;
using System.Collections.Generic;
using System;

namespace ThSitePlan
{
    public class ThSitePlanApp : IExtensionApplication
    {
        public void Initialize()
        {
            //
        }

        public void Terminate()
        {
            //
        }

        [CommandMethod("TIANHUACAD", "THSP", CommandFlags.Modal)]
        public void ThSitePlan()
        {
            ObjectId originFrame = ObjectId.Null;
            var frames = new Queue<Tuple<ObjectId, Vector3d>>();
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                // 确定图框
                // 自动检测图框图层是否有设置好的图框
                originFrame = acadDatabase.Database.Frame(ThSitePlanCommon.LAYER_FRAME);
                if (originFrame.IsNull)
                {
                    // 选择图框或者绘制图框
                    PromptEntityOptions opt = new PromptEntityOptions("\n请指定图框")
                    {
                        AllowNone = true
                    };
                    opt.Keywords.Add("CREATE", "CREATE", "绘制图框(R)");
                    PromptEntityResult result = Active.Editor.GetEntity(opt);
                    if (result.Status == PromptStatus.OK)
                    {
                        originFrame = result.ObjectId;
                        // 将图框放置到指定图层
                        var ent = acadDatabase.Element<Entity>(originFrame, true);
                        ent.LayerId = acadDatabase.Database.AddLayer(ThSitePlanCommon.LAYER_FRAME);

                    }
                    else if (result.Status == PromptStatus.Keyword)
                    {
                        // 绘制图框
                        var pline = ThSitePlanFrameUtils.CreateFrame();
                        if (pline == null)
                        {
                            return;
                        }

                        // 创建图框
                        originFrame = acadDatabase.ModelSpace.Add(pline, true);
                        // 将图框放置到指定图层
                        pline.LayerId = acadDatabase.Database.AddLayer(ThSitePlanCommon.LAYER_FRAME);
                    }
                    else
                    {
                        return;
                    }
                }

                // 指定解构图集的放置区
                var frameObj = acadDatabase.Element<Polyline>(originFrame);
                Vector3d offset = offset = ThSitePlanFrameUtils.PickFrameOffset(frameObj);
                if (offset.IsZeroLength())
                {
                    return;
                }
        
                for(int i = 0; i < 5; i++)
                {
                    for(int j = 0; j < 5; j++)
                    {
                        double deltaX = frameObj.GeometricExtents.Width() * 6.0 / 5.0 * j;
                        double deltaY = frameObj.GeometricExtents.Height() * 6.0 / 5.0 * i;
                        Vector3d delta = new Vector3d(deltaX, -deltaY, 0.0);
                        Matrix3d displacement = Matrix3d.Displacement(offset + delta);
                        var objId = acadDatabase.ModelSpace.Add(frameObj.GetTransformedCopy(displacement));
                        frames.Enqueue(new Tuple<ObjectId, Vector3d>(objId, offset + delta));
                    }
                }
            }

            // 处理流程
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                ThSitePlanEngine.Instance.Containers = frames;
                ThSitePlanEngine.Instance.OriginFrame = originFrame;
                ThSitePlanEngine.Instance.Generators = new List<ThSitePlanGenerator>()
                {
                    new ThSitePlanContentGenerator(),
                    new ThSitePlanPDFGenerator(),
                    new ThSitePlaneImageGenerator()
                };
                ThSitePlanConfigService.Instance.Initialize();
                ThSitePlanEngine.Instance.Run(acadDatabase.Database, ThSitePlanConfigService.Instance.Root);
            }
        }
    }
}
