using System.IO;
using System.Reflection;
using NetSparkle;
using Microsoft.Win32;
using Microsoft.VisualBasic.Devices;
using Autodesk.AutoCAD.Runtime;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace ThAutoUpdate
{
    public class ThAutoUpdateApp : IExtensionApplication
    {
        public void Initialize()
        {
        }

        public void Terminate()
        {
        }
    }


    public class ThAutoUpdateCommand
    {
        private const string AppcastUrl = "http://49.234.60.227/AI/thcad/appcast.xml";

        [CommandMethod("TIANHUACAD", "THAUU", CommandFlags.Modal)]
        public void AutoUpdate()
        {
            string assembly = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
            Sparkle sparkle = new Sparkle(AppcastUrl, ThAutoUpdate.Resource.AppIcon, NetSparkle.Enums.SecurityMode.Strict, null, assembly);
            sparkle.CloseApplication += () =>
            {
#if ACAD2012
                AcadApp.Quit();
#else
                AcadApp.DocumentManager.MdiActiveDocument.SendStringToExecute("quit ", true, false, true);
#endif
            };

            sparkle.CheckForUpdatesAtUserRequest();
        }

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

            using (var ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subkey))
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

            using (RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subkey))
            {
                var ed = AcadApp.DocumentManager.MdiActiveDocument.Editor;

                foreach (string baseKeyName in baseKey.GetSubKeyNames())
                {
                    if (baseKeyName.Contains(".NET Framework"))
                    {
                        using (RegistryKey updateKey = baseKey.OpenSubKey(baseKeyName))
                        {
                            ed.WriteMessage("\n" + baseKeyName);
                            foreach (string kbKeyName in updateKey.GetSubKeyNames())
                            {
                                using (RegistryKey kbKey = updateKey.OpenSubKey(kbKeyName))
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
