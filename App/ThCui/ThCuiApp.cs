using System;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Autodesk.AutoCAD.Internal;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Ribbon;
using Autodesk.AutoCAD.Windows;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.PreferencesFiles;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
using TianHua.AutoCAD.Utility.ExtensionTools;
using ThIdentity.SDK;
using Linq2Acad;
using AcHelper;

namespace TianHua.AutoCAD.ThCui
{
    public class ThCuiApp : IExtensionApplication
    {
        ThPartialCui partialCui = new ThPartialCui();
        ThToolPalette toolPalette = new ThToolPalette();

        private readonly Dictionary<string, string> thcommanfunctiondict = new Dictionary<string, string>
        {
            // 图块图库
            {"THBLI", "图块集"},
            {"THBLS", "图块集配置"},
            {"THBEE", "提电气块转换"},
            {"THBBR", "插块断线"},
            {"THBBE", "选块断线"},
            {"THBBS", "全选断线"},
            
            // 标注工具
            {"THCNU", "车位编号"},
            {"THDTA", "尺寸避让"},
            
            // 图层工具
            {"THALC", "建立建筑图层"},
            {"THSLC", "建立结构图层"},
            {"THMLC", "建立暖通图层"},
            {"THELC", "建立电气图层"},
            {"THPLC", "建立给排水图层"},
            {"THLPM", "暖通用"},
            {"THLPE", "电气用"},
            {"THLPP", "给排水用"},
            {"THMLK", "锁定暖通图层"},
            {"THMUK", "隔离暖通图层"},
            {"THUKA", "解锁所有图层"},
            {"THMOF", "关闭暖通图层"},
            {"THMON", "开启暖通图层"},
            
            // 计算工具
            {"THBPS", "天华单体规整"},
            {"THSPS", "天华总体规整"},
            {"THBAC", "单体面积总汇"},
            {"THTET", "综合经济技术指标表"},
            {"THFET", "消防疏散表"},
            {"THABC", "房间面积框线"},

            // 平面绘图
            {"THSPC", "喷头布置"},

            // 文字表格
            {"THMTC", "文字内容刷"},
            
            // 辅助工具
            {"THQS",        "快选命令集"},
            {"THAL",        "对齐命令集"},
            {"THMSC",       "批量缩放"},
            {"THZ0",        "Z值归零"},
            {"DGNPURGE",    "DGN清理"},
            {"THBPT",       "批量打印PDF"},
            {"THBPD",       "批量打印DWF"},
            {"THBPP",       "批量打印PPT"},
            {"THSVM",       "版次信息修改"},
            {"THLTR",       "管线断线"},
            {"THMIR",       "文字块镜像"},

            // 第三方支持
            {"T20V4", "获取天正看图T20V4.0插件"},
            {"T20V5", "获取天正看图T20V5.0插件"},
        };

