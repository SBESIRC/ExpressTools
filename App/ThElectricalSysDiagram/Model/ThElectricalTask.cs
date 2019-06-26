﻿using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using DotNetARX;
using Linq2Acad;
using NFox.Cad.Collections;
using System;
using System.Collections.Generic;
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
        private static string upStreamLayer = @"天华AI-提资专业块";
        private static string blockTableName = @"天华AI块关系对应表";
        private static string fanTableName = @"风机类型表";
        private static string filePath = ThElectricalSysDiagramUtils.BlockTemplateFilePath();

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

            ////找到已有的，加上新添加的，绑定到车位listview
            //result = viewModel.ParkingLotInfos.Union(thBlockInfos, new CompareElemnet<ThBlockInfo>((i, j) => i.Name == j.Name)).ToList();


            //var realLots = result.Except(result.Join(viewModel.ParkingLotInfos, p1 => p1, p2 => p2, (p1, p2) => p1));
            //foreach (var item in realLots)
            //{
            //    viewModel.ParkingLotInfos.Add(item);
            //}
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
                //通过块表记录，实例化块对应关系(由于表格中存储的是普通块的记录，所以可以正确显示)
                result = table.Rows.Select((r, i) => i).Where(i => i > 1 && table.Cells[i, 0].Contents.Count > 0 && table.Cells[i, 0].Contents[0].BlockTableRecordId != null)
                     .Select(i => new { b1 = table.Cells[i, 0].Contents[0].BlockTableRecordId.GetObjectByID<BlockTableRecord>(), b2 = table.Cells[i, 1].Contents[0].BlockTableRecordId.GetObjectByID<BlockTableRecord>() })
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
        public void ConvertBlock()
        {
            //首先获取表格中的款转换关系记录
            var ruleBlockInfos = GetThRelationInfos();

            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            using (var currentDb = AcadDatabase.Active())
            {
                //初始化图层
                InitialLayer(downStreamLayer, upStreamLayer);
                //关闭上游专业图层
                var layer = currentDb.Layers.Element(upStreamLayer, true);
                layer.IsOff = true;

                //得到所有锁定的图层
                var lockLayers = new List<string>();
                lockLayers = db.GetAllLayers().Where(la => la.IsLocked).Select(la => la.Name).ToList();

                //确定要拾取的块类型，只允许块参照被选中,只允许指定的块名被选中,只允许不被锁定的图层被选中
                //***这里会缺少动态块（匿名块）被选中的情况，但还是使用块名去过滤，因为不这么做，系统一口气选中的态度，可能会导致速度变慢，最后再把那些匿名的动态块加进来处理就好，暂时这么处理
                IEnumerable<BlockReference> blocks = SelectionTool.DocChoose<BlockReference>(() =>
                {
                    return ed.GetSelection(new PromptSelectionOptions() { MessageForAdding = "请选择需要进行转换的上游专业的块参照" }, OpFilter.Bulid(fil => fil.Dxf(0) == "insert" & fil.Dxf(8) != string.Join(",", lockLayers)));

                });
                if (blocks == null)
                {
                    return;
                }

                #region 单独加匿名块的代码段
                ////由于不同的数据库，导入动态块后的匿名块命会发生变化，所以需要找到普通块对应的匿名块
                //var anyBlocks = new List<BlockReference>();
                //using (var acDb = AcadDatabase.Active())
                //{
                //    //先找到所有的匿名块
                //    anyBlocks = acDb.ModelSpace.OfType<BlockReference>().Where(b => b.Name.StartsWith("*U")).ToList();
                //}

                ////把所有的匿名块也加入到选择集中
                //blocks = blocks.Union(anyBlocks); 
                #endregion

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

                //必须重新重模型空间中，找出对应的块参照，放到指定的上游图层去，以实现块的替换效果，但保留原来的图形
                currentDb.ModelSpace.OfType<BlockReference>().Join(infos, b => b.ObjectId, a => a.b.ObjectId, (b, a) => b).ForEach(b => { b.UpgradeOpen(); b.Layer = upStreamLayer; });
            }

        }

        /// <summary>
        /// 直接根据数据库内容进行块转换
        /// </summary>
        /// <param name="thBlockInfos"></param>
        public void ConvertFanBlock()
        {
            //首先获取表格中的款转换关系记录
            var ruleFanInfos = GetThFanInfos();

            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            using (var currentDb = AcadDatabase.Active())
            {
                //初始化图层
                InitialLayer(downStreamLayer, upStreamLayer);
                //关闭上游专业图层
                var layer = currentDb.Layers.Element(upStreamLayer, true);
                layer.IsOff = true;

                //得到所有锁定的图层
                var lockLayers = db.GetAllLayers().Where(la => la.IsLocked).Select(la => la.Name);

                //确定要拾取的块类型，只允许块参照被选中,只允许指定的块名被选中,只允许不被锁定的图层被选中
                IEnumerable<BlockReference> blocks = SelectionTool.DocChoose<BlockReference>(() =>
                {
                    return ed.GetSelection(new PromptSelectionOptions() { MessageForAdding = "请选择需要进行转换的上游专业的块参照" }, OpFilter.Bulid(fil => fil.Dxf(0) == "insert" & fil.Dxf(8) != string.Join(",", lockLayers)));

                });
                if (blocks == null)
                {
                    return;
                }

                //找出包含Kw文字的块，如果不包含，则不属于要转换的块
                blocks = blocks.Where(b => currentDb.Blocks.Element(b.Name).Cast<ObjectId>().SelectMany(id => id.acdbEntGetTypedVals().Where(tvs => tvs.TypeCode == 1)).Any(tvs => Regex.Match(tvs.Value.ToString(), @"kw", RegexOptions.IgnoreCase).Success));

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
                            //修改图层
                            ent.Layer = downStreamLayer;

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

                    //必须重新重模型空间中，找出对应的块参照，放到指定的上游图层去，以实现块的替换效果，但保留原来的图形
                    currentDb.ModelSpace.OfType<BlockReference>().Join(infos, b => b.ObjectId, a => a.b.ObjectId, (b, a) => b).UpgradeOpen().ForEach(b => { b.Layer = upStreamLayer; });
                }


            }

        }


        public void Test()
        {
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            using (var currentDb = AcadDatabase.Active())
            {
                var ents = SelectionTool.DocChoose<Entity>(() =>
                 {
                     return ed.GetSelection();
                 });

                var ent = ents.First();

               var gg= ent.ObjectId.acdbEntGetTypedVals().FirstOrDefault(tpv => tpv.TypeCode == 360);

                System.Windows.Forms.MessageBox.Show(gg.Value.ToString());

                System.Windows.Forms.MessageBox.Show(((BlockReference)ent).GetRealBlockName());
                var xx = currentDb.Blocks.Element(((BlockReference)ent).GetRealBlockName()).OwnerId;

                System.Windows.Forms.MessageBox.Show(currentDb.Blocks.Any(btr => btr.OwnerId.ToString() == gg.Value.ToString()).ToString());

                //BlockTableRecord
                //******明天检查一下adsname，看看可不可以匹配到
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