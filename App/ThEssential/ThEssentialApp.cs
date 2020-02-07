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

        [CommandMethod("TIANHUACAD", "THALIGN", CommandFlags.Modal | CommandFlags.UsePickSet)]
        public void ThAlign()
        {
            using (var cmd = new ThAlignCommand())
            {
                cmd.Execute();
            }
        }

        [CommandMethod("TIANHUACAD", "THMATCHPROPS", CommandFlags.Modal | CommandFlags.UsePickSet)]
        public void ThMatchProps()
        {
            using (var cmd = new ThMatchPropsCommand())
            {
                cmd.Execute();
            }
        }

        [CommandMethod("TIANHUACAD", "THEQUIPMENT", CommandFlags.Modal)]
        public void THEquipment()
        {
            using (var cmd = new ThEquipmentCommand())
            {
                cmd.Execute();
            }
        }
    }
}
