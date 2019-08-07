using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Internal;
using Autodesk.AutoCAD.Runtime;
using DotNetARX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
using TianHua.AutoCAD.Utility.ExtensionTools;
using NFox.Cad.Collections;
using System.Runtime.InteropServices;
using GeometryExtensions;
using ThElectrical.Model.ThColumn;
using ThElectrical.Model;
using ThElectrical.Model.ThDraw;
using ThElectrical.Model.ThElement;
using ThElectrical.Model.ThTable;
using System.Diagnostics;
using System.Collections.ObjectModel;
using Linq2Acad;
using System.Text.RegularExpressions;

namespace ThElectrical
{
    public class ThElectricalExchangeTask
    {
        [CommandMethod("test2")]
        public void Test2()
        {
            var i = SelectionTool.ChooseEntity<DBText>().Get3DCenter();
            var j = SelectionTool.ChooseEntity<DBText>().Get3DCenter();

            var result = i.IsBottomGongXian(j, ThElementFactory.circuitToPowerCapacityTol);
            System.Windows.Forms.MessageBox.Show(result.ToString());
        }

        //******改图纸的名字

        public void Test()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            using (var tr = AcadApp.DocumentManager.MdiActiveDocument.TransactionManager.StartOpenCloseTransaction())
            {
                //先处理配电系统图
                var draws = GetDraws().GroupBy(draw => draw.GetType()).First(gr => gr.Key == typeof(ThDistributionDraw));

                draws.ForEach(draw =>
                {
                    //实例化所有的列信息
                    var columns = GetColumns(draw.MinPoint, draw.MaxPoint);

                    //将所有列按照表格进行分组
                    var columnsGrp = columns.GroupTake(col => false, cols => ThColumnFactory.columnNames.Count);

                    //如果分组结果不符合标准，则不继续执行了
                    if (columnsGrp.Any(grp => grp.Count() != ThColumnFactory.columnNames.Count))
                    {
                        return;
                    }

                    //对每一个表格进行遍历
                    columnsGrp.ForEach(cols =>
                    {
                        //根据列求出列的所有元素
                        var elements = cols.SelectMany(column => column.GetThElements());

                        //把四列在一行的归为一组,每一组都由高到低排序,整体也由高到低排序
                        //以回路数重复为一组,从第一个开始拿，拿的个数为下一次第一个的编号出现为止，如果找不到下一个，则全部拿掉
                        var records = elements.OfType<ThCircuitElement>().OrderByDescending(ele => ele.Center.Y).Join(elements.OfType<ThPowerCapacityElement>().OrderByDescending(ele => ele.Center.Y), eleCir => eleCir.Center, eleCap => eleCap.Center, (eleCir, eleCap) => new ThCabinetRecord(eleCir, eleCap), new CompareElemnet<Point3d>((i, j) => i.IsBottomGongXian(j, ThElementFactory.circuitToPowerCapacityTol))).Join(elements.OfType<ThOutCableElement>().OrderByDescending(ele => ele.MinPoint.Y), ele => ele.CircuitElement.Center, eleCab => eleCab.MinPoint, (ele, eleCab) => { ele.OutCableElement = eleCab; return ele; }, new CompareElemnet<Point3d>((i, j) => i.IsBottomGongXian(j, ThElementFactory.circuitToOutCableTol))).Join(elements.OfType<ThBranchSwitchElement>().OrderByDescending(ele => ele.MinPoint.Y), ele => ele.CircuitElement.Center, eleSwi => eleSwi.MinPoint, (ele, eleSwi) => { ele.BranchSwitchElement = eleSwi; return ele; }, new CompareElemnet<Point3d>((i, j) => i.IsBottomGongXian(j, ThElementFactory.circuitToBranchSwitchTol))).OrderByDescending(a => a.CircuitElement.Center.Y).GroupTake(re => false, res => res.Select(re => re.CircuitElement.Number).TakeNumber(res.Select(re => re.CircuitElement.Number).First(), 2));

                        //最后加入表格，形成配电箱实体
                        var cabinets = records.Join(elements.OfType<ThCabinetElement>().OrderByDescending(ele => ele.MaxPoint.Y), res => res, ele => ele, (res, ele) => new ThCabinet(ele, res.ToObservableCollection()), new CompareChildElement<object, IEnumerable<ThCabinetRecord>, ThCabinetElement>((i, j) => i.Any(re => re.CircuitElement.Center.Y <= j.MaxPoint.Y && re.CircuitElement.Center.Y >= j.MinPoint.Y)));

                        var cabinet = cabinets.FirstOrDefault();
                        if (cabinet != null)
                        {
                            cabinet.Records.ForEach(re =>
                            {
                                re.PowerCapacityElement.AddObserver(new NotifyEventHandler(re.BranchSwitchElement.ReceiveAndUpdate));

                                re.PowerCapacityElement.Update();

                            });
                        }



                    });

                });

                tr.Commit();
            }

