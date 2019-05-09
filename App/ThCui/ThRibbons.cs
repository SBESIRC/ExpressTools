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

        /// <summary>
        /// 显示指定tab的ribbon
        /// </summary>
        /// <param name="tab"></param>
        public void ShowRibbon(RibbonTab tab)
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
        public void CloseRibbon(RibbonTab tab)
        {
            foreach (var item in tab.Panels)
            {
                item.IsVisible = false;
            }
            var panel = tab.Panels[0];
            panel.IsVisible = true;
        }

        /// <summary>
        /// 动态生成ribbon界面，并添加至cad
        /// </summary>
        [CommandMethod("TIANHUACAD", "AddRibbonXAML", CommandFlags.Modal)]
        public void AddRibbonXAML()
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


            var btnHelp = (RibbonButton)tab.FindItem("ID_BtnHelp");

            RibbonControl ribbonControl = ComponentManager.Ribbon;//获取Ribbon界面
            ribbonControl.Tabs.Add(tab);//将选项卡添加到Ribbon界面中

            //关闭所有的ribbon,仅保留登录模块
            CloseRibbon(tab);
        }

        /// <summary>
        /// 登录
        /// </summary>
        [CommandMethod("TIANHUACAD", "THLOGIN", CommandFlags.Modal)]
        public void Login()
        {
            var usualDate = new DateTime(2019, 5, 5);
            var dateTime = DateTime.Today;
            if ((dateTime - usualDate).Days <= 62)
            {
                ShowRibbon(ComponentManager.Ribbon.ActiveTab);
                ////重启命令
                //Autodesk.AutoCAD.Internal.CommandCallback cc = new Autodesk.AutoCAD.Internal.CommandCallback(CheWeiBianHao);
                //Autodesk.AutoCAD.Internal.Utils.AddCommand("MyGroup", "THCHU", "THCHU", CommandFlags.Modal, cc);
            }
            else
            {
                //Autodesk.AutoCAD.Internal.Utils.RemoveCommand("MyGroup", "THCNU");
                //System.Windows.Forms.MessageBox.Show("天华效率工具已过期，请使用最新版本！");
            }
        }

        /// <summary>
        /// 退出
        /// </summary>
        [CommandMethod("TIANHUACAD", "THLOGOUT", CommandFlags.Modal)]
        public void Logout()
        {
            CloseRibbon(ComponentManager.Ribbon.ActiveTab);
        }

    }
}
