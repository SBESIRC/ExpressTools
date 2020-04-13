using System;
using AcHelper.Commands;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Linq2Acad;
using AcHelper;
using Dreambuild.AutoCAD;
using ThEssential.Equipment;

namespace ThEssential.Command
{
    public class ThEquipmentCommand : IAcadCommand, IDisposable
    {
        public void Dispose()
        {
        }

        public void Execute()
        {
            // 根据流量值q，扬程h，计算设备型号
            double qx = Interaction.GetValue("请输入流量：", 1000);
            double hx = Interaction.GetValue("请输入扬程：", 15);

            // 选择设备选型文件
            string file = Interaction.OpenFileDialogBySystem("选择设备文件", null, "(*.dwg)|*.dwg");
            if (file.IsNullOrEmpty())
            {
                return;
            }

            // 打开设备选型图
            using (AcadDatabase acadDatabase = AcadDatabase.Open(file, DwgOpenMode.ReadOnly))
            {
                // 根据指定的流量和扬程，计算目标设备区域
                var coordinate = acadDatabase.Database.EquipmentCoordinateSystem();
                ThAnchorPoint target = coordinate.Target(qx, hx);

                // 根据目标设备区域，提取目标设备编号
                var models = acadDatabase.Database.Model(target);

                // 显示目标设备编号
                foreach (ObjectId model in models)
                {
                    Active.Editor.WriteLine(acadDatabase.Database.Model(model));
                }
            }
        }
    }
}
