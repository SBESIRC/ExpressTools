using System;
using System.Linq;
using Autodesk.Windows;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace TianHua.AutoCAD.ThCui
{
    public class ThRibbonUtils
    {
        public static RibbonTab Tab
        {
            get
            {
                if (ComponentManager.Ribbon == null)
                {
                    return null;
                }
                foreach (RibbonTab tab in ComponentManager.Ribbon.Tabs)
                {
                    if (tab.Name == ThCADCommon.RibbonTabName &&
                        tab.Title == ThCADCommon.RibbonTabTitle)
                    {
                        return tab;
                    }
                }
                return null;
            }
        }

        public static void OpenAllPanels()
        {
            // 登入状态，开启所有面板
            Tab.Panels.ForEach(o => o.IsEnabled = true);
            foreach (var panel in Tab.Panels.Where(o => o.UID == "pnl" + "Help"))
            {
                // 隐藏登陆按钮，显示退出按钮
                panel.Source.Items.Where(o => o.Id == "ID_THLOGIN").ForEach(o => o.IsVisible = false);
                panel.Source.Items.Where(o => o.Id == "ID_THLOGOUT").ForEach(o => o.IsVisible = true);
            }
        }

        public static void CloseAllPanels()
        {
            // 登出状态，关闭所有面板
            Tab.Panels.ForEach(o => o.IsEnabled = false);
            foreach(var panel in Tab.Panels.Where(o => o.UID == "pnl" + "Help"))
            {
                // 开启“登陆”Panel
                panel.IsEnabled = true;
                // 显示登陆按钮，隐藏退出按钮
                panel.Source.Items.Where(o => o.Id == "ID_THLOGIN").ForEach(o => o.IsVisible = true);
                panel.Source.Items.Where(o => o.Id == "ID_THLOGOUT").ForEach(o => o.IsVisible = false);
            }
        }

        public static void RegisterTabActivated(EventHandler handler)
        {
            Tab.Activated += handler;
        }

        public static void UnRegisterTabActivated(EventHandler handler)
        {
            Tab.Activated -= handler;
        }
    }
}
