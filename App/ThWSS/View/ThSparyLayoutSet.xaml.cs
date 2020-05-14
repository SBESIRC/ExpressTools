using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace ThWss.View
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ThSparyLayoutSet : Window
    {
        public ThSparyLayoutSet()
        {
            InitializeComponent();

            this.custom.IsChecked = true;
        }

        private void StandardRb_Checked(object sender, RoutedEventArgs e)
        {
            if (this.standard.IsChecked == true)
            {
                this.customControl.Visibility = Visibility.Collapsed;
                this.standControl.Visibility = Visibility.Visible;
            }
            else
            {
                this.customControl.Visibility = Visibility.Visible;
                this.standControl.Visibility = Visibility.Collapsed;
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (this.HasBeam.IsChecked == true)
            {
                this.beamHeight.IsEnabled = true;
                this.plateThick.IsEnabled = true;
            }
            else
            {
                this.beamHeight.IsEnabled = false;
                this.plateThick.IsEnabled = false;
            }
        }

        private void ProtectStra_Btn_Click(object sender, RoutedEventArgs e)
        {
            ProtectStrategyWindow protectStrategyWindow = new ProtectStrategyWindow();
            protectStrategyWindow.ShowDialog();
        }

        private void HasBeam_Btn_Click(object sender, RoutedEventArgs e)
        {
            BeamInfluenceWindow beamInfluenceWindow = new BeamInfluenceWindow();
            beamInfluenceWindow.ShowDialog();
        }

        private void BeamHeight_Btn_Click(object sender, RoutedEventArgs e)
        {
            DefaultBeamHeightWindow defaultBeamHeightWindow = new DefaultBeamHeightWindow();
            defaultBeamHeightWindow.ShowDialog();
        }

        private void PlateThick_Btn_Click(object sender, RoutedEventArgs e)
        {
            PlateThickWindow plateThickWindow = new PlateThickWindow();
            plateThickWindow.ShowDialog();
        }

        private void Cancel_Btn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Run_Btn_Click(object sender, RoutedEventArgs e)
        {
            if (VerifyInputInfo()) { return; };
        }

        private bool VerifyInputInfo()
        {
            return VerifyBeamInfo();
        }

        private bool VerifyBeamInfo()
        {
            if (this.HasBeam.IsChecked == true)
            {
                if (this.plateThick.Text == null || this.beamHeight.Text == null)
                {
                    MessageBox.Show("在考虑梁影响的情况下默认梁高和顶板厚度都不能为空！");
                    return false;
                }
                else
                {
                    if (Convert.ToInt32(this.plateThick.Text) > Convert.ToInt32(this.beamHeight.Text))
                    {
                        this.plateThick.Text = null;
                        MessageBox.Show("顶板厚度不能超过默认梁高！");
                        return false;
                    }
                }
            }
            return true;
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
    }
}