            sw.Stop();
            TimeSpan ts2 = sw.Elapsed;
            ed.WriteMessage("Stopwatch总共花费{0}ms.", ts2.Seconds);

        }

        /// <summary>
        /// 添加删除对象的监听
        /// </summary>
        public void AddErasedMornitor(ObjectErasedEventHandler objectErasedEvent)
        {
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            db.ObjectErased += objectErasedEvent;
        }

        /// <summary>
        /// 移除删除事件的监听
        /// </summary>
        /// <param name="objectErasedEvent"></param>
        public void RemoveErasedMornitor(ObjectErasedEventHandler objectErasedEvent)
        {
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            db.ObjectErased -= objectErasedEvent;
        }


        /// <summary>
        /// 更新配电箱信息
        /// </summary>
        public void UpdateCabinetInfo(ThCabinet cabinet)
        {
            cabinet.Records.ForEach(re =>
            {
                re.PowerCapacityElement.Update();
            });
        }

        public void UpdateCabinetInfo(ThCabinetRecord record)
        {
            record.PowerCapacityElement.Update();

        }

        /// <summary>
        /// 添加联动机制
        /// </summary>
        /// <param name="cabinet"></param>
        public void AddCabinetObserve(ThCabinet cabinet)
        {
            cabinet.Records.ForEach(re =>
            {
                re.PowerCapacityElement.AddObserver(new NotifyEventHandler(re.BranchSwitchElement.ReceiveAndUpdate));
            });
        }

        public void AddCabinetObserve(ThCabinetRecord record)
        {
            record.PowerCapacityElement.AddObserver(new NotifyEventHandler(record.BranchSwitchElement.ReceiveAndUpdate));
            record.PowerCapacityElement.AddObserver(new NotifyEventHandler(record.OutCableElement.ReceiveAndUpdate));
        }

        public void RemoveCabinetObserve(ThCabinetRecord record)
        {
            record.PowerCapacityElement.RemoveObserver(new NotifyEventHandler(record.BranchSwitchElement.ReceiveAndUpdate));
            record.PowerCapacityElement.RemoveObserver(new NotifyEventHandler(record.OutCableElement.ReceiveAndUpdate));
        }


        /// <summary>
        /// 获取所有的配电柜系统图，同时实例化列信息
        /// </summary>
        /// <returns></returns>
        public List<ThDistributionDraw> GetDistributionDraws()
        {
            var draws = GetDraws().OfType<ThDistributionDraw>();

            draws.ForEach(draw =>
            {
                //获取所有列信息，并将所有列按照表格进行分组
                draw.ColumnGrps = GetColumns(draw.MinPoint, draw.MaxPoint).GroupTake(col => false, cols => ThColumnFactory.columnNames.Count).Select(a => a.ToList()).ToObservableCollection();

            });

            return draws.ToList();
        }


