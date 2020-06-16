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
using ThWss.View;

namespace ThWSS.View
{
    /// <summary>
    /// StandardUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class StandardUserControl : UserControl
    {
        public StandardUserControl()
        {
            InitializeComponent();
        }

        private void HazardLvl_Btn_Click(object sender, RoutedEventArgs e)
        {
            StandardBindWindow standardBindWindow = new StandardBindWindow();
            standardBindWindow.ShowDialog();
        }
    }
}
