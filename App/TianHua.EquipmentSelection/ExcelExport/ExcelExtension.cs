﻿using Microsoft.Office.Interop.Excel;

namespace TianHua.FanSelection.ExcelExport
{
    public static class ExcelExtension
    {
        public static void SetCellValue(this Worksheet sheet, string cellname, string value)
        {
            Range cell = sheet.Range[cellname];
            cell.Value2 = value;
        }

        public static Worksheet GetSheetFromSheetName(this Workbook workbook, string sheetname)
        {
            try
            {
                return workbook.Worksheets[sheetname];
            }
            catch (System.Exception)
            {
                return null;
            }

        }
    }
}
