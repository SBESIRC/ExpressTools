using System.Windows;

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
    }
}
