using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Linq2Acad;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThElectrical
{
    public static class ThELectricalUtils
    {
        public static string kaiBaoTableName = "凯保开关表";
        public static string cableTableName = "出线电缆与容量表";

        public static string BlockTemplateFilePath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                                @"Autodesk\ApplicationPlugins\ThCADPlugin.bundle\Contents\Support",
                                @"电气系统图规则表.dwg");
        }

        public static bool IsIn(this Point3d pt, Point3d minPoint, Point3d maxPoint)
        {
            return pt.X > minPoint.X && pt.X < maxPoint.X && pt.Y > minPoint.Y && pt.Y < maxPoint.Y;
        }

        public static List<string> powerValueRanges = GetPowerCapacities();//用静态变量保存容量范围

        /// <summary>
        /// 获取所有的容量
        /// </summary>
        /// <returns></returns>
        public static List<string> GetPowerCapacities()
        {
            using (var db = AcadDatabase.Open(ThElectrical.ThELectricalUtils.BlockTemplateFilePath(), DwgOpenMode.ReadOnly))
            {
                var table = db.ModelSpace.OfType<Table>().First(t => t.Cells[0, 0].Value != null && t.Cells[0, 0].GetRealTextString() == kaiBaoTableName);

                //找到所有的尺寸
                return table.Rows.Select((r, i) => i).Where(i => i > 1).Select(i => table.Cells[i, 3].GetRealTextString()).ToList();

            }
        }

        /// <summary>
        /// 相线规格
        /// </summary>
        /// <returns></returns>
        public static List<string> GetPhraseStyle()
        {
            using (var db = AcadDatabase.Open(ThElectrical.ThELectricalUtils.BlockTemplateFilePath(), DwgOpenMode.ReadOnly))
            {
                var table = db.ModelSpace.OfType<Table>().First(t => t.Cells[0, 0].Value != null && t.Cells[0, 0].GetRealTextString() == cableTableName);

                //找到所有的相线规格
                return table.Rows.Select((r, i) => i).Where(i => i > 1).Select(i => table.Cells[i, 1].GetRealTextString()).Distinct().ToList();

            }
        }

        /// <summary>
        /// 地线规格
        /// </summary>
        /// <returns></returns>
        public static List<string> GetGroundStyle()
        {
            using (var db = AcadDatabase.Open(ThElectrical.ThELectricalUtils.BlockTemplateFilePath(), DwgOpenMode.ReadOnly))
            {
                var table = db.ModelSpace.OfType<Table>().First(t => t.Cells[0, 0].Value != null && t.Cells[0, 0].GetRealTextString() == cableTableName);

                //找到所有的地线规格
                return table.Rows.Select((r, i) => i).Where(i => i > 1).Select(i => table.Cells[i, 2].GetRealTextString()).Distinct().ToList();

            }
        }


        /// <summary>
        /// 获取开关的范围
        /// </summary>
        /// <returns></returns>
        public static List<string> GetMainCurrents()
        {
            using (var db = AcadDatabase.Open(ThElectrical.ThELectricalUtils.BlockTemplateFilePath(), DwgOpenMode.ReadOnly))
            {
                var table = db.ModelSpace.OfType<Table>().First(t => t.Cells[0, 0].Value != null && t.Cells[0, 0].GetRealTextString() == kaiBaoTableName);

                //找到所有的尺寸
                return table.Rows.Select((r, i) => i).Where(i => i > 1).Select(i => table.Cells[i, 1].GetRealTextString()).ToList();

            }
        }

        /// <summary>
        /// 获取所有的开关的额定电流
        /// </summary>
        /// <returns></returns>
        public static List<string> GetBranchSwitchCurrents()
        {
            using (var db = AcadDatabase.Open(ThElectrical.ThELectricalUtils.BlockTemplateFilePath(), DwgOpenMode.ReadOnly))
            {
                var table = db.ModelSpace.OfType<Table>().First(t => t.Cells[0, 0].Value != null && t.Cells[0, 0].GetRealTextString() == kaiBaoTableName);

                //找到所有
                return table.Rows.Select((r, i) => i).Where(i => i > 1).Select(i => table.Cells[i, 2].GetRealTextString()).ToList();

            }
        }

        /// <summary>
        /// 获取所有的尺寸
        /// </summary>
        /// <param name="cableType"></param>s
        /// <returns></returns>
        public static List<string> GetPipeSize()
        {
            using (var db = AcadDatabase.Open(ThElectrical.ThELectricalUtils.BlockTemplateFilePath(), DwgOpenMode.ReadOnly))
            {
                var table = db.ModelSpace.OfType<Table>().First(t => t.Cells[0, 0].Value != null && t.Cells[0, 0].GetRealTextString() == cableTableName);

                //找到所有的尺寸
                return table.Rows.Select((r, i) => i).Where(i => i > 1).Select(i => table.Cells[i, 3].GetRealTextString()).Union(table.Rows.Select((r, i) => i).Where(i => i > 1).Select(i => table.Cells[i, 4].GetRealTextString())).Distinct().ToList();

            }
        }






    }
}
