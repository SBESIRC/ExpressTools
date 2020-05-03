using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using DotNetARX;
using System;
using System.IO;
using System.Linq;
using AcHelper;
using Linq2Acad;
using System.Windows;
using NFox.Cad.Collections;
using System.Collections.Generic;
using TianHua.AutoCAD.Utility.ExtensionTools;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
using Dreambuild.AutoCAD;

namespace TianHua.AutoCAD.Parking
{
    public class ThNumberingTasks
    {
        public void Numbering(ThParkingNumberViewModel viewModel)
        {
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            //必须锁定文档
            using (doc.LockDocument())
            {
                //初始化图层
                WithTrans(() =>
                {
                    InitialLayer(viewModel.NumberStyle.PolyLayerName, viewModel.NumberStyle.NumberLayerName);
                });

                //得到所有锁定的图层
                var lockLayers = new List<string>();
                WithTrans(() => { lockLayers = db.GetAllLayers().Where(la => la.IsLocked).Select(la => la.Name).ToList(); });

                ////确定要拾取的车位类型，只允许块参照被选中,只允许不被锁定的图层被选中
                //var blocks = SelectionTool.DocChoose<BlockReference>(() => ed.GetSelection(new PromptSelectionOptions() { MessageForAdding = "请选择需要编号的车位块类型" }, OpFilter.Bulid(fil => fil.Dxf(0) == "insert" & fil.Dxf(8) != string.Join(",", lockLayers))));
                //if (blocks == null)
                //{
                //    return;
                //}
                //var blockNameLists = blocks.Distinct(new CompareElemnet<BlockReference>((i, j) => i.Name == j.Name)).Select(b => b.Name);

                ////确定起始编号
                //var startNumber = GetStartNumber();
                //if (startNumber == -1)
                //{
                //    return;
                //}

                //获取所有进入选择得块名，如果没有则不执行，并提醒用户选择
                var blockNames = viewModel.ParkingLotInfos.Select(info => info.Name);
                if (!blockNames.Any())
                {
                    System.Windows.Forms.MessageBox.Show("请选择要进行编号的车位！");
                    return;
                }

                //按照配置信息，选择图纸中的车位,只允许不被锁定的图层被选中*******如果用fence,窗口一缩小就看不到的就不对了，
                var ents = SelectionTool.DocChoose<BlockReference>(() => ed.SelectAll(OpFilter.Bulid(fil => fil.Dxf(0) == "insert" & fil.Dxf(2) == string.Join(",", blockNames.ToArray()) & fil.Dxf(8) != string.Join(",", lockLayers))));
                //如果没有选中则直接返回
                if (ents == null)
                {
                    return;
                }

                //绘制多段线轨迹
                var poly = AddPoly(viewModel.NumberStyle.PolyLayerName, 0);
                if (poly == null)
                {
                    return;
                }

                //找外边界
                var boundarys = ents.Select(b => new { Block = b, Boundary = GetWaiJieRec(b) });

                //如果外边界错误，则不执行程序,因为编号是连续的，有一个断了就没有意义
                if (boundarys.Any(a => a.Boundary == null))
                {
                    ed.WriteMessage("\n车位块为非规则图形，无法编号，请绘制标准的车位块");
                    return;
                }

                //按照和轨迹相交的情况，选择车位,以距离多段线起始点的距离进行排序，并计算编号次数(交点/2)
                var parks = boundarys.Select(a => new
                {
                    a.Block,
                    a.Boundary,
                    Points = a.Boundary.GetIntersectPoints(poly, new Plane(Point3d.Origin, Vector3d.ZAxis), Intersect.OnBothOperands).Cast<Point3d>().OrderBy(p => poly.GetDistAtPoint(p)).ToList()
                }).Where(a => a.Points.Count > 0).OrderBy(a => poly.GetDistAtPoint(a.Points.First())).Select(a => new { a.Block, a.Boundary, Count = a.Points.Count / 2 }).ToList();



                //实例化车位
                var cheweis = parks.Select(p => new ParkingLot(p.Block, p.Boundary, p.Count)).ToList();

                //为车位进行编号
                //*******这里可以留有接口，为块参照和文字做出关联
                cheweis.ForEach(chewei =>
                {
                    chewei.SetNumber(viewModel.NumberInfo, viewModel.NumberStyle);
                    viewModel.NumberInfo.StartNumber = viewModel.NumberInfo.NumberWithComplementaryAdd(chewei.Count);

                });

                //将编好的号设置文字样式，并加入模型空间
                WithTrans(() =>
                {
                    //确保能够找到文字样式，如果找不到，使用当前的
                    var textStyleTable = db.TextStyleTableId.GetObjectByID<TextStyleTable>();
                    var numberStyleID = textStyleTable.Has(viewModel.NumberStyle.NumberTextStyle) ? textStyleTable[viewModel.NumberStyle.NumberTextStyle] : db.Textstyle;

                    //将文字样式设置为希望的文字样式
                    cheweis.ForEach(chewei => chewei.Numbers.ForEach(num => num.TextStyleId = numberStyleID));

                    //加入模型空间
                    db.AddToModelSpace(cheweis.SelectMany(chewei => chewei.Numbers).ToArray());

                });

                //AcadApp.ShowModalWindow(this);
            }

        }

