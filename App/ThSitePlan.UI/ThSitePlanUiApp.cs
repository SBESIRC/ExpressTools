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
using ThSitePlan.Log;
using ThSitePlan.Configuration;
using ThSitePlan.Photoshop;
using NFox.Cad.Collections;
using System.Windows.Forms;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
using System.IO;
using NLog;
using NLog.Targets;
using Exception = System.Exception;

[assembly:ExtensionApplication(typeof(ThSitePlan.UI.ThSitePlanApp))]
[assembly:CommandClass(typeof(ThSitePlan.UI.ThSitePlanApp))]

namespace ThSitePlan.UI
{
    public class ThSitePlanApp : IExtensionApplication
    {
        public static NLog.Logger Logger;

        public void Initialize()
        {
            ThSitePlanDocCollectionEventHandler.Instance.Register();
            ThSitePlanDbEventHandler.Instance.SubscribeToDb(Active.Database);
            ThSitePlanDocEventHandler.Instance.SubscribeToDoc(Active.Document);

            var config = new NLog.Config.LoggingConfiguration();
            
            // Add temporary file log
            string temporaryPath = Path.GetTempPath();
            string dateTimeNow = DateTime.Now.ToString("yyyy-MM-ss-FFF");
            string logFilePath = Path.Combine(temporaryPath, "ThSitePlan" + dateTimeNow + ".log");
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = logFilePath };
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);

