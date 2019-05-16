﻿using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using DotNetARX;
using NFox.Cad.Collections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TianHua.AutoCAD.Parking;
using TianHua.AutoCAD.Utility.ExtensionTools;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
using Path = System.IO.Path;
using Polyline = Autodesk.AutoCAD.DatabaseServices.Polyline;

namespace TianHua.AutoCAD.Parking
{
    /// <summary>
    /// CheWeiWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CheWeiWindow : Window
    {
        public ObservableCollection<ParkingLotInfo> ParkingLotInfos { get; set; }//车位配置信息
        public CheWeiWindow()
        {

            InitializeComponent();

            //背景色设置为经典样式，xmal代码写不来。。。。
            winCheWei.Background = SystemColors.ControlBrush;
            WithTrans(() =>
            {
                //ReCatchIcon();
                this.ParkingLotInfos = GetFormalParkingLotInfos();

            });


            //**********
            //样式设定：1、button大小是固定的
            //可输入的control，上面都有一个label解释
            //一组要输入的内容，总是放在一个groupbox内
            //combox\textbox,横向拉伸，纵向不拉伸
            //radio\check,大小不变
        }


        /// <summary>
        /// 根据用户选择得图形对象，初始化车位类型配置信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnChooseBlock_Click(object sender, RoutedEventArgs e)
        {
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            WithTrans(() =>
            {
                //得到所有锁定的图层
                var lockLayers = new List<string>();
                lockLayers = db.GetAllLayers().Where(la => la.IsLocked).Select(la => la.Name).ToList();

                //确定要拾取的车位类型，只允许块参照被选中,只允许不被锁定的图层被选中
                var blocks = SelectionTool.DocChoose<BlockReference>(() => ed.GetSelection(new PromptSelectionOptions() { MessageForAdding = "请选择需要编号的车位块类型" }, OpFilter.Bulid(fil => fil.Dxf(0) == "insert" & fil.Dxf(8) != string.Join(",", lockLayers))));
                if (blocks == null)
                {
                    return;
                }

                //获取图标临时文件夹路径
                string tempDirName = GetTempDirectory().FullName;

                //去重复，并计算输出图像路径
                var parkingLotInfors = blocks.Distinct(new CompareElemnet<BlockReference>((i, j) => i.Name == j.Name)).Select(b => new ParkingLotInfo(b.Name, tempDirName + "\\" + b.Name + ".bmp", b.BlockTableRecord.GetObject(OpenMode.ForWrite) as BlockTableRecord));


                //找到已有的，加上新添加的，绑定到车位listview
                this.ParkingLotInfos = this.ParkingLotInfos.Union(parkingLotInfors, new CompareElemnet<ParkingLotInfo>((i, j) => i.Name == j.Name)).ToObservableCollection();

                //******必须重新绑定才可以显示，不知道有没有什么优化方法
                listViewBlocks.ItemsSource = this.ParkingLotInfos;

            });

        }

        private void ReCatchIcon()
        {
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            //获取所有临时文件夹下的图标文件，这些文件存储着上一次的选择信息
            var tempIconFiles = GetTempDirectory().GetFiles("*.bmp");

            foreach (var f in tempIconFiles)
            {
                var cmd = "BLOCKICON\n" + Path.GetFileNameWithoutExtension(f.FullName) + "\n";
                //// 获取NOMUTT系统变量，该变量用来控制是否禁止显示提示信息
                //object nomutt = AcadApp.GetSystemVariable("NOMUTT");
                //// 命令字符串，表示在命令调用结束时，将NOMUTT设置为原始值
                //cmd += "_NOMUTT " + nomutt.ToString() + " ";
                //// 在命令调用之前，设置NOMUTT为1，表示禁止显示提示信息
                //AcadApp.SetSystemVariable("NOMUTT", 1);

                //不管有没有截图，都重新截图，确保块参照缩略图最新
                doc.SendCommand(cmd);

                //ResultBuffer rb = new ResultBuffer();
                //rb.Add(new TypedValue(5005, "BLOCKICON")); // 字符串，表示命令
                //rb.Add(new TypedValue(5005, this.Name)); // 实体的Id
                //rb.Add(new TypedValue(5000)); // 打断的第二点
                //ed.AcedCmd(rb);
            }


        }

        /// <summary>
        /// 在当前程序得目录下，创建一个临时文件夹，用来存放文件中的块预览图标
        /// </summary>
        /// <returns></returns>
        private static DirectoryInfo GetTempDirectory()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string ppath = Uri.UnescapeDataString(uri.Path);
            var tempDirName = System.IO.Path.GetDirectoryName(ppath) + @"\Temp";

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

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
        }