        /// <summary>
        /// 获取模型空间中的车位配置信息
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        public List<ParkingLotInfo> ChooseParkingLot(ThParkingNumberViewModel viewModel, Window window)
        {
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            var result = new List<ParkingLotInfo>();
            try
            {
                using (doc.LockDocument())
                {
                    WithTrans(() =>
                    {
                        //得到所有锁定的图层
                        var lockLayers = new List<string>();
                        lockLayers = db.GetAllLayers().Where(la => la.IsLocked).Select(la => la.Name).ToList();


                        //确定要拾取的车位类型，只允许块参照被选中,只允许不被锁定的图层被选中
                        var blocks = SelectionTool.DocChoose<BlockReference>(() =>
                        {
                            return ed.GetSelection(new PromptSelectionOptions() { MessageForAdding = "请选择需要编号的车位块类型" }, OpFilter.Bulid(fil => fil.Dxf(0) == "insert" & fil.Dxf(8) != string.Join(",", lockLayers)));

                        });
                        if (blocks == null)
                        {
                            return;
                        }
                        //获取图标临时文件夹路径
                        string tempDirName = GetTempDirectory().FullName;

                        //去重复，并计算输出图像路径
                        var parkingLotInfors = blocks.Distinct(new CompareElemnet<BlockReference>((i, j) => i.Name == j.Name)).Select(b => new ParkingLotInfo(b.Name, tempDirName + "\\" + b.Name + ".bmp", b.BlockTableRecord.GetObject(OpenMode.ForWrite) as BlockTableRecord));


                        //找到已有的，加上新添加的，绑定到车位listview
                        result = viewModel.ParkingLotInfos.Union(parkingLotInfors, new CompareElemnet<ParkingLotInfo>((i, j) => i.Name == j.Name)).ToList();


                        //var realLots = result.Except(result.Join(viewModel.ParkingLotInfos, p1 => p1, p2 => p2, (p1, p2) => p1));
                        //foreach (var item in realLots)
                        //{
                        //    viewModel.ParkingLotInfos.Add(item);
                        //}

                    });

                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message + "\n" + ex.Source + "\n" + ex.StackTrace);
            }


            return result;

        }

        /// <summary>
        /// 在当前程序得目录下，创建一个临时文件夹，用来存放文件中的块预览图标
        /// </summary>
        /// <returns></returns>
        private static DirectoryInfo GetTempDirectory()
        {
            //string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            //UriBuilder uri = new UriBuilder(codeBase);
            //string ppath = Uri.UnescapeDataString(uri.Path);
            //var tempDirName = System.IO.Path.GetDirectoryName(ppath) + @"\Temp";
            //获取系统临时目录路径,统一将程序产生的临时文件
            var tempDirName = System.Environment.GetEnvironmentVariable("TEMP") + @"\ThCADPlugin\BlockImage";

            //声明一个文件夹变量，如果没有则创建，有则实例化
            DirectoryInfo tempDirec = null;
            if (!Directory.Exists(tempDirName))
            {
                tempDirec = Directory.CreateDirectory(tempDirName);
            }
            else
            {
                tempDirec = new DirectoryInfo(tempDirName);
            }

            return tempDirec;
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

        /// <summary>
        /// 初始化需要的图层
        /// </summary>
        /// <param name="layers"></param>
        public void InitialLayer(params string[] layers)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                foreach(var layer in layers)
                {
                    acadDatabase.Database.AddLayer(layer);
                }
            }
        }


