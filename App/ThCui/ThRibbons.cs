using System;
using System.Windows;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Windows;
using WinApp = System.Windows.Application;

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
            foreach (var item in tab.Panels)
            {
                item.IsVisible = true;
            }
        }

        /// <summary>
        /// 关闭除指定panel外的所有ribbon
        /// </summary>
        /// <param name="tab"></param>
        /// <param name="panel"></param>
        public void CloseRibbon()
        {
            var tab = ComponentManager.Ribbon.ActiveTab;
            foreach (var item in tab.Panels)
            {
                item.IsVisible = false;
            }
            var panel = tab.Panels[0];
            panel.IsVisible = true;
        }

        /// <summary>
        /// 登录
        /// </summary>
        public void Login()
        {
            var usualDate = new DateTime(2019, 5, 5);
            var dateTime = DateTime.Today;
            if ((dateTime - usualDate).Days <= 62)
            {
                ShowAllRibbon(ComponentManager.Ribbon.ActiveTab);
            }
            else
            {
                //
            }
        }

        /// <summary>
        /// 退出
        /// </summary>
        public void Logout()
        {
            CloseRibbon();
        }
    }
}
