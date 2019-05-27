using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
using Autodesk.AutoCAD.Internal;
using Autodesk.Windows;

namespace TianHua.AutoCAD.ThCui
{
    public class ThCuiApp : IExtensionApplication
    {
        const string CMD_GROUPNAME = "TIANHUACAD";
        const string CMD_THCADLOGIN_GLOBAL_NAME = "THCADLOGIN";
        const string CMD_THCADLOGOUT_GLOBAL_NAME = "THCADLOGOUT";
        const string CMD_THHLP_GLOBAL_NAME = "THHLP";
        const string CMD_THBLS_GLOBAL_NAME = "THBLS";
        const string CMD_THBLI_GLOBAL_NAME = "THBLI";


        ThRibbons ribbons = new ThRibbons();
        ThToolPalette toolPalette = new ThToolPalette();

        public void Initialize()
        {
            //
            RegisterCommands();

            //安装事件
            AcadApp.DocumentManager.DocumentLockModeChanged += Docs_DocumentLockModeChanged;
        }

        public void Terminate()
        {
            //
            UnregisterCommands();

            //卸载事件
            AcadApp.DocumentManager.DocumentLockModeChanged -= Docs_DocumentLockModeChanged;

            //卸载Ribbon
            RemoveRibbon();
        }

        public void RegisterCommands()
        {
            //注册登录命令
            Utils.AddCommand(CMD_GROUPNAME, CMD_THCADLOGIN_GLOBAL_NAME, CMD_THCADLOGIN_GLOBAL_NAME, CommandFlags.Modal, new CommandCallback(ribbons.Login));
            //注册退出命令
            Utils.AddCommand(CMD_GROUPNAME, CMD_THCADLOGOUT_GLOBAL_NAME, CMD_THCADLOGOUT_GLOBAL_NAME, CommandFlags.Modal, new CommandCallback(ribbons.Logout));

            //注册帮组和命令
            Utils.AddCommand(CMD_GROUPNAME, CMD_THHLP_GLOBAL_NAME, CMD_THHLP_GLOBAL_NAME, CommandFlags.Modal, new CommandCallback(ribbons.ShowHelpFile));

            //注册打开工具选项板配置命令
            Utils.AddCommand(CMD_GROUPNAME, CMD_THBLS_GLOBAL_NAME, CMD_THBLS_GLOBAL_NAME, CommandFlags.Modal, new CommandCallback(ShowToolPaletteConfigDialog));

            //注册工具选项板开关命令
            Utils.AddCommand(CMD_GROUPNAME, CMD_THBLI_GLOBAL_NAME, CMD_THBLI_GLOBAL_NAME, CommandFlags.Modal, new CommandCallback(toolPalette.ShowToolPalette));
        }

        public void UnregisterCommands()
        {
            Utils.RemoveCommand(CMD_GROUPNAME, CMD_THCADLOGIN_GLOBAL_NAME);
            Utils.RemoveCommand(CMD_GROUPNAME, CMD_THCADLOGOUT_GLOBAL_NAME);
            Utils.RemoveCommand(CMD_GROUPNAME, CMD_THHLP_GLOBAL_NAME);
            Utils.RemoveCommand(CMD_GROUPNAME, CMD_THBLS_GLOBAL_NAME);
            Utils.RemoveCommand(CMD_GROUPNAME, CMD_THBLI_GLOBAL_NAME);

        }

        public void RemoveRibbon()
        {
            RibbonControl rc = ComponentManager.Ribbon;
            if (rc == null)
            {
                return;
            }

            RibbonTab tab = rc.FindTab("ID_TabMyRibbon");
            if (tab != null)
                rc.Tabs.Remove(tab);
        }

        /// <summary>
        /// 加载ribbon事件
        /// </summary>
        /// <param name="sendger"></param>
        /// <param name="e"></param>
        void Docs_DocumentLockModeChanged(object sendger, EventArgs e)
        {
            RibbonControl rc = ComponentManager.Ribbon;
            if (rc == null)
            {
                return;
            }

            if (rc.FindTab("ID_TabMyRibbon") == null)
            {
                RibbonTab tab = ribbons.CreateRibbon();
                if (tab != null)
                {
                    rc.Tabs.Add(tab);
                }
            }

            RibbonTab theTab = rc.FindTab("ID_TabMyRibbon");
            if (theTab != null)
            {
                theTab.IsVisible = true;
                ribbons.Count++;
            }

            //如果是第一次创建完毕后，仅呈现登录模块
            if (ribbons.Count==1)
            {
                ribbons.CloseTabRibbon();
            }

        }

        /// <summary>
        /// 显示工具选项板配置
        /// </summary>
        public void ShowToolPaletteConfigDialog()
        {
            AcadApp.ShowModalDialog(toolPalette);
        }


    }
}
