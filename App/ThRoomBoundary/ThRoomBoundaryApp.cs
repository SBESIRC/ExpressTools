using Linq2Acad;
using DotNetARX;
using AcHelper;
using System.Linq;
using Autodesk.AutoCAD.Runtime;


namespace ThRoomBoundary
{
    public class ThRoomBoundaryApp : IExtensionApplication
    {
        public void Initialize()
        {
        }

        public void Terminate()
        {
        }

        [CommandMethod("TIANHUACAD", "THABC", CommandFlags.Modal)]
        static public void RoomBoundaryCalculate()
        {
            //using (AcadDatabase acadDatabase = AcadDatabase.Active())
            //{
            //    //acadDatabase.Layers.w
            //}
        }
    }
}
