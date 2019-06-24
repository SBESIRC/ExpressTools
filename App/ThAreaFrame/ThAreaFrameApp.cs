using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using AcHelper;
using AcHelper.Wrappers;

[assembly: CommandClass(typeof(ThAreaFrame.ThAreaFrameCommands))]
[assembly: ExtensionApplication(typeof(ThAreaFrame.ThAreaFrameApp))]

namespace ThAreaFrame
{
    public class ThAreaFrameApp : IExtensionApplication
    {
        public void Initialize()
        {
        }

        public void Terminate()
        {
        }
    }

    public class ThAreaFrameCommands
    {
        [CommandMethod("HELLO", CommandFlags.Modal)]
        public void HelloCommand()
        {
            var ed = Active.Editor;

            // 选取插入点
            PromptPointResult pr = ed.GetPoint("\n请输入表格插入点: ");
            if (pr.Status != PromptStatus.OK)
                return;

            // 创建面积引擎
            ThAreaFrameEngine engine = ThAreaFrameEngine.ResidentialEngine();

            // 创建表单
            Point3d position = pr.Value.TransformBy(ed.CurrentUserCoordinateSystem);
            ObjectId id = ThAreaFrameTableBuilder.CreateBuildingAreaTable(position);
            using (AcTransaction tr = new AcTransaction())
            {
                Table table = (Table)tr.Transaction.GetObject(id, OpenMode.ForRead);
                using (new WriteEnabler(table))
                {
                    int column = 0;
                    int dataRow = (table.Rows.Count - 1);
                    int headerRow = (table.Rows.Count - 2);

                    // 建筑编号
                    table.Cells[dataRow, column++].Value = engine.Name();

                    // 普通楼层面积
                    var ordinaryStoreys = engine.Building.OrdinaryStoreys();
                    if (ordinaryStoreys.Count > 0)
                    {
                        table.InsertColumns(column, ThAreaFrameTableBuilder.ColumnWidth, ordinaryStoreys.Count);
                        foreach (ResidentialStorey storey in ordinaryStoreys)
                        {
                            table.Cells[headerRow, column].Alignment = CellAlignment.MiddleCenter;
                            table.Cells[headerRow, column].TextHeight = ThAreaFrameTableBuilder.TextHeight;
                            table.Cells[headerRow, column].Value = ThAreaFrameTableBuilder.OrdinaryStoreyColumnHeader(storey.number);

                            table.Cells[dataRow, column].Alignment = CellAlignment.MiddleCenter;
                            table.Cells[dataRow, column].TextHeight = ThAreaFrameTableBuilder.TextHeight;
                            table.Cells[dataRow, column].SetAreaValue(engine.AreaOfFloor(storey.number)); 

                            column++;
                        }
                    }

                    // 标准楼层
                    var areaOfStandardStoreys = engine.AreaOfStandardStoreys();
                    if (areaOfStandardStoreys.Count > 0)
                    {
                        table.InsertColumns(column, ThAreaFrameTableBuilder.ColumnWidth, areaOfStandardStoreys.Count);
                        for (int i = 0; i < areaOfStandardStoreys.Count; i++)
                        {
                            table.Cells[headerRow, column].Alignment = CellAlignment.MiddleCenter;
                            table.Cells[headerRow, column].TextHeight = ThAreaFrameTableBuilder.TextHeight;
                            table.Cells[headerRow, column].Value = ThAreaFrameTableBuilder.StandardStoreyColumnHeader(i);

                            table.Cells[dataRow, column].Alignment = CellAlignment.MiddleCenter;
                            table.Cells[dataRow, column].TextHeight = ThAreaFrameTableBuilder.TextHeight;
                            table.Cells[dataRow, column].SetAreaValue(areaOfStandardStoreys[i]);

                            column++;
                        }
                    }

                    // 地上层数
                    table.Cells[dataRow, column++].Value = engine.AboveGroundStoreyNumber();

                    // 出屋面楼梯间及屋顶机房
                    table.Cells[dataRow, column++].SetAreaValue(engine.AreaOfRoof());

                    // 报建面积
                    table.Cells[dataRow, column++].SetAreaValue(engine.AreaOfApplication());

                    // 计容面积
                    table.Cells[dataRow, column++].SetAreaValue(engine.AreaOfCapacityBuilding());

                    // 地下楼层
                    var underGroundStoreys = engine.Building.UnderGroundStoreys();
                    if (underGroundStoreys.Count > 0)
                    {
                        table.InsertColumns(column, ThAreaFrameTableBuilder.ColumnWidth, underGroundStoreys.Count);
                        foreach (ResidentialStorey storey in underGroundStoreys)
                        {
                            table.Cells[headerRow, column].Alignment = CellAlignment.MiddleCenter;
                            table.Cells[headerRow, column].TextHeight = ThAreaFrameTableBuilder.TextHeight;
                            table.Cells[headerRow, column].Value = ThAreaFrameTableBuilder.UnderGroundStoreyColumnHeader(storey.number);

                            table.Cells[dataRow, column].Alignment = CellAlignment.MiddleCenter;
                            table.Cells[dataRow, column].TextHeight = ThAreaFrameTableBuilder.TextHeight;
                            table.Cells[dataRow, column].SetAreaValue(engine.AreaOfFloor(storey.number));

                            column++;
                        }
                    }

                    // 地下总面积
                    table.Cells[dataRow, column++].SetAreaValue(engine.AreaOfUnderGround());

                    // 楼栋基底面积
                    table.Cells[dataRow, column++].SetAreaValue(engine.AreaOfFoundation());
                }
            }
        }
    }
}
