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
using ThLicense;

namespace TianHua.AutoCAD.ThCui
{
    public class ThCuiApp : IExtensionApplication
    {
        const string CMD_GROUPNAME = "TIANHUACAD";
        const string CMD_THCADLOGIN_GLOBAL_NAME = "THCADLOGIN";
        const string CMD_THCADLOGOUT_GLOBAL_NAME = "THCADLOGOUT";
        const string CMD_THT20PLUGINV4_GLOBAL_NAME = "T20V4";
        const string CMD_THT20PLUGINV5_GLOBAL_NAME = "T20V5";
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
            OverrideSupportPathPreferences(bOverride);
            OverridePrinterPathPreferences(bOverride);
        }

        private void OverridePrinterPathPreferences(bool bOverride = true)
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
#else
            // 在CAD 2012和2014中，通过上面修改Preferences搜索路径的方式不能成功
            // 这里采用一个变通办法，即把相应的文件拷贝到Roamable路径中 
            var roamableRootPath = (string)AcadApp.GetSystemVariable("ROAMABLEROOTPREFIX");
            var roamableRootPlottersPath = Path.Combine(roamableRootPath, "Plotters");
            foreach (var file in Directory.GetFiles(ThCADCommon.PlottersPath(), "*.pc3"))
            {
                try
                {
                    var plotter = Path.GetFileName(file);
                    var plotterPath = Path.Combine(roamableRootPlottersPath, plotter);
                    if (bOverride)
                    {
                        File.Copy(file, plotterPath, true);
                    }
                }
                catch
                {
                    //
                }
            }

            foreach(var file in Directory.GetFiles(ThCADCommon.PrinterDescPath(), "*.pmp"))
            {
                try
                {
                    var printerDesc = Path.GetFileName(file);
                    var printerDescPath = Path.Combine(roamableRootPlottersPath, printerDesc);
                    if (bOverride)
                    {
                        File.Copy(file, printerDescPath, true);
                    }
                }
                catch
                {
                    //
                }
            }

            foreach(var file in Directory.GetFiles(ThCADCommon.PrinterStyleSheetPath(), "*.ctb"))
            {
                try
                {
                    var printerStyleSheet = Path.GetFileName(file);
                    var printerStyleSheetPath = Path.Combine(roamableRootPlottersPath, printerStyleSheet);
                    if (bOverride && !File.Exists(printerStyleSheetPath))
                    {
                        File.Copy(file, printerStyleSheetPath);
                    }
                }
                catch
                {
                    //
                }
            }
#endif
        }

        private void OverrideSupportPathPreferences(bool bOverride = true)
        {
            // ATC文件中的"SourceFile"指向一个图纸，这个图纸提供了ATC文件中的块定义
            // 为了能正确的正确的部署ATC文件以及其引用的图纸，不建议为"SourceFile"指定一个绝对路径
            // 一个更好的做法是：
            //  1. 在"SourceFile"中只指定图纸名
            //  2. 将图纸的路径添加到"支持文件搜索路径"中
            // 剩下的就交给CAD去根据"支持文件搜索路径"指定的路径来寻找图纸
            // https://knowledge.autodesk.com/support/autocad/troubleshooting/caas/sfdcarticles/sfdcarticles/Using-Source-File-field-for-tool-palette-block-data.html
            var supportPath = new SupportPath(AcadApp.Preferences);
            foreach (var item in new string[] { "电气", "给排水", "暖通" })
            {
                var path = Path.Combine(ThCADCommon.SupportPath(), "ToolPalette", item);
                if (bOverride && !supportPath.Contains(path))
                {
                    supportPath.Add(path);
                }
                else if (!bOverride && supportPath.Contains(path))
                {
                    supportPath.Remove(path);
                }
            }
            supportPath.SaveChanges();
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
