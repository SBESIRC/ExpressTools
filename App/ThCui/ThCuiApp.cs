using System;
using System.Net;
using System.IO;
using System.Diagnostics;
using Autodesk.Windows;
using Autodesk.AutoCAD.Internal;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices.PreferencesFiles;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace TianHua.AutoCAD.ThCui
{
    public class ThCuiApp : IExtensionApplication
    {
        const string CMD_GROUPNAME = "TIANHUACAD";
        const string CMD_THCADLOGIN_GLOBAL_NAME = "THCADLOGIN";
        const string CMD_THCADLOGOUT_GLOBAL_NAME = "THCADLOGOUT";
        const string CMD_THT20PLUGINV4_GLOBAL_NAME = "THT20PLUGINV4";
        const string CMD_THT20PLUGINV5_GLOBAL_NAME = "THT20PLUGINV5";
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
            if ((DateTime.Today - ThCADCommon.Global_Expire_Start_Date).Days <= ThCADCommon.Global_Expire_Duration)
            {
                //装载局部CUI文件
                LoadPartialCui();

                //注册命令
                RegisterCommands();

                //定制Preferences
                OverridePreferences(true);

                //创建自定义Ribbon
                //  https://www.theswamp.org/index.php?topic=44440.0
                ThRibbonHelper.OnRibbonFound(this.CreateRibbon);
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

            //恢复Preferences
            OverridePreferences(false);
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

            //注册下载T20天正插件命令
            Utils.AddCommand(CMD_GROUPNAME, CMD_THT20PLUGINV4_GLOBAL_NAME, CMD_THT20PLUGINV4_GLOBAL_NAME, CommandFlags.Modal, new CommandCallback(DownloadT20PlugInV4));
            Utils.AddCommand(CMD_GROUPNAME, CMD_THT20PLUGINV5_GLOBAL_NAME, CMD_THT20PLUGINV5_GLOBAL_NAME, CommandFlags.Modal, new CommandCallback(DownloadT20PlugInV5));
        }

        public void UnregisterCommands()
        {
            Utils.RemoveCommand(CMD_GROUPNAME, CMD_THCADLOGIN_GLOBAL_NAME);
            Utils.RemoveCommand(CMD_GROUPNAME, CMD_THCADLOGOUT_GLOBAL_NAME);
            Utils.RemoveCommand(CMD_GROUPNAME, CMD_THHLP_GLOBAL_NAME);
            Utils.RemoveCommand(CMD_GROUPNAME, CMD_THBLS_GLOBAL_NAME);
            Utils.RemoveCommand(CMD_GROUPNAME, CMD_THBLI_GLOBAL_NAME);
            Utils.RemoveCommand(CMD_GROUPNAME, CMD_THT20PLUGINV4_GLOBAL_NAME);
            Utils.RemoveCommand(CMD_GROUPNAME, CMD_THT20PLUGINV5_GLOBAL_NAME);
        }

        private void OverridePreferences(bool bOverride = true)
        {
#if ACAD_ABOVE_2014
            var path = ThCADCommon.PlottersPath();
            var printerConfigPath = new PrinterConfigPath(AcadApp.Preferences);
            if (bOverride && !printerConfigPath.Contains(path))
            {
                printerConfigPath.Add(path);
                printerConfigPath.SaveChanges();
            }
            else if (!bOverride && printerConfigPath.Contains(path))
            {
                printerConfigPath.Remove(path);
                printerConfigPath.SaveChanges();
            }

            path = ThCADCommon.PrinterDescPath();
            var printerDescPath = new PrinterDescPath(AcadApp.Preferences);
            if (bOverride && !printerDescPath.Contains(path))
            {
                printerDescPath.Add(path);
                printerDescPath.SaveChanges();
            }
            else if (!bOverride && printerDescPath.Contains(path))
            {
                printerDescPath.Remove(path);
                printerDescPath.SaveChanges();
            }

            path = ThCADCommon.PrinterStyleSheetPath();
            var printerStyleSheetPath = new PrinterStyleSheetPath(AcadApp.Preferences);
            if (bOverride && !printerStyleSheetPath.Contains(path))
            {
                printerStyleSheetPath.Add(path);
                printerStyleSheetPath.SaveChanges();
            }
            else if (!bOverride && printerStyleSheetPath.Contains(path))
            {
                printerStyleSheetPath.Remove(path);
                printerStyleSheetPath.SaveChanges();
            }
#endif
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

        private void CreateRibbon(RibbonControl rc)
        {
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

        /// <summary>
        /// 显示工具选项板配置
        /// </summary>
        public void ShowToolPaletteConfigDialog()
        {
            AcadApp.ShowModalDialog(toolPalette);
        }

        /// <summary>
        /// 下载T20天正插件
        /// </summary>
        public void DownloadT20PlugInV4()
        {
            DownloadT20PlugInWorker("T20-PlugIn", "V4.0");
        }

        public void DownloadT20PlugInV5()
        {
            DownloadT20PlugInWorker("T20-PlugIn", "V5.0");
        }

        private void DownloadT20PlugInWorker(string appName, string appVersion)
        {
            string fileName = string.Format("{0} {1}.exe", appName, appVersion);
            Uri downloadUri = new Uri(string.Join("/", ThCADCommon.ServerUrl, "Release", fileName));
            string downloadFile = Path.Combine(Path.GetTempPath(), fileName);

            try
            {
                var wc = new WebClient();
                var dlg = new ThT20PlugInDownloadDlg(appName, appVersion);
                dlg.FormClosing += (o, e) =>
                {
                    // 在下载中关闭进度条窗口，取消下载
                    if ((wc != null) && wc.IsBusy)
                    {
                        wc.CancelAsync();
                    }
                };
                wc.DownloadProgressChanged += (o, e) =>
                {
                    // 在下载中更新进度条窗口
                    dlg.OnDownloadProgressChanged(o, e);
                };
                wc.DownloadFileCompleted += (o, e) => {
                    // 取消下载
                    if (e.Cancelled)
                    {
                        return;
                    }
                    // 下载完成
                    if (e.Error == null)
                    {
                        // 关闭进度条窗口
                        dlg.Close();

                        // 执行下载安装程序
                        if (File.Exists(downloadFile))
                        {
                            Process.Start(downloadFile);
                        }
                    }
                };

                AcadApp.ShowModelessDialog(dlg);
                wc.DownloadFileAsync(downloadUri, downloadFile);
            }
            catch
            {
            }
        }
    }
}
