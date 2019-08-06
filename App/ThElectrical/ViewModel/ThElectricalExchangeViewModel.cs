using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ThElectrical.Model.ThDraw;
using ThElectrical.Model.ThTable;
using TianHua.AutoCAD.Utility.ExtensionTools;
using DotNetARX;
using System.Windows.Media;
using Linq2Acad;
using ThResourceLibrary;
using Autodesk.AutoCAD.DatabaseServices;
using Winform = System.Windows.Forms;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;


namespace ThElectrical.ViewModel
{
    public class ThElectricalExchangeViewModel : ThNotifyObject
    {
        public static bool cabinetChanged = false;//配电箱是否改变了，默认没改变

        public bool ErasedSomething { get; set; }//是否删掉了某些东西
        public bool SelectedRecordChanged { get; set; }//配电箱记录值是否改变了
        public List<string> PowerCapacities { get; set; }//所有的容量规格
        public List<string> PipeMatiralStyle { get; set; }//所有的棺材
        public ObservableCollection<string> PipeSizeStyle { get; set; }//所有的管材规格
        public List<string> GroundWireStyle { get; set; }//所有地线的规格
        public List<string> PhraseWireStyle { get; set; }//所有相线的规格

        public ObservableCollection<string> MainCurrents { get; set; }//所有的主体电流
        public ObservableCollection<string> BranchSwitchCurrents { get; set; }//开关电流

        public ThElectricalExchangeTask Task { get; set; }//所有命令
        public ObservableCollection<ThCabinet> Cabinets { get; set; }

        private ThCabinet _selectedCabinet;//当前选中的配电箱
        public ThCabinet SelectedCabinet
        {
            get
            {
                return _selectedCabinet;
            }
            set
            {
                _selectedCabinet = value;
                RaisePropertyChanged("SelectedCabinet");
            }
        }


        private ThCabinetRecord _selectedRecord;//当前选中的配电箱信息
        public ThCabinetRecord SelectedRecord
        {
            get
            {
                return _selectedRecord;
            }
            set
            {
                _selectedRecord = value;
                RaisePropertyChanged("SelectedRecord");
            }
        }



        public ObservableCollection<ThDistributionDraw> DistributionDraws { get; set; }//所有图纸

        /// <summary>
        /// 窗体启动命令
        /// </summary>
        private ThCommand _loadedCommand;
        public ThCommand LoadedCommand
        {
            get
            {
                if (_loadedCommand == null)
                    _loadedCommand = new ThCommand(o =>
                    {
                        //找到所有的配电柜系统图，并找到每一张图的配电柜
                        this.Task.GetDistributionDraws().ForEach(draw =>
                        {
                            try
                            {
                                draw.Cabinets = this.Task.GetCabinets(draw).ToObservableCollection();

                                draw.Cabinets.ForEach(cabinet =>
                                {
                                    //找到配电箱的所有回路
                                    cabinet.Records = this.Task.GetCabinetCircuit(draw, cabinet).ToObservableCollection();
                                });

                                this.DistributionDraws.Add(draw);
                            }
                            catch (Exception ex)
                            {
                                Winform.MessageBox.Show(ex.Source + "\n" + ex.Message + "\n" + ex.StackTrace);
                            }

                        });

                        //完成读取后再设置窗口的拉伸属性
                        var window = o as Window;
                        window.SizeToContent = SizeToContent.Width;

                        //添加删除事件
                        this.Task.AddErasedMornitor(ObjectErased);

                    });
                return _loadedCommand;
            }
        }