        public void Initialize()
        {
            //add code to run when the ExtApp initializes. Here are a few examples:
            //  Checking some host information like build #, a patch or a particular Arx/Dbx/Dll;
            //  Creating/Opening some files to use in the whole life of the assembly, e.g. logs;
            //  Adding some ribbon tabs, panels, and/or buttons, when necessary;
            //  Loading some dependents explicitly which are not taken care of automatically;
            //  Subscribing to some events which are important for the whole session;
            //  Etc.

            //注册命令
            RegisterCommands();

            //定制Preferences
            OverridePreferences(true);

            //复写打印机配置文件到Roaming目录
            OverwritePlotConfigurations();

            // CUI界面更新
            AcadApp.Idle += Application_OnIdle_Ribbon;
            AcadApp.Idle += Application_OnIdle_Menubar;

            //注册RibbonPaletteSet事件
            if (RibbonServices.RibbonPaletteSet == null)
            {
                // Ribbon未创建
                RibbonServices.RibbonPaletteSetCreated += RibbonPaletteSetCreated;
            }
            else
            {
                // Ribbon已创建
                RibbonServices.RibbonPaletteSet.StateChanged += RibbonPaletteSet_StateChanged;
            }

            //注册DocumentCollection事件
            AcadApp.DocumentManager.DocumentLockModeChanged += DocCollEvent_DocumentLockModeChanged_Handler;
            //注册SystemVariableChanged 事件
            AcadApp.SystemVariableChanged += SystemVariableChangedHandler;

#if DEBUG
            //  在装载模块时主动装载局部CUIX文件
            LoadPartialCui(true);
#endif
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

            //反注册RibbonPaletteSet事件
            if (RibbonServices.RibbonPaletteSet == null)
            {
                // Ribbon未创建状态
                // 反注册Ribbon创建事件
                RibbonServices.RibbonPaletteSetCreated -= RibbonPaletteSetCreated;
            }
            else
            {
                // Ribbon已创建
                // 反注册Ribbon状态改变事件
                RibbonServices.RibbonPaletteSet.StateChanged -= RibbonPaletteSet_StateChanged;
            }

            //反注册DocumentCollection事件
            AcadApp.DocumentManager.DocumentLockModeChanged -= DocCollEvent_DocumentLockModeChanged_Handler;
            //反注册SystemVariableChanged 事件
            AcadApp.SystemVariableChanged -= SystemVariableChangedHandler;

#if DEBUG
            //  在卸载模块时主动卸载局部CUIX文件
            LoadPartialCui(false);
#endif
        }

        private void Application_OnIdle_Cmd_Veto(object sender, EventArgs e)
        {
            AcadApp.Idle -= Application_OnIdle_Cmd_Veto;

            // 显示登陆提示
            Active.Editor.WriteLine("请先登陆后再使用天华CAD效率工具！");

            // 运行登陆命令
            ThCuiCmdHandler.ExecuteFromCommandLine(ThCuiCommon.CMD_THLOGIN_GLOBAL_NAME);
        }

        private void Application_OnIdle_Ribbon(object sender, EventArgs e)
        {
            // 使用AutoCAD Windows runtime API来配置自定义Tab中的Panels
            // 需要保证Ribbon自定义Tab是存在的，并且自定义Tab中的Panels也是存在的。
            if (ThRibbonUtils.Tab != null && ThRibbonUtils.Tab.Panels.Count != 0)
            {
                // 配置完成后就不需要Idle事件
                AcadApp.Idle -= Application_OnIdle_Ribbon;

                // 更新Ribbon
                UpdateRibbonUserInterface();

                // 更新Toolbar
                UpdateToolbarUserInterface();
            }
        }

        private void Application_OnIdle_Menubar(object sender, EventArgs e)
        {
            if (ThMenuBarUtils.PopupMenu != null)
            {
                // 配置完成后就不需要Idle事件
                AcadApp.Idle -= Application_OnIdle_Menubar;

                // 根据当前的登录信息配置菜单栏
                if (ThIdentityService.IsLogged())
                {
                    ThMenuBarUtils.EnableMenuItems();
                }
                else
                {
                    ThMenuBarUtils.DisableMenuItems();
                }
            }
        }

        /// <summary>
        /// 更新当前的Ribbon界面
        /// </summary>
        private void UpdateRibbonUserInterface()
        {
            // 根据当前的Profile配置Panels
            ThRibbonUtils.ConfigPanelsWithCurrentProfile();

            // 根据当前的登录信息配置Panels
            ThRibbonUtils.ConfigPanelsWithCurrentUser();
        }

        private void UpdateToolbarUserInterface()
        {
            // 根据当前的Profile配置Toolbars
            ThToolbarUtils.ConfigToolbarsWithCurrentProfile();

            // 根据当前的登录信息配置Toolbars
            ThToolbarUtils.ConfigToolbarsWithCurrentUser();
        }

        private void Application_OnIdle_RibbonPaletteSet(object sender, EventArgs e)
        {
            if (RibbonServices.RibbonPaletteSet != null)
            {
                AcadApp.Idle -= Application_OnIdle_RibbonPaletteSet;
                RibbonServices.RibbonPaletteSet.StateChanged -= RibbonPaletteSet_StateChanged;
            }
        }