        /// <summary>
        /// 绘制多边形多段线
        /// </summary>
        /// <param name="layerName"></param>
        /// <returns></returns>
        public Polyline AddPoly(string layerName, short color)
        {
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            var mar = ed.CurrentUserCoordinateSystem;//获得当前UCS坐标系，用于坐标转换

            double width = 0; //初始化线宽
            short colorIndex = color; //初始化颜色索引值
            int index = 2; //初始化多段线顶点数
            Polyline polyEnt = null;
            ObjectId polyEntId = ObjectId.Null; //声明多段线的ObjectId
                                                //定义第一个点的用户交互类
            PromptPointOptions optPoint = new PromptPointOptions("\n请在屏幕绘制车位编号轨迹线的第一个点或[拾取车位轨迹多段线(P)]");
            optPoint.AllowNone = true; //允许用户回车响应
            //为点交互类添加关键字
            optPoint.Keywords.Add("P");

            //返回点的用户提示类
            PromptPointResult resPoint = ed.GetPoint(optPoint);
            //用户按下ESC键，退出
            if (resPoint.Status == PromptStatus.Cancel)
                return null;
            //进入拾取模式
            if (resPoint.Status == PromptStatus.Keyword)
            {
                var ent = SelectionTool.DocChoose<Polyline>(() => ed.GetSelection(new PromptSelectionOptions() { MessageForAdding = "\n请拾取车位轨迹多段线", SingleOnly = true }, OpFilter.Bulid(fil => fil.Dxf(0) == "lwpolyline")));

                if (ent == null)
                {
                    return null;
                }
                return ent.First();
            }


            Point3d ptStart; //声明第一个输入点
                             //用户按回车键
            if (resPoint.Status == PromptStatus.None)
                //得到第一个输入点的默认值
                ptStart = Point3d.Origin;
            else
                //得到第一个输入点
                ptStart = resPoint.Value;
            Point3d ptPrevious = ptStart;//保存当前点

            var message = "\n请绘制车位轨迹的下一个点或[撤回(U)]";
            //定义输入下一点的点交互类
            PromptPointOptions optPtKey = new PromptPointOptions(message);
            //为点交互类添加关键字
            optPtKey.Keywords.Add("Y");
            optPtKey.Keywords.Add("U");
            optPtKey.Keywords.Default = "Y"; //设置默认的关键字
            optPtKey.UseBasePoint = true; //允许使用基准点
            optPtKey.BasePoint = ptPrevious;//设置基准点
            optPtKey.AppendKeywordsToMessage = false;//不将关键字列表添加到提示信息中
                                                     //提示用户输入点
            PromptPointResult resKey = ed.GetPoint(optPtKey);
            bool chehui = false;
            //如果用户输入点或关键字，则一直循环
            while (resKey.Status == PromptStatus.OK || resKey.Status == PromptStatus.Keyword)
            {
                Point3d ptNext = resKey.Value;//声明下一个输入点
                                              //如果用户输入的是关键字集合对象中的关键字
                if (resKey.Status == PromptStatus.Keyword)
                {
                    switch (resKey.StringResult)
                    {
                        case "U":
                            if (index == 2)
                            {
                                return null;
                            }
                            ed.WriteMessage(message);

                            //修改多段线，删除一个顶点
                            WithTrans(() =>
                            {
                                //打开多段线的状态为写
                                polyEnt = doc.TransactionManager.GetObject(polyEntId, OpenMode.ForWrite) as Polyline;
                                if (polyEnt != null)
                                {
                                    polyEnt.RemoveVertexAt(polyEnt.NumberOfVertices - 1);

                                    ptPrevious = polyEnt.GetPoint3dAt(polyEnt.NumberOfVertices - 1).TransformBy(mar.Inverse());
                                    //将基点设置回先前的点,进行坐标转换后执行
                                    optPtKey.BasePoint = polyEnt.GetPoint3dAt(polyEnt.NumberOfVertices - 1).TransformBy(mar.Inverse());
                                    //记录撤回状态
                                    chehui = true;

                                    //减少顶点总数
                                    index--;
                                }

                            });

                            break;
                        case "Y":
                            ed.WriteMessage("\n车位轨迹绘制完成，开始编号");
                            return polyEnt;
                        default:
                            ed.WriteMessage("\n输入了无效关键字");
                            break;
                    }
                }
                else
                {
                    ptNext = resKey.Value;//得到户输入的下一点
                    if (index == 2) //新建多段线
                    {
                        //提取三维点的X、Y坐标值，转化为二维点
                        Point2d pt1 = ptPrevious.toPoint2d();
                        Point2d pt2 = ptNext.toPoint2d();
                        polyEnt = new Polyline();//新建一条多段线
                                                 //给多段线添加顶点，设置线宽
                        polyEnt.AddVertexAt(0, pt1, 0, width, width);
                        polyEnt.AddVertexAt(1, pt2, 0, width, width);
                        //设置多段线的颜色
                        polyEnt.Color = Autodesk.AutoCAD.Colors.Color.FromColorIndex(ColorMethod.ByColor, colorIndex);
                        //先进行坐标转换，将多段线添加到图形数据库并返回一个ObjectId(在绘图窗口动态显示多段线)
                        polyEnt.TransformBy(mar);
                        polyEntId = db.AddToModelSpace(polyEnt);
                    }
                    else  //修改多段线，添加最后一个顶点
                    {
                        WithTrans(() =>
                        {
                            //打开多段线的状态为写
                            polyEnt = doc.TransactionManager.GetObject(polyEntId, OpenMode.ForWrite) as Polyline;
                            if (polyEnt != null)
                            {
                                //继续添加多段线的顶点
                                Point2d ptCurrent = ptNext.TransformBy(mar).toPoint2d();
                                polyEnt.AddVertexAt(index - 1, ptCurrent, 0, width, width);
                                //重新设置多段线的颜色和线宽
                                polyEnt.Color = Autodesk.AutoCAD.Colors.Color.FromColorIndex(ColorMethod.ByColor, colorIndex);
                                polyEnt.ConstantWidth = width;
                            }

                        });

                    }
                    index++;
                }

                //绘制完毕后，设置多段线的图层
                WithTrans(() =>
                {
                    polyEnt = doc.TransactionManager.GetObject(polyEntId, OpenMode.ForWrite, false, true) as Polyline;
                    polyEnt.Layer = layerName;

                });

                //如果没有撤回再执行此步骤
                if (!chehui)
                {
                    //否则继续循环，输入新的点
                    ptPrevious = ptNext;
                    optPtKey.BasePoint = ptPrevious;//重新设置基准点
                }
                //重新设置撤回状态
                chehui = false;
                resKey = ed.GetPoint(optPtKey); //提示用户输入新的顶点
            }

            return polyEnt;
        }

