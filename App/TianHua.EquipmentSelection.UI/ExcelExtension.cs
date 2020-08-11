using Microsoft.Office.Interop.Excel;

namespace TianHua.FanSelection.UI
{
    public static class ExcelExtension
    {
        public static void SetCellValue(this Worksheet sheet,string cellname,string value)
        {
            Range cell = sheet.Range[cellname];
            cell.Value2 = value;
        }

        public static Worksheet GetSheetFromSheetName(this Workbook workbook, string sheetname)
        {
            return workbook.Worksheets[sheetname];
        }
    }
}
