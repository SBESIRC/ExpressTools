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
            ThSitePlanDocEventHandler.Instance.SubscribeToDoc(Active.Document);
        }

        public void Terminate()
        {
            ThSitePlanDocCollectionEventHandler.Instance.UnRegister();
            ThSitePlanDbEventHandler.Instance.UnsubscribeFromDb(Active.Database);
            ThSitePlanDocEventHandler.Instance.UnsubscribeFromDoc(Active.Document);
        }

        /// <summary>
        /// 一键生成
        /// </summary>
        [CommandMethod("TIANHUACAD", "THPGE", CommandFlags.Modal)]
        public void ThSitePlanGenerate()
        {
            Vector3d offset;
            ObjectId originFrame = ObjectId.Null;
            Extents3d originFrameExtents = new Extents3d();
            var frames = new Queue<Tuple<ObjectId, Vector3d>>();
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                // 确定图框
                ThSitePlanDbEngine.Instance.Initialize(Active.Database);
                originFrame = ThSitePlanDbEngine.Instance.FrameByName(ThSitePlanCommon.ThSitePlan_Frame_Name_Original);
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

            // CAD原始数据处理流程
            ThSitePlanConfigService.Instance.Initialize();
            ThSitePlanConfigService.Instance.EnableAll(UpdateStaus.UpdateCAD);
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

            // 初始化解构区的线框信息
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                ThSitePlanDbEngine.Instance.Initialize(acadDatabase.Database);
            }

            // CAD衍生数据处理流程
            ItemGeneratePreocess(frames,
                ObjectId.Null,
                new List<ThSitePlanGenerator>() { new ThSitePlanDerivedContentGenerator() },
                true,
                true,
                true);

            // CAD种树处理流程
            ItemGeneratePreocess(frames,
                ObjectId.Null,
                new List<ThSitePlanGenerator>() { new ThSitePlanPlantGenerator() },
                true,
                true,
                true);

            // CAD填充处理流程
            ItemGeneratePreocess(frames,
                ObjectId.Null,
                new List<ThSitePlanGenerator>() { new ThSitePlanBoundaryGenerator() },
                true,
                true,
                true);

            // CAD阴影处理流程
            ItemGeneratePreocess(frames,
                ObjectId.Null,
                new List<ThSitePlanGenerator>()
                { new ThSitePlanShadowContentGenerator(),
                  new ThSitePlanShadowGenerator()},
                true,
                true,
                true);

            // 未识别图框清理流程
            ItemGeneratePreocess(frames,
                playgroundFrame.Item1,
                new List<ThSitePlanGenerator>()
                { new ThSitePlanUndefineCleanGenerator()},
                true,
                true,
                true);

            // CAD打印流程
            ItemGeneratePreocess(frames,
                ObjectId.Null,
                new List<ThSitePlanGenerator>()
                { new ThSitePlanPDFGenerator()},
                true,
                true,
                true);

            //PS处理流程
            ThSitePlanConfigService.Instance.Initialize();
            ThSitePlanConfigService.Instance.EnableAll(UpdateStaus.UpdateCAD);
            using (var psService = new ThSitePlanPSService())
            {
                // 连接PS失败
                if (psService.ErrorCode != 0)
                {
                    Application.ShowAlertDialog(psService.ErrorMessage);
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

                // PS处理流程
                ThSitePlanPSEngine.Instance.Generators = new List<ThSitePlanPSGenerator>()
                 {
                    new ThSitePlanPSDefaultGenerator(psService),
                 };
                ThSitePlanPSEngine.Instance.PSRun(
                    ThSitePlanSettingsService.Instance.OutputPath,
                    ThSitePlanConfigService.Instance.Root);

                // 保存PS生成的文档
                psService.ExportToFile(ThSitePlanSettingsService.Instance.OutputPath);
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

                    //记住配置界面的新的设置
                    var currentConfig = dlg.m_ColorGeneralConfig;

                    // 比较配置变化，判断更新
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
                Application.ShowModalDialog(dlg);
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
                var undifineframe = ThSitePlanDbEngine.Instance.FrameByName(ThSitePlanCommon.ThSitePlan_Frame_Name_Unrecognized);
                var originalframe = ThSitePlanDbEngine.Instance.FrameByName(ThSitePlanCommon.ThSitePlan_Frame_Name_Original);
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
                ObjectIdCollection cadupdateframeid = new ObjectIdCollection();

                ThSitePlanDbEngine.Instance.Initialize(Active.Database);

                foreach (var curitem in currentallitems)
                {
                    bool preidincurrent = false;
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
                                        Tuple<ObjectId, Vector3d> updframe = new Tuple<ObjectId, Vector3d>(updframeid, new Vector3d(0, 0, 0));
                                        psupdateframes.Enqueue(updframe);
                                    }
                                }

                                //改变了CAD图层名或者脚本ID
                                else
                                {
                                    ObjectId updframeid = ThSitePlanDbEngine.Instance.FrameByName(prefullname);
                                    cadupdateframeid.Add(updframeid);
                                }
                            }
                            //全名不一致，或变组或改名
                            else
                            {
                                ObjectId updframeid = ThSitePlanDbEngine.Instance.FrameByName(prefullname);
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
                            }
                            preidincurrent = true;
                            break;
                        }
                    }
                    //新item的ID在旧的item列表中找不到对应项，则该项为新增
                    if (!preidincurrent)
                    {

                    }
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
                    Application.ShowAlertDialog(psService.ErrorMessage);
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
                psService.ExportToFile(ThSitePlanSettingsService.Instance.OutputPath);
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
                        Application.ShowAlertDialog(psService.ErrorMessage);
                        return;
                    }

                    //启动PS引擎，开始更新 
                    ThSitePlanPSEngine.Instance.Generators = new List<ThSitePlanPSGenerator>()
                    {
                        new ThSitePlanPSDefaultGenerator(psService),
                    };
                    OpenUpdateItems(ThSitePlanConfigService.Instance.Root, updateframes, UpdateStaus.UpdateCAD, ifnosid);
                    ThSitePlanPSEngine.Instance.PSUpdate(
                        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                        ThSitePlanConfigService.Instance.Root);

                    // 保存PS生成的文档
                    psService.ExportToFileForUpdate(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
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

        public void InsertToCADProcess()
        {

        }
    }
}

