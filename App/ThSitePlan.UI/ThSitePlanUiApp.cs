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
using Autodesk.AutoCAD.ApplicationServices;
using System.IO;
using System.Text;

namespace ThSitePlan.UI
{
    public class ThSitePlanApp : IExtensionApplication
    {
        public Tuple<ObjectId, Vector3d> OriginFrame { get; set; }
        public Tuple<ObjectId, Vector3d> PlaygroundFrame { get; set; }
        public void Initialize()
        {
            //
        }

        public void Terminate()
        {
            //
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

            // 记录原始图框和偏移
            OriginFrame = new Tuple<ObjectId, Vector3d>(originFrame, offset);

            // 首先将原线框内的所有图元复制一份放到解构图集放置区的第一个线框里
            // 这个线框里面的图元会被移动到到解构图集放置区对应的线框中
            // 未被移走的图元将会保留在这个图框中，并作为“未标识”对象
            // 这个图框里面的块引用将会被“炸”平成基本图元后处理
            // 解构图集放置区的第一个线框
            PlaygroundFrame = frames.First();
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                acadDatabase.Database.CopyWithMove(originFrame, offset);
                acadDatabase.Database.ExplodeToOwnerSpace(PlaygroundFrame.Item1);
            }

            // CAD数据处理流程
            ThSitePlanConfigService.Instance.Initialize();
            ThSitePlanConfigService.Instance.EnableAll(true);
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                ThSitePlanEngine.Instance.Containers = frames;
                ThSitePlanEngine.Instance.OriginFrame = PlaygroundFrame.Item1;
                ThSitePlanEngine.Instance.Generators = new List<ThSitePlanGenerator>()
                {
                    new ThSitePlanFrameNameGenerator(),
                    new ThSitePlanContentGenerator(),
                    new ThSitePlanTrimGenerator(),
                    new ThSitePlanBoundaryGenerator(),
                    new ThSitePlanHatchGenerator(),
                    new ThSitePlanShadowGenerator()
                };
                ThSitePlanEngine.Instance.Run(acadDatabase.Database, ThSitePlanConfigService.Instance.Root);
            }

            // CAD打印流程
            //读取已经被标注的图框
            ThSitePlanDbEngine.Instance.Initialize(Active.Database);
            ThSitePlanConfigService.Instance.Initialize();
            ThSitePlanConfigService.Instance.EnableAll(true);
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var plotpdfframes = new Queue<Tuple<ObjectId, Vector3d>>();
                foreach (ObjectId fra in ThSitePlanDbEngine.Instance.Frames)
                {
                    plotpdfframes.Enqueue(new Tuple<ObjectId, Vector3d>(fra, new Vector3d(0, 0, 0)));
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
            ThSitePlanConfigService.Instance.Initialize();
            ThSitePlanConfigService.Instance.EnableAll(true);
            using (var psService = new ThSitePlanPSService())
            {
                if (!psService.IsValid)
                {
                    Application.ShowAlertDialog("未识别到PhotoShop,请确认是否正确安装PhotoShop");
                    return;
                }

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
                Vector3d unusedtoundifineoffset = acadDatabase.Database.FrameOffset(undifineframe, unusedframe);

                //初始化图框配置
                //这里先“关闭”所有的图框
                //后面会根据用户的选择“打开”需要更新的图框
                ThSitePlanConfigService.Instance.Initialize();
                ThSitePlanConfigService.Instance.EnableAll(false);

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
                        updateframes.Enqueue(new Tuple<ObjectId, Vector3d>(frame, acadDatabase.Database.FrameOffset(unusedframe, frame)));
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
                            updateframes.Enqueue(new Tuple<ObjectId, Vector3d>(frame, acadDatabase.Database.FrameOffset(unusedframe, frame)));
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
                acadDatabase.Database.CopyWithMove(OriginFrame.Item1, acadDatabase.Database.FrameOffset(OriginFrame.Item1, undifineframe) + unusedtoundifineoffset);
                acadDatabase.Database.ExplodeToOwnerSpace(unusedframe);

                //接着将需要更新的图框清空
                foreach (var item in SortedUpdateFrames)
                {
                    ThSitePlanDbEngine.Instance.EraseItemInFrame(item.Item1, PolygonSelectionMode.Crossing);
                }

                //启动CAD引擎，开始更新 
                ThSitePlanEngine.Instance.Containers = SortedUpdateFrames;
                ThSitePlanEngine.Instance.OriginFrame = unusedframe;
                ThSitePlanEngine.Instance.Generators = new List<ThSitePlanGenerator>()
                 {
                    new ThSitePlanContentGenerator(),
                    new ThSitePlanTrimGenerator(),
                    new ThSitePlanBoundaryGenerator(),
                    new ThSitePlanHatchGenerator(),
                    new ThSitePlanShadowGenerator(),
                    new ThSitePlanPDFGenerator()
                 };
                ThSitePlanEngine.Instance.Run(acadDatabase.Database, ThSitePlanConfigService.Instance.Root);
                //Update后先清除初始元素Copy frame内部的图元
                ThSitePlanDbEngine.Instance.EraseItemInFrame(unusedframe, PolygonSelectionMode.Crossing);

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

