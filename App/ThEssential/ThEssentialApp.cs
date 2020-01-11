using Autodesk.AutoCAD.Runtime;
using ThEssential.Command;

namespace ThEssential
{
    public class ThEssentialApp : IExtensionApplication
    {
        public void Initialize()
        {
            //
        }

        public void Terminate()
        {
            //
        }

        [CommandMethod("TIANHUACAD", "THQSELECT", CommandFlags.Modal)]
        public void ThQSelect()
        {
            using (var cmd = new ThQSelectCommand())
            {
                cmd.Execute();
            }
        }
    }
}
