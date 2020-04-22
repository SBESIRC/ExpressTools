using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using AcHelper;
using DotNetARX;
using Linq2Acad;
using ThSitePlan.Engine;
using ThSitePlan.Configuration;
using ThSitePlan.Photoshop;

namespace ThSitePlan
{
    public class ThSitePlanApp : IExtensionApplication
    {
        public Tuple<ObjectId, Vector3d> OriginFrame { get; set; }
        public Tuple<ObjectId, Vector3d> OriginFrameCopy { get; set; }
        string SelFrameName { get; set; }
        public void Initialize()
        {
            //
        }

        public void Terminate()
        {
            //
        }

        [CommandMethod("TIANHUACAD", "THSPUPD", CommandFlags.Modal)]
        public void ThSitePlanUpdate()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                PromptEntityOptions frasel = new PromptEntityOptions("\n请选择要更新的图框")
                {
                    AllowNone = true
                };
                PromptEntityResult SelResult = Active.Editor.GetEntity(frasel);
                if (SelResult.Status != PromptStatus.OK)
                {
                    return;
                }

                //将初始图纸中内容复制到后面一个空的Frame中
                var newframes = ThSitePlanEngine.Instance.Containers;
                if (OriginFrameCopy.IsNull())
                {
                    OriginFrameCopy = newframes.Dequeue();
                }

                // 首先将原线框内的所有图元复制一份放到解构图集放置区的最后一个线框里
                ObjectId SelframeId = SelResult.ObjectId;
                var SelFramenPol = acadDatabase.Element<Polyline>(SelframeId);
                var PolOgFmCp = acadDatabase.Element<Polyline>(OriginFrameCopy.Item1);
                Vector3d SelToCopyFrame = SelFramenPol.GeometricExtents.MaxPoint - PolOgFmCp.GeometricExtents.MaxPoint;
                acadDatabase.Database.CopyWithMove(OriginFrame.Item1, OriginFrame.Item2 + OriginFrameCopy.Item2);
                acadDatabase.Database.ExplodeToOwnerSpace(OriginFrameCopy.Item1);

                //获取所选择的框对应的图元的图层分组名
                ThSitePlanDbEngine.Instance.Initialize(Active.Database);
                ThSitePlanDbEngine.Instance.EraseItemInFrame(SelframeId);
                SelFrameName = ThSitePlanDbEngine.Instance.NameByFrame(SelframeId);

                //根据图层名在ThSitePlanConfigService找到对应的ConfigItem
                ThSitePlanConfigService.Instance.Initialize();
                ThSitePlanConfigItemGroup UpdateConfigItemGroup = new ThSitePlanConfigItemGroup();
                UpdateConfigItemGroup.Items.Enqueue(ThSitePlanConfigService.Instance.FindItemByName(SelFrameName));

                // Update CAD处理流程
                var updateframes = new Queue<Tuple<ObjectId, Vector3d>>();
                updateframes.Enqueue(new Tuple<ObjectId, Vector3d>(SelframeId, SelToCopyFrame));

                ThSitePlanEngine.Instance.Containers = updateframes;
                ThSitePlanEngine.Instance.OriginFrame = OriginFrameCopy.Item1;
                ThSitePlanEngine.Instance.Generators = new List<ThSitePlanGenerator>()
                        {
                            new ThSitePlanContentGenerator(),
                            new ThSitePlanHatchGenerator(),
                            new ThSitePlanPDFGenerator()
                        };
                ThSitePlanEngine.Instance.Run(acadDatabase.Database, UpdateConfigItemGroup);
                //每次Update后先清除初始元素Copy frame内部的图元
                //PromptSelectionResult TextUpFrame = Active.Editor.SelectByPolyline(OriginFrameCopy.Item1, PolygonSelectionMode.Crossing, null);
            }

            //// Update PS处理流程
            //ThSitePlanConfigService.Instance.Initialize();
            //ThSitePlanConfigItemGroup UpdatePSConfigItemGroup = new ThSitePlanConfigItemGroup();
            //UpdatePSConfigItemGroup.Items.Enqueue(ThSitePlanConfigService.Instance.FindItemByName(SelFrameName));
            //using (var psService = new ThSitePlanPSService())
            //{
            //    // PS处理流程
            //    ThSitePlanPSEngine.Instance.Generators = new List<ThSitePlanPSGenerator>()
            //             {
            //                new ThSitePlanPSDefaultGenerator(psService),
            //             };
            //    ThSitePlanPSEngine.Instance.Run(
            //        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            //        UpdatePSConfigItemGroup);

            //    // 保存PS生成的文档
            //    psService.ExportToFile(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            //}
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
                offset = ThSitePlanFrameUtils.PickFrameOffset(frameObj);
                if (offset.IsZeroLength())
                {
                    return;
                }
        
                for(int i = 0; i < 7; i++)
                {
                    for(int j = 0; j < 7; j++)
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

            // 记录原始图框和偏移
            OriginFrame = new Tuple<ObjectId, Vector3d>(originFrame, offset);

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

            // CAD处理流程
            ThSitePlanConfigService.Instance.Initialize();
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                ThSitePlanEngine.Instance.Containers = frames;
                ThSitePlanEngine.Instance.OriginFrame = item.Item1;
                ThSitePlanEngine.Instance.Generators = new List<ThSitePlanGenerator>()
                {
                    new ThSitePlanFrameNameGenerator(),
                    new ThSitePlanContentGenerator(),
                    new ThSitePlanBoundaryGenerator(),
                    new ThSitePlanHatchGenerator(),
                    new ThSitePlanShadowGenerator(),
                    new ThSitePlanPDFGenerator()
                };
                ThSitePlanEngine.Instance.Run(acadDatabase.Database, ThSitePlanConfigService.Instance.Root);
            }

            // PS处理流程
            ThSitePlanConfigService.Instance.Initialize();
            using (var psService = new ThSitePlanPSService())
            {
                // 创建空白文档
                psService.NewEmptyDocument("MyNewDocument");

                // PS处理流程
                ThSitePlanPSEngine.Instance.Generators = new List<ThSitePlanPSGenerator>()
                 {
                    new ThSitePlanPSDefaultGenerator(psService),
                 };
                ThSitePlanPSEngine.Instance.Run(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    ThSitePlanConfigService.Instance.Root);

                // 保存PS生成的文档
                psService.ExportToFile(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            }
        }
    }
}