            // Add AutoCAD editor prompt log
            MethodCallTarget methodCallTarget = new MethodCallTarget("AutoCAD Editor Prompt Log", AcadLogHandler);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, methodCallTarget);

            NLog.LogManager.Configuration = config;

            Logger = NLog.LogManager.GetCurrentClassLogger();
    }

        private void AcadLogHandler(LogEventInfo logEventInfo, object[] objects)
        {
            //var activeEditor = Active.Editor;
            //if(activeEditor != null)
            //{
            //    activeEditor.WriteMessage(logEventInfo != null ? logEventInfo.ToString() : String.Empty, objects);
            //    activeEditor.WriteMessage("/n");
            //    //activeEditor.WriteLine(logEventInfo != null ? logEventInfo.ToString() : String.Empty, objects);
            //}
        }

        public void Terminate()
        {
            ThSitePlanDocCollectionEventHandler.Instance.UnRegister();
            ThSitePlanDbEventHandler.Instance.UnsubscribeFromDb(Active.Database);
            ThSitePlanDocEventHandler.Instance.UnsubscribeFromDoc(Active.Document);
        }

        public static void LogException(Exception e, string title)
        {
            var message = title + ": \r\n" + e.Message + "\r\n" + e.StackTrace;
            ThSitePlanApp.Logger.Error(message);
        }

        /// <summary>
        /// 一键生成
        /// </summary>
        [CommandMethod("TIANHUACAD", "THPGE", CommandFlags.Modal)]
        public void ThSitePlanGenerate()
        {

            ThSitePlanApp.Logger.Info("开始运行一键彩总");

            Vector3d offset;
            ObjectId originFrame = ObjectId.Null;
            Extents3d originFrameExtents = new Extents3d();
            var frames = new Queue<Tuple<ObjectId, Vector3d>>();
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                // 确定图框
                ThSitePlanDbEngine.Instance.Initialize(Active.Database);
                originFrame = ThSitePlanDbEngine.Instance.FrameByName(ThSitePlanCommon.ThSitePlan_Frame_Name_Original);

                //首先检查当前图纸中是否已存在之前生成的图框，则先清除所有旧图框
                bool environmentclean = true;
                if (ThSitePlanDbEngine.Instance.Frames.Count != 0)
                {
                    ObjectIdCollection framesexorg = new ObjectIdCollection();
                    foreach (ObjectId item in ThSitePlanDbEngine.Instance.Frames)
                    {
                        if (originFrame.IsValid && item.Equals(originFrame))
                        {
                            continue;
                        }
                        framesexorg.Add(item);
                    }
                    if (framesexorg.Count != 0)
                    {
                        environmentclean = EnvironmentInitialize(framesexorg);
                    }
                }
                if (!environmentclean)
                {
                    return;
                }

                if (originFrame.IsValid)
                {
                    DialogResult clrsu = MessageBox.Show("检测到当前图纸中已设置原始打印图框，是否使用该图框？", "选择原始打印图框", MessageBoxButtons.YesNo);
                    if (clrsu != DialogResult.Yes)
                    {
                        //删除之前设置的原始图框的Xdat
                        originFrame.RemoveXData(ThSitePlanCommon.RegAppName_ThSitePlan_Frame_Name);
                        originFrame = ObjectId.Null;
                    }

                    //PromptKeywordOptions keywordOptions = new PromptKeywordOptions("\n检测到当前图纸中已设置原始打印图框，是否使用该图框？")
                    //{
                    //    AllowNone = true
                    //};
                    //keywordOptions.Keywords.Add("Yes", "Yes", "是(Y)");
                    //keywordOptions.Keywords.Add("No", "No", "否(N)");
                    //keywordOptions.Keywords.Default = "Yes";
                    //PromptResult userresult = Active.Editor.GetKeywords(keywordOptions);
                    //if (userresult.StringResult == "No")
                    //{
                    //    //删除之前设置的原始图框的Xdat
                    //    originFrame.RemoveXData(ThSitePlanCommon.RegAppName_ThSitePlan_Frame_Name);
                    //    originFrame = ObjectId.Null;
                    //}
                }

                if (originFrame.IsNull)
                {
                    // 选择图框或者绘制图框
                    PromptEntityOptions opt = new PromptEntityOptions("\n请指定图框")
                    {
                        AllowNone = true
                    };
                    opt.SetRejectMessage("\nMust be a polyline frame: ");
                    opt.AddAllowedClass(typeof(Polyline), true);
                    opt.Keywords.Add("CREATE", "CREATE", "绘制图框(C)");
                    PromptEntityResult result = Active.Editor.GetEntity(opt);
                    if (result.Status == PromptStatus.OK)
                    {
                        originFrame = result.ObjectId;
                        // 将图框放置到指定图层
                        var ent = acadDatabase.Element<Entity>(originFrame, true);
                        ent.LayerId = acadDatabase.Database.AddLayer(ThSitePlanCommon.LAYER_FRAME);

                        TypedValueList valulist = new TypedValueList
                        {
                            { (int)DxfCode.ExtendedDataBinaryChunk, Encoding.UTF8.GetBytes(ThSitePlanCommon.ThSitePlan_Frame_Name_Original) },
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
                            { (int)DxfCode.ExtendedDataBinaryChunk, Encoding.UTF8.GetBytes(ThSitePlanCommon.ThSitePlan_Frame_Name_Original) },
                        };
                        originFrame.AddXData(ThSitePlanCommon.RegAppName_ThSitePlan_Frame_Name, valulist);

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
                originFrameExtents = frameObj.GeometricExtents;
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
                        { (int)DxfCode.ExtendedDataBinaryChunk, Encoding.UTF8.GetBytes(ThSitePlanCommon.ThSitePlan_Frame_Name_Unused) },
                    };
                    fm.Item1.AddXData(ThSitePlanCommon.RegAppName_ThSitePlan_Frame_Name, valueList);
                }
            }

            // 每次生成操作都重置文件输出路径
            ThSitePlanSettingsService.Instance.ResetOutputPath();

            //显示进度条窗体
            ThSitePlanConfigService.Instance.Initialize(true);
            ThSitePlanConfigService.Instance.EnableAll(UpdateStaus.UpdateCAD);
            int totalupdatetimes = ThSitePlanConfigService.Instance.Root.GetItemsCount();
            ProgressBarForm pgform = new ProgressBarForm(totalupdatetimes + 36);
            AcadApp.ShowModelessDialog(pgform);

            // 首先将原线框内的所有图元复制一份放到解构图集放置区的第一个线框里
            // 这个线框里面的图元会被移动到到解构图集放置区对应的线框中
            // 未被移走的图元将会保留在这个图框中，并作为“未标识”对象
            // 这个图框里面的块引用将会被“炸”平成基本图元后处理
            // 解构图集放置区的第一个线框
            var playgroundFrame = frames.First();
            try
            {
                using (AcadDatabase acadDatabase = AcadDatabase.Active())
                {
                    acadDatabase.Database.CopyWithMove(originFrame, offset);
                    acadDatabase.Database.ExplodeToOwnerSpace(playgroundFrame.Item1);
                    Active.Editor.TrimCmd(acadDatabase.Element<Polyline>(playgroundFrame.Item1));
                }
                pgform.SetProgressBar("已完成复制流程", "正在执行CAD原始数据处理.....", 1);

                ThSitePlanApp.Logger.Info("已完成复制流程，正在执行CAD原始数据处理");
            }
            catch(Exception e)
            {
                LogException(e, "复制过程中出现错误");
                return;
            }

            // CAD原始数据处理流程
            ThSitePlanConfigService.Instance.Initialize();
            ThSitePlanConfigService.Instance.EnableAll(UpdateStaus.UpdateCAD);
            try
            {
                using (AcadDatabase acadDatabase = AcadDatabase.Active())
                {
                    ThSitePlanEngine.Instance.Containers = frames;
                    ThSitePlanEngine.Instance.OriginFrame = playgroundFrame.Item1;
                    ThSitePlanEngine.Instance.Generators = new List<ThSitePlanGenerator>()
                {
                    new ThSitePlanFrameNameGenerator(),
                    new ThSitePlanContentGenerator(),
                };
                    ThSitePlanEngine.Instance.Run(acadDatabase.Database, ThSitePlanConfigService.Instance.Root);
                }
                pgform.SetProgressBar("已完成CAD数据处理流程", "正在执行CAD衍生数据处理.....", 1);

                ThSitePlanApp.Logger.Info("已完成CAD数据处理流程，正在执行CAD衍生数据处理");
            }
            catch(Exception e)
            {
                LogException(e, "CAD数据处理过程出现错误");
                return;
            }

            // 初始化解构区的线框信息
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                ThSitePlanDbEngine.Instance.Initialize(acadDatabase.Database);
            }

            try
            {
                // CAD衍生数据处理流程
                ItemGeneratePreocess(frames,
                    ObjectId.Null,
                    new List<ThSitePlanGenerator>() { new ThSitePlanDerivedContentGenerator() },
                    true,
                    true,
                    true);
                pgform.SetProgressBar("已完成CAD衍生数据处理流程", "正在执行CAD种树处理.....", 2);

                ThSitePlanApp.Logger.Info("已完成CAD衍生数据处理流程，正在执行CAD种树处理");
            }
            catch(Exception e)
            {
                LogException(e, "CAD衍生数据处理错误");
                return;
            }

            try
            {
                // CAD种树处理流程
                ItemGeneratePreocess(frames,
                    ObjectId.Null,
                    new List<ThSitePlanGenerator>() { new ThSitePlanPlantGenerator() },
                    true,
                    true,
                    true);
                pgform.SetProgressBar("已完成CAD种树处理流程", "正在执行CAD填充处理.....", 2);

                ThSitePlanApp.Logger.Info("已完成CAD种树处理流程，正在执行CAD填充处理");
            }
            catch(Exception e)
            {
                LogException(e, "CAD种树过程中出现错误");
                return;
            }

            try
            {
                // CAD填充处理流程
                ItemGeneratePreocess(frames,
                    ObjectId.Null,
                    new List<ThSitePlanGenerator>() { new ThSitePlanBoundaryGenerator() },
                    true,
                    true,
                    true);
                pgform.SetProgressBar("已完成CAD填充处理流程", "正在执行CAD阴影处理.....", 10);

                ThSitePlanApp.Logger.Info("已完成CAD填充处理流程，正在执行CAD阴影处理");
            }
            catch(Exception e)
            {
                LogException(e, "CAD填充过程出现错误");
                return;
            }

            try
            {
                // CAD阴影处理流程
                ItemGeneratePreocess(frames,
                    ObjectId.Null,
                    new List<ThSitePlanGenerator>()
                    { new ThSitePlanShadowContentGenerator(),
                  new ThSitePlanShadowGenerator()},
                    true,
                    true,
                    true);
                pgform.SetProgressBar("已完成CAD阴影处理流程", "正在执行图框清理.....", 6);

                ThSitePlanApp.Logger.Info("已完成CAD阴影处理流程，正在执行图框清理");
            }
            catch (Exception e)
            {
                LogException(e, "CAD阴影处理过程中出现错误");
                return;
            }

            try
            {
                // 未识别图框清理流程
                ItemGeneratePreocess(frames,
                    playgroundFrame.Item1,
                    new List<ThSitePlanGenerator>()
                    { new ThSitePlanUndefineCleanGenerator()},
                    true,
                    true,
                    true);
                pgform.SetProgressBar("已完成图框清理流程", "正在执行CAD打印.....", 2);

                ThSitePlanApp.Logger.Info("已完成图框清理流程，正在执行CAD打印");
            }
            catch(Exception e)
            {
                LogException(e, "未识别图框清理过程出现错误");
                return;
            }

            try
            {
                // CAD打印流程
                ItemGeneratePreocess(frames,
                    ObjectId.Null,
                    new List<ThSitePlanGenerator>()
                    { new ThSitePlanPDFGenerator(new ThSitePlanLogger(ThSitePlanApp.Logger))},
                    true,
                    true,
                    true);
                pgform.SetProgressBar("已完成 CAD打印流程", "正在执行PS处理.....", 5);

                ThSitePlanApp.Logger.Info("已完成 CAD打印流程，正在执行PS处理");
            }
            catch(Exception e)
            {
                LogException(e, "CAD打印过程中出现错误");
                return;
            }

            try
            {
                //PS处理流程
                ThSitePlanConfigService.Instance.Initialize();
                ThSitePlanConfigService.Instance.EnableAll(UpdateStaus.UpdateCAD);
                using (var psService = new ThSitePlanPSService())
                {
                    // 连接PS失败
                    if (psService.ErrorCode != 0)
                    {
                        AcadApp.ShowAlertDialog(psService.ErrorMessage);

                        ThSitePlanApp.Logger.Error(psService.ErrorMessage);

                        return;
                    }

                    // 创建空白文档
                    // 因为用A1纸打印，所以PS文档默认为Portrait
                    // 若打印图框为Landscape，则设置PS文档为Landscape
                    double psdocwidth = 41.84;
                    double psdocheight = 59.17;
                    if (originFrameExtents.Width() > originFrameExtents.Height())
                    {
                        psdocwidth = 59.17;
                        psdocheight = 41.84;
                    }
                    psService.NewEmptyDocument("MyNewDocument", psdocwidth, psdocheight);

                    ThSitePlanApp.Logger.Info("创建新PS文档");

                    // PS处理流程
                    ThSitePlanPSEngine.Instance.Generators = new List<ThSitePlanPSGenerator>()
                    {
                        new ThSitePlanPSDefaultGenerator(psService)
                    };
                    ThSitePlanPSEngine.Instance.PSRun(
                        ThSitePlanSettingsService.Instance.OutputPath,
                        ThSitePlanConfigService.Instance.Root,
                        pgform.Handle, "progressBar1");
                    pgform.SetProgressBar("已完成PhotoShop生成", "正在保存PS文件.....", 1);

                    ThSitePlanApp.Logger.Info("已完成PhotoShop生成，正在保存PS文件");

                    // 保存PS生成的文档
                    psService.ExportToFile(ThSitePlanSettingsService.Instance.OutputPath);
                    pgform.SetProgressBar("Done !", "Done", 6);
                    pgform.setconfirmbtn();
                }
            }
            catch(Exception e)
            {
                LogException(e, "PS处理过程中出现错误");
                return;
            }

            ThSitePlanApp.Logger.Info("一键彩总结束");
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
                var unusedframe = ThSitePlanDbEngine.Instance.FrameByName(ThSitePlanCommon.ThSitePlan_Frame_Name_Unused);
                if (unusedframe.IsNull)
                {
                    return;
                }
                var undifineframe = ThSitePlanDbEngine.Instance.FrameByName(ThSitePlanCommon.ThSitePlan_Frame_Name_Unrecognized);
                if (undifineframe.IsNull)
                {
                    return;
                }
                var originalframe = ThSitePlanDbEngine.Instance.FrameByName(ThSitePlanCommon.ThSitePlan_Frame_Name_Original);
                if (originalframe.IsNull)
                {
                    return;
                }

                var frames = new ObjectIdCollection(SelResult.Value.GetObjectIds());
                if (frames.Contains(originalframe))
                {
                    // 如果选择了原始图框，需要重新生成内容

                    //初始化图框配置
                    //这里先“关闭”所有的图框
                    //后面会根据用户的选择“打开”需要更新的图框
                    ThSitePlanConfigService.Instance.Initialize();
                    ThSitePlanConfigService.Instance.EnableAll(UpdateStaus.NoUpdate);

                    //获取需要更新的图框
                    var updateframes = new Queue<Tuple<ObjectId, Vector3d>>();
                    foreach (var layer in ThSitePlanDbEventHandler.Instance.UpdatedContents)
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
                                //获取选择框与复制框之间的偏移量
                                var name = item.Properties["Name"] as string;
                                var frame = ThSitePlanDbEngine.Instance.FrameByName(name);
                                if (frame.IsNull)
                                {
                                    return;
                                }
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

                    //首先将原线框内的所有图元复制一份放到解构图集放置区的最后一个线框里
                    var originFrame = ThSitePlanDbEngine.Instance.FrameByName(ThSitePlanCommon.ThSitePlan_Frame_Name_Original);
                    if (originFrame.IsNull)
                    {
                        return;
                    }
                    Vector3d unusedtoundifineoffset = acadDatabase.Database.FrameOffset(undifineframe, unusedframe);
                    acadDatabase.Database.CopyWithMove(originFrame, acadDatabase.Database.FrameOffset(originFrame, undifineframe) + unusedtoundifineoffset);
                    acadDatabase.Database.ExplodeToOwnerSpace(unusedframe);
                    Active.Editor.TrimCmd(acadDatabase.Element<Polyline>(unusedframe));

                    //接着将需要更新的图框清空
                    foreach (var item in updateframes)
                    {
                        ThSitePlanDbEngine.Instance.EraseItemInFrame(item.Item1, PolygonSelectionMode.Crossing);
                    }

                    CADUpdatePreocess(undifineframe,unusedframe,updateframes,false);
                    PhotoshopUpdatePreocess(undifineframe, unusedframe, updateframes,false);
                }
                else
                {
                    // 如果只选择解构图框，则只需要打印内容

                    //初始化图框配置
                    //这里先“关闭”所有的图框
                    //后面会根据用户的选择“打开”需要更新的图框
                    ThSitePlanConfigService.Instance.Initialize();
                    ThSitePlanConfigService.Instance.EnableAll(UpdateStaus.NoUpdate);

                    var filterframes = new ObjectIdCollection();
                    foreach(ObjectId frame in frames)
                    {
                        // 过滤掉未使用的空白解构框
                        string framename = ThSitePlanDbEngine.Instance.NameByFrame(frame);
                        if (string.IsNullOrEmpty(framename))
                        {
                            filterframes.Add(frame);
                        }
                        if (framename == ThSitePlanCommon.ThSitePlan_Frame_Name_Unused)
                        {
                            filterframes.Add(frame);
                        }
                    }
                    foreach(ObjectId frame in filterframes)
                    {
                        frames.Remove(frame);
                    }
                    if (frames.Count == 0)
                    {
                        return;
                    }

                    var cadupdateframe = new Queue<Tuple<ObjectId, Vector3d>>();
                    foreach (ObjectId frameid in frames)
                    {
                        var frametuple = new Tuple<ObjectId, Vector3d>(frameid, new Vector3d(0, 0, 0));
                        cadupdateframe.Enqueue(frametuple);
                    }

                    PhotoshopUpdatePreocess(undifineframe, unusedframe, cadupdateframe, true);
                    PSUpdateSavePreocess();
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
                dlg.m_ColorDefaultConfig = ThSitePlanConfigService.Instance.DefaultJsonString;

                // 记住当前配置界面的设置
                var previousConfig = dlg.m_ColorGeneralConfig;

                // 弹出配置界面
                var ConfigFormResult = AcadApp.ShowModalDialog(dlg);
                if (ConfigFormResult == System.Windows.Forms.DialogResult.OK)
                {
                    using (AcadDatabase acadDatabase = AcadDatabase.Active())
                    {
                        ThSitePlanConfigService.Instance.WriteConfigIntoDb(dlg.m_ColorGeneralConfig, acadDatabase, true);
                    }

                    //记住配置界面的新的设置
                    var currentConfig = dlg.m_ColorGeneralConfig;

                    CompareCurAndPreConfig(previousConfig, currentConfig);
                }
            }
        }

        /// <summary>
        /// 脚本控制器
        /// </summary>
        [CommandMethod("TIANHUACAD", "THPOP", CommandFlags.Modal)]
        public void ThSitePlanOptions()
        {
            using (var dlg = new fmThSitePlan())
            {
                AcadApp.ShowModalDialog(dlg);
            }
        }

        [CommandMethod("TIANHUACAD", "THPOPUD", CommandFlags.Modal | CommandFlags.UsePickSet)]
        public void ThSitePlanOpUpdate()
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
                var unusedframe = ThSitePlanDbEngine.Instance.FrameByName(ThSitePlanCommon.ThSitePlan_Frame_Name_Unused);
                if (unusedframe.IsNull)
                {
                    return;
                }
                var undifineframe = ThSitePlanDbEngine.Instance.FrameByName(ThSitePlanCommon.ThSitePlan_Frame_Name_Unrecognized);
                if (undifineframe.IsNull)
                {
                    return;
                }
                var originalframe = ThSitePlanDbEngine.Instance.FrameByName(ThSitePlanCommon.ThSitePlan_Frame_Name_Original);
                if (originalframe.IsNull)
                {
                    return;
                }
                Vector3d unusedtoundifineoffset = acadDatabase.Database.FrameOffset(undifineframe, unusedframe);

                var frames = new ObjectIdCollection(SelResult.Value.GetObjectIds());
                Queue<Tuple<ObjectId, Vector3d>> cadupdateframe = new Queue<Tuple<ObjectId, Vector3d>>();
                foreach (ObjectId frameid in frames)
                {
                    Vector3d unusedtoselectoffset = acadDatabase.Database.FrameOffset(unusedframe, frameid);
                    Tuple<ObjectId, Vector3d> frametuple = new Tuple<ObjectId, Vector3d>(frameid, unusedtoselectoffset);
                    cadupdateframe.Enqueue(frametuple);
                }

                //首先将原线框内的所有图元复制一份放到解构图集放置区的最后一个线框里
                acadDatabase.Database.CopyWithMove(originalframe, acadDatabase.Database.FrameOffset(originalframe, undifineframe) + unusedtoundifineoffset);
                acadDatabase.Database.ExplodeToOwnerSpace(unusedframe);
                Active.Editor.TrimCmd(acadDatabase.Element<Polyline>(unusedframe));

                //接着将需要更新的图框清空
                foreach (var item in cadupdateframe)
                {
                    ThSitePlanDbEngine.Instance.EraseItemInFrame(item.Item1, PolygonSelectionMode.Crossing);
                }

                CADUpdatePreocess(undifineframe, unusedframe, cadupdateframe, false);
                PhotoshopUpdatePreocess(undifineframe, unusedframe, cadupdateframe, true);
                PSUpdateSavePreocess();
            }
        }

        [CommandMethod("TIANHUACAD", "THPBR", CommandFlags.Modal)]
        public void ThSitePlanOpenFilePath()
        {
            System.Diagnostics.Process.Start(ThSitePlanSettingsService.Instance.OutputPath);
            using (var psService = new ThSitePlanPSService())
            {
                psService.ShowPSApp();
            }
        }

        public bool EnvironmentInitialize(ObjectIdCollection dbframes)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                DialogResult clrsu = MessageBox.Show("当前图纸中存在上次生成的解构图框，需清理后再执行生成操作，是否清理 ?","清理解构图框",MessageBoxButtons.YesNo);
                if (clrsu != DialogResult.Yes)
                {
                    return false;
                }
                Extents3d totalframeexten = new Extents3d();
                foreach (ObjectId frameid in dbframes)
                {
                    Entity frameent = acadDatabase.Element<Entity>(frameid,false);
                    totalframeexten.AddExtents(frameent.GeometricExtents);
                }

                Point3d newmaxpoint = new Point3d(totalframeexten.MaxPoint.X,
                    totalframeexten.MaxPoint.Y+ ThSitePlanCommon.frame_annotation_offset_Y+5, 
                    totalframeexten.MaxPoint.Z);
                PromptSelectionResult psr = Active.Editor.SelectByWindow(totalframeexten.MinPoint,
                newmaxpoint,
                PolygonSelectionMode.Crossing,
                null);
                if (psr.Status == PromptStatus.OK)
                {
                    Active.Editor.EraseCmd(psr.Value.GetObjectIds().ToObjectIdCollection());
                }
                return true;
            }
        }

        public void CompareCurAndPreConfig(string preconfig, string curconfig)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                ThSitePlanConfigService.Instance.Initialize();

                ThSitePlanConfigItemGroup curroot = ThSitePlanConfigService.Instance.StringToRoot(curconfig);
                ThSitePlanConfigItemGroup preroot = ThSitePlanConfigService.Instance.StringToRoot(preconfig);

                if (preroot.IsNull())
                {
                    return;
                }
                List<ThSitePlanConfigItem> currentallitems = curroot.GetAllItems();
                List<ThSitePlanConfigItem> previousitems = preroot.GetAllItems();

                Queue<Tuple<ObjectId, Vector3d>> psupdateframes = new Queue<Tuple<ObjectId, Vector3d>>();
                Queue<Tuple<ObjectId, Vector3d>> psinsertframes = new Queue<Tuple<ObjectId, Vector3d>>();
                Queue<Tuple<ObjectId, Vector3d>> newinsertframe = new Queue<Tuple<ObjectId, Vector3d>>();
                ObjectIdCollection cadupdateframeid = new ObjectIdCollection();
                ObjectIdCollection cleanframesid = new ObjectIdCollection();
                ThSitePlanConfigItemGroup groupfornewadded = new ThSitePlanConfigItemGroup();
                ThSitePlanConfigItemGroup newgroupforcadclean = new ThSitePlanConfigItemGroup();
                ThSitePlanConfigItemGroup newgroupforpsclean = new ThSitePlanConfigItemGroup();


                ThSitePlanDbEngine.Instance.Initialize(Active.Database);
                bool anyconfigchange = false;
                foreach (var curitem in currentallitems)
                {
                    bool preincur = false;
                    foreach (var preitem in previousitems)
                    {
                        //若新的item的ID能在旧的item列表中能找到对应项，则进一步查看新的item有无改名，变组，改值
                        if (curitem.Properties["ID"].ToString().Equals(preitem.Properties["ID"].ToString()))
                        {
                            string curfullname = curitem.Properties["Name"].ToString();
                            string prefullname = preitem.Properties["Name"].ToString();
                            //全名完全相等，说明既没有改名也没有变组
                            if (curfullname.Equals(prefullname))
                            {
                                //仅仅只是改了配置面板中的PS颜色及透明度
                                var curlayers = curitem.Properties["CADLayer"] as List<string>;
                                var prelayers = preitem.Properties["CADLayer"] as List<string>;
                                if (curitem.Properties["CADScriptID"].ToString() == preitem.Properties["CADScriptID"].ToString()&&
                                    curlayers.Count == prelayers.Count && curlayers.Count(t => !prelayers.Contains(t)) == 0)
                                {
                                    if (curitem.Properties["Color"].ToString() != preitem.Properties["Color"].ToString() ||
                                    curitem.Properties["Opacity"].ToString() != preitem.Properties["Opacity"].ToString())
                                    {
                                        ObjectId updframeid = ThSitePlanDbEngine.Instance.FrameByName(prefullname);
                                        if (updframeid.IsNull)
                                        {
                                            return;
                                        }
                                        Tuple<ObjectId, Vector3d> updframe = new Tuple<ObjectId, Vector3d>(updframeid, new Vector3d(0, 0, 0));
                                        psupdateframes.Enqueue(updframe);

                                        anyconfigchange = true;
                                    }
                                }

                                //改变了CAD图层名或者脚本ID
                                else
                                {
                                    ObjectId updframeid = ThSitePlanDbEngine.Instance.FrameByName(prefullname);
                                    if (updframeid.IsNull)
                                    {
                                        return;
                                    }
                                    cadupdateframeid.Add(updframeid);

                                    anyconfigchange = true;
                                }
                            }

                            //全名不一致，或变组或改名
                            else
                            {
                                ObjectId updframeid = ThSitePlanDbEngine.Instance.FrameByName(prefullname);
                                if (updframeid.IsNull)
                                {
                                    return;
                                }
                                //改变图框Xdate
                                // 更新框的标签
                                updframeid.RemoveXData(ThSitePlanCommon.RegAppName_ThSitePlan_Frame_Name);
                                TypedValueList valueList = new TypedValueList
                                    {
                                        { (int)DxfCode.ExtendedDataBinaryChunk,  Encoding.UTF8.GetBytes(curfullname) },
                                    };
                                updframeid.AddXData(ThSitePlanCommon.RegAppName_ThSitePlan_Frame_Name, valueList);
                                Tuple<ObjectId, Vector3d> updframe = new Tuple<ObjectId, Vector3d>(updframeid, new Vector3d(0, 0, 0));
                                psinsertframes.Enqueue(updframe);
                                newgroupforpsclean.Items.Enqueue(preitem);

                                anyconfigchange = true;
                            }

                            preincur = true;
                            break;
                        }
                    }

                    //新item的ID在旧的item列表中找不到对应项，则该项为新增
                    if (!preincur)
                    {
                        groupfornewadded.Items.Enqueue(curitem);
                        anyconfigchange = true;
                    }
                }

                foreach (var preitem in previousitems)
                {
                    bool curinpre = false;
                    foreach (var curitem in currentallitems)
                    {
                        if (curitem.Properties["ID"].ToString().Equals(preitem.Properties["ID"].ToString()))
                        {
                            curinpre = true;
                        }
                    }

                    //item在旧的config中存在，新的config中不存在，即该项被删除
                    if (!curinpre)
                    {
                        string prefullname = preitem.Properties["Name"].ToString();

                        using (AcadDatabase CadDatabase = AcadDatabase.Active())
                        {
                            ThSitePlanDbEngine.Instance.Initialize(Active.Database);
                            ThSitePlanDbEngine.Instance.Initialize(CadDatabase.Database);

                            newgroupforcadclean.Items.Enqueue(preitem);
                            newgroupforpsclean.Items.Enqueue(preitem);
                        }
                        anyconfigchange = true;
                    }

                }

                //若检测到有更新，则询问用户是否立即刷新图纸
                if (anyconfigchange == true)
                {
                    DialogResult conupdresult = MessageBox.Show(
                        "检测到映射配置有更新,是否立即更新图纸", 
                        "映射配置更新", 
                        MessageBoxButtons.YesNo);
                    if (conupdresult != DialogResult.Yes)
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
                var unusedframe = ThSitePlanDbEngine.Instance.FrameByName(ThSitePlanCommon.ThSitePlan_Frame_Name_Unused);
                if (unusedframe.IsNull)
                {
                    return;
                }
                var undifineframe = ThSitePlanDbEngine.Instance.FrameByName(ThSitePlanCommon.ThSitePlan_Frame_Name_Unrecognized);
                if (undifineframe.IsNull)
                {
                    return;
                }
                var originalframe = ThSitePlanDbEngine.Instance.FrameByName(ThSitePlanCommon.ThSitePlan_Frame_Name_Original);
                if (originalframe.IsNull)
                {
                    return;
                }

                if (cadupdateframeid.Count != 0)
                {
                    //初始化图框配置
                    //这里先“关闭”所有的图框
                    //后面会根据用户的选择“打开”需要更新的图框
                    ThSitePlanConfigService.Instance.Initialize();
                    ThSitePlanConfigService.Instance.EnableAll(UpdateStaus.NoUpdate);

                    //首先将原线框内的所有图元复制一份放到解构图集放置区的最后一个线框里
                    Vector3d unusedtoundifineoffset = acadDatabase.Database.FrameOffset(undifineframe, unusedframe);
                    acadDatabase.Database.CopyWithMove(originalframe, acadDatabase.Database.FrameOffset(originalframe, undifineframe) + unusedtoundifineoffset);
                    acadDatabase.Database.ExplodeToOwnerSpace(unusedframe);
                    Active.Editor.TrimCmd(acadDatabase.Element<Polyline>(unusedframe));

                    //创建所有更新图框与拷贝图框的位移关系Vector3d
                    Queue<Tuple<ObjectId, Vector3d>> cadupdateframes = new Queue<Tuple<ObjectId, Vector3d>>();
                    foreach (ObjectId frameid in cadupdateframeid)
                    {
                        Vector3d unusedtoselectoffset = acadDatabase.Database.FrameOffset(unusedframe, frameid);
                        Tuple<ObjectId, Vector3d> frametuple = new Tuple<ObjectId, Vector3d>(frameid, unusedtoselectoffset);
                        cadupdateframes.Enqueue(frametuple);
                    }

                    //接着将需要更新的图框清空
                    foreach (var item in cadupdateframes)
                    {
                        ThSitePlanDbEngine.Instance.EraseItemInFrame(item.Item1, PolygonSelectionMode.Crossing);
                    }
                    CADUpdatePreocess(undifineframe, unusedframe, cadupdateframes, false);
                    PhotoshopUpdatePreocess(undifineframe, unusedframe, cadupdateframes, true);
                }
                InsertToPhotoshop(psinsertframes);
                PhotoshopUpdatePreocess(undifineframe, unusedframe, psupdateframes, true);
                CADGenerateWorkflow(groupfornewadded);

                Queue<Tuple<ObjectId, Vector3d>> contframes = new Queue<Tuple<ObjectId, Vector3d>>();
                foreach (ObjectId item in ThSitePlanDbEngine.Instance.Frames)
                {
                    contframes.Enqueue(new Tuple<ObjectId, Vector3d>(item, new Vector3d(0, 0, 0)));
                }
                CADCleanWorkflow(newgroupforcadclean, contframes, originalframe);
                PhotoShopCleanWorkflow(newgroupforpsclean);

                PSUpdateSavePreocess();
            }
        }

        private bool SameGroupName(string groupnamea, string groupnameb)
        {
            List<string> groupa = groupnamea.Split('-').ToList();
            List<string> groupb = groupnameb.Split('-').ToList();

            groupa.RemoveAt(groupa.Count-1);
            groupb.RemoveAt(groupb.Count-1);

            if (groupa.Count == groupb.Count && groupa.Count(t => !groupb.Contains(t)) == 0)
            {
                return true;
            }

            return false;
        }

        private void InsertToPhotoshop(Queue<Tuple<ObjectId, Vector3d>> updateframes)
        {
            if (updateframes.Count == 0)
            {
                return;
            }
            // CAD打印流程
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                //启动CAD引擎，开始FrameNameGenerator
                ItemGeneratePreocess(updateframes,
                    ObjectId.Null,
                    new List<ThSitePlanGenerator>() { new ThSitePlanFrameNameGenerator() },
                    false,
                    false,
                    true);

                //启动CAD引擎，开始PDFGenerator
                ItemGeneratePreocess(updateframes,
                    ObjectId.Null,
                    new List<ThSitePlanGenerator>() { new ThSitePlanPDFGenerator() },
                    false,
                    false,
                    true);
            }

            //PS处理流程
            ThSitePlanConfigService.Instance.Initialize();
            ThSitePlanConfigService.Instance.EnableAll(UpdateStaus.NoUpdate);
            using (var psService = new ThSitePlanPSService())
            {
                // 连接PS失败
                if (psService.ErrorCode != 0)
                {
                    AcadApp.ShowAlertDialog(psService.ErrorMessage);
                    return;
                }

                // PS处理流程
                ThSitePlanPSEngine.Instance.Generators = new List<ThSitePlanPSGenerator>()
                 {
                    new ThSitePlanPSDefaultGenerator(psService),
                 };
                OpenUpdateItems(ThSitePlanConfigService.Instance.Root, updateframes, UpdateStaus.UpdateCAD, true);
                ThSitePlanPSEngine.Instance.PSRun(
                    ThSitePlanSettingsService.Instance.OutputPath,
                    ThSitePlanConfigService.Instance.Root);

                // 保存PS生成的文档
                //psService.ExportToFile(ThSitePlanSettingsService.Instance.OutputPath);
            }
        }

        /// <summary>
        /// Generate与update通用流程
        /// </summary>
        /// <param name="containers">ThSitePlanEngine.Instance.Containers (Generate流程传入所有图框，update流程传入需要更新的图框)</param>
        /// <param name="originframe">ThSitePlanEngine.Instance.OriginFrame (原始图框)</param>
        /// <param name="generators">ThSitePlanEngine.Instance.Generators</param>
        /// <param name="refreshcontainers">是否需要更新containers中的图框 (Generate流程生成结构图时，需要排除originframe)</param>
        /// <param name="enableallitem">是否需要打开所有ConfigItem (Generate流程需要打开所有ConfigItem，update流程只打开需要更新的item)</param>
        /// <param name="ifnosid">该参数用于更新流程中设置是否需要打开兄弟节点，仅PSD/PDF更新不需打开兄弟节点</param>
        public void ItemGeneratePreocess(Queue<Tuple<ObjectId, Vector3d>> containers, ObjectId originframe, List<ThSitePlanGenerator> generators, bool refreshcontainers, bool enableallitem, bool ifnosid)
        {
            if (enableallitem)
            {
                ThSitePlanConfigService.Instance.Initialize();
                ThSitePlanConfigService.Instance.EnableAll(UpdateStaus.UpdateCAD);
            }
            else
            {
                OpenUpdateItems(ThSitePlanConfigService.Instance.Root, containers,UpdateStaus.UpdateCAD, ifnosid);
            }
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                //Generate流程生成结构图时，需要排除originframe
                //update流程不需这一步
                if (refreshcontainers)
                {
                    containers.Clear();
                    foreach (ObjectId frame in ThSitePlanDbEngine.Instance.Frames)
                    {
                        if (!frame.Equals(originframe))
                        {
                            containers.Enqueue(new Tuple<ObjectId, Vector3d>(frame, new Vector3d(0, 0, 0)));
                        }
                    }
                }
                ThSitePlanEngine.Instance.Containers = containers;
                ThSitePlanEngine.Instance.OriginFrame = originframe;
                ThSitePlanEngine.Instance.Generators = generators;

                ThSitePlanEngine.Instance.Update(acadDatabase.Database, ThSitePlanConfigService.Instance.Root);
            }
        }

        public void ItemGenerateWithNewRoot(ThSitePlanConfigItemGroup root,Queue<Tuple<ObjectId, Vector3d>> containers, ObjectId originframe, List<ThSitePlanGenerator> generators, bool refreshcontainers)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                ThSitePlanConfigItemGroup temproot = new ThSitePlanConfigItemGroup();
                temproot.Status = UpdateStaus.UpdateCAD;
                foreach (var item in root.Items)
                {
                    temproot.Items.Enqueue(item);
                }
                //Generate流程生成结构图时，需要排除originframe
                //update流程不需这一步
                if (refreshcontainers)
                {
                    containers.Clear();
                    foreach (ObjectId frame in ThSitePlanDbEngine.Instance.Frames)
                    {
                        if (!frame.Equals(originframe))
                        {
                            containers.Enqueue(new Tuple<ObjectId, Vector3d>(frame, new Vector3d(0, 0, 0)));
                        }
                    }
                }
                ThSitePlanEngine.Instance.Containers = containers;
                ThSitePlanEngine.Instance.OriginFrame = originframe;
                ThSitePlanEngine.Instance.Generators = generators;

                ThSitePlanEngine.Instance.Update(acadDatabase.Database, temproot);
            }
        }

        public void CADUpdatePreocess(ObjectId undifineframe, ObjectId unusedframe, Queue<Tuple<ObjectId, Vector3d>> updateframes, bool ifnosid)
        {
            if (updateframes.Count == 0)
            {
                return;
            }
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                ThSitePlanEngine.Instance.Containers = updateframes;
                ThSitePlanEngine.Instance.OriginFrame = unusedframe;

                //启动CAD引擎，开始ContentGenerator 
                ItemGeneratePreocess(updateframes,
                    unusedframe,
                    new List<ThSitePlanGenerator>() { new ThSitePlanContentGenerator() },
                    false,
                    false,
                    ifnosid);
                ThSitePlanDbEngine.Instance.EraseItemInFrame(unusedframe, PolygonSelectionMode.Crossing);

                //启动CAD引擎，开始DerivedContentGenerator
                ItemGeneratePreocess(updateframes,
                    unusedframe,
                    new List<ThSitePlanGenerator>() { new ThSitePlanDerivedContentGenerator() },
                    false,
                    false,
                    ifnosid);

                //启动CAD引擎，开始PlantGenerator
                ItemGeneratePreocess(updateframes,
                    unusedframe,
                    new List<ThSitePlanGenerator>() { new ThSitePlanPlantGenerator() },
                    false,
                    false,
                    ifnosid);

                //启动CAD引擎，开始BoundaryGenerator
                ItemGeneratePreocess(updateframes,
                    unusedframe,
                    new List<ThSitePlanGenerator>() { new ThSitePlanBoundaryGenerator() },
                    false,
                    false,
                    ifnosid);

                //启动CAD引擎，开始ShadowGenerator
                ItemGeneratePreocess(updateframes,
                    unusedframe,
                    new List<ThSitePlanGenerator>()
                    { new ThSitePlanShadowContentGenerator(),
                      new ThSitePlanShadowGenerator()},
                    false,
                    false,
                    ifnosid);
            }
        }

        public void PhotoshopUpdatePreocess(ObjectId undifineframe, ObjectId unusedframe, Queue<Tuple<ObjectId, Vector3d>> updateframes, bool ifnosid)
        {
            if (updateframes.Count==0)
            {
                return;
            }
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                //启动CAD引擎，开始PDFGenerator
                ItemGeneratePreocess(updateframes,
                    unusedframe,
                    new List<ThSitePlanGenerator>() { new ThSitePlanPDFGenerator() },
                    false,
                    false,
                    ifnosid);

                Queue<Tuple<ObjectId, Vector3d>> psupdateframe = new Queue<Tuple<ObjectId, Vector3d>>();
                foreach (var item in updateframes)
                {
                    Tuple<ObjectId, Vector3d> frametuple = new Tuple<ObjectId, Vector3d>(item.Item1, new Vector3d(0, 0, 0));
                    psupdateframe.Enqueue(frametuple);
                }

                using (var psService = new ThSitePlanPSService())
                {
                    // 异常处理：无法连接PS
                    if (psService.ErrorCode != 0)
                    {
                        AcadApp.ShowAlertDialog(psService.ErrorMessage);
                        return;
                    }

                    //启动PS引擎，开始更新 
                    ThSitePlanPSEngine.Instance.Generators = new List<ThSitePlanPSGenerator>()
                    {
                        new ThSitePlanPSDefaultGenerator(psService),
                    };
                    OpenUpdateItems(ThSitePlanConfigService.Instance.Root, updateframes, UpdateStaus.UpdateCAD, ifnosid);
                    ThSitePlanPSEngine.Instance.PSUpdate(
                        ThSitePlanSettingsService.Instance.OutputPath,
                        ThSitePlanConfigService.Instance.Root);

                    // 保存PS生成的文档
                    //psService.ExportToFileForUpdate(ThSitePlanSettingsService.Instance.OutputPath);
                }

            }
        }

        public void OpenUpdateItems(ThSitePlanConfigItemGroup root, Queue<Tuple<ObjectId, Vector3d>> updateframes, UpdateStaus staus, bool ifnosid)
        {
            ThSitePlanConfigService.Instance.Initialize();
            ThSitePlanConfigService.Instance.EnableAll(UpdateStaus.NoUpdate);
            //根据用户选择，更新图框配置
            foreach (var item in updateframes)
            {
                //获取所选择的框对应的图元的图层分组名
                string selFrameName = ThSitePlanDbEngine.Instance.NameByFrame(item.Item1);

                //打开需要的工作
                if (ifnosid)
                {
                    ThSitePlanConfigService.Instance.EnableItemAndAncestorNoSib(selFrameName, staus);
                    continue;
                }
                ThSitePlanConfigService.Instance.EnableItemAndItsAncestor(selFrameName, staus);
            }
            root = ThSitePlanConfigService.Instance.Root;
        }

        public void CADGenerateWorkflow(ThSitePlanConfigItemGroup newroot)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                if (newroot.Items.Count == 0)
                {
                    return;
                }
                var orgframe = ThSitePlanDbEngine.Instance.FrameByName(ThSitePlanCommon.ThSitePlan_Frame_Name_Original);
                if (orgframe.IsNull)
                {
                    return;
                }

                ThSitePlanDbEngine.Instance.Initialize(Active.Database);
                ObjectIdCollection allunusedframes = ThSitePlanDbEngine.Instance.FindAllUnusedFrame();

                Vector3d origintoplayground = acadDatabase.Database.FrameOffset(orgframe, allunusedframes.ToArray().First());
                Tuple<ObjectId, Vector3d> newplaygroundframe = new Tuple<ObjectId, Vector3d>(allunusedframes.ToArray().First(), origintoplayground);

                Queue<Tuple<ObjectId, Vector3d>> currentunusedframe = new Queue<Tuple<ObjectId, Vector3d>>();
                foreach (ObjectId unusedframeid in allunusedframes)
                {
                    if (unusedframeid.Equals(newplaygroundframe.Item1))
                    {
                        continue;
                    }
                    Vector3d unusedtoorigin = acadDatabase.Database.FrameOffset(newplaygroundframe.Item1, unusedframeid);
                    currentunusedframe.Enqueue(new Tuple<ObjectId, Vector3d>(unusedframeid, unusedtoorigin));
                }

                // 首先将原线框内的所有图元复制一份放到解构图集放置区的第一个线框里
                // 这个线框里面的图元会被移动到到解构图集放置区对应的线框中
                // 未被移走的图元将会保留在这个图框中，并作为“未标识”对象
                // 这个图框里面的块引用将会被“炸”平成基本图元后处理
                // 解构图集放置区的第一个线框
                var playgroundFrame = newplaygroundframe;

                acadDatabase.Database.CopyWithMove(orgframe, origintoplayground);
                acadDatabase.Database.ExplodeToOwnerSpace(playgroundFrame.Item1);
                Active.Editor.TrimCmd(acadDatabase.Element<Polyline>(playgroundFrame.Item1));

                // CAD原始数据处理流程
                ThSitePlanConfigService.Instance.Initialize();
                newroot.EnableAllGroupAndItem();
                ThSitePlanConfigItemGroup temproot = new ThSitePlanConfigItemGroup();
                temproot.Status = UpdateStaus.UpdateCAD;
                foreach (var item in newroot.Items)
                {
                    temproot.Items.Enqueue(item);
                }
                ThSitePlanConfigService.Instance.EnableAll(UpdateStaus.UpdateCAD);
                ThSitePlanEngine.Instance.Containers = currentunusedframe;
                ThSitePlanEngine.Instance.OriginFrame = playgroundFrame.Item1;
                ThSitePlanEngine.Instance.Generators = new List<ThSitePlanGenerator>()
                {
                    new ThSitePlanFrameNameGenerator(),
                    new ThSitePlanContentGenerator(),
                };
                ThSitePlanEngine.Instance.Run(acadDatabase.Database, temproot);

                // 初始化解构区的线框信息
                ThSitePlanDbEngine.Instance.Initialize(acadDatabase.Database);

                // CAD衍生数据处理流程
                ItemGenerateWithNewRoot(newroot,currentunusedframe,
                    ObjectId.Null,
                    new List<ThSitePlanGenerator>() { new ThSitePlanDerivedContentGenerator() },
                    true);

                // CAD种树处理流程
                ItemGenerateWithNewRoot(newroot, currentunusedframe,
                    ObjectId.Null,
                    new List<ThSitePlanGenerator>() { new ThSitePlanPlantGenerator() },
                    true);

                // CAD填充处理流程
                ItemGenerateWithNewRoot(newroot, currentunusedframe,
                    ObjectId.Null,
                    new List<ThSitePlanGenerator>() { new ThSitePlanBoundaryGenerator() },
                    true);

                // CAD阴影处理流程
                ItemGenerateWithNewRoot(newroot, currentunusedframe,
                    ObjectId.Null,
                    new List<ThSitePlanGenerator>()
                    { new ThSitePlanShadowContentGenerator(),
                  new ThSitePlanShadowGenerator()},
                    true);

                // 未识别图框清理流程
                ItemGenerateWithNewRoot(newroot, currentunusedframe,
                    playgroundFrame.Item1,
                    new List<ThSitePlanGenerator>()
                    { new ThSitePlanUndefineCleanGenerator()},
                    true);

                // CAD打印流程
                ItemGenerateWithNewRoot(newroot, currentunusedframe,
                    ObjectId.Null,
                    new List<ThSitePlanGenerator>()
                    { new ThSitePlanPDFGenerator()},
                    true);

                //PS处理流程
                ThSitePlanConfigService.Instance.Initialize();
                ThSitePlanConfigService.Instance.EnableAll(UpdateStaus.NoUpdate);
                newroot.EnableAllGroupAndItem();
                ThSitePlanConfigItemGroup pstemproot = new ThSitePlanConfigItemGroup();
                pstemproot.Status = UpdateStaus.UpdateCAD;
                foreach (var item in newroot.Items)
                {
                    pstemproot.Items.Enqueue(item);
                }
                using (var psService = new ThSitePlanPSService())
                {
                    // 连接PS失败
                    if (psService.ErrorCode != 0)
                    {
                        AcadApp.ShowAlertDialog(psService.ErrorMessage);
                        return;
                    }

                    // PS处理流程
                    ThSitePlanPSEngine.Instance.Generators = new List<ThSitePlanPSGenerator>()
                    {
                       new ThSitePlanPSDefaultGenerator(psService),
                    };
                    //OpenUpdateItems(ThSitePlanConfigService.Instance.Root, updateframes, UpdateStaus.UpdateCAD, true);
                    ThSitePlanPSEngine.Instance.PSRun(
                        ThSitePlanSettingsService.Instance.OutputPath,
                        pstemproot);

                    newroot.UnableAllGroupAndItem();

                    // 保存PS生成的文档
                    //psService.ExportToFile(ThSitePlanSettingsService.Instance.OutputPath);
                }
            }

        }

        public void PhotoShopCleanWorkflow(ThSitePlanConfigItemGroup root)
        {
            if (root.Items.Count == 0)
            {
                return;
            }
            ThSitePlanConfigItemGroup temproot = new ThSitePlanConfigItemGroup();
            foreach (var item in root.Items)
            {
                temproot.Items.Enqueue(item);
            }
            temproot.Status = UpdateStaus.UpdateCAD;

            using (var psService = new ThSitePlanPSService())
            {
                // 异常处理：无法连接PS
                if (psService.ErrorCode != 0)
                {
                    AcadApp.ShowAlertDialog(psService.ErrorMessage);
                    return;
                }

                //启动PS引擎，开始更新 
                ThSitePlanPSEngine.Instance.Generators = new List<ThSitePlanPSGenerator>()
                {
                    new ThSitePlanPSDefaultGenerator(psService),
                };
                temproot.EnableAllGroupAndItem();
                //OpenUpdateItems(temproot, updateframes,UpdateStaus.UpdateCAD,true);
                ThSitePlanPSEngine.Instance.PSClean(temproot);

                root.UnableAllGroupAndItem();

                // 保存PS生成的文档
                //psService.ExportToFileForUpdate(ThSitePlanSettingsService.Instance.OutputPath);
            }
        }

        public void CADCleanWorkflow(ThSitePlanConfigItemGroup root, Queue<Tuple<ObjectId, Vector3d>> containers, ObjectId originframe)
        {
            if (root.Items.Count == 0)
            {
                return;
            }
            ThSitePlanConfigItemGroup temproot = new ThSitePlanConfigItemGroup();
            foreach (var item in root.Items)
            {
                temproot.Items.Enqueue(item);
            }
            temproot.EnableAllGroupAndItem();
            ItemGenerateWithNewRoot(temproot,
                containers,
                originframe,
                new List<ThSitePlanGenerator>()
                { new ThSitePlanFrameCleanGenerator()},
                false);

            root.UnableAllGroupAndItem();
        }

        public void PSUpdateSavePreocess()
        {
            using (var psService = new ThSitePlanPSService())
            {
                // 保存PS生成的文档
                psService.ExportToFileForUpdate(ThSitePlanSettingsService.Instance.OutputPath);
            }
        }
    }
}

