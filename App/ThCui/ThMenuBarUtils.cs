﻿using System;
using System.Linq;
using Autodesk.AutoCAD.Interop;
using TianHua.AutoCAD.Utility.ExtensionTools;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
using ThIdentity.SDK;
using System.Collections.Generic;

namespace TianHua.AutoCAD.ThCui
{
    public static class ThMenuBarUtils
    {
        public static readonly Dictionary<Profile, string> Profiles = new Dictionary<Profile, string>()
        {
            { Profile.WSS, "给排水" },
            { Profile.HAVC, "暖通" },
            { Profile.STRUCTURE, "结构" },
            { Profile.ELECTRICAL, "电气" },
            { Profile.PROJECTPLAN, "方案" },
            { Profile.ARCHITECTURE, "建筑" },
        };

        public static AcadPopupMenu PopupMenu
        {
            get
            {
#if ACAD_ABOVE_2014
                //  2016启动时可能进入Zero doc state，
                //  这时候获取MenuGroups会抛出COM Exception
                //  http://help.autodesk.com/view/ACD/2016/ENU/?guid=GUID-CB7D7DC2-C8C1-4EF9-A638-C4C6184BFC85
                if (AcadApp.DocumentManager.Count == 0)
                {
                    return null;
                }
#endif
                var acadApp = AcadApp.AcadApplication as AcadApplication;
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
            var menuItem = thePopupMenu.Item(0);
            if (menuItem.TagString == "ID_登录")
            {
                menuItem.Delete();
            }

            // 添加登出菜单项
            thePopupMenu.AddMenuItem(0,
                "退出",
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
            var menuItem = thePopupMenu.Item(0);
            if (menuItem.TagString == "ID_退出")
            {
                menuItem.Delete();
            }

            // 添加登入菜单项
            thePopupMenu.AddMenuItem(0,
                "登录",
                "\x001B\x001B\x005F" + ThCuiCommon.CMD_THLOGIN_GLOBAL_NAME + "\x0020");
        }

        public static void ConfigMenubarWithCurrentUser()
        {
            if (ThIdentityService.IsLogged())
            {
                EnableMenuItems();
            }
            else
            {
                DisableMenuItems();
            }
        }

        public static void ConfigMenubarWithCurrentProfile()
        {
            var thePopupMenu = PopupMenu;
            if (thePopupMenu == null)
            {
                return;
            }

            Profile profile = ThCuiProfileManager.Instance.CurrentProfile;
            var profileItem = Profiles.Where(o => o.Key == profile).First();
            foreach (AcadPopupMenuItem item in thePopupMenu)
            {
                if (item.TagString == "ID_THMenu_Profile")
                {
                    foreach (AcadPopupMenuItem subItem in item.SubMenu)
                    {
                        if (subItem.Caption.Contains(profileItem.Value))
                        {
                            subItem.Check = true;
                        }
                        else
                        {
                            subItem.Check = false;
                        }
                    }
                }
            }
        }
    }
}
