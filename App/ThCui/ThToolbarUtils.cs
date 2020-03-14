using System;
using ThIdentity.SDK;
using Autodesk.AutoCAD.Interop;
using System.Collections.Generic;
using TianHua.AutoCAD.Utility.ExtensionTools;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace TianHua.AutoCAD.ThCui
{
    public class ThToolbarUtils
    { 
        public static readonly Dictionary<Profile, string> Toolbars = new Dictionary<Profile, string>()
        {
            { Profile.WSS, "天华给排水" },
            { Profile.HAVC, "天华暖通" },
            { Profile.STRUCTURE, "天华结构" },
            { Profile.ELECTRICAL, "天华电气" },
            { Profile.ARCHITECTURE, "天华建筑" },
        };

        private static AcadMenuGroup MenuGroup
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
                        return menuGroup;
                    }
                }
                return null;
            }
        }

        private static AcadToolbar FindToolbarWithName(string name)
        {
            foreach(AcadToolbar toolbar in MenuGroup.Toolbars)
            {
                if (toolbar.Name == name)
                {
                    return toolbar;
                }
            }

            return null;
        }

        public static void OpenAllToolbars()
        {
            FindToolbarWithName("天华通用").Visible = true;
            foreach (var item in Toolbars)
            {
                FindToolbarWithName(item.Value).Visible = true;
            }
        }

        public static void CloseAllToolbars()
        {
            FindToolbarWithName("天华通用").Visible = false;
            foreach (var item in Toolbars)
            {
                FindToolbarWithName(item.Value).Visible = false;
            }
        }

        public static void ConfigToolbarsWithCurrentUser()
        {
            if (ThIdentityService.IsLogged())
            {
                OpenAllToolbars();
            }
            else
            {
                CloseAllToolbars();
            }
        }

        public static void ConfigToolbarsWithCurrentProfile()
        { 
            FindToolbarWithName("天华通用").Visible = true;
            foreach (var item in Toolbars)
            {
                if (item.Key == ThCuiProfileManager.Instance.CurrentProfile)
                {
                    FindToolbarWithName(item.Value).Visible = true;
                }
                else
                {
                    FindToolbarWithName(item.Value).Visible = false;
                }
            }
        }
    }
}