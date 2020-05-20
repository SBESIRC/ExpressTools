using System;
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
    public partial class ThSparyLayoutSet : Window
    {
        public ThSparyLayoutSet()
        {
            InitializeComponent();

            this.custom.IsChecked = true;
            this.tab1.IsChecked = true;

            ReadConfigInfo();
        }

        #region 事件触发
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

        private void tabRB_Checked(object sender, RoutedEventArgs e)
        {
            ReadConfigInfo();
        }

        /// <summary>
        /// 取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cancel_Btn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        /// <summary>
        /// 确定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Run_Btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!VerifyInputInfo()) { return; };
                SaveConfigInfo();
            }
            catch (Exception ex)
            {
            }
            finally
            {
                this.Close();
            }
        }

        #region 操作配置
        /// <summary>
        /// 存储上一次信息
        /// </summary>
        private void SaveConfigInfo()
        {
            LayoutCacheModel layoutCacheModel = new LayoutCacheModel();
            Constraint constraint = new Constraint();
            //自定义/标准约束
            if (this.custom.IsChecked == true)
            {
                constraint.constraintType = this.custom.Name;
                Custom custom = new Custom();
                custom.sparySpacing = this.customControl.sparySSpcing.Text + "," + this.customControl.sparyESpcing.Text;
                custom.otherSpacing = this.customControl.otherSSpcing.Text + "," + this.customControl.otherESpcing.Text;
                constraint.custom = custom;
            }
            else
            {
                constraint.constraintType = this.standard.Name;
                ThWSS.Config.Model.Standard standard = new ThWSS.Config.Model.Standard();
                standard.hazardLevel = this.standControl.danLevel.SelectedIndex.ToString();
                if (this.standControl.expandRange.IsChecked == true)
                {
                    standard.range = this.standControl.expandRange.Name;
                }
                else
                {
                    standard.range = this.standControl.standRange.Name;
                }
                constraint.standard = standard;
            }
            layoutCacheModel.constraint = constraint;
            //上喷下喷
            if (this.downSpary.IsChecked == true)
            {
                layoutCacheModel.nozzleType = downSpary.Name;
            }
            else
            {
                layoutCacheModel.nozzleType = upSpary.Name;
            }
            //保护策略
            if (this.radiusPro.IsChecked == true)
            {
                layoutCacheModel.protectStrategy = this.radiusPro.Name;
            }
            else
            {
                layoutCacheModel.protectStrategy = this.rectanglePro.Name;
            }
            layoutCacheModel.protectRadius = radius.Text;
            //考虑梁
            HasBeam hasBeam = new HasBeam();
            if (this.HasBeam.IsChecked == true)
            {
                hasBeam.considerBeam = "1";
                hasBeam.beamHeight = this.beamHeight.Text;
                hasBeam.plateThick = this.plateThick.Text;
            }
            else
            {
                hasBeam.considerBeam = "0";
                hasBeam.beamHeight = this.beamHeight.Text;
                hasBeam.plateThick = this.plateThick.Text;
            }
            layoutCacheModel.hasBeam = hasBeam;
            //布置对象
            if (this.frame.IsChecked == true)
            {
                layoutCacheModel.layoutType = this.frame.Name;
            }
            else if (this.customPart.IsChecked == true)
            {
                layoutCacheModel.layoutType = this.customPart.Name;
            }
            else
            {
                 layoutCacheModel.layoutType = this.fire.Name;
            }
            string nodeName = GetTabName();
            ConfigHelper configHelper = new ConfigHelper();
            configHelper.elementNode = configHelper.ReadNode(nodeName).FirstOrDefault();
            configHelper.Serialize(layoutCacheModel);
        }

        /// <summary>
        /// 读取上一次信息
        /// </summary>
        private void ReadConfigInfo()
        {
            string nodeName = GetTabName();
            ConfigHelper configHelper = new ConfigHelper();
            configHelper.elementNode = configHelper.ReadNode(nodeName).FirstOrDefault();
            LayoutCacheModel layoutCacheModel = configHelper.Deserialize<LayoutCacheModel>();
            //自定义/标准约束
            if(layoutCacheModel.constraint.constraintType == this.standard.Name)
            {
                this.standard.IsChecked = true;
                bool parse = int.TryParse(layoutCacheModel.constraint.standard.hazardLevel, out int res);
                this.standControl.danLevel.SelectedIndex = parse ? res : 0;
                if (layoutCacheModel.constraint.standard.range == this.standControl.expandRange.Name)
                {
                    this.standControl.expandRange.IsChecked = true;
                }
                else
                {
                    this.standControl.standRange.IsChecked = true;
                }
            }
            else
            {
                this.custom.IsChecked = true;
                var spcing = layoutCacheModel.constraint.custom.sparySpacing.Split(',');
                this.customControl.sparySSpcing.Text = spcing[0];
                this.customControl.sparyESpcing.Text = spcing.Length > 1 ? spcing[1] : null;
                spcing = layoutCacheModel.constraint.custom.otherSpacing.Split(',');
                this.customControl.otherSSpcing.Text = spcing[0];
                this.customControl.otherESpcing.Text = spcing.Length > 1 ? spcing[1] : null;
            }
            //上喷下喷
            if (layoutCacheModel.nozzleType == this.downSpary.Name)
            {
                this.downSpary.IsChecked = true;
            }
            else
            {
                this.upSpary.IsChecked = true;
            }
            //保护策略
            if (layoutCacheModel.protectStrategy == this.radiusPro.Name)
            {
                this.radiusPro.IsChecked = true;
            }
            else
            {
                this.rectanglePro.IsChecked = true;
            }
            this.radius.Text = layoutCacheModel.protectRadius;
            //考虑梁
            if (layoutCacheModel.hasBeam.considerBeam != null && layoutCacheModel.hasBeam.considerBeam != "0")
            {
                this.HasBeam.IsChecked = true;
                this.beamHeight.IsEnabled = true;
                this.plateThick.IsEnabled = true;
            }
            this.beamHeight.Text = layoutCacheModel.hasBeam.beamHeight;
            this.plateThick.Text = layoutCacheModel.hasBeam.plateThick;
            //布置对象
            if (layoutCacheModel.layoutType == frame.Name)
            {
                this.frame.IsChecked = true;
            }
            else if (layoutCacheModel.layoutType == customPart.Name)
            {
                this.customPart.IsChecked = true;
            }
            else
            {
                this.fire.IsChecked = true;
            }
        }

        private string GetTabName()
        {
            string res = this.tab1.Name;
            if (this.tab2.IsChecked == true)
            {
                res = this.tab2.Name;
            }
            else if (this.tab3.IsChecked == true)
            {
                res = this.tab3.Name;
            }
            else if (this.tab4.IsChecked == true)
            {
                res = this.tab4.Name;
            }
            
            return res;
        }
        #endregion

        #region 验证输入信息
        /// <summary>
        /// 验证输入信息
        /// </summary>
        /// <returns></returns>
        private bool VerifyInputInfo()
        {
            return VerifyBeamInfo();
        }

        /// <summary>
        /// 验证考虑梁输入信息
        /// </summary>
        /// <returns></returns>
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
        #endregion

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

        #region 提示窗
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
        #endregion
    }
}