        /// <summary>
        /// 获取所有系统图
        /// </summary>
        public List<ThDraw> GetDraws()
        {
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            //找到所有图纸类别的文字
            var texts = SelectionTool.DocChoose<Entity>(() => ed.SelectAll(OpFilter.Bulid(fil => fil.Dxf(0) == "mtext,text" & fil.Dxf(1) == string.Join(",", ThDrawFactory.drawNames))));

            //如果找不到文字则不执行
            if (texts == null)
            {
                return new List<ThDraw>();
            }

            var result = new List<ThDraw>();
            var blocks = SelectionTool.DocChoose<BlockReference>(() => ed.SelectAll(OpFilter.Bulid(fil => fil.Dxf(0) == "insert" & fil.Dxf(2) == "*_thape_a*")), (sender, e) =>
            {
                var ids = e.AddedObjects.GetObjectIds().ToList();

                using (var tr = AcadApp.DocumentManager.MdiActiveDocument.TransactionManager.StartOpenCloseTransaction())
                {
                    //图纸类别名在图纸类别文字的右侧，且距离较小,这里采用中心点进行比较
                    result = ids.Select(id => id.GetObjectByID<BlockReference>(tr)).Join(texts, b => b, txt => txt, (b, txt) => ThDrawFactory.CreateDraw(txt.ObjectId.acdbEntGetTypedVals().First(tv => tv.TypeCode == 1).Value.ToString().GetRealCADTextstring(), b.ObjectId, b.GeometricExtents.MinPoint, b.GeometricExtents.MaxPoint), new CompareChildElement<object, BlockReference, Entity>((i, j) => GetDrawNamePosition(i).X < j.Get3DCenter().X && GetDrawNamePosition(i).DistanceTo(j.Get3DCenter()) < ThDrawFactory.drawNameDis)).ToList();


                    tr.Commit();
                }
            });
            if (blocks == null)
            {
                return new List<ThDraw>();
            }

            //*****这里多种类型的图纸，暂时不知道怎么去排序和分类，因为设计师没有统一的逻辑，暂时先放一放，实现一张的
            return result;

        }

        /// <summary>
        /// 获取当前图纸的配电柜
        /// </summary>
        /// <returns></returns>
        public List<ThCabinet> GetCabinets(ThDistributionDraw draw)
        {
            //实例化所有的列信息,找出配电柜的列
            var columns = GetColumns(draw.MinPoint, draw.MaxPoint).OfType<ThCabinetColumn>();

            //如果没有配电柜列，不执行
            if (!columns.Any())
            {
                return new List<ThCabinet>();
            }

            var cabinets = columns.SelectMany(col => col.GetThElements().OfType<ThCabinetElement>().OrderByDescending(ele => ele.MaxPoint.Y).Select(ele => new ThCabinet(ele))).ToList();

            //获取当前图纸的表格信息
            var infos = GetTableInfos(draw);

            cabinets.ForEach(cabinet =>
            {
                //在配电箱范围内的表格线
                var rangeInfos = infos.Where(info => cabinet.Element.CabinetCenter.X < info.MaxPoint.X && cabinet.Element.CabinetCenter.X > info.MinPoint.X);

                //找出下面中距离最小的
                var minPoint = rangeInfos.Where(info => info.MinPoint.Y < cabinet.Element.CabinetCenter.Y).MinElement(info => info.MinPoint.DistanceTo(cabinet.Element.CabinetCenter)).MinPoint;
                cabinet.TableMinPoint = minPoint;

                //找出上面中距离最小的
                var maxPoint = rangeInfos.Where(info => info.MinPoint.Y > cabinet.Element.CabinetCenter.Y).MinElement(info => info.MinPoint.DistanceTo(cabinet.Element.CabinetCenter)).MaxPoint;
                cabinet.TableMaxPoint = maxPoint;
            });

            //返回
            return cabinets;

        }


