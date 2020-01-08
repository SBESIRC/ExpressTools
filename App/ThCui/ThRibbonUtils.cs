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
            Tab.Panels.ForEach(o => o.IsEnabled = true);
        }

        public static void CloseAllPanels()
        {
            Tab.Panels.ForEach(o => o.IsEnabled = false);
            Tab.Panels.Where(o => o.UID == "pnl" + "Help").ForEach(o => o.IsEnabled = true);
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
