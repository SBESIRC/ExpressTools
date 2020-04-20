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
    public partial class ParameterSet : Window
    {
        public ParameterSetVM parameterSetVM = null;
        public ParameterSet(ParameterSetVM psv)
        {
            InitializeComponent();
            this.parameterSetVM = psv;
            this.DataContext = this.parameterSetVM;
            ParameterSetVM.isOpened = true;
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key==Key.Escape)
            {
                this.Close();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            ParameterSetVM.isOpened = false;
        }
    }
}
