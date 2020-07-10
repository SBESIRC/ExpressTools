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

namespace ThWSS.View
{
    /// <summary>
    /// CustomizeUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class CustomizeUserControl : UserControl
    {
        public CustomizeUserControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 控制只能输入数字
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tb_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex re = new Regex("[^0-9]+");
            e.Handled = re.IsMatch(e.Text);
        }

        private void sparySSpcing_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.sparySSpcing.Text))
            {
                if (!string.IsNullOrEmpty(this.sparyESpcing.Text) && Convert.ToInt32(this.sparySSpcing.Text) > Convert.ToInt32(this.sparyESpcing.Text))
                {
                    this.sparySSpcing.Text = this.sparyESpcing.Text;
                }
                if (Convert.ToInt32(this.sparySSpcing.Text) < 500 || Convert.ToInt32(this.sparySSpcing.Text) > 5000)
                {
                    this.sparySSpcing.Text = "500";
                }
            } 
        }

        private void sparyESpcing_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.sparyESpcing.Text))
            {
                if (!string.IsNullOrEmpty(this.sparySSpcing.Text) && Convert.ToInt32(this.sparySSpcing.Text) > Convert.ToInt32(this.sparyESpcing.Text))
                {
                    this.sparyESpcing.Text = this.sparySSpcing.Text;
                }
                if (Convert.ToInt32(this.sparyESpcing.Text) < 500 || Convert.ToInt32(this.sparyESpcing.Text) > 5000)
                {
                    this.sparyESpcing.Text = "5000";
                }
            }
        }

        private void otherSSpcing_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.otherSSpcing.Text))
            {
                if (!string.IsNullOrEmpty(this.otherESpcing.Text) && Convert.ToInt32(this.otherSSpcing.Text) > Convert.ToInt32(this.otherESpcing.Text))
                {
                    this.otherSSpcing.Text = this.otherESpcing.Text;
                }
                if (Convert.ToInt32(this.otherSSpcing.Text) < 100 || Convert.ToInt32(this.otherSSpcing.Text) > 3000)
                {
                    this.otherSSpcing.Text = "100";
                }
            }
        }

        private void otherESpcing_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.otherESpcing.Text))
            {
                if (!string.IsNullOrEmpty(this.otherSSpcing.Text) && Convert.ToInt32(this.otherSSpcing.Text) > Convert.ToInt32(this.otherESpcing.Text))
                {
                    this.otherESpcing.Text = this.otherSSpcing.Text;
                }
                if (Convert.ToInt32(this.otherESpcing.Text) < 100 || Convert.ToInt32(this.otherESpcing.Text) > 3000)
                {
                    this.otherESpcing.Text = "3000";
                }
            }
        }
    }
}
