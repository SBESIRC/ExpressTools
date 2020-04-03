using AcHelper;
using DotNetARX;
using Linq2Acad;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using ThSitePlan.Engine;
using ThSitePlan.Photoshop;
using ThSitePlan.Configuration;
using System.Collections;
using System.Collections.Generic;
using System;

using Photoshop;
using PsApplication = Photoshop.Application;

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
            Vector3d offset;
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
                offset = offset = ThSitePlanFrameUtils.PickFrameOffset(frameObj);
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
                        frames.Enqueue(new Tuple<ObjectId, Vector3d>(objId, delta));
                    }
                }
            }

            // 首先将原线框内的所有图元复制一份放到解构图集放置区的第一个线框里
            // 这个线框里面的图元会被移动到到解构图集放置区对应的线框中
            // 未被移走的图元将会保留在这个图框中，并作为“未标识”对象
            // 这个图框里面的块引用将会被“炸”平成基本图元后处理
            // 解构图集放置区的第一个线框
            var item = frames.Dequeue();
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                acadDatabase.Database.CopyWithMove(originFrame, offset);
                acadDatabase.Database.ExplodeToOwnerSpace(item.Item1);
            }

            // 处理流程
            ThSitePlanConfigService.Instance.Initialize();
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                ThSitePlanEngine.Instance.Containers = frames;
                ThSitePlanEngine.Instance.OriginFrame = item.Item1;
                ThSitePlanEngine.Instance.Generators = new List<ThSitePlanGenerator>()
                {
                    new ThSitePlanContentGenerator(),
                    new ThSitePlanHatchGenerator(),
                    new ThSitePlanPDFGenerator()
                };
                ThSitePlanEngine.Instance.Run(acadDatabase.Database, ThSitePlanConfigService.Instance.Root);
            }

            //PS流程
            PsApplication PsApplication = new PsApplication();
            ThSitePlanPSEngine.Instance.Generators = new List<ThSitePlanPSGenerator>()
            {
                new ThSitePlanPSDefaultGenerator(),
            };
            ThSitePlanPSEngine.Instance.PsAppInstance = PsApplication;
            ThSitePlanPSEngine.Instance.Run(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), ThSitePlanConfigService.Instance.Root);
            PsApplication.ActiveDocument.SaveAs(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            PsApplication.Quit();
        }
    }
}
