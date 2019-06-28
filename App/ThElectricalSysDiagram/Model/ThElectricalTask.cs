using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using DotNetARX;
using Linq2Acad;
using NFox.Cad.Collections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using TianHua.AutoCAD.Utility.ExtensionTools;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace ThElectricalSysDiagram
{
    public class ThElectricalTask
    {
        private static string downStreamLayer = @"天华AI-电气设备块";
        //private static string upStreamLayer = @"天华AI-提资专业块";
        private static string blockTableName = @"天华AI块关系对应表";
        private static string fanTableName = @"风机类型表";
        private static string filePath = ThElectricalSysDiagramUtils.BlockTemplateFilePath();
        private static string convertBlockName = "";

        public List<ThBlockInfo> GetThBlockInfos()
        {
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            var blocks = new List<BlockReference>();
            WithTrans(() =>
            {
                //得到所有锁定的图层
                var lockLayers = new List<string>();
                lockLayers = db.GetAllLayers().Where(la => la.IsLocked).Select(la => la.Name).ToList();


                //确定要拾取的车位类型，只允许块参照被选中,只允许不被锁定的图层被选中
                blocks = SelectionTool.DocChoose<BlockReference>(() =>
           {
               return ed.GetSelection(new PromptSelectionOptions() { MessageForAdding = "请选择需要进行转换的上游专业块类型" }, OpFilter.Bulid(fil => fil.Dxf(0) == "insert" & fil.Dxf(8) != string.Join(",", lockLayers)));

           });
                if (blocks == null)
                {
                    return;
                }
            });
            ////获取图标临时文件夹路径
            //string tempDirName = GetTempDirectory().FullName;

            //去重复，并计算输出图像路径


            var result = new List<ThBlockInfo>();
            using (var acDb = AcadDatabase.Active())
            {
                //先求出块的真实名称,并根据块参照获取块表记录
                //再找到从普通块定义中找到和它名字相同的那个普通块
                //根据获取的普通块，获取块截图，实例化对象
                result = blocks.Distinct(new CompareElemnet<BlockReference>((i, j) => i.Name == j.Name)).Select(b => new { realBlockName = b.GetRealBlockName(), BtrRecord = b.BlockTableRecord.GetObjectByID<BlockTableRecord>() })
                            .Join(acDb.Blocks.OfType<BlockTableRecord>(), a => a.realBlockName, block => block.Name, (a, block) => new ThBlockInfo(a.BtrRecord.Name, a.realBlockName, block.PreviewIcon)).ToList();

            }

            return result;
        }

        /// <summary>
        /// 从外部数据源获取已有上下游块对应关系
        /// </summary>
        /// <returns></returns>
        public List<ThRelationBlockInfo> GetThRelationInfos()
        {
            var result = new List<ThRelationBlockInfo>();
            using (var db = AcadDatabase.Open(filePath, DwgOpenMode.ReadOnly))
            {
                //获取数据源
                var table = db.ModelSpace
                              .OfType<Table>().First(t => t.Cells[0, 0].Value != null &&
                              t.Cells[0, 0].Value.ToString().Contains(blockTableName));

                //去除标题和表头
                //通过第一行和第二行，获取对应的块表记录
                //通过块表记录，实例化块对应关系(由于表格中存储的是普通块的记录，所以可以正确显示,也不需要获取匿名，因为在不同的数据库中，匿名是不同的)
                result = table.Rows.Select((r, i) => i).Where(i => i > 1 && table.Cells[i, 0].Contents.Count > 0 && table.Cells[i, 0].Contents[0].BlockTableRecordId != null)
                     .Select(i => new { b1 = db.Blocks.Element(table.Cells[i, 0].Contents[0].BlockTableRecordId), b2 = db.Blocks.Element(table.Cells[i, 1].Contents[0].BlockTableRecordId) })
                     .Select(a => new ThRelationBlockInfo(new ThBlockInfo(a.b1.Name, a.b1.PreviewIcon), new ThBlockInfo(a.b2.Name, a.b2.PreviewIcon))).ToList();

            }
            return result;

        }


        public List<ThRelationFanInfo> GetThFanInfos()
        {
            var result = new List<ThRelationFanInfo>();
            using (var db = AcadDatabase.Open(filePath, DwgOpenMode.ReadOnly))
            {
                //获取数据源
                var table = db.ModelSpace
                              .OfType<Table>().First(t => t.Cells[0, 0].Value != null &&
                              t.Cells[0, 0].GetRealTextString().Contains(fanTableName));

                //去除标题和表头
                //通过第一行和第二行，获取对应的块表记录
                //通过块表记录，实例化块对应关系
                result = table.Rows.Select((r, i) => i).Where(i => i > 1 && table.Cells[i, 0].Value != null && table.Cells[i, 1].Value != null && table.Cells[i, 2].Contents.Count > 0 && table.Cells[i, 2].Contents[0].BlockTableRecordId != null)
                     .Select(i => new ThRelationFanInfo(table.Cells[i, 0].GetRealTextString(), table.Cells[i, 1].GetRealTextString(), table.Cells[i, 2].Contents[0].BlockTableRecordId.GetObjectByID<BlockTableRecord>().Name, db.Blocks.Element(table.Cells[i, 2].Contents[0].BlockTableRecordId).PreviewIcon)).ToList();

            }
            return result;

        }


        /// <summary>
        /// 块表记录导入的示例
        /// </summary>
        public void GetTableInfo()
        {
            var table = new Table();
            using (var db = AcadDatabase.Open(filePath, DwgOpenMode.ReadOnly))
            {
                table = db.ModelSpace
                             .OfType<Table>().First();

                ////去除标题和表头，通过第一行和第二行，获取对应的块表记录
                //var btrs = table.Rows.Select((r, i) => i).Where(i => i > 1 && table.Cells[i, 0].Contents.Count > 0 && table.Cells[i, 0].Contents[0].BlockTableRecordId != null)
                //    .Select(i => new { b1 = table.Cells[i, 0].Contents[0].BlockTableRecordId.GetObjectByID<BlockTableRecord>(), b2 = table.Cells[i, 1].Contents[0].BlockTableRecordId.GetObjectByID<BlockTableRecord>() })
                //    .Select(a => new { handel1 = a.b1.GetNormalBlockHandle(), name1 = a.b1.Name, handel2 = a.b2.GetNormalBlockHandle() }).Skip(2);

                //去除标题和表头，通过第一行和第二行，获取对应的块表记录
                var btrs = table.Rows.Select((r, i) => i).Where(i => i > 1 && table.Cells[i, 0].Contents.Count > 0 && table.Cells[i, 0].Contents[0].BlockTableRecordId != null)
                    .Select(i => new { b1 = table.Cells[i, 0].Contents[0].BlockTableRecordId, b2 = table.Cells[i, 1].Contents[0].BlockTableRecordId })
                    .Select(a => a.b1.GetObjectByID<BlockTableRecord>().Name);

                //ObjectIdCollection blockIds = new ObjectIdCollection();
                //btrs.ForEach(a => { blockIds.Add(a.b1); blockIds.Add(a.b2); });

                //using (var destDb = AcadDatabase.Active())
                //{
                //    //定义一个IdMapping对象
                //    IdMapping mapping = new IdMapping();
                //    //从源数据库向目标数据库复制块表记录
                //    db.Database.WblockCloneObjects(blockIds, destDb.Database.BlockTableId, mapping, DuplicateRecordCloning.Replace, false);
                //}



            }
        }


        /// <summary>
        /// 删除指定行
        /// </summary>
        public void DeleteTableRow(int row)
        {
            //*****如果将块至于表格中，表格内只能储存块定义,且此块定义为可见性块的普通块的块定义，所以，到底使用块定义还是块参照来管理，先搁置一下

            using (var db = AcadDatabase.Open(filePath, DwgOpenMode.ReadWrite))
            {
                db.ModelSpace.OfType<Table>().UpgradeOpen().ForEach(tb =>
                {
                    //如果删除的是第三个，对应表格中其实是第五行,实际就是删除四，所以最后是加1
                    tb.DeleteRows(row + 1, 1);

                });

                db.Save();
            }

        }

        /// <summary>
        /// 在指定位置添加行
        /// </summary>
        public void AddTableRow(int row)
        {
            using (var db = AcadDatabase.Open(filePath, DwgOpenMode.ReadWrite))
            {
                db.ModelSpace.OfType<Table>().UpgradeOpen().ForEachDbObject(tb =>
                {
                    //如果从第二行插入，对应表格中为第四行，实际就是从第五行插入，遍历从0开始故为4，所以最后是加2
                    tb.InsertRows(row + 2, tb.Rows[1].Height, 1);

                });

                db.Save();
            }

        }


        /// <summary>
        /// 直接根据数据库内容进行块转换
        /// </summary>
        public void ConvertBlock(ObservableCollection<ThRelationBlockInfo> ruleBlockInfos)
        {
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            using (var currentDb = AcadDatabase.Active())
            {
                //初始化图层
                InitialLayer(downStreamLayer);

                //得到所有规则的块名的过滤器的通配字符的连接字符串
                convertBlockName = string.Join(",", ruleBlockInfos.Select(rule => rule.UpstreamBlockInfo.RealName));

                //得到所有锁定的图层
                var lockLayers = new List<string>();
                lockLayers = db.GetAllLayers().Where(la => la.IsLocked).Select(la => la.Name).ToList();

                //确定要拾取的块类型，只允许块参照被选中,只允许指定的块名被选中,只允许不被锁定的图层被选中
                IEnumerable<BlockReference> blocks = SelectionTool.DocChoose<BlockReference>(() =>
                {
                    return ed.GetSelection(new PromptSelectionOptions() { MessageForAdding = "请选择需要进行转换的上游专业的块参照" }, OpFilter.Bulid(fil => fil.Dxf(0) == "insert" & fil.Dxf(2) == "`*U*," + convertBlockName & fil.Dxf(8) != string.Join(",", lockLayers)));

                }, OnSelectionAddedDynamicFilter);
                if (blocks == null)
                {
                    return;
                }

                //只允许存在于表格记录中的块名被操作
                //并从转换记录中，取出要转换的块名
                var infos = blocks.Join(ruleBlockInfos, b => b.GetRealBlockName(), rule => rule.UpstreamBlockInfo.RealName, (b, rule) => new { b, rule }).ToList();

                using (var sourceDb = AcadDatabase.Open(filePath, DwgOpenMode.ReadOnly))
                {
                    //打开外部库的块表记录，根据上面求出的要进行转换的块名,找出其中需要导入的记录信息，注意去重
                    var ids = sourceDb.Database.BlockTableId.GetObjectByID<BlockTable>().Cast<ObjectId>().Select(id => id.GetObjectByID<BlockTableRecord>()).Join(infos, btr => btr.Name, info => info.rule.DownstreamBlockInfo.RealName, (btr, info) => btr.ObjectId).Distinct();

                    //从源数据库向目标数据库复制块表记录
                    sourceDb.Database.WblockCloneObjects(new ObjectIdCollection(ids.ToArray()), db.BlockTableId, new IdMapping(), DuplicateRecordCloning.Replace, false);
                }

                infos.ForEach(info =>
                {
                    //在指定位置插入下游专业的块
                    db.GetModelSpaceId().InsertBlockReference(downStreamLayer, info.rule.DownstreamBlockInfo.RealName, info.b.Position, new Scale3d(), 0);
                });

                var optKey = new PromptKeywordOptions("\n是否要删除提资专业块？[是(Y)/否(N)]");
                //为点交互类添加关键字
                optKey.Keywords.Add("Y");
                optKey.Keywords.Add("N");
                optKey.Keywords.Default = "Y"; //设置默认的关键字

                optKey.AppendKeywordsToMessage = false;//将关键字列表添加到提示信息中
                var res = ed.GetKeywords(optKey);
                //没有选择，全部回滚
                if (res.Status != PromptStatus.OK) currentDb.DiscardChanges();

                switch (res.StringResult)
                {
                    case "Y":
                        //将源对象删除
                        currentDb.ModelSpace.OfType<BlockReference>().Join(infos, b => b.ObjectId, a => a.b.ObjectId, (b, a) => b).UpgradeOpen().ForEach(b =>
                        {
                            b.Erase();
                        });
                        ed.WriteMessage("\n块替换完成");
                        break;

                    case "U":
                        ed.WriteMessage("\n块替换完成");
                        break;
                }

            }

        }

        /// <summary>
        /// 直接根据数据库内容进行块转换
        /// </summary>
        /// <param name="thBlockInfos"></param>
        public void ConvertFanBlock(ObservableCollection<ThRelationFanInfo> ruleFanInfos)
        {
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            using (var currentDb = AcadDatabase.Active())
            {
                //初始化图层
                InitialLayer(downStreamLayer);
                ////关闭上游专业图层
                //var layer = currentDb.Layers.Element(upStreamLayer, true);
                //layer.IsOff = true;

                //得到所有锁定的图层
                var lockLayers = db.GetAllLayers().Where(la => la.IsLocked).Select(la => la.Name);

                //确定要拾取的块类型，只允许块参照被选中,只允许指定的块名被选中,只允许不被锁定的图层被选中
                IEnumerable<BlockReference> blocks = SelectionTool.DocChoose<BlockReference>(() =>
                {
                    return ed.GetSelection(new PromptSelectionOptions() { MessageForAdding = "请选择需要进行转换的上游专业的块参照" }, OpFilter.Bulid(fil => fil.Dxf(0) == "insert" & fil.Dxf(8) != string.Join(",", lockLayers)));

                }, OnSelectionAddedPowerFilter);
                if (blocks == null)
                {
                    return;
                }

                //只允许存在于表格记录中的块名被操作
                //并从转换记录中，取出要转换的块名
                var infos = blocks.Join(ruleFanInfos, b => b.Layer, rule => rule.LayerName, (b, rule) => new { b, rule }).ToList();

                //经以上两个条件过滤后，还依然有待转换的块，再进行如下操作
                if (infos.Any())
                {
                    using (var sourceDb = AcadDatabase.Open(filePath, DwgOpenMode.ReadOnly))
                    {
                        //打开外部库的块表记录，根据上面求出的要进行转换的块名,找出其中需要导入的记录信息，注意去重
                        var ids = sourceDb.Blocks.Join(infos, btr => btr.Name, info => info.rule.FanBlockName, (btr, info) => btr.ObjectId).Distinct();

                        //从源数据库向目标数据库复制块表记录
                        sourceDb.Database.WblockCloneObjects(new ObjectIdCollection(ids.ToArray()), db.BlockTableId, new IdMapping(), DuplicateRecordCloning.Replace, false);
                    }

                    infos.ForEach(info =>
                    {
                        //实例化块参照
                        var fanBlock = new BlockReference(info.b.Position, currentDb.Blocks.Element(info.rule.FanBlockName).ObjectId);

                        //炸开实体，将风机类型和功率值换为正确的值
                        DBObjectCollection objs = new DBObjectCollection();
                        fanBlock.Explode(objs);

                        //遍历块中实体，获取真实的功率信息,找到属于文字的，并从中找出文字内容包含kw的文字信息
                        var powerInfo = currentDb.Blocks.Element(info.b.Name).Cast<ObjectId>().SelectMany(id => id.acdbEntGetTypedVals().Where(tvs => tvs.TypeCode == 1)).FirstOrDefault(tvs => Regex.Match(tvs.Value.ToString(), @"kw", RegexOptions.IgnoreCase).Success);

                        //处理后赋值给规则
                        info.rule.PowerInfo = powerInfo.Value.ToString();

                        //然后修改样式和值
                        objs.Cast<Entity>().ForEach(ent =>
                        {
                            //*****不修改图层，炸开后保持原来块内的图层
                            //ent.Layer = downStreamLayer;

                            var dbText = ent as DBText;
                            if (dbText != null)
                            {
                                if (dbText.TextString == "风机类型")
                                {
                                    dbText.TextString = info.rule.FanStyleName;
                                }
                                if (dbText.TextString == "功率：xkW")
                                {
                                    dbText.TextString = dbText.TextString.Replace("xkW", info.rule.PowerInfo.ToString());
                                }
                            }
                        });

                        //修改完毕后，添加进入模型空间
                        db.AddToModelSpace(objs.Cast<Entity>().ToArray());

                        fanBlock.Dispose();

                    });


                    var optKey = new PromptKeywordOptions("\n是否要删除提资专业块？[是(Y)/否(N)]");
                    //为点交互类添加关键字
                    optKey.Keywords.Add("Y");
                    optKey.Keywords.Add("N");
                    optKey.Keywords.Default = "Y"; //设置默认的关键字

                    optKey.AppendKeywordsToMessage = false;//将关键字列表添加到提示信息中
                    var res = ed.GetKeywords(optKey);
                    //没有选择，全部回滚
                    if (res.Status != PromptStatus.OK) currentDb.DiscardChanges();

                    switch (res.StringResult)
                    {
                        case "Y":
                            //将源对象删除
                            currentDb.ModelSpace.OfType<BlockReference>().Join(infos, b => b.ObjectId, a => a.b.ObjectId, (b, a) => b).UpgradeOpen().ForEach(b => b.Erase());
                            ed.WriteMessage("\n块替换完成");
                            break;

                        case "U":
                            ed.WriteMessage("\n块替换完成");
                            break;
                    }


                }


            }

        }


        /// <summary>
        /// 过滤带kw的块
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnSelectionAddedPowerFilter(object sender, SelectionAddedEventArgs e)
        {
            var ids = e.AddedObjects.GetObjectIds();
            //没有取到就不执行
            if (ids.Count() == 0)
            {
                return;
            }
            using (var currentDb = AcadDatabase.Active())
            {
                for (int i = 0; i < ids.Length; i++)
                {
                    var block = currentDb.ModelSpace.OfType<BlockReference>().First(b => b.ObjectId == ids[i]);
                    //对每一个块，过滤其中包含了文字的，且文字信息带kw的
                    if (!currentDb.Blocks.Element(block.Name).Cast<ObjectId>().Any(id => id.acdbEntGetTypedVals().Any(tvs => tvs.TypeCode == 1 && Regex.Match(tvs.Value.ToString(), @"kw", RegexOptions.IgnoreCase).Success)))
                    {
                        e.Remove(i);
                    }
                }

            }

        }


        /// <summary>
        /// 过滤符合普通块名的动态块
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnSelectionAddedDynamicFilter(object sender, SelectionAddedEventArgs e)
        {
            var ids = e.AddedObjects.GetObjectIds();
            //没有取到就不执行
            if (ids.Count() == 0)
            {
                return;
            }
            using (var tr = ids[0].Database.TransactionManager.StartOpenCloseTransaction())
            {
                for (int i = 0; i < ids.Length; i++)
                {
                    var br = (BlockReference)tr.GetObject(ids[i], OpenMode.ForRead);
                    var btr = (BlockTableRecord)tr.GetObject(br.DynamicBlockTableRecord, OpenMode.ForRead);
                    if (!Autodesk.AutoCAD.Internal.Utils.WcMatchEx(btr.Name, convertBlockName, true))
                        e.Remove(i);
                }
                tr.Commit();
            }
        }

        /// <summary>
        /// 初始化需要的图层
        /// </summary>
        /// <param name="strs"></param>
        private void InitialLayer(params string[] strs)
        {
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            strs.ForEach(str => db.AddLayer(str));
        }

        public void WithTrans(Action action)
        {
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                try
                {
                    action();
                    trans.Commit();
                }
                catch (System.Exception)
                {
                    trans.Abort();
                }
            }
        }
    }

}
