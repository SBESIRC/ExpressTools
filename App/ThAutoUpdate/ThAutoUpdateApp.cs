using System.IO;
using System.Reflection;
using NetSparkle;
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
        private const string AppcastUrl = "http://118.89.179.187/appcast.xml";

        [CommandMethod("TIANHUACAD", "THAUU", CommandFlags.Modal)]
        public void AutoUpdate()
        {
            string assembly = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
            Sparkle sparkle = new Sparkle(AppcastUrl, null, NetSparkle.Enums.SecurityMode.Strict, null, assembly);
            sparkle.CloseApplication += () =>
            {
                AcadApp.DocumentManager.MdiActiveDocument.SendStringToExecute("quit ", true, false, true);
            };

            sparkle.CheckForUpdatesAtUserRequest();
        }
    }
}
