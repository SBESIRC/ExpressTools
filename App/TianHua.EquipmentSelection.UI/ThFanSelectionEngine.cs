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
                    model.SetModelIdentifier(dataModel.ID, dataModel.ListVentQuan[i]);
                    UpdateModelName(model, dataModel);
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

                    // 更新风机型号
                    UpdateModelName(model.value.ObjectId, dataModel);
                }
            }
        }

        private static void UpdateModelName(ObjectId model, FanDataModel dataModel)
        {
            if (dataModel.VentStyle.Contains(ThFanSelectionCommon.AXIAL_BLOCK_NAME))
            {
                model.SetModelName(model.AXIALModelName(dataModel.FanModelName));
            }
            else
            {
                model.SetModelName(model.HTFCModelName(dataModel.FanModelNum, dataModel.IntakeForm));
            }
        }

        public static void ZoomToModels(FanDataModel dataModel)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var blockReferences = acadDatabase.ModelSpace
                    .OfType<BlockReference>()
                    .Where(o => o.ObjectId.IsModel(dataModel.ID));
                if (!blockReferences.Any())
                {
                    return;
                }
                if (CurrentModel == dataModel.ID)
                {
                    var models = blockReferences.Where(o => o.ObjectId.GetModelNumber() > CurrentModelNumber).ToList();
                    if (models.Count > 0)
                    {
                        // 找到第一个比当前编号大的图块
                        CurrentModelNumber = models[0].ObjectId.GetModelNumber();
                        Active.Editor.ZoomToModel(models[0].ObjectId, 3);
                    }
                    else
                    {
                        // 未找到一个比当前编号大的图块，回到第一个图块
                        CurrentModelNumber = blockReferences.First().ObjectId.GetModelNumber();
                        Active.Editor.ZoomToModel(blockReferences.First().ObjectId, 3);
                    }
                }
                else
                {
                    CurrentModel = dataModel.ID;
                    CurrentModelNumber = blockReferences.First().ObjectId.GetModelNumber();
                    Active.Editor.ZoomToModel(blockReferences.First().ObjectId, 3);
                }
            }
        }
    }
}
