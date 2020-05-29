using System.ComponentModel;
using System.Windows;
using System.Windows.Input;


namespace ThColumnInfo.View
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ParameterSetTip : Window, INotifyPropertyChanged
    {
        public ParameterSetTip()
        {
            InitializeComponent();
            this.DataContext = this;
        }
        private string tip = "  请确认图纸是否为柱表或表格表达方式，如是，请指定柱表框线图层再继续。";

        public string Tip
        {
            set
            {
                tip = value;
                NotifyPropertyChange("Tip");
            }
            get
            {
                return tip;
            }
        }
        private bool isGoOn=false;
        public bool IsGoOn
        {
            get
            {
                return isGoOn;
            }
            set
            {
                isGoOn = value;
            }
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key==Key.Escape)
            {
                this.isGoOn = false;
                this.Close();
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChange(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void BtnContinue_Click(object sender, RoutedEventArgs e)
        {
            this.isGoOn = true;
            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.isGoOn = false;
            this.Close();
        }
    }
}
