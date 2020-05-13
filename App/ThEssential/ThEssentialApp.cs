using Autodesk.AutoCAD.Runtime;
using ThEssential.Command;
using ThEssential.BlockConvert;

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

        [CommandMethod("TIANHUACAD", "THQS", CommandFlags.Modal | CommandFlags.UsePickSet | CommandFlags.Redraw)]
        public void ThQSelect()
        {
            using (var cmd = new ThQSelectCommand())
            {
                cmd.Execute();
            }
        }

        [CommandMethod("TIANHUACAD", "THAL", CommandFlags.Modal | CommandFlags.UsePickSet)]
        public void ThAlign()
        {
            using (var cmd = new ThAlignCommand())
            {
                cmd.Execute();
            }
        }

        [CommandMethod("TIANHUACAD", "THMA", CommandFlags.Modal | CommandFlags.UsePickSet)]
        public void ThMatchProps()
        {
            using (var cmd = new ThMatchPropsCommand())
            {
                cmd.Execute();
            }
        }

        [CommandMethod("TIANHUACAD", "THCO", CommandFlags.Modal | CommandFlags.UsePickSet)]
        public void ThCopy()
        {
            using (var cmd = new ThCopyCommand())
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

        [CommandMethod("TIANHUACAD", "THOVERKILL", CommandFlags.Modal)]
        public void THOverkill()
        {
            using (var cmd = new ThOverkillCommand())
            {
                cmd.Execute();
            }
        }

        [CommandMethod("TIANHUACAD", "THPBE", CommandFlags.Modal)]
        public void ThStrongCurrentBlockConvert()
        {
            using (var cmd = new ThBConvertCommand(ConvertMode.STRONGCURRENT))
            {
                cmd.Execute();
            }
        }

        [CommandMethod("TIANHUACAD", "THLBE", CommandFlags.Modal)]
        public void ThWeakCurrentBlockConvert()
        {
            using (var cmd = new ThBConvertCommand(ConvertMode.WEAKCURRENT))
            {
                cmd.Execute();
            }
        }
    }
}
