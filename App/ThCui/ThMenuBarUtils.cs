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
            foreach(AcadPopupMenuItem item in PopupMenu)
            {
                item.Enable = true;
            }
            // 添加登出菜单
            PopupMenu.AddMenuItem(PopupMenu.Count, 
                "退出 <THLOGOUT>",
                "\x001B\x001B\x005F" + ThCuiCommon.CMD_THLOGOUT_GLOBAL_NAME + "\x0020");
        }

        public static void DisableMenuItems()
        {
            foreach (AcadPopupMenuItem item in PopupMenu)
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
            // 添加登陆菜单
            PopupMenu.AddMenuItem(PopupMenu.Count,
                "登陆 <THLOGIN>",
                "\x001B\x001B\x005F" + ThCuiCommon.CMD_THLOGIN_GLOBAL_NAME + "\x0020");
        }
    }
}