        /// <summary>
        /// 获取配电箱图的表格信息
        /// </summary>
        /// <param name="draw"></param>
        /// <returns></returns>
        public List<ThTableInfo> GetTableInfos(ThDistributionDraw draw)
        {
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            //找到表格直线所在图层的直线，拿出指定要求的直线，作为配电箱的分割线信息
            var lines = SelectionTool.DocChoose<Line>(() => ed.SelectWindow(draw.MinPoint, draw.MaxPoint, OpFilter.Bulid(fil => fil.Dxf(0) == "line" & fil.Dxf(8) == ThTableInfo.LineLayerName)));
            if (lines == null)
            {
                return new List<ThTableInfo>();
            }

            return lines.Where(l => l.Length > ThTableInfo.LineLengthTol && Math.Abs(l.Delta.Y) < ThTableInfo.LineDeltaYTol).Select(l => new ThTableInfo(l.ObjectId, l.GeometricExtents.MinPoint, l.GeometricExtents.MaxPoint)).ToList();
        }



        /// <summary>
        /// 获取配电箱出来的各种信息
        /// </summary>
        /// <returns></returns>
        public List<ThCabinetRecord> GetCabinetRecords(ThDistributionDraw draw, ThCabinet thCabinet)
        {
            //***个别图纸还是有问题

            //如果列数量不对，不执行
            if (draw.ColumnGrps.Any(grp => grp.Count != ThColumnFactory.columnNames.Count))
            {
                return new List<ThCabinetRecord>();
            }

            var colGrp = new List<ThColumn>();
            try
            {
                //System.Windows.Forms.MessageBox.Show((thCabinet.Element==null).ToString());
                //System.Windows.Forms.MessageBox.Show((thCabinet.Element.MinPoint==null).ToString());

                //找到配电箱对应的那一组列信息
                colGrp = draw.ColumnGrps.FirstOrDefault(grp => grp.Cast<ThCabinetColumn>().Any(col => Math.Abs(col.Center.X - thCabinet.Element.MinPoint.X) < 2300));
                //System.Windows.Forms.MessageBox.Show((colGrp==null).ToString());
            }
            catch (System.Exception)
            {

                throw;
            }

            //缩放确保数据可以被正确获取
            COMTool.ZoomWindow(draw.MinPoint, draw.MaxPoint);

            //根据列求出列的所有元素，配电箱的列不用求
            var elements = colGrp.Where(colum => !(colum is ThCabinetColumn)).SelectMany(column => column.GetThElements());

            //把四列在一行的归为一组,每一组都由高到低排序,整体也由高到低排序
            var records = elements.OfType<ThCircuitElement>().OrderByDescending(ele => ele.Center.Y).Join(elements.OfType<ThPowerCapacityElement>().OrderByDescending(ele => ele.Center.Y), eleCir => eleCir.Center, eleCap => eleCap.Center, (eleCir, eleCap) => new ThCabinetRecord(eleCir, eleCap), new CompareElemnet<Point3d>((i, j) => i.IsBottomGongXian(j, ThElementFactory.circuitToPowerCapacityTol))).Join(elements.OfType<ThOutCableElement>().OrderByDescending(ele => ele.MinPoint.Y), ele => ele.CircuitElement.Center, eleCab => eleCab.MinPoint, (ele, eleCab) => { ele.OutCableElement = eleCab; return ele; }, new CompareElemnet<Point3d>((i, j) => i.IsBottomGongXian(j, ThElementFactory.circuitToOutCableTol))).Join(elements.OfType<ThBranchSwitchElement>().OrderByDescending(ele => ele.MinPoint.Y), ele => ele.CircuitElement.Center, eleSwi => eleSwi.MinPoint, (ele, eleSwi) => { ele.BranchSwitchElement = eleSwi; return ele; }, new CompareElemnet<Point3d>((i, j) => i.IsBottomGongXian(j, ThElementFactory.circuitToBranchSwitchTol))).OrderByDescending(a => a.CircuitElement.Center.Y);

            //随便找一列，列的元素在配电箱里，就是我们要的记录
            var result = records.Where(re => re.BranchSwitchElement.MinPoint.IsIn(thCabinet.TableMinPoint, thCabinet.TableMaxPoint));



            return result.Any() ? result.ToList() : new List<ThCabinetRecord>();
        }


