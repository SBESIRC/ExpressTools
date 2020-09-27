using AcHelper;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Linq2Acad;
using NFox.Cad.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using ThWSS.Config;
using ThWSS.Config.Model;

namespace ThWss.View
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ThProtectRangeWindow : Window
    {
        public ThProtectRangeWindow()
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
            Regex re = new Regex("[^0-9.-]+");
            e.Handled = re.IsMatch(e.Text);
        }

        private void TextBox_LostFocusWallSpace(object sender, RoutedEventArgs e)
        {
            if (Convert.ToDouble(this.wallSpace.Text) > 100)
            {
                MessageBox.Show("需要输入低于规范值100mm！");
                this.drawingBtn.IsEnabled = false;
            }
            else
            {
                this.drawingBtn.IsEnabled = true;
            }
        }

        private void TextBox_LostFocusLineSpace(object sender, RoutedEventArgs e)
        {
            if (Convert.ToDouble(this.lineSpace.Text) < 4000)
            {
                MessageBox.Show("需要输入高于规范值4000mm！");
                this.drawingBtn.IsEnabled = false;
            }
            else
            {
                this.drawingBtn.IsEnabled = true;
            }
        }

        private void TextBox_LostFocusProtectRange(object sender, RoutedEventArgs e)
        {
            if (Convert.ToDouble(this.protectRange.Text) * 100 < 4000)
            {
                MessageBox.Show("需要输入高于规范值4000mm！");
                this.drawingBtn.IsEnabled = false;
            }
            else
            {
                this.drawingBtn.IsEnabled = true;
            }
        }
    }
}
