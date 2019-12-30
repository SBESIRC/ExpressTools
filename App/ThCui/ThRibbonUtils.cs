using Autodesk.Windows;

namespace TianHua.AutoCAD.ThCui
{
    public class ThRibbonUtils
    {
        public static void OpenAllPanels()
        {
            var tab = ComponentManager.Ribbon.ActiveTab;
            foreach (var panel in tab.Panels)
            {
                panel.IsVisible = true;
                foreach (var item in panel.Source.Items)
                {
                    item.IsVisible = true;
                }
            }
        }

        public static void CloseAllPanels()
        {
            var tab = ComponentManager.Ribbon.ActiveTab;
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
    }
}
