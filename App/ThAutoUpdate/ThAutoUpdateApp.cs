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
        private const string DSAPublicKey = @"MIHwMIGoBgcqhkjOOAQBMIGcAkEA08wtBHSUzHt0h42o5voEBzP8BhJYJYHx/JuG
zUTiQJJFRjxI+QruKoUwjEEtMUz/XRMmuQcWdD8qvfnqGxOsAwIVAL5nXeAX3D3g
n0bKai8azB2OHGAdAkAGze0FcM1cE2E14Ovz+BisiASwCEk8MZEGypHOLCJYyEVm
vRomftZyaBXaG6rtVWsAgBE5xIMC5Hge0c/FmwBBA0MAAkA9+dxLIe3MoSiDIk8z
mzY1oPM5Zml49MqI79NDTKA2VVniG2n20YJb7ldi4SMEm1FlI2ysseSzGrm07Ua/
ioNu";

        [CommandMethod("TIANHUACAD", "THAUU", CommandFlags.Modal)]
        public void AutoUpdate()
        {
            string assembly = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
            Sparkle sparkle = new Sparkle(AppcastUrl, null, NetSparkle.Enums.SecurityMode.UseIfPossible, DSAPublicKey, assembly);
            sparkle.CloseApplication += (() =>
            {
                AcadApp.DocumentManager.MdiActiveDocument.SendStringToExecute("quit ", true, false, true);
            });

            sparkle.CheckForUpdatesAtUserRequest();
        }
    }
}
