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


        ThMenuBar menuBar = new ThMenuBar();
        ThRibbons ribbons = new ThRibbons();
        ThToolPalette toolPalette = new ThToolPalette();

        public void Initialize()
        {
            //add code to run when the ExtApp initializes. Here are a few examples:
            //  Checking some host information like build #, a patch or a particular Arx/Dbx/Dll;
            //  Creating/Opening some files to use in the whole life of the assembly, e.g. logs;
            //  Adding some ribbon tabs, panels, and/or buttons, when necessary;
            //  Loading some dependents explicitly which are not taken care of automatically;
            //  Subscribing to some events which are important for the whole session;
            //  Etc.

            //将程序有效期验证为3个月，一旦超过时限，要求用户更新，不进行命令注册
            var usualDate = new DateTime(2019, 8, 15);
            var dateTime = DateTime.Today;
            if ((dateTime - usualDate).Days <= 62)
            {
                //装载局部CUI文件
                LoadPartialCui();

                //注册命令
                RegisterCommands();

                //注册事件
                Autodesk.AutoCAD.Ribbon.RibbonServices.RibbonPaletteSetCreated +=
                    new EventHandler(RibbonServices_RibbonPaletteSetCreated);
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("天华效率工具已经过期，请及时更新！");
            }
        }

        public void Terminate()
        {
            //add code to clean up things when the ExtApp terminates. For example:
            //  Closing the log files;
            //  Deleting the custom ribbon tabs/panels/buttons;
            //  Unloading those dependents;
            //  Un-subscribing to those events;
            //  Etc.
        }

        private void LoadPartialCui()
        {
            menuBar.LoadThMenu();
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

        private void RemoveRibbon()
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

        private void CreateRibbon()
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
            if (ribbons.Count == 1)
            {
                ribbons.CloseTabRibbon();
            }
        }

        private void RibbonServices_RibbonPaletteSetCreated(object sender, EventArgs e)
        {
            AcadApp.Idle += new EventHandler(Application_OnIdle);
        }

        private void Application_OnIdle(object sender, EventArgs e)
        {
            AcadApp.Idle -= new EventHandler(Application_OnIdle);

            CreateRibbon();
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