        private void RibbonPaletteSetCreated(object sender, EventArgs e)
        {
            // 通过捕捉Ribbon创建事件，在Ribbon创建完成后监听Ribbon状态改变事件
            // 在这一刻，Ribbon未必完全创建完毕，这里通过Idle事件故意延后一段时间
            AcadApp.Idle += Application_OnIdle_RibbonPaletteSet;
            RibbonServices.RibbonPaletteSetCreated -= RibbonPaletteSetCreated;
        }

        private void RibbonPaletteSet_StateChanged(object sender, PaletteSetStateEventArgs e)
        {
            if (e.NewState == StateEventIndex.Show)
            {
                // 使用AutoCAD Windows runtime API来配置自定义Tab中的Panels
                // 需要保证Ribbon自定义Tab是存在的，并且自定义Tab中的Panels也是存在的。
                if (ThRibbonUtils.Tab != null && ThRibbonUtils.Tab.Panels.Count != 0)
                {
                    ThRibbonUtils.ConfigPanelsWithCurrentUser();
                }
            }
        }

        private void DocCollEvent_DocumentLockModeChanged_Handler(object sender, DocumentLockModeChangedEventArgs e)
        {
            if (!e.GlobalCommandName.StartsWith("#"))
            {
                // Lock状态，可以看做命令开始状态
                var cmdName = e.GlobalCommandName;

                // 过滤""命令
                // 通常发生在需要“显式”锁文档的场景中
                if (cmdName == "")
                {
                    return;
                }

                // 天华Lisp命令都是以“TH”开头
                // 特殊情况（C:THZ0）
                bool bVeto = false;
                if (Regex.Match(cmdName, @"^\([cC]:THZ0\)$").Success)
                {
                    bVeto = !ThIdentityService.IsLogged();
                }

                // 正常情况（C:THXXX）
                if (Regex.Match(cmdName, @"^\([cC]:TH[A-Z]{3,}\)$").Success)
                {
                    bVeto = !ThIdentityService.IsLogged();
                }

                // 天华ARX命令
                if (thcommanfunctiondict.ContainsKey(cmdName))
                {
                    // 在未登陆的情况下，不能运行
                    bVeto = !ThIdentityService.IsLogged();
                }

                // 
                if (bVeto)
                {
                    e.Veto();
                    AcadApp.Idle += Application_OnIdle_Cmd_Veto;
                }
            }
        }