        /// <summary>
        /// 求块参照的外边界
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        private Polyline GetWaiJieRec(BlockReference blockReference)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(blockReference.Database))
            {
                // 这里有两个技术难点需要解决：
                //  计算块引用在WCS下的AABB（axis-aligned bounding box）
                //  处理Nonuniform Scaling的块引用（其X，Y，Z方向的缩放比例不一致）
                // 为了解决这两个技术难点，这里采用的方法是：
                //  1. 在块定义空间（即BCS）下计算其Bounding Box,即为AABB
                //  2. 在块定义空间（即BCS）下将其Bounding Box投影到XY平面（3D降维到2D）
                //  2. 对于每一个块引用，将Bounding Box的四个顶点转换到WCS下
                //  3. 将转换后的四个顶点作为这个块应用在WCS下的AABB
                // 可能存在的问题：
                //  1. 嵌套块（会导致块定义的AABB不准）
                //  2. 块引用Z不等于0
                var ecs2wcs = blockReference.BlockTransform;
                var btr = acadDatabase.Element<BlockTableRecord>(blockReference.BlockTableRecord);
                var extents = btr.GetObjectIds().GetExtents();
                // 将Bounding Box投影到XY平面
                Plane plane = new Plane(Point3d.Origin, Vector3d.ZAxis);
                Matrix3d matrix = Matrix3d.Projection(plane, plane.Normal);
                var ll = extents.MinPoint.TransformBy(matrix);
                var rt = extents.MaxPoint.TransformBy(matrix);
                var lt = new Point3d(extents.MinPoint.X, extents.MaxPoint.Y, 0);
                var rl = new Point3d(extents.MaxPoint.X, extents.MinPoint.Y, 0);
                var pline = new Polyline()
                {
                    Closed = true,
                };
                pline.CreatePolyline(new Point3dCollection()
                {
                    ll.TransformBy(ecs2wcs),
                    lt.TransformBy(ecs2wcs),
                    rt.TransformBy(ecs2wcs),
                    rl.TransformBy(ecs2wcs),
                });
                return pline;
            }
        }
    }
}
