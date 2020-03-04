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

namespace ThEssential.MatchProps
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MarchProperty : Window
    {
        public MarchProperty()
        {
            InitializeComponent();
        }
        private void Control_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(sender is CheckBox cb)
            {
                SetCheckBox(cb.Name);
            }
        }        
        private void SetCheckBox(string name)
        {
            List<CheckBox> chkNames = new List<CheckBox> { this.cbLayer, this.cbColor, this.cbLineType, this.cbLineWeight, this.cbTextSize,
                this.cbTextContent, this.cbTextDirection };
            bool isEmptySelectMode = true;
            foreach (var item in chkNames)
            {
                if (item.IsChecked==true)
                {
                    isEmptySelectMode = false;
                    break;
                }
            }
            //在空选状态下，右击任意选项，全部选中
            if(isEmptySelectMode)
            {
                foreach (var item in chkNames)
                {
                    item.IsChecked = true;
                }
            }
            else
            {
                //在非空选状态下，右击任意选项，此项选中,其他项清空
                foreach (var item in chkNames)
                {
                    if (item.Name == name)
                    {
                        item.IsChecked = true;
                    }
                    else
                    {
                        item.IsChecked = false;
                    }
                }
            }
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.L:
                    this.cbLayer.IsChecked = !this.cbLayer.IsChecked;
                    break;
                case Key.C:
                    this.cbColor.IsChecked = !this.cbColor.IsChecked;
                    break;
                case Key.E:
                    this.cbLineType.IsChecked = !this.cbLineType.IsChecked;
                    break;
                case Key.W:
                    this.cbLineWeight.IsChecked = !this.cbLineWeight.IsChecked;
                    break;
                case Key.S:
                    this.cbTextSize.IsChecked = !this.cbTextSize.IsChecked;
                    break;
                case Key.T:
                    this.cbTextContent.IsChecked = !this.cbTextContent.IsChecked;
                    break;
                case Key.D:
                    this.cbTextDirection.IsChecked = !this.cbTextDirection.IsChecked;
                    break;
                case Key.A:
                    this.cbAcadInitConfig.IsChecked = !this.cbAcadInitConfig.IsChecked;
                    break;
                case Key.Escape:
                    this.Close();
                    break;
            }
        }

        private void CbAcadInitConfig_Click(object sender, RoutedEventArgs e)
        {
            this.cbColor.IsChecked = true;
            this.cbLayer.IsChecked = true;
            this.cbLineType.IsChecked = true;
            this.cbLineWeight.IsChecked = true;
            this.cbTextSize.IsChecked = true;

            this.cbTextContent.IsChecked = false;
            this.cbTextDirection.IsChecked = false;
        }

        private void CbLayer_Click(object sender, RoutedEventArgs e)
        {
            if (this.cbLayer.IsChecked == false)
            {
                this.cbAcadInitConfig.IsChecked = false;
            }
        }

        private void CbColor_Click(object sender, RoutedEventArgs e)
        {
            if (this.cbColor.IsChecked == false)
            {
                this.cbAcadInitConfig.IsChecked = false;
            }
        }

        private void CbLineType_Click(object sender, RoutedEventArgs e)
        {
            if (this.cbLineType.IsChecked == false)
            {
                this.cbAcadInitConfig.IsChecked = false;
            }
        }

        private void CbLineWeight_Click(object sender, RoutedEventArgs e)
        {
            if (this.cbLineWeight.IsChecked == false)
            {
                this.cbAcadInitConfig.IsChecked = false;
            }
        }

        private void CbTextSize_Click(object sender, RoutedEventArgs e)
        {
            if (this.cbTextSize.IsChecked == false)
            {
                this.cbAcadInitConfig.IsChecked = false;
            }
        }

        private void CbTextContent_Click(object sender, RoutedEventArgs e)
        {
            if (this.cbTextContent.IsChecked == true)
            {
                this.cbAcadInitConfig.IsChecked = false;
            }
        }

        private void CbTextDirection_Click(object sender, RoutedEventArgs e)
        {
            if (this.cbTextDirection.IsChecked == true)
            {
                this.cbAcadInitConfig.IsChecked = false;
            }
        }
    }
}
