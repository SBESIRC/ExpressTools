using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using AcHelper;
using DotNetARX;
using Linq2Acad;
using ThSitePlan.Engine;
using ThSitePlan.Configuration;
using ThSitePlan.Photoshop;
using NFox.Cad.Collections;

namespace ThSitePlan.UI
{
    public class ThSitePlanApp : IExtensionApplication
    {
        public void Initialize()
        {
            ThSitePlanDocCollectionEventHandler.Instance.Register();
            ThSitePlanDbEventHandler.Instance.SubscribeToDb(Active.Database);
        }

        public void Terminate()
        {
            ThSitePlanDocCollectionEventHandler.Instance.UnRegister();
            ThSitePlanDbEventHandler.Instance.UnsubscribeFromDb(Active.Database);
        }

        /// <summary>
        /// 一键生成
        /// </summary>
        [CommandMethod("TIANHUACAD", "THPGE", CommandFlags.Modal)]
        public void ThSitePlanGenerate()
        {
            Vector3d offset;
            ObjectId originFrame = ObjectId.Null;
            var frames = new Queue<Tuple<ObjectId, Vector3d>>();
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                // 确定图框
                ThSitePlanDbEngine.Instance.Initialize(Active.Database);
                originFrame = ThSitePlanDbEngine.Instance.FrameByName("天华彩总原始图框");
                if (originFrame.IsValid)
                {
                    PromptKeywordOptions keywordOptions = new PromptKeywordOptions("\n检测到当前图纸中已设置原始打印图框，是否使用该图框？")
                    {
                        AllowNone = true
                    };
                    keywordOptions.Keywords.Add("Yes", "Yes", "是(Y)");
                    keywordOptions.Keywords.Add("No", "No", "否(N)");
                    keywordOptions.Keywords.Default = "Yes";
                    PromptResult userresult = Active.Editor.GetKeywords(keywordOptions);
                    if (userresult.StringResult == "No")
                    {
                        //删除之前设置的原始图框的Xdat
                        originFrame.RemoveXData(ThSitePlanCommon.RegAppName_ThSitePlan_Frame_Name);
                        originFrame = ObjectId.Null;
                    }
                }

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

                        TypedValueList valulist = new TypedValueList
                        {
                            { (int)DxfCode.ExtendedDataBinaryChunk, Encoding.UTF8.GetBytes("天华彩总原始图框") },
                        };
                        result.ObjectId.AddXData(ThSitePlanCommon.RegAppName_ThSitePlan_Frame_Name,valulist);
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

                        TypedValueList valulist = new TypedValueList
                        {
                            { (int)DxfCode.ExtendedDataBinaryChunk, Encoding.UTF8.GetBytes("天华彩总原始图框") },
                        };
                        result.ObjectId.AddXData(ThSitePlanCommon.RegAppName_ThSitePlan_Frame_Name, valulist);

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

                for (int i = 0; i < 7; i++)
                {
                    for (int j = 0; j < 7; j++)
                    {
                        double deltaX = frameObj.GeometricExtents.Width() * 6.0 / 5.0 * j;
                        double deltaY = frameObj.GeometricExtents.Height() * 6.0 / 5.0 * i;
                        Vector3d delta = new Vector3d(deltaX, -deltaY, 0.0);
                        Matrix3d displacement = Matrix3d.Displacement(offset + delta);
                        var objId = acadDatabase.ModelSpace.Add(frameObj.GetTransformedCopy(displacement));
                        frames.Enqueue(new Tuple<ObjectId, Vector3d>(objId, delta));
                    }
                }

                // 为每个框打“标签”
                foreach (var fm in frames)
                {
                    TypedValueList valueList = new TypedValueList
                    {
                                { (int)DxfCode.ExtendedDataBinaryChunk, Encoding.UTF8.GetBytes("天华彩总") },
                    };
                    fm.Item1.AddXData(ThSitePlanCommon.RegAppName_ThSitePlan_Frame_Name, valueList);
                }
            }