        /// <summary>
        /// 获取上一次的车位数据记录
        /// </summary>
        /// <returns></returns>
        private ObservableCollection<ParkingLotInfo> GetFormalParkingLotInfos()
        {
            //****上一次的记录，针对的是同一个文档，如果文档发生变化，只要块名不相同，不会显示记录。
            //****在第三版考虑优化，针对每一个dwg文件，都进行单独的记录的留存。(不一定需要这样的优化，因为无法保证文档名是不改变的)
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            //获取所有临时文件夹下的图标文件，这些文件存储着上一次的选择信息
            var tempIconFiles = GetTempDirectory().GetFiles("*.bmp");

            //***如在此期间，用户对已有的车位块进行了编辑，则获取的缩略图也应相应的变化,所以实例化车位信息时，始终要获取块表记录，以得到最新的缩略图保持同步
            //创建车位信息类并返回
            return tempIconFiles.Select(f => new ParkingLotInfo(Path.GetFileNameWithoutExtension(f.FullName), f.FullName, (db.BlockTableId.GetObject(OpenMode.ForWrite) as BlockTable)[Path.GetFileNameWithoutExtension(f.FullName)].GetObject(OpenMode.ForWrite) as BlockTableRecord)).ToObservableCollection();
        }

        /// <summary>
        /// 将选中项删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            //通过Observercollection,对数据源的处理可直接反映到绑定对象上
            this.ParkingLotInfos.Remove((ParkingLotInfo)this.listViewBlocks.SelectedItem);

