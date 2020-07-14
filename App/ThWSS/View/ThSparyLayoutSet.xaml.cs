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
using ThWSS.Engine;
using TianHua.AutoCAD.Utility.ExtensionTools;

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

            this.standard.IsChecked = true;
            ReadConfigInfo();
        }

        #region 事件触发
        private void StandardRb_Checked(object sender, RoutedEventArgs e)
        {
            if (this.standard.IsChecked == true)
            {
                this.customControl.Visibility = System.Windows.Visibility.Collapsed;
                this.standControl.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                this.customControl.Visibility = System.Windows.Visibility.Visible;
                this.standControl.Visibility = System.Windows.Visibility.Collapsed;
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
            //SaveConfigInfo();
            this.Close();
        }
        
        /// <summary>
        /// 确定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Run_Btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //验证
                if (!VerifyInputInfo()) { return; };

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
            }
            finally
            {
                //this.Close();
            }
        }
        #endregion

        #region 操作配置
        /// <summary>
        /// 存储上一次信息
        /// </summary>
        public void SaveConfigInfo()
        {
            LayoutCacheModel layoutCacheModel = new LayoutCacheModel();
            Constraint constraint = new Constraint();
            //存储tab页
            if (this.tab1.IsChecked == true)
            {
                layoutCacheModel.tanName = this.tab1.Name;
            }
            else if (this.tab2.IsChecked == true)
            {
                layoutCacheModel.tanName = this.tab2.Name;
            }
            else if (this.tab3.IsChecked == true)
            {
                layoutCacheModel.tanName = this.tab3.Name;
            }
            else if (this.tab4.IsChecked == true)
            {
                layoutCacheModel.tanName = this.tab4.Name;
            }
            else if (this.tab5.IsChecked == true)
            {
                layoutCacheModel.tanName = this.tab5.Name;
            }
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
            layoutCacheModel.protectRadius = "0";
            //上喷下喷
            if (this.downSpary.IsChecked == true)
            {
                layoutCacheModel.nozzleType = downSpary.Name;
            }
            else
            {
                layoutCacheModel.nozzleType = upSpary.Name;
            }
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
            else
            {
                 layoutCacheModel.layoutType = this.fire.Name;
            }
            string nodeName = GetTabName();
            ConfigHelper configHelper = new ConfigHelper();
            configHelper.elementNode = configHelper.ReadNode(nodeName).FirstOrDefault();
            configHelper.SaveConfig(layoutCacheModel);
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
            //存储tab页
            if (this.tab1.IsChecked != true && this.tab2.IsChecked != true && this.tab3.IsChecked != true
                && this.tab4.IsChecked != true && this.tab5.IsChecked != true)
            {
                if (string.IsNullOrEmpty(layoutCacheModel.tanName))
                {
                    this.tab1.IsChecked = true;
                }
                else
                {
                    if (layoutCacheModel.tanName == this.tab1.Name)
                    {
                        this.tab1.IsChecked = true;
                    }
                    else if (layoutCacheModel.tanName == this.tab2.Name)
                    {
                        this.tab2.IsChecked = true;
                    }
                    else if (layoutCacheModel.tanName == this.tab3.Name)
                    {
                        this.tab3.IsChecked = true;
                    }
                    else if (layoutCacheModel.tanName == this.tab4.Name)
                    {
                        this.tab4.IsChecked = true;
                    }
                    else if (layoutCacheModel.tanName == this.tab5.Name)
                    {
                        this.tab5.IsChecked = true;
                    }
                }
            }
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
            //考虑梁
            if (!string.IsNullOrEmpty(layoutCacheModel.hasBeam.considerBeam) && layoutCacheModel.hasBeam.considerBeam == "0")
            {
                this.HasBeam.IsChecked = false;
                this.beamHeight.IsEnabled = false;
                this.plateThick.IsEnabled = false;
            }
            else
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
            else
            {
                this.fire.IsChecked = true;
            }

            SetDefaultValues();
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

        /// <summary>
        /// 设置默认值
        /// </summary>
        private void SetDefaultValues()
        {
            if (string.IsNullOrEmpty(this.beamHeight.Text))
            {
                this.beamHeight.Text = "600";
            }

            if (string.IsNullOrEmpty(this.plateThick.Text))
            {
                this.plateThick.Text = "100";
            }

            if (string.IsNullOrEmpty(this.customControl.sparySSpcing.Text))
            {
                this.customControl.sparySSpcing.Text = "1500";
            }

            if (string.IsNullOrEmpty(this.customControl.sparyESpcing.Text))
            {
                this.customControl.sparyESpcing.Text = "2200";
            }

            if (string.IsNullOrEmpty(this.customControl.otherSSpcing.Text))
            {
                this.customControl.otherSSpcing.Text = "300";
            }

            if (string.IsNullOrEmpty(this.customControl.otherESpcing.Text))
            {
                this.customControl.otherESpcing.Text = "2700";
            }
        }
        #endregion

        #region 验证输入信息
        /// <summary>
        /// 验证输入信息
        /// </summary>
        /// <returns></returns>
        private bool VerifyInputInfo()
        {
            return VerifySparySpacing() && VerifyBeamInfo();
        }

        /// <summary>
        /// 验证考虑梁输入信息
        /// </summary>
        /// <returns></returns>
        private bool VerifyBeamInfo()
        {
            if (this.HasBeam.IsChecked == true)
            {
                if (string.IsNullOrEmpty(this.plateThick.Text) || string.IsNullOrEmpty(this.beamHeight.Text))
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
        /// 验证自定义距离约束输入信息
        /// </summary>
        /// <returns></returns>
        private bool VerifySparySpacing()
        {
            if (this.custom.IsChecked == true)
            {
                if (string.IsNullOrEmpty(this.customControl.sparySSpcing.Text)|| string.IsNullOrEmpty(this.customControl.sparyESpcing.Text))
                {
                    MessageBox.Show("在自定义距离约束的情况下喷头间距不能为空！");
                    return false;
                }
                else
                {
                    if (Convert.ToInt32(this.customControl.sparySSpcing.Text) > Convert.ToInt32(this.customControl.sparyESpcing.Text))
                    {
                        this.customControl.sparyESpcing.Text = null;
                        MessageBox.Show("喷头最小间距不能大于最大间距！");
                        return false;
                    }
                }

                if (string.IsNullOrEmpty(this.customControl.otherSSpcing.Text) || string.IsNullOrEmpty(this.customControl.otherESpcing.Text))
                {
                    MessageBox.Show("在自定义距离约束的情况下距离墙/柱间距不能为空！");
                    return false;
                }
                else
                {
                    if (Convert.ToInt32(this.customControl.otherSSpcing.Text) > Convert.ToInt32(this.customControl.otherESpcing.Text))
                    {
                        this.customControl.otherESpcing.Text = null;
                        MessageBox.Show("距离墙/柱最小距离不能大于最大距离！");
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
            Regex re = new Regex("[^0-9]+");
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

        /// <summary>
        /// 按ESC关闭窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Esc_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)//Esc键  
            {
                //SaveConfigInfo();
                this.Close();
            }
        }
    }
}
