using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace ThXClip
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ThXclipProgressBar : Window
    {
        public ThXclipProgressBar()
        {
            InitializeComponent();
            this.SetValue(1000);
        }
        private void SetValue(int count)
        {
            for(int i=0;i<count;i++)
            {
                BeginImport(i);
            }
        }
        void Store_ThXClipWorkFinshed(ThXclipCommands thXClipApp, bool res)
        {
            if (res)
            {
                BeginImport(1000);
                this.Close();
            }
              
        }
        public void Register(ThXclipCommands thXClipCmd)
        {
            thXClipCmd.WorkFinished += Store_ThXClipWorkFinshed;
        }

        private delegate void UpdateProgressBarDelegate(System.Windows.DependencyProperty dp, Object value);
        private void BeginImport(int i)
        {
            pb_import.Maximum = 1000;
            pb_import.Value = 0;
            UpdateProgressBarDelegate updatePbDelegate = new UpdateProgressBarDelegate(pb_import.SetValue);
            Thread thread = new Thread(new ThreadStart(() =>
            {
                Dispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { System.Windows.Controls.ProgressBar.ValueProperty, Convert.ToDouble(i + 1) });
                Thread.Sleep(100);
            }));
            thread.Start();
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
