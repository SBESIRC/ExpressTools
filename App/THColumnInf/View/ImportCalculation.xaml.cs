using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ThColumnInfo.ViewModel;

namespace ThColumnInfo.View
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ImportCalculation : Window
    {
        private CalculationInfoVM calculationInfoVM = null;
        public ImportCalculation(CalculationInfoVM calculationInfoVM)
        {
            InitializeComponent();
            this.calculationInfoVM = calculationInfoVM;
            this.DataContext = this.calculationInfoVM;
        }
        private void RbSelectByNatural_Checked(object sender, RoutedEventArgs e)
        {
            calculationInfoVM.UpdateSelectFloorList();
        }

        private void RbSelectByStandard_Checked(object sender, RoutedEventArgs e)
        {
            calculationInfoVM.UpdateSelectFloorList();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = this.calculationInfoVM;
        }

        private void CbYjkFilePath_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(string.IsNullOrEmpty(this.cbYjkFilePath.Text))
            {
                return;
            }
           calculationInfoVM.UpdateSelectFloorList();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.YjkUsePath = this.cbYjkFilePath.Text;
            if(!string.IsNullOrEmpty(this.tbAngle.Text))
            {
                double angle = 0.0;
                bool res=double.TryParse(this.tbAngle.Text, out angle);
                if(res)
                {
                    Properties.Settings.Default.Angle = Convert.ToDouble(this.tbAngle.Text);
                }
            }
            if(cbModelPoint.IsChecked!=null)
            {
                Properties.Settings.Default.ModelPoint = cbModelPoint.IsChecked==true?true:false;
            }
            System.Collections.Specialized.StringCollection collection= new System.Collections.Specialized.StringCollection();
            foreach(var item in this.calculationInfoVM.CalculateInfo.YjkUsedPathList)
            {
                collection.Add(item);
            }
            Properties.Settings.Default.YjkUsedPathList = collection;

            System.Collections.Specialized.StringCollection selectFloorList = new System.Collections.Specialized.StringCollection();
            foreach (var item in this.calculationInfoVM.CalculateInfo.SelectLayers)
            {
                selectFloorList.Add(item);
            }
            Properties.Settings.Default.SelectFloorList = selectFloorList;
            Properties.Settings.Default.Save();
        }
    }
}
