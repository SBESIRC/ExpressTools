using System;
using System.Linq;
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
using NFox.Cad.Collections;

namespace ThSitePlan
{
    public class ThSitePlanApp : IExtensionApplication
    {
        public Tuple<ObjectId, Vector3d> OriginFrame { get; set; }
        public Tuple<ObjectId, Vector3d> OriginFrameCopy { get; set; }
        public void Initialize()
        {
            //
        }

        public void Terminate()
        {
            //
        }

        [CommandMethod("TIANHUACAD", "THSPSET", CommandFlags.Modal)]
        public void ThSitePlanSet()
        {
            ThSitePlanForm SpForm = new ThSitePlanForm();
            SpForm.ShowInTaskbar = true;
            SpForm.Show();
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
                    for(int j = 0; j < 6; j++)
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
            ThSitePlanConfigService.Instance.EnableAll(true);
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
            ThSitePlanConfigService.Instance.EnableAll(true);
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

        [CommandMethod("TIANHUACAD", "THSPUPD", CommandFlags.Modal | CommandFlags.UsePickSet)]
        public void ThSitePlanUpdate()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                PromptSelectionOptions options = new PromptSelectionOptions()
                {
                    AllowDuplicates = false,
                    RejectObjectsOnLockedLayers = true,
                };
                var filterlist = OpFilter.Bulid(o =>
                    o.Dxf((int)DxfCode.Start) == RXClass.GetClass(typeof(Polyline)).DxfName &
                    o.Dxf((int)DxfCode.LayerName) == ThSitePlanCommon.LAYER_FRAME &
                    o.Dxf((int)DxfCode.ExtendedDataRegAppName) == ThSitePlanCommon.RegAppName_ThSitePlan_Frame_Name);
                PromptSelectionResult SelResult = Active.Editor.GetSelection(options, filterlist);
                if (SelResult.Status != PromptStatus.OK)
                {
                    return;
                }

                //TODO:
                //选择一个合适的位置作为新的图框
                //将初始图纸中内容复制到后面一个空的图框中
                var newframes = ThSitePlanEngine.Instance.Containers;
                if (OriginFrameCopy.IsNull())
                {
                    OriginFrameCopy = newframes.Dequeue();
                }
                
                //初始化图框配置
                //这里先“关闭”所有的图框
                //后面会根据用户的选择“打开”需要更新的图框
                ThSitePlanConfigService.Instance.Initialize();
                ThSitePlanConfigService.Instance.EnableAll(false);

                //读取已经被标注的图框
                ThSitePlanDbEngine.Instance.Initialize(Active.Database);

                //获取需要更新的图框
                var updateframes = new Queue<Tuple<ObjectId, Vector3d>>();
                foreach (ObjectId SelframeId in SelResult.Value.GetObjectIds())
                {
                    //找到图框的兄弟
                    string selFrameName = ThSitePlanDbEngine.Instance.NameByFrame(SelframeId);
                    var group = ThSitePlanConfigService.Instance.FindGroupByItemName(selFrameName);
                    if (group == ThSitePlanConfigService.Instance.Root)
                    {
                        ThSitePlanConfigItem CurrentItem = ThSitePlanConfigService.Instance.FindItemByName(selFrameName);

                        var name = CurrentItem.Properties["Name"] as string;
                        var frame = ThSitePlanDbEngine.Instance.FrameByName(name);
                        //获取选择框与复制框之间的偏移量
                        var SelFramenPol = acadDatabase.Element<Polyline>(frame);
                        var PolOgFmCp = acadDatabase.Element<Polyline>(OriginFrameCopy.Item1);
                        Vector3d SelToCopyFrame = SelFramenPol.GeometricExtents.MaxPoint - PolOgFmCp.GeometricExtents.MaxPoint;
                        updateframes.Enqueue(new Tuple<ObjectId, Vector3d>(frame, SelToCopyFrame));
                    }
                    else
                    {
                        foreach (ThSitePlanConfigItem item in group.Items)
                        {
                            var name = item.Properties["Name"] as string;
                            var frame = ThSitePlanDbEngine.Instance.FrameByName(name);
                            if (updateframes.Where(o => o.Item1 == frame).Any())
                            {
                                continue;
                            }
                            //获取选择框与复制框之间的偏移量
                            var SelFramenPol = acadDatabase.Element<Polyline>(frame);
                            var PolOgFmCp = acadDatabase.Element<Polyline>(OriginFrameCopy.Item1);
                            Vector3d SelToCopyFrame = SelFramenPol.GeometricExtents.MaxPoint - PolOgFmCp.GeometricExtents.MaxPoint;
                            updateframes.Enqueue(new Tuple<ObjectId, Vector3d>(frame, SelToCopyFrame));
                        }
                    }
                }

                //为了保证后面的更新是按照正确的顺序进行
                //将获取的需要更新的图框排序（从左到右，从上到下）
                Queue<Tuple<ObjectId, Vector3d>> SortedUpdateFrames = new Queue<Tuple<ObjectId, Vector3d>>();
                foreach (var item in updateframes.OrderByDescending(i => i.Item2.Y).GroupBy(i => i.Item2.Y))
                {
                    var odgp = item.OrderBy(p => p.Item2.X);
                    for (int i = 0; i < odgp.Count(); i++)
                    {
                        SortedUpdateFrames.Enqueue(odgp.ElementAt(i));
                    }
                }

                //根据用户选择，更新图框配置
                foreach (var item in SortedUpdateFrames)
                {
                    //获取所选择的框对应的图元的图层分组名
                    string selFrameName = ThSitePlanDbEngine.Instance.NameByFrame(item.Item1);

                    //打开需要的工作
                    ThSitePlanConfigService.Instance.EnableItemAndItsAncestor(selFrameName, true);
                }

                //首先将原线框内的所有图元复制一份放到解构图集放置区的最后一个线框里
                acadDatabase.Database.CopyWithMove(OriginFrame.Item1, OriginFrame.Item2 + OriginFrameCopy.Item2);
                acadDatabase.Database.ExplodeToOwnerSpace(OriginFrameCopy.Item1);

                //接着将需要更新的图框清空
                foreach (var item in SortedUpdateFrames)
                {
                    ThSitePlanDbEngine.Instance.EraseItemInFrame(item.Item1, PolygonSelectionMode.Window);
                }

                //启动CAD引擎，开始更新 
                ThSitePlanEngine.Instance.Containers = SortedUpdateFrames;
                ThSitePlanEngine.Instance.OriginFrame = OriginFrameCopy.Item1;
                ThSitePlanEngine.Instance.Generators = new List<ThSitePlanGenerator>()
                 {
                    new ThSitePlanContentGenerator(),
                    new ThSitePlanBoundaryGenerator(),
                    new ThSitePlanHatchGenerator(),
                    new ThSitePlanShadowGenerator(),
                    new ThSitePlanPDFGenerator()
                 };
                ThSitePlanEngine.Instance.Run(acadDatabase.Database, ThSitePlanConfigService.Instance.Root);
                //Update后先清除初始元素Copy frame内部的图元
                ThSitePlanDbEngine.Instance.EraseItemInFrame(OriginFrameCopy.Item1, PolygonSelectionMode.Crossing);

                //初始化图框配置
                //这里先“关闭”所有的图框
                //后面会根据用户的选择“打开”需要更新的图框
                ThSitePlanConfigService.Instance.Initialize();
                ThSitePlanConfigService.Instance.EnableAll(false);

                //根据用户选择，更新图框配置
                foreach (var item in updateframes)
                {
                    //获取所选择的框对应的图元的图层分组名
                    string selFrameName = ThSitePlanDbEngine.Instance.NameByFrame(item.Item1);

                    //打开需要的工作
                    ThSitePlanConfigService.Instance.EnableItemAndItsAncestor(selFrameName, true);
                }

                using (var psService = new ThSitePlanPSService())
                {
                    //启动PS引擎，开始更新 
                    ThSitePlanPSEngine.Instance.Generators = new List<ThSitePlanPSGenerator>()
                     {
                        new ThSitePlanPSDefaultGenerator(psService),
                     };
                    ThSitePlanPSEngine.Instance.PSUpdate(
                        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                        ThSitePlanConfigService.Instance.Root);

                    // 保存PS生成的文档
                    psService.ExportToFile(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
                }
            }
        }
    }
}