        /// <summary>
        /// 得到配电箱的所有回路
        /// </summary>
        /// <param name="draw"></param>
        /// <param name="thCabinet"></param>
        /// <returns></returns>
        public List<ThCabinetRecord> GetCabinetCircuit(ThDistributionDraw draw, ThCabinet thCabinet)
        {
            //***个别图纸还是有问题

            //如果列数量不对，不执行
            if (draw.ColumnGrps.Any(grp => grp.Count != ThColumnFactory.columnNames.Count))
            {
                return new List<ThCabinetRecord>();
            }

            var colGrp = new List<ThColumn>();

            //找到配电箱对应的那一组列信息
            colGrp = draw.ColumnGrps.FirstOrDefault(grp => grp.Where(gr => gr is ThCabinetColumn).Any(col => Math.Abs(col.Center.X - thCabinet.Element.MinPoint.X) < 2300));
            //System.Windows.Forms.MessageBox.Show((colGrp==null).ToString());



            //缩放确保数据可以被正确获取
            COMTool.ZoomWindow(draw.MinPoint, draw.MaxPoint);

            //根据回路列求出回路的值
            var elements = colGrp.Where(colum => colum is ThCircuitColumn).SelectMany(column => column.GetThElements());

            //由高到低排序后实例化
            var records = elements.OfType<ThCircuitElement>().OrderByDescending(ele => ele.Center.Y).Select(ele => new ThCabinetRecord(ele));

            //随便找一列，列的元素在配电箱里，就是我们要的记录
            var result = records.Where(re => re.CircuitElement.Center.IsIn(thCabinet.TableMinPoint, thCabinet.TableMaxPoint));

            return result.Any() ? result.ToList() : new List<ThCabinetRecord>();
        }


        /// <summary>
        /// 获取配电箱出来的各种信息
        /// </summary>
        /// <returns></returns>
        public void GetCabinetRecords(ThDistributionDraw draw, ThCabinet thCabinet, ThCabinetRecord record)
        {
            //***个别图纸还是有问题

            //如果列数量不对，不执行
            if (draw.ColumnGrps.Any(grp => grp.Count != ThColumnFactory.columnNames.Count))
            {
                return;
            }

            var colGrp = new List<ThColumn>();

            //找到配电箱对应的那一组列信息
            //*****这里用cast会强制转换失败，不明原因
            colGrp = draw.ColumnGrps.FirstOrDefault(grp => grp.Where(gr => gr is ThCabinetColumn).Any(col => Math.Abs(col.Center.X - thCabinet.Element.MinPoint.X) < 2300));

            if (colGrp!=null)
            {
                //缩放确保数据可以被正确获取
                COMTool.ZoomWindow(thCabinet.TableMinPoint, thCabinet.TableMaxPoint);

                //根据列求出列的所有元素，配电箱和回路的列不用求
                var elements = colGrp.Where(colum => !(colum is ThCabinetColumn || colum is ThCircuitColumn)).SelectMany(column => column.GetThElements());

                record.PowerCapacityElement = elements.OfType<ThPowerCapacityElement>().FirstOrDefault(ele => ele.Center.IsBottomGongXian(record.CircuitElement.Center, ThElementFactory.circuitToPowerCapacityTol));

                record.OutCableElement = elements.OfType<ThOutCableElement>().FirstOrDefault(ele => ele.MinPoint.IsBottomGongXian(record.CircuitElement.Center, ThElementFactory.circuitToOutCableTol));

                record.BranchSwitchElement = elements.OfType<ThBranchSwitchElement>().FirstOrDefault(ele => ele.MinPoint.IsBottomGongXian(record.CircuitElement.Center, ThElementFactory.circuitToBranchSwitchTol));
            }


        }

        /// <summary>
        /// 高亮配电箱记录
        /// </summary>
        public void HighlightRecord(ThCabinetRecord record)
        {
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;

            using (doc.LockDocument())
            using (var tr = record.CircuitElement.ElementId.Database.TransactionManager.StartOpenCloseTransaction())
            {
                //加入空的判断，以免特殊图纸出错崩溃
                if (record.CircuitElement != null)
                {
                    record.CircuitElement.ElementId.Highlight(tr);
                }
                if (record.PowerCapacityElement != null)
                {
                    record.PowerCapacityElement.ElementId.Highlight(tr);
                }
                if (record.OutCableElement != null)
                {
                    record.OutCableElement.ElementId.Highlight(tr);
                }
                if (record.BranchSwitchElement != null)
                {
                    record.BranchSwitchElement.ElementId.Highlight(tr);
                }

                tr.Commit();
            }
        }

