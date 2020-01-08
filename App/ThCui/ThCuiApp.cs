﻿using System;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Windows.Forms;
using Autodesk.Windows;
using Autodesk.AutoCAD.Internal;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices.PreferencesFiles;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
using TianHua.AutoCAD.Utility.ExtensionTools;
using ThIdentity.SDK;

namespace TianHua.AutoCAD.ThCui
{
    public class ThCuiApp : IExtensionApplication
    {
        const string CMD_GROUPNAME = "TIANHUACAD";
        const string CMD_THCADLOGIN_GLOBAL_NAME = "THCADLOGIN";
        const string CMD_THCADLOGOUT_GLOBAL_NAME = "THCADLOGOUT";
        const string CMD_THCADUMPCUI_GLOBAL_NAME = "THCADDUMPCUI";
        const string CMD_THT20PLUGINV4_GLOBAL_NAME = "T20V4";
        const string CMD_THT20PLUGINV5_GLOBAL_NAME = "T20V5";
        const string CMD_THHLP_GLOBAL_NAME = "THHLP";
        const string CMD_THBLS_GLOBAL_NAME = "THBLS";
        const string CMD_THBLI_GLOBAL_NAME = "THBLI";

        ThPartialCui partialCui = new ThPartialCui();
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
                //注册命令
                RegisterCommands();

                //定制Preferences
                OverridePreferences(true);

#if DEBUG
                //  在装载模块时主动装载局部CUIX文件
                LoadPartialCui(true);
#endif

                AcadApp.Idle += Application_OnIdle;
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

#if DEBUG
            //  在卸载模块时主动卸载局部CUIX文件
            LoadPartialCui(false);
#endif
        }

        private void Application_OnIdle(object sender, EventArgs e)
        {
            // 使用AutoCAD Windows runtime API来配置自定义Tab中的Panels
            // 需要保证Ribbon自定义Tab是存在的，并且自定义Tab中的Panels也是存在的。
            if (ThRibbonUtils.Tab != null && ThRibbonUtils.Tab.Panels.Count != 0)
            {
                // 配置完成后就不需要Idle事件
                AcadApp.Idle -= Application_OnIdle;

                // 根据当前的登录信息配置Panels
                if (ThIdentityService.IsLogged())
                {
                    ThRibbonUtils.OpenAllPanels();
                }
                else
                {
                    ThRibbonUtils.CloseAllPanels();
                }
            }
        }

        private void LoadPartialCui(bool bLoad = true)
        {
            if (bLoad)
            {
                partialCui.Load(Path.Combine(ThCADCommon.ResourcesPath(), ThCADCommon.CuixFile), ThCADCommon.CuixMenuGroup);
            }
            else
            {
                partialCui.UnLoad(Path.Combine(ThCADCommon.ResourcesPath(), ThCADCommon.CuixFile), ThCADCommon.CuixMenuGroup);
            }
        }

        private void CreatePartialCui()
        {
            partialCui.Create(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), ThCADCommon.CuixFile), ThCADCommon.CuixMenuGroup);
        }

        public void RegisterCommands()
        {
            //注册登录命令
            Utils.AddCommand(CMD_GROUPNAME, CMD_THCADLOGIN_GLOBAL_NAME, CMD_THCADLOGIN_GLOBAL_NAME, CommandFlags.Modal, new CommandCallback(OnLogIn));
            //注册退出命令
            Utils.AddCommand(CMD_GROUPNAME, CMD_THCADLOGOUT_GLOBAL_NAME, CMD_THCADLOGOUT_GLOBAL_NAME, CommandFlags.Modal, new CommandCallback(OnLogOut));

#if DEBUG
            Utils.AddCommand(CMD_GROUPNAME, CMD_THCADUMPCUI_GLOBAL_NAME, CMD_THCADUMPCUI_GLOBAL_NAME, CommandFlags.Modal, new CommandCallback(OnDumpCui));
#endif

            //注册帮组和命令
            Utils.AddCommand(CMD_GROUPNAME, CMD_THHLP_GLOBAL_NAME, CMD_THHLP_GLOBAL_NAME, CommandFlags.Modal, new CommandCallback(OnHelp));

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

#if DEBUG
        private void OnDumpCui()
        {
            CreatePartialCui();
        }
#endif

        private void OnLogIn()
        {
            using (var dlg = new ThLoginDlg())
            {
                if (AcadApp.ShowModalDialog(dlg) != DialogResult.OK)
                {
                    return;
                }

                ThIdentityService.Login(dlg.User, dlg.Password);
            }

            // 更新Ribbon
            if (ThIdentityService.IsLogged())
            {
                ThRibbonUtils.OpenAllPanels();
            }

        }

        private void OnLogOut()
        {
            ThIdentityService.Logout();

            // 更新Ribbon
            if (!ThIdentityService.IsLogged())
            {
                ThRibbonUtils.CloseAllPanels();
            }
        }

        private void OnHelp()
        {
            Process.Start(ThCADCommon.OnlineHelpUrl);
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
