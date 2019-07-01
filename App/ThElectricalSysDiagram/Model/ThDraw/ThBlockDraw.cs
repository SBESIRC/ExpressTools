using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using DotNetARX;
using Linq2Acad;
using NFox.Cad.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TianHua.AutoCAD.Utility.ExtensionTools;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;


namespace ThElectricalSysDiagram
{
    public class ThBlockDraw : ThDraw
    {
        public List<ThRelationBlockInfo> BlockInfos { get; set; }
        public ThBlockDraw(List<ThRelationBlockInfo> infos)
        {
            this.BlockInfos = infos;
        }

        private static string convertBlockName = "";

        public override List<ThElement> GetElements()
        {
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            using (var currentDb = AcadDatabase.Active())
            {
                //获取按块名配置的规则
                var ruleBlockInfos = this.BlockInfos;
                ////初始化图层
                //InitialLayer(downStreamLayer);

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
                    return new List<ThElement>();
                }

                //并从转换记录中，取出要转换的块名
                var elements = blocks.Join(ruleBlockInfos, b => b.GetRealBlockName(), rule => rule.UpstreamBlockInfo.RealName, (b, rule) => new ThBlockElement(b.ObjectId, b.Name, rule));

                return elements.Cast<ThElement>().ToList();

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

        public override void ImportRule()
        {
            using (var currentDb = AcadDatabase.Active())
            {
                using (var sourceDb = AcadDatabase.Open(ThElectricalTask.filePath, DwgOpenMode.ReadOnly))
                {
                    //打开外部库的块表记录，根据上面求出的要进行转换的块名,找出其中需要导入的记录信息，注意去重
                    var ids = sourceDb.Blocks.Join(this.Elements, btr => btr.Name, info => ((ThBlockElement)info).BlockInfo.DownstreamBlockInfo.RealName, (btr, info) => btr.ObjectId).Distinct();

                    //从源数据库向目标数据库复制块表记录
                    sourceDb.Database.WblockCloneObjects(new ObjectIdCollection(ids.ToArray()), currentDb.Database.BlockTableId, new IdMapping(), DuplicateRecordCloning.Replace, false);
                }
            }
        }

        protected override Func<ThElement, string> InfoFunc()
        {
            return info => ((ThBlockElement)info).BlockInfo.DownstreamBlockInfo.RealName;
        }

        public override void Deal()
        {
            using (var currentDb = AcadDatabase.Active())
            {
                this.Elements.ForEach(element =>
                {
                    //在指定位置插入下游专业的块,这里设置插入点为中心位置
                    currentDb.Database.GetModelSpaceId().InsertBlockReference(ThElectricalTask.downStreamLayer, ((ThBlockElement)element).BlockInfo.DownstreamBlockInfo.RealName, (currentDb.ModelSpace.Element(element.ElementId) as BlockReference).GetCenter().toPoint3d(), new Scale3d(), 0);
                });
            }
        }
    }
}
