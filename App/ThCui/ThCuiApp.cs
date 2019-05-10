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
        const string CMD_THBLS_GLOBAL_NAME = "THBLS";

        ThRibbons ribbons = new ThRibbons();

        public void Initialize()
        {
            //
            RegisterCommands();

            //安装事件
            AcadApp.DocumentManager.DocumentActivated += Docs_DocumentActivated;
        }

        public void Terminate()
        {
            //
            UnregisterCommands();

            //卸载事件
            AcadApp.DocumentManager.DocumentActivated -= Docs_DocumentActivated;

            //卸载Ribbon
            RemoveRibbon();
        }

        public void RegisterCommands()
        {
            //注册登录命令
            Utils.AddCommand(CMD_GROUPNAME, CMD_THCADLOGIN_GLOBAL_NAME, CMD_THCADLOGIN_GLOBAL_NAME, CommandFlags.Modal, new CommandCallback(ribbons.Login));
            //注册退出命令
            Utils.AddCommand(CMD_GROUPNAME, CMD_THCADLOGOUT_GLOBAL_NAME, CMD_THCADLOGOUT_GLOBAL_NAME, CommandFlags.Modal, new CommandCallback(ribbons.Logout));

            //注册打开工具选项板配置命令
            Utils.AddCommand(CMD_GROUPNAME, CMD_THBLS_GLOBAL_NAME, CMD_THBLS_GLOBAL_NAME, CommandFlags.Modal, new CommandCallback(ShowToolPalette));
        }

        public void UnregisterCommands()
        {
            Utils.RemoveCommand(CMD_GROUPNAME, CMD_THCADLOGIN_GLOBAL_NAME);
            Utils.RemoveCommand(CMD_GROUPNAME, CMD_THCADLOGOUT_GLOBAL_NAME);
            Utils.RemoveCommand(CMD_GROUPNAME, CMD_THBLS_GLOBAL_NAME);
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
        void Docs_DocumentActivated(object sendger, EventArgs e)
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
            }
        }

        /// <summary>
        /// 显示工具选项板配置
        /// </summary>
        public void ShowToolPalette()
        {
            ThToolPalette toolPalette = new ThToolPalette();
            AcadApp.ShowModalDialog(toolPalette);
        }
    }
}
