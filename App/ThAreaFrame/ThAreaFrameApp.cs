using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using AcHelper;
using AcHelper.Wrappers;
using System.Windows.Forms;

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
        [CommandMethod("TIANHUACAD", "THBAC", CommandFlags.Modal)]
        public void AreaCommand()
        {
            var ed = Active.Editor;

            // 选取插入点
            PromptPointResult pr = ed.GetPoint("\n请输入表格插入点: ");
            if (pr.Status != PromptStatus.OK)
                return;

            // 创建面积引擎
            ThAreaFrameDriver driver = ThAreaFrameDriver.ResidentialDriver();

            // 创建表单
            Point3d position = pr.Value.TransformBy(ed.CurrentUserCoordinateSystem);
            ObjectId id = ThAreaFrameTableBuilder.CreateBuildingAreaTable(position);
            using (AcTransaction tr = new AcTransaction())
            {
                Table table = (Table)tr.Transaction.GetObject(id, OpenMode.ForRead);
                using (new WriteEnabler(table))
                {
                    int column = 0;
                    int headerRow = (table.Rows.Count - 2);

                    // 建筑编号
                    column++;

                    // 普通楼层面积
                    var ordinaryStoreys = driver.OrdinaryStoreys();
                    if (ordinaryStoreys.Count > 0)
                    {
                        table.InsertColumns(column, ThAreaFrameTableBuilder.ColumnWidth, ordinaryStoreys.Count);
                        foreach (ResidentialStorey storey in ordinaryStoreys)
                        {
                            table.Cells[headerRow, column].Alignment = CellAlignment.MiddleCenter;
                            table.Cells[headerRow, column].TextHeight = ThAreaFrameTableBuilder.TextHeight;
                            table.Cells[headerRow, column].Value = ThAreaFrameTableBuilder.OrdinaryStoreyColumnHeader(storey.number);
                            column++;
                        }
                    }

                    // 标准楼层
                    int standardStoreyCount = driver.StandardStoreyCount();
                    if (standardStoreyCount > 0)
                    {
                        table.InsertColumns(column, ThAreaFrameTableBuilder.ColumnWidth, standardStoreyCount);
                        for (int index = 0; index < standardStoreyCount; index++)
                        {
                            table.Cells[headerRow, column + index].Alignment = CellAlignment.MiddleCenter;
                            table.Cells[headerRow, column + index].TextHeight = ThAreaFrameTableBuilder.TextHeight;
                            table.Cells[headerRow, column + index].Value = ThAreaFrameTableBuilder.StandardStoreyColumnHeader(index);
                        }
                    }
                    column += driver.StandardStoreyCount();

                    // 地上层数
                    column++;

                    // 出屋面楼梯间及屋顶机房
                    column++;

                    // 报建面积
                    column++;

                    // 计容面积
                    column++;

                    // 地下楼层
                    var underGroundStoreys = driver.UnderGroundStoreys();
                    if (underGroundStoreys.Count > 0)
                    {
                        table.InsertColumns(column, ThAreaFrameTableBuilder.ColumnWidth, underGroundStoreys.Count);
                        foreach (ResidentialStorey storey in underGroundStoreys)
                        {
                            table.Cells[headerRow, column].Alignment = CellAlignment.MiddleCenter;
                            table.Cells[headerRow, column].TextHeight = ThAreaFrameTableBuilder.TextHeight;
                            table.Cells[headerRow, column].Value = ThAreaFrameTableBuilder.UnderGroundStoreyColumnHeader(storey.number);
                            column++;
                        }
                    }

                    // 地下总面积
                    column++;

                    // 楼栋基底面积
                    column++;
                }
            }

            // 添加数据行
            using (AcTransaction tr = new AcTransaction())
            {
                Table table = (Table)tr.Transaction.GetObject(id, OpenMode.ForRead);
                using (new WriteEnabler(table))
                {
                    table.InsertRowsAndInherit(table.Rows.Count, table.Rows.Count - 1, driver.engines.Count - 1);
                }
            }

            // 填充数据
            using (AcTransaction tr = new AcTransaction())
            {
                Table table = (Table)tr.Transaction.GetObject(id, OpenMode.ForRead);
                using (new WriteEnabler(table))
                {
                    // 配置进度条
                    ProgressMeter pm = new ProgressMeter();
                    pm.Start(@"正在处理图纸");
                    pm.SetLimit(driver.engines.Count);

                    int column = 0;
                    int dataRow = 2;
                    foreach (ThAreaFrameEngine engine in driver.engines)
                    {
                        // 更新进度条
                        pm.MeterProgress();

                        // 建筑编号
                        table.Cells[dataRow, column++].Value = engine.Name;

                        // 普通楼层面积
                        foreach (ResidentialStorey storey in engine.Building.OrdinaryStoreys())
                        {
                            int index = driver.OrdinaryStoreys().FindIndex(x => x.number == storey.number);
                            table.Cells[dataRow, column + index].Alignment = CellAlignment.MiddleCenter;
                            table.Cells[dataRow, column + index].TextHeight = ThAreaFrameTableBuilder.TextHeight;
                            table.Cells[dataRow, column + index].SetAreaValue(engine.AreaOfFloor(storey.number)); 
                        }
                        column += driver.OrdinaryStoreys().Count;

                        // 标准楼层
                        for (int i = 0; i < engine.Building.StandardStoreys().Count; i++)
                        {
                            table.Cells[dataRow, column + i].Alignment = CellAlignment.MiddleCenter;
                            table.Cells[dataRow, column + i].TextHeight = ThAreaFrameTableBuilder.TextHeight;
                            table.Cells[dataRow, column + i].SetAreaValue(engine.AreaOfStandardStoreys()[i]);

                        }
                        column += driver.StandardStoreyCount();

                        // 地上层数
                        table.Cells[dataRow, column++].Value = engine.AboveGroundStoreyNumber();

                        // 出屋面楼梯间及屋顶机房
                        table.Cells[dataRow, column++].SetAreaValue(engine.AreaOfRoof());

                        // 报建面积
                        table.Cells[dataRow, column++].SetAreaValue(engine.AreaOfApplication());

                        // 计容面积
                        table.Cells[dataRow, column++].SetAreaValue(engine.AreaOfCapacityBuilding());

                        // 地下楼层
                        foreach (ResidentialStorey storey in engine.Building.UnderGroundStoreys())
                        {
                            int index = driver.UnderGroundStoreys().FindIndex(x => x.number == storey.number);
                            table.Cells[dataRow, column + index].Alignment = CellAlignment.MiddleCenter;
                            table.Cells[dataRow, column + index].TextHeight = ThAreaFrameTableBuilder.TextHeight;
                            table.Cells[dataRow, column + index].SetAreaValue(engine.AreaOfFloor(storey.number));
                        }
                        column += driver.UnderGroundStoreys().Count;

                        // 地下总面积
                        table.Cells[dataRow, column++].SetAreaValue(engine.AreaOfUnderGround());

                        // 楼栋基底面积
                        table.Cells[dataRow, column++].SetAreaValue(engine.AreaOfFoundation());

                        // 重置开始列
                        column = 0;
                        // 更新开始行
                        dataRow++;

                        // 让CAD在长时间任务处理时任然能接收消息
                        Application.DoEvents();
                    }
                    pm.Stop();  // 停止进度条更新
                }
            }

            // 销毁面积引擎
            driver.Dispose();
        }
    }
}
