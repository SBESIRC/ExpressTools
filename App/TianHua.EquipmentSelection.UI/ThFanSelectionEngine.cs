using AcHelper;
using Linq2Acad;
using System.Linq;
using DotNetARX;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;

namespace TianHua.FanSelection.UI
{
    public static class ThFanSelectionEngine
    {
        private static string CurrentModel { get; set; }
        private static int CurrentModelNumber { get; set; }

        public static void InsertModels(FanDataModel dataModel)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                // 选取插入点
                PromptPointResult pr = Active.Editor.GetPoint("\n请输入插入点: ");
                if (pr.Status != PromptStatus.OK)
                    return;

                // 若检测到图纸中没有对应的风机图块，则在鼠标的点击处插入风机
                var blockName = ThFanSelectionUtils.BlockName(dataModel.VentStyle);
                Active.Database.ImportModel(blockName);
                var objId = Active.Database.InsertModel(blockName, dataModel.Attributes());
                var blockRef = acadDatabase.Element<BlockReference>(objId);
                for (int i = 0; i < dataModel.VentQuan; i++)
                {
                    double deltaX = blockRef.GeometricExtents.Width() * 2 * i;
                    Vector3d delta = new Vector3d(deltaX, 0.0, 0.0);
                    Matrix3d displacement = Matrix3d.Displacement(pr.Value.GetAsVector() + delta);
                    var model = acadDatabase.ModelSpace.Add(blockRef.GetTransformedCopy(displacement));
                    model.AttachModel(dataModel.ID, dataModel.ListVentQuan[i]);
                    model.SetModelName(dataModel.FanModelName);
                }

                // 删除初始图块
                blockRef.UpgradeOpen();
                blockRef.Erase();
            }
        }

        public static void RemoveModels(FanDataModel dataModel)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var models = acadDatabase.ModelSpace
                    .OfType<BlockReference>()
                    .Where(o => o.ObjectId.IsModel(dataModel.ID));
                foreach(var model in models)
                {
                    model.UpgradeOpen();
                    model.Erase();
                }
            }
        }

        public static void ModifyModels(FanDataModel dataModel)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var models = acadDatabase.ModelSpace
                    .OfType<BlockReference>()
                    .Where(o => o.ObjectId.IsModel(dataModel.ID));
                foreach (var model in models.Select((value, i) => new { i, value }))
                {
                    // 更新属性值
                    model.value.ObjectId.ModifyModelAttributes(dataModel.Attributes());

                    // 更新编号
                    model.value.ObjectId.SetModelNumber(dataModel.ListVentQuan[model.i]);
                }
            }
        }

        public static void ZoomToModels(FanDataModel dataModel)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var blockReferences = acadDatabase.ModelSpace
                    .OfType<BlockReference>()
                    .Where(o => o.ObjectId.IsModel(dataModel.ID));
                if (blockReferences.Any())
                {
                    return;
                }
                if (CurrentModel == dataModel.ID)
                {
                    var models = blockReferences.Where(o => o.ObjectId.GetModelNumber() > CurrentModelNumber);
                    if (models.Any())
                    {
                        // 找到第一个比当前编号大的图块
                        CurrentModelNumber = models.First().ObjectId.GetModelNumber();
                        ViewTableTools.ZoomObject(Active.Editor, models.First().ObjectId);
                    }
                    else
                    {
                        // 未找到第一个比当前编号大的图块，回到第一个图块
                        CurrentModelNumber = blockReferences.First().ObjectId.GetModelNumber();
                        ViewTableTools.ZoomObject(Active.Editor, blockReferences.First().ObjectId);
                    }
                }
                else
                {
                    CurrentModel = dataModel.ID;
                    CurrentModelNumber = blockReferences.First().ObjectId.GetModelNumber();
                    ViewTableTools.ZoomObject(Active.Editor, blockReferences.First().ObjectId);
                }
            }
        }
    }
}