        /// <summary>
        /// 取消传入配电箱记录的高亮
        /// </summary>
        /// <param name="record"></param>
        public void UnHighLightsRecord(ThCabinetRecord record)
        {
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;

            using (doc.LockDocument())
            using (var tr = record.CircuitElement.ElementId.Database.TransactionManager.StartOpenCloseTransaction())
            {
                //加入空的判断，以免特殊图纸出错崩溃
                if (record.CircuitElement != null)
                {
                    record.CircuitElement.ElementId.UnHighlight(tr);
                }
                if (record.PowerCapacityElement != null)
                {
                    record.PowerCapacityElement.ElementId.UnHighlight(tr);
                }
                if (record.OutCableElement != null)
                {
                    record.OutCableElement.ElementId.UnHighlight(tr);
                }
                if (record.BranchSwitchElement != null)
                {
                    record.BranchSwitchElement.ElementId.UnHighlight(tr);
                }

                tr.Commit();
            }
        }

        /// <summary>
        /// 更新到模型空间
        /// </summary>
        /// <param name="record"></param>
        public void UpdateToDwg(ThCabinetRecord record)
        {
            if (record.PowerCapacityElement!=null)
            {
                record.PowerCapacityElement.UpdateToDwg();
            }
            if (record.OutCableElement!=null)
            {
                record.OutCableElement.UpdateToDwg();
            }
            if (record.BranchSwitchElement!=null)
            {
                record.BranchSwitchElement.UpdateToDwg();
            }
        }

        /// <summary>
        /// 更新屏幕
        /// </summary>
        public void UpdateScreen()
        {
            AcadApp.UpdateScreen();
        }


        public void GetCapacity()
        {
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            var capacitys = SelectionTool.DocChoose<DBText>(() => ed.SelectAll(OpFilter.Bulid(fil => fil.Dxf(0) == "text")));

            capacitys.Highlight();
        }

