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
            //关闭所有图层
            using (var db = AcadDatabase.Active())
            {
                var layers = db.Layers; ;
                foreach (var layer in layers)
                {
                    //layerNames.Add(layer.Name);
                }
            }

            //打开需要图层

            

            // 矩形框裁剪


        }
    }
}
