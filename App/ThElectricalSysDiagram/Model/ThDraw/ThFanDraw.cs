using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using DotNetARX;
using Linq2Acad;
using NFox.Cad.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TianHua.AutoCAD.Utility.ExtensionTools;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;


namespace ThElectricalSysDiagram
{
    public class ThFanDraw : ThDraw
    {
        public List<ThRelationFanInfo> FanInfos { get; set; }
        public ThFanDraw(List<ThRelationFanInfo> infos)
        {
            this.FanInfos = infos;
            //this.Elements = GetElements();
        }

        public ThFanDraw(List<ThElement> elements)
        {
            this.Elements = elements;
        }

        protected override Func<ThElement, string> InfoFunc()
        {
            return info => ((ThFanElement)info).FanInfo.FanBlockName;
        }

        public override List<ThElement> GetElements()
        {
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            using (var currentDb = AcadDatabase.Active())
            {
                //获取图层名配置的规则
                var ruleFanInfos = this.FanInfos;

                //初始化图层
                InitialLayer("EQUIP-消防");

                //得到所有锁定的图层
                var lockLayers = db.GetAllLayers().Where(la => la.IsLocked).Select(la => la.Name);

                //确定要拾取的块类型，只允许块参照被选中,只允许指定的块名被选中,只允许不被锁定的图层被选中
                IEnumerable<BlockReference> blocks = SelectionTool.DocChoose<BlockReference>(() =>
                {
                    return ed.GetSelection(new PromptSelectionOptions() { MessageForAdding = "请选择需要进行转换的上游专业的块参照" }, OpFilter.Bulid(fil => fil.Dxf(0) == "insert" & fil.Dxf(8) == string.Join(",", ruleFanInfos.Select(rule => rule.LayerName)) & fil.Dxf(8) != string.Join(",", lockLayers)));

                }, OnSelectionAddedPowerFilter);
                if (blocks == null)
                {
                    return new List<ThElement>();
                }

                //只允许存在于表格记录中的块名被操作
                //并从转换记录中，取出要转换的块名
                var elements = blocks.Join(ruleFanInfos, b => b.Layer, rule => rule.LayerName, (b, rule) => new ThFanElement(b.ObjectId, b.Name, rule));

                return elements.Cast<ThElement>().ToList();
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

        public override void Deal()
        {
            using (var currentDb = AcadDatabase.Active())
            {
                this.Elements.ForEach(element =>
            {
                try
                {
                    var info = ((ThFanElement)element).FanInfo;
                    //实例化块参照,插入点为上游块图形的下角点
                    var fanBlock = new BlockReference((currentDb.ModelSpace.Element(element.ElementId) as BlockReference).GeometricExtents.MinPoint, currentDb.Blocks.Element(info.FanBlockName).ObjectId);


                    //炸开实体，将风机类型和功率值换为正确的值
                    DBObjectCollection objs = new DBObjectCollection();
                    fanBlock.Explode(objs);

                    //遍历块中实体，获取真实的功率信息,找到属于文字的，并从中找出文字内容包含kw的文字信息
                    var powerInfo = currentDb.Blocks.Element(element.Name).Cast<ObjectId>().SelectMany(id => id.acdbEntGetTypedVals().Where(tvs => tvs.TypeCode == 1)).FirstOrDefault(tvs => Regex.Match(tvs.Value.ToString(), @"kw", RegexOptions.IgnoreCase).Success);

                    //处理后赋值给规则
                    info.PowerInfo = powerInfo.Value.ToString();

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
                                dbText.TextString = info.FanStyleName;
                            }
                            if (dbText.TextString == "功率：xkW")
                            {
                                dbText.TextString = dbText.TextString.Replace("xkW", info.PowerInfo.ToString());
                            }
                        }
                    });

                    //修改完毕后，添加进入模型空间
                    currentDb.Database.AddToModelSpace(objs.Cast<Entity>().ToArray());

                    fanBlock.Dispose();
                }
                //捕获边界有问题的块的异常，不进行进一步处理
                catch (Autodesk.AutoCAD.Runtime.Exception ex)
                {
                    currentDb.Database.GetEditor().WriteMessage("\n" + ex.Source + ":" + ex.Message);
                }

            });
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

    }
}
