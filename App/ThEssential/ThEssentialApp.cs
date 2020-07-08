using Autodesk.AutoCAD.Runtime;
using ThEssential.Command;
using ThEssential.LayerState;
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

        [CommandMethod("TIANHUACAD", "THTF", CommandFlags.Modal)]
        public void ThTF()
        {
            using (var cmd = new ThLayerStateCommand(State.VENTILATE))
            {
                cmd.Execute();
            }
        }

        [CommandMethod("TIANHUACAD", "THSG", CommandFlags.Modal)]
        public void ThSG()
        {
            using (var cmd = new ThLayerStateCommand(State.PIPE))
            {
                cmd.Execute();
            }
        }

        [CommandMethod("TIANHUACAD", "THXF", CommandFlags.Modal)]
        public void ThXF()
        {
            using (var cmd = new ThLayerStateCommand(State.EXTINGUISHMENT))
            {
                cmd.Execute();
            }
        }

        [CommandMethod("TIANHUACAD", "THNT", CommandFlags.Modal)]
        public void ThNT()
        {
            using (var cmd = new ThLayerStateCommand(State.ALL))
            {
                cmd.Execute();
            }
        }

    }
}
