using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public partial class ThXclipProgressBar : Window, INotifyPropertyChanged
    {
        private BackgroundWorker bgworker = new BackgroundWorker();
        public BackgroundWorker Bgworker
        {
            get
            {
                return bgworker;
            }
        }
        private int percentage = 0;
        public int Percentage
        {
            get
            {
                return percentage;
            }
            set
            {
                percentage = value;
                NotifyPropertyChanged("StopProgress");
            }
        }
        private string tip = "";
        public string Tip
        {
            get
            {
                return tip;
            }
            set
            {
                tip = value;
                NotifyPropertyChanged("Tip");
            }
        }

        public ThXclipProgressBar()
        {
            InitializeComponent();
            //ProgressBegin();
            InitWork();           
        }
        void Store_ThXClipWorkFinshed(ThXclipCommands thXClipApp, bool res)
        {
            if (res)
            {     
                this.Close();
            }              
        }
        public void Register(ThXclipCommands thXClipCmd)
        {
            thXClipCmd.WorkFinished += Store_ThXClipWorkFinshed;
        }
        private void ProgressBegin()
        {
            Thread thread = new Thread(new ThreadStart(() =>
            {
                int i = 1;
                while (i<=101)
                {
                    i = i % 101;
                    this.pb_import.Dispatcher.BeginInvoke((ThreadStart)delegate { this.pb_import.Value = i++; });
                    Thread.Sleep(100);
                }
            }));
            thread.Start();
        }
        /// <summary>
        /// 初始化bgwork
        /// </summary>
        private void InitWork()
        {
            bgworker.WorkerReportsProgress = true;
            bgworker.DoWork += new DoWorkEventHandler(DoWork);
            bgworker.ProgressChanged += new ProgressChangedEventHandler(BgworkChange);
        }
        private void DoWork(object sender, DoWorkEventArgs e)
        {
            //for(int i = 1;i<101;i++)
            //{
            //    bgworker.ReportProgress(i);
            //    Thread.Sleep(100);
            //    if(i==100)
            //    {
            //        i = 0;
            //    }
            //}
        }
        /// <summary>
        ///改变进度条的值
        /// </summary>
        private void BgworkChange(object sender, ProgressChangedEventArgs e)
        {
            this.pb_import.Value = e.ProgressPercentage;
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