        private void SystemVariableChangedHandler(object sender, SystemVariableChangedEventArgs e)
        {
            if (e.Name == "WSCURRENT")
            {
                // CUI界面更新
                AcadApp.Idle += Application_OnIdle_Ribbon;
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
            Utils.AddCommand(
                ThCuiCommon.CMD_GROUPNAME, 
                ThCuiCommon.CMD_THLOGIN_GLOBAL_NAME, 
                ThCuiCommon.CMD_THLOGIN_GLOBAL_NAME, 
                CommandFlags.Modal, 
                new CommandCallback(OnLogIn));
            //注册退出命令
            Utils.AddCommand(
                ThCuiCommon.CMD_GROUPNAME,
                ThCuiCommon.CMD_THLOGOUT_GLOBAL_NAME,
                ThCuiCommon.CMD_THLOGOUT_GLOBAL_NAME, 
                CommandFlags.Modal, 
                new CommandCallback(OnLogOut));

#if DEBUG
            Utils.AddCommand(
                ThCuiCommon.CMD_GROUPNAME,
                ThCuiCommon.CMD_THDUMPCUI_GLOBAL_NAME, 
                ThCuiCommon.CMD_THDUMPCUI_GLOBAL_NAME, 
                CommandFlags.Modal, 
                new CommandCallback(OnDumpCui));
#endif

            //注册帮组和命令
            Utils.AddCommand(
                ThCuiCommon.CMD_GROUPNAME,
                ThCuiCommon.CMD_THHLP_GLOBAL_NAME,
                ThCuiCommon.CMD_THHLP_GLOBAL_NAME, 
                CommandFlags.Modal, 
                new CommandCallback(OnHelp));

            //注册打开工具选项板配置命令
            Utils.AddCommand(
                ThCuiCommon.CMD_GROUPNAME,
                ThCuiCommon.CMD_THBLS_GLOBAL_NAME,
                ThCuiCommon.CMD_THBLS_GLOBAL_NAME, 
                CommandFlags.Modal, 
                new CommandCallback(ShowToolPaletteConfigDialog));

            //注册工具选项板开关命令
            Utils.AddCommand(
                ThCuiCommon.CMD_GROUPNAME,
                ThCuiCommon.CMD_THBLI_GLOBAL_NAME,
                ThCuiCommon.CMD_THBLI_GLOBAL_NAME, 
                CommandFlags.Modal, 
                new CommandCallback(toolPalette.ShowToolPalette));

            //注册下载T20天正插件命令
            Utils.AddCommand(
                ThCuiCommon.CMD_GROUPNAME, 
                ThCuiCommon.CMD_THT20PLUGINV4_GLOBAL_NAME,
                ThCuiCommon.CMD_THT20PLUGINV4_GLOBAL_NAME, 
                CommandFlags.Modal, 
                new CommandCallback(DownloadT20PlugInV4));
            Utils.AddCommand(
                ThCuiCommon.CMD_GROUPNAME,
                ThCuiCommon.CMD_THT20PLUGINV5_GLOBAL_NAME,
                ThCuiCommon.CMD_THT20PLUGINV5_GLOBAL_NAME, 
                CommandFlags.Modal, 
                new CommandCallback(DownloadT20PlugInV5));

            //专业切换
            Utils.AddCommand(
                ThCuiCommon.CMD_GROUPNAME,
                ThCuiCommon.CMD_THPROFILE_GLOBAL_NAME,
                ThCuiCommon.CMD_THPROFILE_GLOBAL_NAME,
                CommandFlags.Modal,
                new CommandCallback(OnSwitchProfile));
        }

        public void UnregisterCommands()
        {
            Utils.RemoveCommand(ThCuiCommon.CMD_GROUPNAME, ThCuiCommon.CMD_THLOGIN_GLOBAL_NAME);
            Utils.RemoveCommand(ThCuiCommon.CMD_GROUPNAME, ThCuiCommon.CMD_THLOGOUT_GLOBAL_NAME);
            Utils.RemoveCommand(ThCuiCommon.CMD_GROUPNAME, ThCuiCommon.CMD_THHLP_GLOBAL_NAME);
            Utils.RemoveCommand(ThCuiCommon.CMD_GROUPNAME, ThCuiCommon.CMD_THBLS_GLOBAL_NAME);
            Utils.RemoveCommand(ThCuiCommon.CMD_GROUPNAME, ThCuiCommon.CMD_THBLI_GLOBAL_NAME);
            Utils.RemoveCommand(ThCuiCommon.CMD_GROUPNAME, ThCuiCommon.CMD_THT20PLUGINV4_GLOBAL_NAME);
            Utils.RemoveCommand(ThCuiCommon.CMD_GROUPNAME, ThCuiCommon.CMD_THT20PLUGINV5_GLOBAL_NAME);
            Utils.RemoveCommand(ThCuiCommon.CMD_GROUPNAME, ThCuiCommon.CMD_THPROFILE_GLOBAL_NAME);
        }

        private void OverwritePlotConfigurations()
        { 
            if (Properties.Settings.Default.OverwritePlotConfigurations == true)
            {
                return;
            }

            string roaming = (string)AcadApp.GetSystemVariable("ROAMABLEROOTPREFIX");
            if (!string.IsNullOrEmpty(roaming))
            {
                var plotters = new DirectoryInfo(ThCADCommon.PlottersPath());
                foreach(var file in plotters.GetFiles("*.pc3", SearchOption.TopDirectoryOnly))
                {
                    file.CopyTo(Path.Combine(roaming, "Plotters", file.Name), true);
                }

                var printerDesc = new DirectoryInfo(ThCADCommon.PrinterDescPath());
                foreach(var file in printerDesc.GetFiles("*.pmp", SearchOption.TopDirectoryOnly))
                {
                    file.CopyTo(Path.Combine(roaming, "Plotters", "PMP Files", file.Name), true);
                }

                var printerStyleSheet = new DirectoryInfo(ThCADCommon.PrinterStyleSheetPath());
                foreach(var file in printerStyleSheet.GetFiles("*.ctb", SearchOption.TopDirectoryOnly))
                {
                    file.CopyTo(Path.Combine(roaming, "Plotters", "Plot Styles", file.Name), true);
                }

                //
                Properties.Settings.Default.OverwritePlotConfigurations = true;
                Properties.Settings.Default.Save();
            }
        }

        private void OverridePreferences(bool bOverride = true)
        {
            OverrideSupportPathPreferences(bOverride);
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
            }

            // 更新Ribbon
            if (ThIdentityService.IsLogged())
            {
                ThRibbonUtils.OpenAllPanels();
                ThToolbarUtils.OpenAllToolbars();
                ThMenuBarUtils.EnableMenuItems();
            }

            // 根据当前的Profile配置Panels
            ThRibbonUtils.ConfigPanelsWithCurrentProfile();
            // 根据当前的Profile配置Toolbars
            ThToolbarUtils.ConfigToolbarsWithCurrentProfile();
        }

