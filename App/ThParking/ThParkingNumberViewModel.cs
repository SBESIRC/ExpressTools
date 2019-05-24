using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Windows.Data;
using DotNetARX;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using TianHua.AutoCAD.Utility.ExtensionTools;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
using DataGrid = System.Windows.Controls.DataGrid;

namespace TianHua.AutoCAD.Parking
{
    public class ThParkingNumberViewModel : ThNotifyObject
    {
        public ThNumberingTasks NumberingTasks { get; set; }//与车位编号相关的方法类

        //private bool _canExecute;
        //public bool CanExecute
        //{
        //    get { return _canExecute; }
        //    set
        //    {
        //        _canExecute = value;
        //        RaisePropertyChanged("CanExecute");
        //    }
        //}

        #region 绑定生成表格列的命令，似乎不应该用这种方法
        ///// <summary>
        ///// 表格读取行命令
        ///// </summary>
        //private ThGenCommand<DataGridRowEventArgs> _loadingRowCommand;
        //public ThGenCommand<DataGridRowEventArgs> LoadingRowCommand
        //{
        //    get
        //    {
        //        //读取表格时，自动生成列序号
        //        if (_loadingRowCommand == null)
        //        {
        //            _loadingRowCommand = new ThGenCommand<DataGridRowEventArgs>(
        //e => e.Row.Header = e.Row.GetIndex() + 1);
        //        }
        //        return _loadingRowCommand;
        //    }

        //}

        ///// <summary>
        ///// 表格删除行命令
        ///// </summary>
        //private ThGenCommand<DataGridRowEventArgs> _unloadingRowCommand;
        //public ThGenCommand<DataGridRowEventArgs> UnloadingRowCommand
        //{
        //    get
        //    {
        //        //删除行时，自动更新序号
        //        if (_unloadingRowCommand == null)
        //        {
        //            _unloadingRowCommand = new ThGenCommand<DataGridRowEventArgs>(
        //e =>
        //{
        //    System.Windows.Forms.MessageBox.Show(e.GetType().ToString());
        //    //e.Row.Header = e.Row.GetIndex() + 1;
        //});
        //        }
        //        return _unloadingRowCommand;
        //    }

        //} 
        #endregion

        /// <summary>
        /// 窗体读取命令
        /// </summary>
        private ThCommand _activatedCommand;
        public ThCommand ActivatedCommand
        {
            get
            {
                if (_activatedCommand == null)
                    _activatedCommand = new ThCommand(
    o =>
    {
        WithTrans(() =>
        {
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            //窗体启动后，设置为当前样式
            this.NumberStyle.NumberTextStyle = db.Textstyle.GetObjectByID<TextStyleTableRecord>().Name;
        });
    });
                return _activatedCommand;
            }
        }

        #region 重写关闭命令，暂时无法实现
        //    /// <summary>
        //    /// 窗体关闭命令
        //    /// </summary>
        //    private ThGenCommand<CancelEventArgs> _onClosingCommand;
        //    public ThGenCommand<CancelEventArgs> OnClosingCommand
        //    {
        //        get
        //        {
        //            if (_onClosingCommand == null)
        //                _onClosingCommand = new ThGenCommand<CancelEventArgs>(
        //e =>
        //{
        //    Hide();
        //    e.Cancel = true;
        //});
        //            return _onClosingCommand;
        //        }

        //    } 
        #endregion

        /// <summary>
        /// 选择车位命令
        /// </summary>
        private ThCommand _chooseCommand;
        public ThCommand ChooseCommand
        {
            get
            {
                //绑定车位选择
                if (_chooseCommand == null)
                {
                    _chooseCommand = new ThCommand(
        o =>
        {
            //过滤出列表中没有的那些个块，避免重复添加
            var lots = this.NumberingTasks.ChooseParkingLot(this);
            var realLots = lots.Except(lots.Join(this.ParkingLotInfos, p1 => p1, p2 => p2, (p1, p2) => p1));
            foreach (var item in realLots)
            {
                this.ParkingLotInfos.Add(item);
            }
        });
                }
                return _chooseCommand;
            }

        }

        /// <summary>
        /// 删除表格行命令
        /// </summary>
        private ThCommand _deleteCommand;
        public ThCommand DeleteCommand
        {
            get
            {
                //绑定删除车位配置信息
                if (_deleteCommand == null)
                {
                    _deleteCommand = new ThCommand(
        o =>
        {
            //通过Observercollection,对数据源的处理可直接反映到绑定对象上
            this.ParkingLotInfos.Remove((ParkingLotInfo)o);
            //同时，删除临时文件夹下的缩略图
            ((ParkingLotInfo)o).DeleteIcon();
        },
        o => this.ParkingLotInfos.Select(info => info.Name).Any());

                }
                return _deleteCommand;
            }

        }

