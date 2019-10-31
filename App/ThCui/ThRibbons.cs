using System;
using System.Windows;
using System.Windows.Forms;
using Autodesk.Windows;
using ThIdentity.SDK;
using WinApp = System.Windows.Application;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace TianHua.AutoCAD.ThCui
{
    public partial class ThRibbons
    {
        //调用资源字典
        ResourceDictionary resourceDictionary = (ResourceDictionary)WinApp.LoadComponent(new Uri("/ThCui;component/ThRibbonDictionary.xaml", UriKind.Relative));

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
            using (var dlg = new ThLoginDlg())
            {
                if (AcadApp.ShowModalDialog(dlg) != DialogResult.OK)
                {
                    return;
                }

                ThIdentityService.Login(dlg.User, dlg.Password);
            }

            // 开启DA会话
            if (ThIdentityService.IsLogged())
            {
                THRecordingService.SessionBegin();
            }

            // 更新Ribbon
            if (ThIdentityService.IsLogged())
            {
                ShowAllRibbon(ComponentManager.Ribbon.ActiveTab);
            }
        }

        /// <summary>
        /// 退出
        /// </summary>
        public void Logout()
        {
            // 结束DA会话
            if (ThIdentityService.IsLogged())
            {
                THRecordingService.SessionEnd();
            }

            ThIdentityService.Logout();

            // 更新Ribbon
            if (!ThIdentityService.IsLogged())
            {
                CloseTabRibbon();
            }
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
