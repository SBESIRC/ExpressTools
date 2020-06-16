using System.IO;
using System.Windows.Forms;
using Autodesk.AutoCAD.Runtime;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace TianHua.CAD.QuickCmd
{
    public class ThQuickCmdApp : IExtensionApplication
    {
        public void Initialize()
        {
            //
        }

        public void Terminate()
        {
            //
        }

        [CommandMethod("TIANHUACAD", "THALIAS", CommandFlags.Modal)]
        public void ThAlias()
        {
            using (fmQuickCmd _fmQuickCmd = new fmQuickCmd())
            {
                string roaming = (string)AcadApp.GetSystemVariable("ROAMABLEROOTPREFIX");
                _fmQuickCmd.m_PgpPath = Path.Combine(roaming, "Support", "acad.pgp");
                _fmQuickCmd.m_Profession = ThQuickCmdUtils.Profile();
                var result = AcadApp.ShowModalDialog(_fmQuickCmd);
                if (result == DialogResult.OK)
                {
                    AcadApp.SetSystemVariable("RE-INIT", 16);
                }
            }
        }
    }
}