        private void OnLogOut()
        {
            ThIdentityService.Logout();
            ThCuiProfileManager.Instance.Reset();

            // 更新Ribbon
            if (!ThIdentityService.IsLogged())
            {
                ThRibbonUtils.CloseAllPanels();
                ThToolbarUtils.CloseAllToolbars();
                ThMenuBarUtils.DisableMenuItems();
            }
        }

        private void OnHelp()
        {
            Process.Start(ThCADCommon.OnlineHelpUrl);
        }

        private void OnSwitchProfile()
        {
            // 指定专业
            PromptKeywordOptions keywordOptions = new PromptKeywordOptions("\n请指定专业：")
            {
                AllowNone = true
            };
            keywordOptions.Keywords.Add("ARCHITECTURE", "ARCHITECTURE", "建筑(A)");
            keywordOptions.Keywords.Add("STRUCTURE", "STRUCTURE", "结构(S)");
            keywordOptions.Keywords.Add("HAVC", "HAVC", "暖通(H)");
            keywordOptions.Keywords.Add("ELECTRICAL", "ELECTRICAL", "电气(E)");
            keywordOptions.Keywords.Add("WATER", "WATER", "给排水(W)");
            keywordOptions.Keywords.Add("PROJECT", "PROJECT", "方案(P)");
            keywordOptions.Keywords.Default = "ARCHITECTURE";
            PromptResult result = Active.Editor.GetKeywords(keywordOptions);
            if (result.Status != PromptStatus.OK)
            {
                return;
            }

            switch(result.StringResult)
            {
                case "ARCHITECTURE":
                    {
                        ThCuiProfileManager.Instance.CurrentProfile = Profile.ARCHITECTURE;
                    }
                    break;
                case "STRUCTURE":
                    {
                        ThCuiProfileManager.Instance.CurrentProfile = Profile.STRUCTURE;
                    }
                    break;
                case "HAVC":
                    {
                        ThCuiProfileManager.Instance.CurrentProfile = Profile.HAVC;
                    }
                    break;
                case "ELECTRICAL":
                    {
                        ThCuiProfileManager.Instance.CurrentProfile = Profile.ELECTRICAL;
                    }
                    break;
                case "WATER":
                    {
                        ThCuiProfileManager.Instance.CurrentProfile = Profile.WSS;
                    }
                    break;
                case "PROJECT":
                    {
                        ThCuiProfileManager.Instance.CurrentProfile = Profile.PROJECTPLAN;
                    }
                    break;
                default:
                    return;
            }

            ThRibbonUtils.ConfigPanelsWithCurrentProfile();
            ThToolbarUtils.ConfigToolbarsWithCurrentProfile();
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
