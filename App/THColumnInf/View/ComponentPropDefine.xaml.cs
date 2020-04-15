using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using winForm= System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
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
    public partial class ComponentPropDefine : Window
    {
        private ComponentPropDefineVM componentPropDefineVM = null;
        private List<FrameworkElement> elements = new List<FrameworkElement>();
        public static bool isOpened = false;
        public ComponentPropDefine(ComponentPropDefineVM componentPropDefineVM)
        {
            InitializeComponent();
            isOpened = true;
            this.componentPropDefineVM = componentPropDefineVM;
            this.DataContext = this.componentPropDefineVM;
            elements.AddRange(new List<FrameworkElement> { this.cbAntiSeismicGrade,this.cbConcreteStrength,
                this.cbCornerColumn,this.cbHoopFullHeightEncryption,this.tbHoopReinEnlargeTimes,
                this.tbLongitudalEnlargeTimes,this.tbProtectThickness});
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                isOpened = false;
                this.Close();
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            WpfTopMostTool.SetTopmost(hwnd);
            this.lbPropertiyNames.SelectedIndex = 0;
            ShowHideSetValueControl(this.componentPropDefineVM.PropertyInfos[0].Name);
        }
        private void ShowHideSetValueControl(string controlName)
        {
            for(int i=0;i<this.elements.Count;i++)
            {
                if(this.elements[i].Name== controlName)
                {
                    this.elements[i].Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    this.elements[i].Visibility = System.Windows.Visibility.Hidden;
                }
            }
        }

        private void LbPropertiyNames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(sender is ListBox lb)
            {
                PropertyInfo pi= lb.SelectedItem as PropertyInfo;
                this.componentPropDefineVM.PropertySetText = pi.Text;
            }
            ShowHideSetValueControl(this.componentPropDefineVM.PropertyName);
            this.componentPropDefineVM.UpdateTitle();
            this.Title = this.componentPropDefineVM.Title;
            this.componentPropDefineVM.UpdatePropertyList();
            this.lbProperties.ItemsSource = this.componentPropDefineVM.CtInfos;
            this.componentPropDefineVM.Refresh();
        }

        private void ChkRecoveryInit_Checked(object sender, RoutedEventArgs e)
        {
            EnableSetValueControl();
        }
        private void EnableSetValueControl()
        {
            bool isEnable = true;
            if(this.componentPropDefineVM.RecoveryInit==true)
            {
                isEnable = false;
            }
            for (int i = 0; i < this.elements.Count; i++)
            {
                this.elements[i].IsEnabled = isEnable;
            }
        }

        private void LbProperties_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(sender is ListBox lb)
            {
                if(lb.SelectedItem is ColorTextInfo cti)
                {
                    this.componentPropDefineVM.PropertyListItemToSetValue(cti.Content);
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.componentPropDefineVM.Hide();
            this.componentPropDefineVM.CancelSelect();
            isOpened = false;
        }
        public void CloseWindow()
        {
            isOpened = false;
            this.Close();
        }
    }
}
