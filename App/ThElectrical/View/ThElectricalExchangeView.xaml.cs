using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ThElectrical.Model.ThTable;
using ThElectrical.ViewModel;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThElectrical.View
{
    /// <summary>
    /// ThElectricalExchangeView.xaml 的交互逻辑
    /// </summary>
    public partial class ThElectricalExchangeView : Window
    {
        public ThElectricalExchangeView()
        {
            InitializeComponent();
            this.DataContext = new ThElectricalExchangeViewModel();
        }

        private void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //如果配电箱没有改变
            if (!ThElectricalExchangeViewModel.cabinetChanged)
            {
                var viewModel = this.DataContext as ThElectricalExchangeViewModel;

                //当且仅当为文本框时才执行
                if (e.OriginalSource is ComboBox)
                {
                    var combo = e.OriginalSource as ComboBox;
                    //有值得时候再换算
                    if (combo.SelectedValue != null && combo.Tag != null && viewModel.SelectedRecord.OutCableElement != null)
                    {
                        if (combo.Tag.ToString() == "相线规格")
                        {
                            viewModel.SelectedRecord.OutCableElement.ResetByPhase();

                        }
                        else if (combo.Tag.ToString() == "地线规格")
                        {
                            viewModel.SelectedRecord.OutCableElement.ResetByGround();

                        }
                        else if (combo.Tag.ToString() == "管材规格")
                        {
                            viewModel.SelectedRecord.OutCableElement.ResetByPipeSize();

                        }
                    }
                }
            }

        }

        /// <summary>
        /// 电缆值的改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GroupBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!ThElectricalExchangeViewModel.cabinetChanged)
            {
                var viewModel = this.DataContext as ThElectricalExchangeViewModel;

                if (e.OriginalSource is TextBox)
                {
                    var textBox = e.OriginalSource as TextBox;

                    if (textBox.Tag != null && textBox.Tag.ToString() == "值")
                    {
                        if (viewModel.SelectedRecord != null && viewModel.SelectedRecord.OutCableElement != null)
                        {
                            //一旦进入了,就表示记录更改了,修改状态
                            viewModel.SelectedRecordChanged = true;

                            viewModel.SelectedRecord.OutCableElement.SetEachPro2();
                        }


                    }
                }
            }
        }

        private void GroupBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //如果配电箱没有改变
            if (!ThElectricalExchangeViewModel.cabinetChanged)
            {
                var viewModel = this.DataContext as ThElectricalExchangeViewModel;

                //当且仅当为组合框时才执行
                if (e.OriginalSource is ComboBox)
                {
                    var combo = e.OriginalSource as ComboBox;
                    //有值得时候再换算
                    if (combo.SelectedValue != null && combo.Tag != null&&viewModel.SelectedRecord.BranchSwitchElement!=null)
                    {
                        if (combo.Tag.ToString() == "主体电流")
                        {
                            viewModel.SelectedRecord.BranchSwitchElement.ResetByMainCurrent();

                        }
                        else if (combo.Tag.ToString() == "开关电流")
                        {
                            viewModel.SelectedRecord.BranchSwitchElement.ResetByBranchSwitchCurrent();
                            //双速的根据对应表应该能够知道对应的是谁和谁
                        }
                    }

                }
            }
        }

        /// <summary>
        /// 回路容量的改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GroupBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            #region 整体联动的逻辑
            //如果配电箱没有改变
            if (!ThElectricalExchangeViewModel.cabinetChanged)
            {
                var viewModel = this.DataContext as ThElectricalExchangeViewModel;

                //当且仅当为组合框时才执行
                if (e.OriginalSource is ComboBox)
                {
                    var combo = e.OriginalSource as ComboBox;
                    //有值得时候再换算
                    if (combo.SelectedValue != null && combo.Tag != null)
                    {
                        if (combo.Tag.ToString() == "容量选型")
                        {
                            if (viewModel.SelectedRecord != null && viewModel.SelectedRecord.PowerCapacityElement != null)
                            {
                                //断开容量文本框变化事件
                                txtRealCap.TextChanged -= GroupBox_TextChanged_1;

                                //一旦进入了,就表示记录更改了,修改状态
                                viewModel.SelectedRecordChanged = true;

                                viewModel.SelectedRecord.PowerCapacityElement.ResetByValue();

                                //执行信息的联动
                                viewModel.Task.AddCabinetObserve(viewModel.SelectedRecord);
                                viewModel.Task.UpdateCabinetInfo(viewModel.SelectedRecord);
                                viewModel.Task.RemoveCabinetObserve(viewModel.SelectedRecord);

                                txtRealCap.TextChanged -= GroupBox_TextChanged_1;

                            }

                        }

                    }

                }
            }
            #endregion

            #region 重新设置值的方法
            //var viewModel = this.DataContext as ThElectricalExchangeViewModel;

            ////当且仅当为组合框时才执行
            //if (e.OriginalSource is ComboBox)
            //{
            //    var combo = e.OriginalSource as ComboBox;
            //    //有值得时候再换算
            //    if (combo.SelectedValue != null && combo.Tag != null)
            //    {
            //        if (combo.Tag.ToString() == "容量选型")
            //        {
            //            viewModel.SelectedRecord.PowerCapacityElement.ResetByValue();

            //        }

            //    }
            //} 
            #endregion

        }


        /// <summary>
        /// 回路真实值的改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GroupBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            //如果配电箱没有改变
            if (!ThElectricalExchangeViewModel.cabinetChanged)
            {
                var viewModel = this.DataContext as ThElectricalExchangeViewModel;

                if (e.OriginalSource is TextBox)
                {
                    var textBox = e.OriginalSource as TextBox;

                    if (textBox.Tag != null && textBox.Tag.ToString() == "值")
                    {
                        //这里因为要执行容量的联动，所以容量也不能为0
                        if (viewModel.SelectedRecord != null && viewModel.SelectedRecord.PowerCapacityElement != null)
                        {
                            //断开cmb的变化事件
                            cmbPower.SelectionChanged -= GroupBox_SelectionChanged_1;

                            //一旦进入了,就表示记录更改了,修改状态
                            viewModel.SelectedRecordChanged = true;

                            viewModel.SelectedRecord.PowerCapacityElement.SetEachPro();

                            //执行信息的联动
                            viewModel.Task.AddCabinetObserve(viewModel.SelectedRecord);
                            viewModel.Task.UpdateCabinetInfo(viewModel.SelectedRecord);
                            viewModel.Task.RemoveCabinetObserve(viewModel.SelectedRecord);

                            //重新连接cmb的变化事件
                            cmbPower.SelectionChanged += GroupBox_SelectionChanged_1;

                        }
                    }
                }
            }
        }

        /// <summary>
        /// 开关的值的改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GroupBox_TextChanged_2(object sender, TextChangedEventArgs e)
        {
            //如果配电箱没有改变
            if (!ThElectricalExchangeViewModel.cabinetChanged)
            {
                var viewModel = this.DataContext as ThElectricalExchangeViewModel;

                if (e.OriginalSource is TextBox)
                {
                    var textBox = e.OriginalSource as TextBox;

                    if (textBox.Tag != null && textBox.Tag.ToString() == "值")
                    {
                        if (viewModel.SelectedRecord != null)
                        {
                            //一旦进入了,就表示记录更改了,修改状态
                            viewModel.SelectedRecordChanged = true;

                            //存在双速风机没有开关型号，加入空值判断
                            if (viewModel.SelectedRecord.BranchSwitchElement != null)
                            {
                                viewModel.SelectedRecord.BranchSwitchElement.SetEachPro();
                            }
                        }


                    }
                }
            }
        }
    }
}