        /// <summary>
        /// 清空命令
        /// </summary>
        private ThCommand _clearCommand;
        public ThCommand ClearCommand
        {
            get
            {
                if (_clearCommand == null)
                {
                    _clearCommand = new ThCommand(
        o =>
        {
            //每次都拿掉第一个，直到全部拿完,记得要删除icon
            for (int i = 0; i < (int)o; i++)
            {
                this.ParkingLotInfos[0].DeleteIcon();
                this.ParkingLotInfos.RemoveAt(0);
            }
        },
        o => this.ParkingLotInfos.Select(info => info.Name).Any());

                }
                return _clearCommand;
            }

        }


        /// <summary>
        /// 确定命令
        /// </summary>
        private ThCommand _numberCommand;
        public ThCommand NumberCommand
        {
            get
            {
                //绑定车位编号命令，当且仅当已经识别了车位类型时才执行
                if (_numberCommand == null)
                {
                    _numberCommand = new ThCommand(
        o =>
        {
            this.ResultState = true;//更改状态
            ((Window)o).Close();
            if (this.ResultState)
            {
                //如果确认配置状态完毕，执行编号操作
                NumberingTasks.Numbering(this);
                this.ResultState = false;
            }
        },
        o => this.ParkingLotInfos.Select(info => info.Name).Any());

                }

                return _numberCommand;
            }
        }

        /// <summary>
        /// 取消命令
        /// </summary>
        private ThCommand _cancelCommand;
        public ThCommand CancelCommand
        {
            get
            {
                //绑定窗体关闭命令
                if (_cancelCommand == null)
                {
                    _cancelCommand = new ThCommand(
        o => ((Window)o).Close());
                }
                return _cancelCommand;
            }

        }

        private ThNumberInfo _numberInfo;//车位编号显示信息
        public ThNumberInfo NumberInfo
        {
            get { return _numberInfo; }
            set
            {
                if (_numberInfo != value)
                {
                    _numberInfo = value;
                    RaisePropertyChanged("NumberInfo");
                }
            }
        }

        private ThNumberStyle _numberStyle;//车位编号样式
        public ThNumberStyle NumberStyle
        {
            get
            {
                return _numberStyle;
            }

            set
            {
                if (_numberStyle != value)
                {
                    _numberStyle = value;
                    RaisePropertyChanged("NumberStyle");
                }
            }
        }

        public ObservableCollection<ParkingLotInfo> ParkingLotInfos { get; set; }//车位配置信息

        public DataItemCollection CADTextStyles { get; set; }//cad字体样式表

        public bool ResultState { get; set; }//结果状态

        public ThParkingNumberViewModel()
        {
            //初始化编号方法类
            this.NumberingTasks = new ThNumberingTasks();

            string startNumber = "1";
            string prefix = "";
            string suffix = "";

            //初始化编号信息
            this.NumberInfo = new ThNumberInfo(startNumber, prefix, suffix);

            //设为当前db的样式
            WithTrans(() =>
            {
                Document doc = AcadApp.DocumentManager.MdiActiveDocument;
                Database db = doc.Database;

                var polyLayerName = "天华车位轨迹线";
                var numberLayerName = "天华车位编号";
                //设置为当前样式
                var numberTextStyleName = db.Textstyle.GetObjectByID<TextStyleTableRecord>().Name;
                double numberHeight = 500;
                double offsetDis = 900;

                //初始化样式信息
                this.NumberStyle = new ThNumberStyle(polyLayerName, numberLayerName, numberTextStyleName, numberHeight, offsetDis);

            });

            this.CADTextStyles = AcadApp.UIBindings.Collections.TextStyles;

            //新建一个空的配置信息，用于初始化
            this.ParkingLotInfos = new ObservableCollection<ParkingLotInfo>();






        }

        ///// <summary>
        ///// 获取上一次的车位数据记录
        ///// </summary>
        ///// <returns></returns>
        //private ObservableCollection<ParkingLotInfo> GetFormalParkingLotInfos()
        //{
        //    //****上一次的记录，针对的是同一个文档，如果文档发生变化，只要块名不相同，不会显示记录。
        //    //****在第三版考虑优化，针对每一个dwg文件，都进行单独的记录的留存。(不一定需要这样的优化，因为无法保证文档名是不改变的)
        //    Document doc = AcadApp.DocumentManager.MdiActiveDocument;
        //    Database db = doc.Database;
        //    Editor ed = doc.Editor;

        //    //获取所有临时文件夹下的图标文件，这些文件存储着上一次的选择信息
        //    var tempIconFiles = GetTempDirectory().GetFiles("*.bmp").Select(f => new { f, Name = Path.GetFileNameWithoutExtension(f.FullName) });
        //    //获取块表
        //    var blockTable = db.BlockTableId.GetObjectByID<BlockTable>();

        //    //***如在此期间，用户对已有的车位块进行了编辑，则获取的缩略图也应相应的变化,所以实例化车位信息时，始终要获取块表记录，以得到最新的缩略图保持同步
        //    //如果块表中没有的，则不获取
        //    //创建车位信息类并返回
        //    return tempIconFiles.Where(a => blockTable.Has(a.Name)).Select(a => new ParkingLotInfo(a.Name, a.f.FullName, blockTable[a.Name].GetObjectByID<BlockTableRecord>())).ToObservableCollection();
        //}

        /// <summary>
        /// 对块参照重新截图
        /// </summary>
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
    }
}

