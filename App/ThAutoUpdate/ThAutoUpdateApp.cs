using System;
using NetSparkle;
using Microsoft.Win32;
using System.Reflection;
using Autodesk.AutoCAD.Runtime;
using Microsoft.VisualBasic.Devices;
using Win32RegistryKey = Microsoft.Win32.RegistryKey;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace ThAutoUpdate
{
    public class ThAutoUpdateApp : IExtensionApplication
    {
        private Sparkle AutoUpdater { get; set; }
        public void Initialize()
        {
            AcadApp.Idle += Application_OnIdle_Sparkle;
        }

        public void Terminate()
        {
            AutoUpdater.StopLoop();
        }

        private void Application_OnIdle_Sparkle(object sender, EventArgs e)
        {
            AcadApp.Idle -= Application_OnIdle_Sparkle;
            AutoUpdater = ThAutoUpdateUtils.CreateSparkle();
            AutoUpdater.StartLoop(true, true);
        }
    }

    public class ThAutoUpdateCommand
    {
        [CommandMethod("TIANHUACAD", "THENV", CommandFlags.Modal)]
        public void Environemnt()
        {
            GetComputerInfo();
            Get45PlusFromRegistry();
            GetSecurityUpdates();
        }

        // References:
        //  https://docs.microsoft.com/en-us/dotnet/framework/migration-guide/how-to-determine-which-versions-are-installed
        private void Get45PlusFromRegistry()
        {
            const string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";

            using (var ndpKey = Win32RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subkey))
            {
                var ed = AcadApp.DocumentManager.MdiActiveDocument.Editor;

                if (ndpKey != null && ndpKey.GetValue("Release") != null)
                {
                    ed.WriteMessage($"\n.NET Framework Version: {CheckFor45PlusVersion((int)ndpKey.GetValue("Release"))}");
                }
                else
                {
                    ed.WriteMessage("\n.NET Framework Version 4.5 or later is not detected.");
                }
            }

            // Checking the version using >= enables forward compatibility.
            string CheckFor45PlusVersion(int releaseKey)
            {
                if (releaseKey >= 528040)
                    return "4.8 or later";
                if (releaseKey >= 461808)
                    return "4.7.2";
                if (releaseKey >= 461308)
                    return "4.7.1";
                if (releaseKey >= 460798)
                    return "4.7";
                if (releaseKey >= 394802)
                    return "4.6.2";
                if (releaseKey >= 394254)
                    return "4.6.1";
                if (releaseKey >= 393295)
                    return "4.6";
                if (releaseKey >= 379893)
                    return "4.5.2";
                if (releaseKey >= 378675)
                    return "4.5.1";
                if (releaseKey >= 378389)
                    return "4.5";
                // This code should never execute. A non-null release key should mean
                // that 4.5 or later is installed.
                return "No 4.5 or later version detected";
            }
        }

        // References:
        //  https://docs.microsoft.com/en-us/dotnet/framework/migration-guide/how-to-determine-which-net-framework-updates-are-installed
        private void GetSecurityUpdates()
        {
            const string subkey = @"SOFTWARE\Microsoft\Updates";

            using (Win32RegistryKey baseKey = Win32RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subkey))
            {
                var ed = AcadApp.DocumentManager.MdiActiveDocument.Editor;

                foreach (string baseKeyName in baseKey.GetSubKeyNames())
                {
                    if (baseKeyName.Contains(".NET Framework"))
                    {
                        using (Win32RegistryKey updateKey = baseKey.OpenSubKey(baseKeyName))
                        {
                            ed.WriteMessage("\n" + baseKeyName);
                            foreach (string kbKeyName in updateKey.GetSubKeyNames())
                            {
                                using (Win32RegistryKey kbKey = updateKey.OpenSubKey(kbKeyName))
                                {
                                    ed.WriteMessage("\n  " + kbKeyName);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void GetComputerInfo()
        {
            var ed = AcadApp.DocumentManager.MdiActiveDocument.Editor;

            ed.WriteMessage($"\nOS Version: {OSVersion()}");
            ed.WriteMessage($"\nOS Name: {OSName()}");
            ed.WriteMessage($"\nOS Platform : {OSPlatform()}");

            string OSVersion()
            {
                return new ComputerInfo().OSVersion;
            }

            string OSName()
            {
                return new ComputerInfo().OSFullName;
            }

            string OSPlatform()
            {
                return new ComputerInfo().OSPlatform;
            }
        }

        [CommandMethod("TIANHUACAD", "THVER", CommandFlags.Modal)]
        public void Version()
        {
            var ed = AcadApp.DocumentManager.MdiActiveDocument.Editor;
            ed.WriteMessage(Assembly.GetExecutingAssembly().GetName().Version.ToString());
        }
    }
}