        /// <summary>
        /// 找到各个字段
        /// </summary>
        public List<ThColumn> GetColumns(Point3d pt1, Point3d pt2)
        {
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            COMTool.ZoomWindow(pt1, pt2);

            var result = new List<ThColumn>();

            //拿出所有含有关键字的列信息
            var colums = SelectionTool.DocChoose<DBText>(() => ed.SelectWindow(pt1, pt2, OpFilter.Bulid(fil => fil.Dxf(0) == "text" & fil.Dxf(1) == string.Join(",", ThColumnFactory.columnNames))), (sender, e) =>
                  {
                      var ids = e.AddedObjects.GetObjectIds().ToList();

                      using (var tr = AcadApp.DocumentManager.MdiActiveDocument.TransactionManager.StartOpenCloseTransaction())
                      {
                          //从图纸中找出所有列关键字，由上至下排序，取出前两个(默认两组)，便是我们需要的列信息,完成后整体从左到右排序
                          result = ThColumnFactory.columnNames.GroupJoin(ids.Select(id => id.GetObjectByID<DBText>(tr)), colName => colName, txt => txt.TextString, (colName, txts) =>
                           {
                               var colTxts = txts.Distinct(new CompareElemnet<DBText>((i, j) =>
   i.Get3DCenter().IsEqualTo(j.Get3DCenter(), ThColumnFactory.columneDistinctTol))).OrderByDescending(txt => txt.Get3DCenter().Y, new CompareSort<double>(ThColumnFactory.columnTolerance)).ThenBy(txt => txt.Get3DCenter().X).Take(2);

                               return colTxts.Select(colTxt => ThColumnFactory.CreateColumn(colTxt.TextString, colTxt.ObjectId, colTxt.GetCenter().toPoint3d(), pt1, pt2));
                           }).SelectMany(a => a).OrderBy(col => col.Center.X).ToList();

                          #region 原来对的代码
                          //for (int i = 0; i < ThColumnFactory.columnNames.Count; i++)
                          //{
                          //    switch (ThColumnFactory.columnNames[i])
                          //    {
                          //        case ThColumnFactory.powerCapacityName:

                          //            ids.Where(id => id.GetObjectByID<DBText>(tr).TextString == ThColumnFactory.powerCapacityName).Select(id => id.GetObjectByID<DBText>(tr)).Select(txt => ThColumnFactory.CreateColumn(ThColumnFactory.powerCapacityName, txt.ObjectId, txt.GetCenter().toPoint3d())).ToList().ForEach(ele =>
                          //                   {
                          //                       //加入结果集
                          //                       powerCapacityColumns.Add(ele);
                          //                       //从选中的集合中删除
                          //                       ids.Remove(ele.ElementId);
                          //                   });
                          //            break;

                          //        case ThColumnFactory.outCableName:

                          //            ids.Where(id => id.GetObjectByID<DBText>(tr).TextString == ThColumnFactory.outCableName).Select(id => id.GetObjectByID<DBText>(tr)).Select(txt => ThColumnFactory.CreateColumn(ThColumnFactory.outCableName, txt.ObjectId, txt.GetCenter().toPoint3d())).ToList().ForEach(ele =>
                          //            {
                          //                //加入结果集
                          //                outCableColumns.Add(ele);
                          //                //从选中的集合中删除
                          //                ids.Remove(ele.ElementId);
                          //            });
                          //            break;

                          //        default:
                          //            break;
                          //    }


                          //} 
                          #endregion

                          tr.Commit();
                      }

                  });

            if (colums == null)
            {
                return new List<ThColumn>();
            }

            #region 原来对的代码
            //var columnss = new List<List<ThColumn>> { powerCapacityColumns, outCableColumns };

            ////如果任意列信息不全，也不执行
            //if (columnss.Any(cols => !cols.Any()))
            //{
            //    new List<ThColumnInfo>();
            //}

            ////对所有的列，先由高到底排序，再从左到右排序，注意去重
            //for (int n = 0; n < columnss.Count; n++)
            //{
            //    columnss[n] = columnss[n].OrderByDescending(col => col.Center.Y, new CompareSort<double>(ThColumnFactory.columnTolerance)).ThenBy(col => col.Center.X).Distinct(new CompareElemnet<ThColumn>((i, j) => i.Center.IsEqualTo(j.Center, ThColumnFactory.columneDistinctTol))).ToList();
            //}

            ////*****默认一张图有两组表
            //var columnsInfo = new List<int>() { 0, 1 }.Select(i => new ThColumnInfo(columnss.First(cols => cols.Any(col => col is ThPowerCapacityColumn)).Cast<ThPowerCapacityColumn>().ElementAt(i), columnss.First(cols => cols.Any(col => col is ThOutCableColumn)).Cast<ThOutCableColumn>().ElementAt(i))).ToList(); 
            #endregion


            return result;
        }

        /// <summary>
        /// 获取图纸名称的位置
        /// </summary>
        /// <returns></returns>
        private Point3d GetDrawNamePosition(BlockReference block)
        {
            Point3d pt;
            using (var tr = block.Database.TransactionManager.StartOpenCloseTransaction())
            {
                //在块定义中找到指定文字的单行文字，返回其位置信息
                pt = block.BlockTableRecord.GetObjectByID<BlockTableRecord>(tr).Cast<ObjectId>().Where(id => id.ObjectClass.Name == "AcDbText").First(id => id.GetObjectByID<DBText>(tr).TextString == ThDrawFactory.drawNameFieldName).GetObjectByID<DBText>(tr).GeometricExtents.MinPoint;

                pt = pt.TransformBy(block.BlockTransform);

                tr.Commit();
            }

            return pt;

        }
    }
}
