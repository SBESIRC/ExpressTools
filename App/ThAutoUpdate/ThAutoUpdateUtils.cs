using System.IO;
using System.Reflection;
using NetSparkle;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace ThAutoUpdate
{
    public class ThAutoUpdateUtils
    {
        private const string AppcastUrl = "http://49.234.60.227/AI/thcad/appcast.xml";

        public static Sparkle CreateSparkle()
        {
            string assembly = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
            // Update the current directory to locate the assmebly itself and the public DSA key.
            // It likes the behavior of NETLOAD to get the directory where the assembly was loaded.
            //  https://through-the-interface.typepad.com/through_the_interface/2007/12/getting-autocad.html
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            Sparkle sparkle = new Sparkle(AppcastUrl, ThAutoUpdate.Resource.AppIcon, NetSparkle.Enums.SecurityMode.Strict, null, assembly);
            //set sparkle not allow skip
            sparkle.HideSkipButton = true;
            sparkle.CloseApplication += () =>
            {
#if ACAD2012 || ACAD2014
                AcadApp.Quit();
#else
                AcadApp.DocumentManager.MdiActiveDocument.SendStringToExecute("quit ", true, false, true);
#endif
            };

            return sparkle;
        }
    }
}