            //*******还差一个删除临时文件夹下的缩略图
        }

        /// <summary>
        /// 将所有项删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            var count = this.listViewBlocks.Items.Count;
            //每次都拿掉第一个，直到全部拿完
            for (int i = 0; i < count; i++)
            {
                this.ParkingLotInfos.RemoveAt(0);
            }

        }

        //自动添加序号列
        private void ListViewBlocks_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }

        //自动减去序号列
        private void ListViewBlocks_UnloadingRow(object sender, DataGridRowEventArgs e)
        {
            var dgData = (DataGrid)sender;
            ListViewBlocks_LoadingRow(sender, e);
            if (dgData.Items != null)
            {
                for (int i = 0; i < dgData.Items.Count; i++)
                {
                    try
                    {
                        DataGridRow row = dgData.ItemContainerGenerator.ContainerFromIndex(i) as DataGridRow;
                        if (row != null)
                        {
                            row.Header = (i + 1).ToString();
                        }
                    }
                    catch { }
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            var polyLayerName = "天华车位轨迹线";
            var numberLayerName = "天华车位编号";
            var numberTextStyleName = "Standard";
            double numberHeight = 500;
            double offsetDis = 900;

            //初始化配置信息
            var manager = new CheWeiManager(polyLayerName, numberLayerName, numberTextStyleName, numberHeight, offsetDis);

            //初始化图层
            InitialLayer(manager.PolyLayerName, manager.NumberLayerName);

            //得到所有锁定的图层
            var lockLayers = new List<string>();
            WithTrans(() => { lockLayers = db.GetAllLayers().Where(la => la.IsLocked).Select(la => la.Name).ToList(); });

            //确定要拾取的车位类型，只允许块参照被选中,只允许不被锁定的图层被选中
            var blocks = SelectionTool.DocChoose<BlockReference>(() => ed.GetSelection(new PromptSelectionOptions() { MessageForAdding = "请选择需要编号的车位块类型" }, OpFilter.Bulid(fil => fil.Dxf(0) == "insert" & fil.Dxf(8) != string.Join(",", lockLayers))));
            if (blocks == null)
            {
                return;
            }
            var blockNameLists = blocks.Distinct(new CompareElemnet<BlockReference>((i, j) => i.Name == j.Name)).Select(b => b.Name);

            //确定起始编号
            var startNumber = GetStartNumber();
            if (startNumber == -1)
            {
                return;
            }

            //绘制多段线轨迹
            var poly = AddPoly(polyLayerName, 0);
            if (poly == null)
            {
                return;
            }

            //按照配置信息，选择图纸中的车位,只允许不被锁定的图层被选中*******如果用fence,窗口一缩小就看不到的就不对了，
            var ents = SelectionTool.DocChoose<BlockReference>(() => ed.SelectAll(OpFilter.Bulid(fil => fil.Dxf(0) == "insert" & fil.Dxf(2) == string.Join(",", blockNameLists.ToArray()) & fil.Dxf(8) != string.Join(",", lockLayers))));
            //如果没有选中则直接返回
            if (ents == null)
            {
                return;
            }

            //按照和轨迹相交的情况，选择车位,以距离多段线起始点的距离进行排序，并计算编号次数(交点/2)
            var parks = ents.Select(b => new { Block = b, Points = GetWaiJieRec(b).GetIntersectPoints(poly, Intersect.OnBothOperands).Cast<Point3d>().OrderBy(p => poly.GetDistAtPoint(p)).ToList() }).Where(a => a.Points.Count > 0).OrderBy(a => poly.GetDistAtPoint(a.Points.First())).Select(a => new { a.Block, Count = a.Points.Count / 2 }).ToList();

            //实例化车位
            var cheweis = parks.Select(p => new ParkingLot(p.Block, p.Count)).ToList();

            //将文字样式设置为希望的文字样式
            WithTrans(() => db.SetCurrentTextStyle(manager.NumberTextStyle));

            //为车位进行编号
            //*******这里可以留有接口，为块参照和文字做出关联
            cheweis.ForEach(chewei =>
            {
                chewei.SetNumber(startNumber.ToString(), manager);
                startNumber += chewei.Count;
            });

            //将编好的号设置文字样式，并加入模型空间
            WithTrans(() =>
            {
                db.AddToModelSpace(cheweis.SelectMany(chewei => chewei.Numbers).ToArray());
            });
        }

        /// <summary>
        /// 获取初始编号数值
        /// </summary>
        /// <returns></returns>
        public int GetStartNumber()
        {
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            var resPoint = ed.GetInteger(new PromptIntegerOptions("\n请输入起始编号数值") { DefaultValue = 1 });
            if (resPoint.Status == PromptStatus.Cancel)
                return -1;
            else
            {
                return resPoint.Value;
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

            double width = 0; //初始化线宽
            short colorIndex = color; //初始化颜色索引值
            int index = 2; //初始化多段线顶点数
            Polyline polyEnt = null;
            ObjectId polyEntId = ObjectId.Null; //声明多段线的ObjectId
                                                //定义第一个点的用户交互类
            PromptPointOptions optPoint = new PromptPointOptions("\n请在屏幕绘制车位编号轨迹线的第一个点");
            optPoint.AllowNone = true; //允许用户回车响应
                                       //返回点的用户提示类
            PromptPointResult resPoint = ed.GetPoint(optPoint);
            //用户按下ESC键，退出
            if (resPoint.Status == PromptStatus.Cancel)
                return null;
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

                                    ptPrevious = polyEnt.GetPoint3dAt(polyEnt.NumberOfVertices - 1);
                                    //将基点设置回先前的点
                                    optPtKey.BasePoint = polyEnt.GetPoint3dAt(polyEnt.NumberOfVertices - 1);
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
                        //将多段线添加到图形数据库并返回一个ObjectId(在绘图窗口动态显示多段线)
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
                                Point2d ptCurrent = ptNext.toPoint2d();
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
        private Polyline GetWaiJieRec(BlockReference block)
        {
            //复制对象
            var cloneBlock = (BlockReference)block.Clone();
            //转成水平的
            cloneBlock.TransformBy(block.BlockTransform.Inverse());

            var poly = new PolylineRec(cloneBlock.GeometricExtents.MinPoint.toPoint2d(), cloneBlock.GeometricExtents.MaxPoint.toPoint2d());

            //转回去
            poly.TransformBy(block.BlockTransform);

            return poly;
        }

        /// <summary>
        /// 初始化需要的图层
        /// </summary>
        /// <param name="strs"></param>
        public void InitialLayer(params string[] strs)
        {
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            WithTrans(() => strs.ForEach(str => db.AddLayer(str)));
        }
    }
}