            // 首先将原线框内的所有图元复制一份放到解构图集放置区的第一个线框里
            // 这个线框里面的图元会被移动到到解构图集放置区对应的线框中
            // 未被移走的图元将会保留在这个图框中，并作为“未标识”对象
            // 这个图框里面的块引用将会被“炸”平成基本图元后处理
            // 解构图集放置区的第一个线框
            var playgroundFrame = frames.First();
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                acadDatabase.Database.CopyWithMove(originFrame, offset);
                acadDatabase.Database.ExplodeToOwnerSpace(playgroundFrame.Item1);
                Active.Editor.TrimCmd(acadDatabase.Element<Polyline>(playgroundFrame.Item1));
            }

            // CAD数据处理流程
            ThSitePlanConfigService.Instance.Initialize();
            ThSitePlanConfigService.Instance.EnableAll(true);
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                ThSitePlanEngine.Instance.Containers = frames;
                ThSitePlanEngine.Instance.OriginFrame = playgroundFrame.Item1;
                ThSitePlanEngine.Instance.Generators = new List<ThSitePlanGenerator>()
                {
                    new ThSitePlanFrameNameGenerator(),
                    new ThSitePlanContentGenerator(),
                    //new ThSitePlanTrimGenerator(),
                    //new ThSitePlanBoundaryGenerator(),
                    //new ThSitePlanHatchGenerator(),
                    //new ThSitePlanShadowGenerator()
                };
                ThSitePlanEngine.Instance.Run(acadDatabase.Database, ThSitePlanConfigService.Instance.Root);
            }

            // CAD衍生数据处理流程
            ThSitePlanConfigService.Instance.Initialize();
            ThSitePlanConfigService.Instance.EnableAll(true);
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var derivedframes = new Queue<Tuple<ObjectId, Vector3d>>();
                ThSitePlanDbEngine.Instance.Initialize(acadDatabase.Database);
                foreach (ObjectId frame in ThSitePlanDbEngine.Instance.Frames)
                {
                    if (!frame.Equals(originFrame))
                    {
                        derivedframes.Enqueue(new Tuple<ObjectId, Vector3d>(frame, new Vector3d(0, 0, 0)));
                    }
                }
                ThSitePlanEngine.Instance.Containers = derivedframes;
                ThSitePlanEngine.Instance.OriginFrame = ObjectId.Null;
                ThSitePlanEngine.Instance.Generators = new List<ThSitePlanGenerator>()
                {
                    new ThSitePlanDerivedContentGenerator()
                };
                ThSitePlanEngine.Instance.Run(acadDatabase.Database, ThSitePlanConfigService.Instance.Root);
            }

            // CAD种树处理流程
            ThSitePlanConfigService.Instance.Initialize();
            ThSitePlanConfigService.Instance.EnableAll(true);
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var derivedframes = new Queue<Tuple<ObjectId, Vector3d>>();
                ThSitePlanDbEngine.Instance.Initialize(acadDatabase.Database);
                foreach (ObjectId frame in ThSitePlanDbEngine.Instance.Frames)
                {
                    if (!frame.Equals(originFrame))
                    {
                        derivedframes.Enqueue(new Tuple<ObjectId, Vector3d>(frame, new Vector3d(0, 0, 0)));
                    }
                }
                ThSitePlanEngine.Instance.Containers = derivedframes;
                ThSitePlanEngine.Instance.OriginFrame = ObjectId.Null;
                ThSitePlanEngine.Instance.Generators = new List<ThSitePlanGenerator>()
                {
                    new ThSitePlanPlantGenerator()
                };
                ThSitePlanEngine.Instance.Run(acadDatabase.Database, ThSitePlanConfigService.Instance.Root);
            }


            // CAD填充处理流程
            ThSitePlanConfigService.Instance.Initialize();
            ThSitePlanConfigService.Instance.EnableAll(true);
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var Boundaryframes = new Queue<Tuple<ObjectId, Vector3d>>();
                ThSitePlanDbEngine.Instance.Initialize(acadDatabase.Database);
                foreach (ObjectId frame in ThSitePlanDbEngine.Instance.Frames)
                {
                    if (!frame.Equals(originFrame))
                    {
                        Boundaryframes.Enqueue(new Tuple<ObjectId, Vector3d>(frame, new Vector3d(0,0,0)));
                    }
                }

                ThSitePlanEngine.Instance.Containers = Boundaryframes;
                ThSitePlanEngine.Instance.OriginFrame = ObjectId.Null;
                ThSitePlanEngine.Instance.Generators = new List<ThSitePlanGenerator>()
                {
                    new ThSitePlanBoundaryGenerator()
                };
                ThSitePlanEngine.Instance.Run(acadDatabase.Database, ThSitePlanConfigService.Instance.Root);
            }

            // CAD阴影处理流程
            ThSitePlanConfigService.Instance.Initialize();
            ThSitePlanConfigService.Instance.EnableAll(true);
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var Boundaryframes = new Queue<Tuple<ObjectId, Vector3d>>();
                ThSitePlanDbEngine.Instance.Initialize(acadDatabase.Database);
                foreach (ObjectId frame in ThSitePlanDbEngine.Instance.Frames)
                {
                    if (!frame.Equals(originFrame))
                    {
                        Boundaryframes.Enqueue(new Tuple<ObjectId, Vector3d>(frame, new Vector3d(0, 0, 0)));
                    }
                }

                ThSitePlanEngine.Instance.Containers = Boundaryframes;
                ThSitePlanEngine.Instance.OriginFrame = ObjectId.Null;
                ThSitePlanEngine.Instance.Generators = new List<ThSitePlanGenerator>()
                {
                    new ThSitePlanShadowContentGenerator(),
                    new ThSitePlanShadowGenerator()
                };
                ThSitePlanEngine.Instance.Run(acadDatabase.Database, ThSitePlanConfigService.Instance.Root);
            }

            // CAD打印流程
            ThSitePlanConfigService.Instance.Initialize();
            ThSitePlanConfigService.Instance.EnableAll(true);
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var plotpdfframes = new Queue<Tuple<ObjectId, Vector3d>>();
                foreach (ObjectId frame in ThSitePlanDbEngine.Instance.Frames)
                {
                    if (!frame.Equals(originFrame))
                    {
                        plotpdfframes.Enqueue(new Tuple<ObjectId, Vector3d>(frame, new Vector3d(0, 0, 0)));
                    }
                }
                ThSitePlanEngine.Instance.Containers = plotpdfframes;
                ThSitePlanEngine.Instance.OriginFrame = ObjectId.Null;
                ThSitePlanEngine.Instance.Generators = new List<ThSitePlanGenerator>()
                {
                    new ThSitePlanPDFGenerator()
                };
                ThSitePlanEngine.Instance.Run(acadDatabase.Database, ThSitePlanConfigService.Instance.Root);
            }

            //PS处理流程
            //ThSitePlanConfigService.Instance.Initialize();
            //ThSitePlanConfigService.Instance.EnableAll(true);
            //using (var psService = new ThSitePlanPSService())
            //{
            //    if (!psService.IsValid)
            //    {
            //        Application.ShowAlertDialog("未识别到PhotoShop,请确认是否正确安装PhotoShop");
            //        return;
            //    }

            //    // 创建空白文档
            //    psService.NewEmptyDocument("MyNewDocument");

            //    // PS处理流程
            //    ThSitePlanPSEngine.Instance.Generators = new List<ThSitePlanPSGenerator>()
            //     {
            //        new ThSitePlanPSDefaultGenerator(psService),
            //     };
            //    ThSitePlanPSEngine.Instance.Run(
            //        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            //        ThSitePlanConfigService.Instance.Root);

            //    // 保存PS生成的文档
            //    psService.ExportToFile(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            //}

            //清除需要更新的图框
            ThSitePlanDbEventHandler.Instance.Clear();
        }

        /// <summary>
        /// 更新
        /// </summary>
        [CommandMethod("TIANHUACAD", "THPUD", CommandFlags.Modal | CommandFlags.UsePickSet)]
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

                //读取已经被标注的图框
                ThSitePlanDbEngine.Instance.Initialize(Active.Database);
                var unusedframe = ThSitePlanDbEngine.Instance.FrameByName("天华彩总");
                var undifineframe = ThSitePlanDbEngine.Instance.FrameByName("未识别对象");
                var originalframe = ThSitePlanDbEngine.Instance.FrameByName("天华彩总原始图框");
                Vector3d unusedtoundifineoffset = acadDatabase.Database.FrameOffset(undifineframe, unusedframe);

                var frames = new ObjectIdCollection(SelResult.Value.GetObjectIds());
                if (frames.Contains(originalframe))
                {
                    // 如果选择了原始图框，需要重新生成内容

                    //初始化图框配置
                    //这里先“关闭”所有的图框
                    //后面会根据用户的选择“打开”需要更新的图框
                    ThSitePlanConfigService.Instance.Initialize();
                    ThSitePlanConfigService.Instance.EnableAll(false);

                    //获取需要更新的图框
                    var updateframes = new Queue<Tuple<ObjectId, Vector3d>>();
                    foreach(var layer in ThSitePlanDbEventHandler.Instance.UpdatedContents)
                    {
                        var group = ThSitePlanConfigService.Instance.FindGroupByLayer(layer);
                        if (group == null)
                        {
                            continue;
                        }
                        foreach(var obj in group.Items)
                        {
                            if (obj is ThSitePlanConfigItem item)
                            {
                                var name = item.Properties["Name"] as string;
                                var frame = ThSitePlanDbEngine.Instance.FrameByName(name);
                                //获取选择框与复制框之间的偏移量
                                if (!updateframes.Where(o => o.Item1 == frame).Any())
                                {
                                    updateframes.Enqueue(new Tuple<ObjectId, Vector3d>(frame, acadDatabase.Database.FrameOffset(unusedframe, frame)));
                                }
                            }
                        }
                    }
                    if (updateframes.Count == 0)
                    {
                        return;
                    }

                    //根据用户选择，更新图框配置
                    foreach (var item in updateframes)
                    {
                        //获取所选择的框对应的图元的图层分组名
                        string selFrameName = ThSitePlanDbEngine.Instance.NameByFrame(item.Item1);

                        //打开需要的工作
                        ThSitePlanConfigService.Instance.EnableItemAndItsAncestor(selFrameName, true);
                    }

                    //首先将原线框内的所有图元复制一份放到解构图集放置区的最后一个线框里
                    var originFrame = ThSitePlanDbEngine.Instance.FrameByName("天华彩总原始图框");
                    acadDatabase.Database.CopyWithMove(originFrame, acadDatabase.Database.FrameOffset(originFrame, undifineframe) + unusedtoundifineoffset);
                    acadDatabase.Database.ExplodeToOwnerSpace(unusedframe);

                    //接着将需要更新的图框清空
                    foreach (var item in updateframes)
                    {
                        ThSitePlanDbEngine.Instance.EraseItemInFrame(item.Item1, PolygonSelectionMode.Crossing);
                    }

                    //启动CAD引擎，开始ContentGenerator 
                    ThSitePlanEngine.Instance.Containers = updateframes;
                    ThSitePlanEngine.Instance.OriginFrame = unusedframe;
                    ThSitePlanEngine.Instance.Generators = new List<ThSitePlanGenerator>()
                     {
                        new ThSitePlanContentGenerator(),
                    };
                    ThSitePlanEngine.Instance.Update(acadDatabase.Database, ThSitePlanConfigService.Instance.Root);
                    //Update后先清除初始元素Copy frame内部的图元
                    ThSitePlanDbEngine.Instance.EraseItemInFrame(unusedframe, PolygonSelectionMode.Crossing);

                    //启动CAD引擎，开始DerivedContentGenerator
                    ThSitePlanConfigService.Instance.Initialize();
                    ThSitePlanConfigService.Instance.EnableAll(false);
                    foreach (var item in updateframes)
                    {
                        //获取所选择的框对应的图元的图层分组名
                        string selFrameName = ThSitePlanDbEngine.Instance.NameByFrame(item.Item1);

                        //打开需要的工作
                        ThSitePlanConfigService.Instance.EnableItemAndItsAncestor(selFrameName, true);
                    }
                    ThSitePlanEngine.Instance.Generators = new List<ThSitePlanGenerator>()
                     {
                        new ThSitePlanDerivedContentGenerator(),
                    };
                    ThSitePlanEngine.Instance.Update(acadDatabase.Database, ThSitePlanConfigService.Instance.Root);

                    //启动CAD引擎，开始BoundaryGenerator
                    ThSitePlanConfigService.Instance.Initialize();
                    ThSitePlanConfigService.Instance.EnableAll(false);
                    foreach (var item in updateframes)
                    {
                        //获取所选择的框对应的图元的图层分组名
                        string selFrameName = ThSitePlanDbEngine.Instance.NameByFrame(item.Item1);

                        //打开需要的工作
                        ThSitePlanConfigService.Instance.EnableItemAndItsAncestor(selFrameName, true);
                    }
                    ThSitePlanEngine.Instance.Generators = new List<ThSitePlanGenerator>()
                     {
                        new ThSitePlanBoundaryGenerator(),
                    };
                    ThSitePlanEngine.Instance.Update(acadDatabase.Database, ThSitePlanConfigService.Instance.Root);

                    //启动CAD引擎，开始ShadowGenerator
                    ThSitePlanConfigService.Instance.Initialize();
                    ThSitePlanConfigService.Instance.EnableAll(false);
                    foreach (var item in updateframes)
                    {
                        //获取所选择的框对应的图元的图层分组名
                        string selFrameName = ThSitePlanDbEngine.Instance.NameByFrame(item.Item1);

                        //打开需要的工作
                        ThSitePlanConfigService.Instance.EnableItemAndItsAncestor(selFrameName, true);
                    }
                    ThSitePlanEngine.Instance.Generators = new List<ThSitePlanGenerator>()
                     {
                        new ThSitePlanShadowGenerator()
                    };
                    ThSitePlanEngine.Instance.Update(acadDatabase.Database, ThSitePlanConfigService.Instance.Root);

                    //启动CAD引擎，开始PDFGenerator
                    ThSitePlanConfigService.Instance.Initialize();
                    ThSitePlanConfigService.Instance.EnableAll(false);
                    foreach (var item in updateframes)
                    {
                        //获取所选择的框对应的图元的图层分组名
                        string selFrameName = ThSitePlanDbEngine.Instance.NameByFrame(item.Item1);

                        //打开需要的工作
                        ThSitePlanConfigService.Instance.EnableItemAndItsAncestor(selFrameName, true);
                    }
                    ThSitePlanEngine.Instance.Generators = new List<ThSitePlanGenerator>()
                     {
                        new ThSitePlanPDFGenerator()
                    };
                    ThSitePlanEngine.Instance.Update(acadDatabase.Database, ThSitePlanConfigService.Instance.Root);

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

                    //PS处理流程
                    using (var psService = new ThSitePlanPSService())
                    {
                        if (!psService.IsValid)
                        {
                            Application.ShowAlertDialog("未识别到PhotoShop,请确认是否正确安装PhotoShop");
                            return;
                        }

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

                    //清除需要更新的图框
                    ThSitePlanDbEventHandler.Instance.Clear();
                }
                else
                {
                    // 如果只选择解构图框，则只需要打印内容

                    Queue<Tuple<ObjectId, Vector3d>> cadupdateframe = new Queue<Tuple<ObjectId, Vector3d>>();
                    foreach (ObjectId frameid in frames)
                    {
                        Tuple<ObjectId, Vector3d> frametuple = new Tuple<ObjectId, Vector3d>(frameid, new Vector3d(0, 0, 0));
                        cadupdateframe.Enqueue(frametuple);
                    }

                    //初始化图框配置
                    //这里先“关闭”所有的图框
                    //后面会根据用户的选择“打开”需要更新的图框
                    ThSitePlanConfigService.Instance.Initialize();
                    ThSitePlanConfigService.Instance.EnableAll(false);

                    //根据用户选择，更新图框配置
                    foreach (var item in cadupdateframe)
                    {
                        //获取所选择的框对应的图元的图层分组名
                        string selFrameName = ThSitePlanDbEngine.Instance.NameByFrame(item.Item1);

                        //打开需要的工作
                        ThSitePlanConfigService.Instance.EnableItemAndAncestorNoSib(selFrameName, true);
                    }

                    //启动CAD引擎，开始更新 
                    ThSitePlanEngine.Instance.Containers = cadupdateframe;
                    ThSitePlanEngine.Instance.Generators = new List<ThSitePlanGenerator>()
                    {
                        new ThSitePlanPDFGenerator()
                    };
                    ThSitePlanEngine.Instance.Update(acadDatabase.Database, ThSitePlanConfigService.Instance.Root);

                    //初始化图框配置
                    //这里先“关闭”所有的图框
                    //后面会根据用户的选择“打开”需要更新的图框
                    ThSitePlanConfigService.Instance.Initialize();
                    ThSitePlanConfigService.Instance.EnableAll(false);

                    Queue<Tuple<ObjectId, Vector3d>> psupdateframe = new Queue<Tuple<ObjectId, Vector3d>>();
                    foreach (ObjectId frameid in SelResult.Value.GetObjectIds())
                    {
                        Tuple<ObjectId, Vector3d> frametuple = new Tuple<ObjectId, Vector3d>(frameid, new Vector3d(0, 0, 0));
                        psupdateframe.Enqueue(frametuple);
                    }

                    //根据用户选择，更新图框配置
                    foreach (var item in psupdateframe)
                    {
                        //获取所选择的框对应的图元的图层分组名
                        string selFrameName = ThSitePlanDbEngine.Instance.NameByFrame(item.Item1);

                        //打开需要的工作
                        ThSitePlanConfigService.Instance.EnableItemAndAncestorNoSib(selFrameName, true);
                    }

                    //PS处理流程
                    using (var psService = new ThSitePlanPSService())
                    {
                        if (!psService.IsValid)
                        {
                            Application.ShowAlertDialog("未识别到PhotoShop,请确认是否正确安装PhotoShop");
                            return;
                        }

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

        /// <summary>
        /// 映射配置器
        /// </summary>
        [CommandMethod("TIANHUACAD", "THPCF", CommandFlags.Modal)]
        public void ThSitePlanConfiguration()
        {
            using (var dlg = new fmConfigManage())
            {
                // 获取当前图纸中的配置
                ThSitePlanConfigService.Instance.Initialize();
                dlg.m_ColorGeneralConfig = ThSitePlanConfigService.Instance.RootJsonString;

                // 弹出配置界面
                var ConfigFormResult = Application.ShowModalDialog(dlg);
                if (ConfigFormResult == System.Windows.Forms.DialogResult.OK)
                {
                    using (AcadDatabase acadDatabase = AcadDatabase.Active())
                    {
                        // 创建一个XRecord
                        ResultBuffer rb = new ResultBuffer(new TypedValue((int)DxfCode.XTextString, dlg.m_ColorGeneralConfig));
                        Xrecord configrecord = new Xrecord()
                        {
                            Data = rb,
                        };

                        // 将XRecord添加到NOD中
                        DBDictionary dbdc = acadDatabase.Element<DBDictionary>(acadDatabase.Database.NamedObjectsDictionaryId, true);
                        dbdc.SetAt(ThSitePlanCommon.Configuration_Xrecord_Name, configrecord);
                        acadDatabase.AddNewlyCreatedDBObject(configrecord);
                    }
                }
            }
        }

        /// <summary>
        /// 脚本控制器
        /// </summary>
        [CommandMethod("TIANHUACAD", "THPOP", CommandFlags.Modal)]
        public void ThSitePlanOptions()
        {
            using (var dlg = new ThSitePlanForm())
            {
                Application.ShowModalDialog(dlg);
            }
        }
    }
}

