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
        private MarchPropertyVM marchPropertyVM;
        public MarchProperty(MarchPropertyVM marchPropertyVM)
        {
            InitializeComponent();
            this.marchPropertyVM = marchPropertyVM;
            this.DataContext = marchPropertyVM;
        }
        private void Control_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(sender is CheckBox cb)
            {
                SetCheckBox(cb.Name);
                Reset_AcadInitConfig();
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
        private void Reset_AcadInitConfig()
        {
            List<CheckBox> chkNames = new List<CheckBox> { this.cbLayer, this.cbColor, this.cbLineType,
                this.cbLineWeight, this.cbTextSize,this.cbTextDirection };
            bool acadInitConfig = true;
            foreach(CheckBox cb in chkNames)
            {
                if(cb.IsChecked==false)
                {
                    acadInitConfig = false;
                    break;
                }
            }
            if(this.cbTextContent.IsChecked==true)
            {
                acadInitConfig = false;
            }
            this.cbAcadInitConfig.IsChecked = acadInitConfig;
        }
        private void Set_AcadInitConfig()
        {
            List<CheckBox> chkNames = new List<CheckBox> { this.cbLayer, this.cbColor, this.cbLineType,
                this.cbLineWeight, this.cbTextSize,this.cbTextDirection };
            chkNames.ForEach(i => i.IsChecked = this.cbAcadInitConfig.IsChecked);
            if(this.cbAcadInitConfig.IsChecked==true)
            {
                this.cbTextContent.IsChecked = false;
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
                    this.marchPropertyVM.Cancel();
                    break;
                case Key.Space:
                case Key.Enter:
                    this.marchPropertyVM.Confirm();
                    break;
            }
        }

        private void CbAcadInitConfig_Click(object sender, RoutedEventArgs e)
        {
            Set_AcadInitConfig();
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
            if (this.cbTextDirection.IsChecked == false)
            {
                this.cbAcadInitConfig.IsChecked = false;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.marchPropertyVM.Executed = false;
        }

        private void CbAcadInitConfig_Checked(object sender, RoutedEventArgs e)
        {
            List<CheckBox> chkNames = new List<CheckBox> { this.cbLayer, this.cbColor, this.cbLineType,
                this.cbLineWeight, this.cbTextSize,this.cbTextDirection };
            if (this.cbAcadInitConfig.IsChecked==true)
            {
                chkNames.ForEach(i => i.IsChecked = true);
            }
            else if(this.cbAcadInitConfig.IsChecked == false)
            {
                chkNames.ForEach(i => i.IsChecked = false);
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if(this.cbAcadInitConfig.IsChecked==true)
            {
                Set_AcadInitConfig();

            }
            this.btnOK.Focus();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key==Key.Space)
            {
                this.btnOK.Focus();
            }
        }
    }
}
