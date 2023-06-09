﻿using System;
using Autodesk.AutoCAD.Interop;
using Autodesk.AutoCAD.ApplicationServices;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace TianHua.AutoCAD.ThCui
{
    public static class ThMenuBarUtils
    {
        public static AcadPopupMenu PopupMenu
        {
            get
            {
                var acadApp = Application.AcadApplication as AcadApplication;
                foreach (AcadMenuGroup menuGroup in acadApp.MenuGroups)
                {
                    if (string.Equals(menuGroup.Name,
                        ThCADCommon.CuixMenuGroup,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (AcadPopupMenu popupMenu in menuGroup.Menus)
                        {
                            if (popupMenu.TagString == "ID_THMenu")
                            {
                                return popupMenu;
                            }
                        }
                    }
                }
                return null;
            }
        }


        public static void EnableMenuItems()
        {
            var thePopupMenu = PopupMenu;
            if (thePopupMenu == null)
            {
                return;
            }

            foreach(AcadPopupMenuItem item in thePopupMenu)
            {
                item.Enable = true;
            }

            // 删除登入菜单项
            int itemCount = thePopupMenu.Count;
            var menuItem = thePopupMenu.Item(itemCount - 1);
            if (menuItem.TagString == "ID_登录 <THLOGIN>")
            {
                menuItem.Delete();
            }

            // 添加登出菜单项
            thePopupMenu.AddMenuItem(itemCount,
                "退出 <THLOGOUT>",
                "\x001B\x001B\x005F" + ThCuiCommon.CMD_THLOGOUT_GLOBAL_NAME + "\x0020");
        }

        public static void DisableMenuItems()
        {
            var thePopupMenu = PopupMenu;
            if (thePopupMenu == null)
            {
                return;
            }

            foreach (AcadPopupMenuItem item in thePopupMenu)
            {
                if (item.TagString == "ID_THHLP" || 
                    item.TagString == "ID_THUPT")
                {
                    item.Enable = true;
                }
                else
                {
                    item.Enable = false;
                }
            }

            // 删除登出菜单项
            int itemCount = thePopupMenu.Count;
            var menuItem = thePopupMenu.Item(itemCount - 1);
            if (menuItem.TagString == "ID_退出 <THLOGOUT>")
            {
                menuItem.Delete();
            }

            // 添加登入菜单项
            thePopupMenu.AddMenuItem(itemCount,
                "登录 <THLOGIN>",
                "\x001B\x001B\x005F" + ThCuiCommon.CMD_THLOGIN_GLOBAL_NAME + "\x0020");
        }
    }
}
