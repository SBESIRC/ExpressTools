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
        public static string downStreamLayer = @"EQUIP-消防";
        //private static string upStreamLayer = @"天华AI-提资专业块";
        public static string blockTableName = @"天华AI块关系对应表";
        public static string fanTableName = @"风机类型表";
        public static string filePath = ThElectricalSysDiagramUtils.BlockTemplateFilePath();
        public static string convertBlockName = "";


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
                result = table.Rows.Select((r, i) => i).Where(i => i > 1 && table.Cells[i, 2].Contents.Count > 0 && table.Cells[i, 2].Contents[0].BlockTableRecordId != null)
                     .Select(i => new { b1 = db.Blocks.Element(table.Cells[i, 2].Contents[0].BlockTableRecordId), b2 = db.Blocks.Element(table.Cells[i, 5].Contents[0].BlockTableRecordId) })
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
                     .Select(i => new ThRelationFanInfo(table.Cells[i, 0].GetRealTextString(), table.Cells[i, 1].GetRealTextString(), db.Blocks.Element(table.Cells[i, 2].Contents[0].BlockTableRecordId).Name, db.Blocks.Element(table.Cells[i, 2].Contents[0].BlockTableRecordId).PreviewIcon)).ToList();

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


        public void ConvertTest(string type, List<ThRelationInfo> infos)
        {
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            using (var currentDb = AcadDatabase.Active())
            {
                //创建转换集
                var thDraw = ThDrawFactory.CreateThDraw(type, infos);

                //获取转换对象
                thDraw.Elements = thDraw.GetElements();
                //如果没有获取则不执行
                if (!thDraw.Elements.Any())
                {
                    return;
                }

                //导入规则
                thDraw.Import();

                //处理转换集
                thDraw.Deal();

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
                        //将转换集中的源对象删除
                        thDraw.Erase();
                        ed.WriteMessage("\n块替换完成");
                        break;

                    case "U":
                        ed.WriteMessage("\n块替换完成");
                        break;
                }
            }
        }

    }

}