        /// <summary>
        /// 删除事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ObjectErased(object sender, ObjectErasedEventArgs e)
        {
            var id = e.DBObject.ObjectId;

            //拿到所有的records
            var records = this.DistributionDraws.SelectMany(draw => draw.Cabinets.SelectMany(cab => cab.Records));

            //删了图纸或者配电箱或者任何有关的东西,必须重新运行程序
            if (this.DistributionDraws.Any(draw => draw.BoundaryId == id) || this.DistributionDraws.Any(draw => draw.Cabinets.Any(cab => cab.Element.ElementId == id)) || records.Any(re => re.CircuitElement.ElementId == id) || records.Any(re => re.PowerCapacityElement != null && re.PowerCapacityElement.ElementId == id) || records.Any(re => re.OutCableElement != null && re.OutCableElement.ElementId == id) || records.Any(re => re.BranchSwitchElement != null && re.BranchSwitchElement.ElementId == id))
            {
                //一旦删了某些东西,就标记为true
                this.ErasedSomething = true;
            }

        }


        /// <summary>
        /// 关闭窗体
        /// </summary>
        private ThCommand _cancelCommand;
        public ThCommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                    _cancelCommand = new ThCommand(o =>
                    {
                        //移除删除事件,改回状态
                        this.Task.RemoveErasedMornitor(ObjectErased);

                        this.ErasedSomething = false;

                        //关闭窗体
                        (o as Window).Close();

                    });
                return _cancelCommand;
            }
        }


        //激活窗体后的重置命令
        private ThCommand _activatedCommand;
        public ThCommand ActivatedCommand
        {
            get
            {
                if (_activatedCommand == null)
                    _activatedCommand = new ThCommand(o =>
                    {
                        Winform.MessageBox.Show("系统图信息部分已被删除,请重新运行程序!");
                        //移除事件
                        this.Task.RemoveErasedMornitor(ObjectErased);

                        //关闭窗体
                        (o as Window).Close();
                    },
                    //当且仅当删除了某些才激活
                    o => this.ErasedSomething);
                return _activatedCommand;
            }
        }



        /// <summary>
        /// 图纸窗口切换命令
        /// </summary>
        private ThGenericCommand<MouseButtonEventArgs> _zoomDrawCommand;
        public ThGenericCommand<MouseButtonEventArgs> ZoomDrawCommand
        {
            get
            {
                if (_zoomDrawCommand == null)
                    _zoomDrawCommand = new ThGenericCommand<MouseButtonEventArgs>(new Action<MouseButtonEventArgs>(e =>
                    {
                        //找到对应的图纸，切换模型空间位置
                        var draw = this.DistributionDraws.First(dr => dr.Name == (e.OriginalSource as TextBlock).Text);
                        COMTool.ZoomWindow(draw.MinPoint, draw.MaxPoint);
                    }),
                    e => this.DistributionDraws.FirstOrDefault(dr => dr.Name == (e.OriginalSource as TextBlock).Text) != null);

                return _zoomDrawCommand;
            }
        }

        /// <summary>
        /// 配电箱窗口缩放命令
        /// </summary>
        private ThGenericCommand<MouseButtonEventArgs> _zoomCabinetCommand;
        public ThGenericCommand<MouseButtonEventArgs> ZoomCabinetCommand
        {
            get
            {
                if (_zoomCabinetCommand == null)
                    _zoomCabinetCommand = new ThGenericCommand<MouseButtonEventArgs>(new Action<MouseButtonEventArgs>(e =>
                    {
                        COMTool.ZoomWindow(this.SelectedCabinet.TableMinPoint, this.SelectedCabinet.TableMaxPoint);
                    }), e => this.DistributionDraws.FirstOrDefault(dr => dr.Name == (e.OriginalSource as TextBlock).Text) == null);

                return _zoomCabinetCommand;
            }
        }

        /// <summary>
        /// 回路缩放命令
        /// </summary>
        private ThGenericCommand<MouseButtonEventArgs> _zoomCircuitCommand;
        public ThGenericCommand<MouseButtonEventArgs> ZoomCircuitCommand
        {
            get
            {
                if (_zoomCircuitCommand == null)
                    _zoomCircuitCommand = new ThGenericCommand<MouseButtonEventArgs>(new Action<MouseButtonEventArgs>(e =>
                    {
                        //下角点由回路的位置决定
                        var circuitMinPoint = new Point3d(this.SelectedRecord.CircuitElement.Center.X - 12500, this.SelectedRecord.CircuitElement.Center.Y - 1400, 0);

                        //上角点由配电箱和回路决定
                        var circuitMaxPoint = new Point3d(this.SelectedCabinet.TableMaxPoint.X, this.SelectedRecord.CircuitElement.Center.Y + 2400, 0);

                        COMTool.ZoomWindow(circuitMinPoint, circuitMaxPoint);

                    }), e => this.DistributionDraws.FirstOrDefault(dr => dr.Name == (e.OriginalSource as TextBlock).Text) == null && this.DistributionDraws.SelectMany(dr => dr.Cabinets).All(cab => cab.Element.CabinetName != (e.OriginalSource as TextBlock).Text));

                return _zoomCircuitCommand;
            }
        }


        /// <summary>
        /// 更改当前treeview的选中项的命令
        /// </summary>
        private ThGenericCommand<RoutedPropertyChangedEventArgs<object>> _changeCabinetCommand;
        public ThGenericCommand<RoutedPropertyChangedEventArgs<object>> ChangeCabinetCommand
        {
            get
            {
                if (_changeCabinetCommand == null)
                    _changeCabinetCommand = new ThGenericCommand<RoutedPropertyChangedEventArgs<object>>(new Action<RoutedPropertyChangedEventArgs<object>>(e =>
                    {
                        var cabinet = e.NewValue as ThCabinet;
                        this.SelectedCabinet = cabinet;
                    }),
                    e => e.NewValue is ThCabinet);

                return _changeCabinetCommand;
            }
        }


        /// <summary>
        /// 更改treeview选中项，当选中项为配电箱信息
        /// </summary>
        private ThGenericCommand<RoutedPropertyChangedEventArgs<object>> _changeRecordCommand;
        public ThGenericCommand<RoutedPropertyChangedEventArgs<object>> ChangeRecordCommand
        {
            get
            {
                if (_changeRecordCommand == null)
                    _changeRecordCommand = new ThGenericCommand<RoutedPropertyChangedEventArgs<object>>(new Action<RoutedPropertyChangedEventArgs<object>>(e =>
                    {
                        try
                        {
                            //配电箱改变了
                            cabinetChanged = true;

                            //如果之前有高亮，则取消其高亮
                            if (this.SelectedRecord != null)
                            {
                                this.Task.UnHighLightsRecord(this.SelectedRecord);
                            }


                            var record = e.NewValue as ThCabinetRecord;

                            //找到对应的配电箱
                            var cabinet = this.DistributionDraws.SelectMany(d => d.Cabinets).FirstOrDefault(cab => cab.Records.Any(re => re.CircuitElement == record.CircuitElement));

                            //找到对应的图纸
                            var draw = this.DistributionDraws.FirstOrDefault(d => d.Cabinets.Any(cab => cab.Element == cabinet.Element));

                            //计算相关信息，并赋值
                            this.Task.GetCabinetRecords(draw, cabinet, record);
                            this.SelectedRecord = record;

                            //高亮显示
                            this.Task.HighlightRecord(this.SelectedRecord);

                            //配电箱完成了改变
                            cabinetChanged = false;
                        }
                        catch (Exception ex)
                        {
                            Winform.MessageBox.Show(ex.Source + "\n" + ex.Message + "\n" + ex.StackTrace);
                        }

                    }),
                    e => e.NewValue is ThCabinetRecord);

                return _changeRecordCommand;
            }
        }

        //是否保存配电箱信息
        private ThGenericCommand<RoutedPropertyChangedEventArgs<object>> _saveRecordCommand;
        public ThGenericCommand<RoutedPropertyChangedEventArgs<object>> SaveRecordCommand
        {
            get
            {
                if (_saveRecordCommand == null)
                    _saveRecordCommand = new ThGenericCommand<RoutedPropertyChangedEventArgs<object>>(new Action<RoutedPropertyChangedEventArgs<object>>(e =>
                    {
                        //如果配电箱记录已经被修改,此时更改了回路选择,则触发此方法
                        if (this.SelectedRecordChanged)
                        {
                            var record = e.OldValue as ThCabinetRecord;

                            Winform.DialogResult MsgBoxResult;//设置对话框的返回值
                            MsgBoxResult = Winform.MessageBox.Show("是否保存已经修改的回路的相关记录?",//对话框的显示内容 
                            "回路记录保存",//对话框的标题 
                           Winform.MessageBoxButtons.YesNo,//定义对话框的按钮，这里定义了YSE和NO两个按钮 
                           Winform.MessageBoxIcon.Exclamation,//定义对话框内的图表式样，这里是一个黄色三角型内加一个感叹号 
                            Winform.MessageBoxDefaultButton.Button2);//定义对话框的按钮式样
                            if (MsgBoxResult == Winform.DialogResult.Yes)//如果对话框的返回值是YES（按"Y"按钮）
                            {
                                this.Task.UnHighLightsRecord(record);
                                this.Task.UpdateToDwg(record);
                                this.Task.HighlightRecord(record);

                                this.Task.UpdateScreen();

                            }
                            //no的话就什么都不做,切换就是了
                            //最后再把状态改回来
                            this.SelectedRecordChanged = false;

                        }

                    }),
                    //当且仅当原来被选中的是配电箱记录
                    e => e.OldValue is ThCabinetRecord);

                return _saveRecordCommand;
            }
        }


        /// <summary>
        /// 获取配电箱信息命令
        /// </summary>
        private ThGenericCommand<RoutedPropertyChangedEventArgs<object>> _setRecordsCommand;
        public ThGenericCommand<RoutedPropertyChangedEventArgs<object>> SetRecordsCommand
        {
            get
            {
                if (_setRecordsCommand == null)
                    _setRecordsCommand = new ThGenericCommand<RoutedPropertyChangedEventArgs<object>>(new Action<RoutedPropertyChangedEventArgs<object>>(e =>
                    {
                        var cabinet = e.NewValue as ThCabinet;
                        var draw = this.DistributionDraws.First(dr => dr.Cabinets.Contains(cabinet));

                        //如果没有被赋值，那么进行一次赋值，延迟实例化
                        if (cabinet.Records == null)
                        {
                            cabinet.Records = this.Task.GetCabinetRecords(draw, cabinet).ToObservableCollection();

                            //添加观察者
                            this.Task.AddCabinetObserve(cabinet);
                        }

                    }),
                    e => (e.NewValue is ThCabinet) && this.DistributionDraws.SelectMany(draw => draw.Cabinets).Any(cab => cab.Element.CabinetName == (e.NewValue as ThCabinet).Element.CabinetName));

                return _setRecordsCommand;
            }
        }

        /// <summary>
        /// 更新模型空间命令
        /// </summary>
        private ThCommand _updateDwgCommand;
        public ThCommand UpdateDwgCommand
        {
            get
            {
                if (_updateDwgCommand == null)
                    _updateDwgCommand = new ThCommand(o =>
                    {
                        this.Task.UnHighLightsRecord(this.SelectedRecord);
                        this.Task.UpdateToDwg(this.SelectedRecord);
                        this.Task.HighlightRecord(this.SelectedRecord);

                        this.Task.UpdateScreen();

                        this.SelectedRecordChanged = false;//修改完了再把状态改回来
                    }, o => this.SelectedRecordChanged);
                return _updateDwgCommand;
            }
        }


        /// <summary>
        /// 配电箱信息更新命令
        /// </summary>
        private ThGenericCommand<DataGridCellEditEndingEventArgs> _cabinetEditCommand;
        public ThGenericCommand<DataGridCellEditEndingEventArgs> CabinetEditCommand
        {
            get
            {
                if (_cabinetEditCommand == null)
                    _cabinetEditCommand = new ThGenericCommand<DataGridCellEditEndingEventArgs>(new Action<DataGridCellEditEndingEventArgs>(e =>
                    {
                        //由于事件是在编辑完之前的，容量值尚未保存,所以手动修改容量值，才可以执行联动
                        var record = e.EditingElement.DataContext as ThCabinetRecord;

                        record.PowerCapacityElement.CapacityValue = (e.EditingElement as TextBox).Text;

                        //执行配电箱信息的联动
                        this.Task.UpdateCabinetInfo(this.SelectedCabinet);

                    }), e => e.Column.Header.ToString() == "容量");

                return _cabinetEditCommand;
            }
        }


        private ThGenericCommand<RoutedEventArgs> _outCableChangedCommand;
        public ThGenericCommand<RoutedEventArgs> OutCableChangedCommand
        {
            get
            {
                if (_outCableChangedCommand == null)
                    _outCableChangedCommand = new ThGenericCommand<RoutedEventArgs>((e =>
                    {
                        System.Windows.Forms.MessageBox.Show("Test2");

                    }));

                return _outCableChangedCommand;
            }
        }


        /// <summary>
        /// 测试用的命令
        /// </summary>
        private ThGenericCommand<RoutedPropertyChangedEventArgs<object>> _testCommand;
        public ThGenericCommand<RoutedPropertyChangedEventArgs<object>> TestCommand
        {
            get
            {
                if (_testCommand == null)
                    _testCommand = new ThGenericCommand<RoutedPropertyChangedEventArgs<object>>((e =>
                    {
                        var draw = e.NewValue as ThDistributionDraw;
                        COMTool.ZoomWindow(draw.MinPoint, draw.MaxPoint);

                        var cabinets = this.Task.GetCabinets(draw);
                        System.Windows.Forms.MessageBox.Show(cabinets.All(cab => cab.TableMaxPoint.X == 0 && cab.TableMaxPoint.Y == 0).ToString());

                        using (var tr = draw.BoundaryId.Database.TransactionManager.StartTransaction())
                        {
                            var cc = cabinets.Select(cab => new PolylineRec(cab.TableMinPoint.toPoint2d(), cab.TableMaxPoint.toPoint2d()));

                            //System.Windows.Forms.MessageBox.Show(cabinets.First().TableMinPoint.X + "," + cabinets.First().TableMinPoint.Y);
                            //System.Windows.Forms.MessageBox.Show(cabinets.First().TableMaxPoint.X + "," + cabinets.First().TableMaxPoint.Y);

                            var ids = draw.BoundaryId.Database.AddToModelSpace(cc.ToArray());

                            ids.HighlightEntities();

                            tr.Commit();
                        }


                    }));

                return _testCommand;
            }
        }




        public ThElectricalExchangeViewModel()
        {
            this.Task = new ThElectricalExchangeTask();
            this.DistributionDraws = new ObservableCollection<ThDistributionDraw>();
            this.SelectedRecordChanged = false;
            this.ErasedSomething = false;

            this.PhraseWireStyle = ThELectricalUtils.GetPhraseStyle();
            this.GroundWireStyle = ThELectricalUtils.GetGroundStyle();

            this.PipeMatiralStyle = new List<string> { "普通电缆穿管", "矿物绝缘电缆穿管" };

            this.PipeSizeStyle = ThELectricalUtils.GetPipeSize().ToObservableCollection();

            this.MainCurrents = ThELectricalUtils.GetMainCurrents().ToObservableCollection();

            this.BranchSwitchCurrents = ThELectricalUtils.GetBranchSwitchCurrents().ToObservableCollection();

            this.PowerCapacities = ThELectricalUtils.GetPowerCapacities();

        }



    }
}
