using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        public CalculationInfoVM calculationInfoVM = null;
        public ImportCalculation(CalculationInfoVM calculationInfoVM)
        {
            InitializeComponent();
            this.calculationInfoVM = calculationInfoVM;
            this.DataContext = this.calculationInfoVM;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = this.calculationInfoVM;
            int index = this.calculationInfoVM.CalculateInfo.YjkUsedPathList.IndexOf(this.calculationInfoVM.CalculateInfo.YjkPath);
            if(index>=0)
            {
                this.cbYjkFilePath.SelectedIndex = index;
            }
            this.rbSelectByStandard.IsChecked = calculationInfoVM.CalculateInfo.SelectByStandard;
            this.rbSelectByNatural.IsChecked = !calculationInfoVM.CalculateInfo.SelectByStandard;
        }
        private void CbYjkFilePath_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(string.IsNullOrEmpty(this.cbYjkFilePath.Text))
            {
                return;
            }
           calculationInfoVM.UpdateSelectFloorList();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key==Key.Escape)
            {
                this.Close();
            }
        }

        private void TbAngle_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex re = new Regex("[^0-9.\\-]+");
            e.Handled = re.IsMatch(e.Text);
        }
        private void RbSelectByNatural_Checked(object sender, RoutedEventArgs e)
        {
            this.lbModelLayers.SelectionMode = SelectionMode.Multiple;            
            calculationInfoVM.UpdateSelectFloorList();
            calculationInfoVM.UpdateSelectMode(false);
        }

        private void RbSelectByNatural_Unchecked(object sender, RoutedEventArgs e)
        {
            calculationInfoVM.UpdateSelectMode(true);
        }

        private void RbSelectByStandard_Checked(object sender, RoutedEventArgs e)
        {
            this.lbModelLayers.SelectionMode = SelectionMode.Single;
            calculationInfoVM.UpdateSelectFloorList();
            calculationInfoVM.UpdateSelectMode(true);
        }

        private void RbSelectByStandard_Unchecked(object sender, RoutedEventArgs e)
        {
            calculationInfoVM.UpdateSelectMode(false);
        }
    }
}
