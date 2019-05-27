using System;
using System.Windows;
using Autodesk.AutoCAD.Internal;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Windows;
using WinApp = System.Windows.Application;

namespace TianHua.AutoCAD.ThCui
{
    public partial class ThRibbons
    {
        //调用资源字典
        ResourceDictionary resourceDictionary = (ResourceDictionary)WinApp.LoadComponent(new Uri("/ThCui;component/ThRibbonDictionary.xaml", UriKind.Relative));

        public int Count { get; set; }//标记第几次创建
        public RibbonTab CreateRibbon()
        {
            //获取由XAML定义的选项卡
            RibbonTab tab = resourceDictionary["TabXaml"] as RibbonTab;
            //查找Ribbon按钮并添加命令事件
            foreach (var panel in tab.Panels)
            {
                foreach (RibbonItem item in panel.Source.Items)
                {
                    if (item is RibbonButton)
                    {
                        ((RibbonButton)item).CommandHandler = new RibbonCommandHandler();
                    }
                    else if (item is RibbonRowPanel)
                    {
                        RibbonRowPanel row = (RibbonRowPanel)item;
                        foreach (RibbonItem rowItem in row.Items)
                        {
                            if (rowItem is RibbonButton)
                                ((RibbonButton)rowItem).CommandHandler = new RibbonCommandHandler();
                        }
                    }
                }
            }

            //添加事件
            var btnTuKuai = (RibbonToggleButton)tab.FindItem("ID_BtnTuKuai");
            btnTuKuai.CheckStateChanged += (sender, e) =>
            {
                var button = (RibbonToggleButton)sender;
                if (!button.IsChecked)
                {
                    button.CommandParameter = "TOOLPALETTESCLOSE";
                }
                else
                {
                    button.CommandParameter = "THBLI";
                }
            };

            return tab;
        }

        /// <summary>
        /// 显示指定tab的ribbon
        /// </summary>
        /// <param name="tab"></param>
        public void ShowAllRibbon(RibbonTab tab)
        {
            foreach (var panel in tab.Panels)
            {
                panel.IsVisible = true;
                foreach (var item in panel.Source.Items)
                {
                    item.IsVisible = true;
                }
            }
        }

        /// <summary>
        /// 关闭除指定panel外的所有ribbon
        /// </summary>
        /// <param name="tab"></param>
        /// <param name="panel"></param>
        public void CloseTabRibbon()
        {
            //找到创建的ribbontab
            var tab = ComponentManager.Ribbon.FindTab("ID_TabMyRibbon");
            //关闭所有的
            foreach (var item in tab.Panels)
            {
                item.IsVisible = false;
            }
            //找到登录Panel,显示打开
            var loginPanel = tab.Panels[0];
            loginPanel.IsVisible = true;

            //遍历所有登录Panel中的item,全部关闭
            foreach (var item in loginPanel.Source.Items)
            {
                item.IsVisible = false;
            }

            //找到登录Panel中的登录模块，显示打开
            var loginItem = loginPanel.Source.Items[0];
            loginItem.IsVisible = true;
        }

        /// <summary>
        /// 登录
        /// </summary>
        public void Login()
        {
            //将程序有效期验证为3个月，一旦超过时限，要求用户更新，反注册命令
            var usualDate = new DateTime(2019, 6, 1);
            var dateTime = DateTime.Today;
            if ((dateTime - usualDate).Days <= 62)
            {
                ShowAllRibbon(ComponentManager.Ribbon.ActiveTab);
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("天华效率工具已经过期，请及时更新！");
                const string CMD_GROUPNAME = "TIANHUACAD";
                const string CMD_THCADLOGIN_GLOBAL_NAME = "THCADLOGIN";
                const string CMD_THCADLOGOUT_GLOBAL_NAME = "THCADLOGOUT";
                const string CMD_THHLP_GLOBAL_NAME = "THHLP";
                const string CMD_THBLS_GLOBAL_NAME = "THBLS";
                const string CMD_THBLI_GLOBAL_NAME = "THBLI";

                Utils.RemoveCommand(CMD_GROUPNAME, CMD_THCADLOGIN_GLOBAL_NAME);
                Utils.RemoveCommand(CMD_GROUPNAME, CMD_THCADLOGOUT_GLOBAL_NAME);
                Utils.RemoveCommand(CMD_GROUPNAME, CMD_THHLP_GLOBAL_NAME);
                Utils.RemoveCommand(CMD_GROUPNAME, CMD_THBLS_GLOBAL_NAME);
                Utils.RemoveCommand(CMD_GROUPNAME, CMD_THBLI_GLOBAL_NAME);

                const string CMD_THCNU_GLOBAL_NAME = "THCNU";
                Utils.RemoveCommand(CMD_GROUPNAME, CMD_THCNU_GLOBAL_NAME);

            }
        }

        /// <summary>
        /// 退出
        /// </summary>
        public void Logout()
        {
            CloseTabRibbon();
        }

        /// <summary>
        /// 显示帮助文档
        /// </summary>
        public void ShowHelpFile()
        {
            var helpFile = @"http://info.thape.com.cn/AI/thcad/help.html";
            System.Diagnostics.Process.Start(helpFile);
        }

    }
}
